using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RAGA.Application.Common.Extensions;
using RAGA.Application.Interfaces;
using RAGA.Domain.Entities;
using RAGA.Infrastructure.Data;
using System.Security.Claims;

namespace RAGA.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly RAGADbContext _dbContext;
    private readonly IConfiguration _configuration;

    public TenantService(
        RAGADbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<int> GetCurrentTenantIdAsync(
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var objectId = user.GetUserObjectId();

        // Existing user: resolve their application tenant.
        var existingTenantUser = await _dbContext.TenantUsers
            .SingleOrDefaultAsync(
                x => x.EntraObjectId == objectId,
                ct);

        if (existingTenantUser != null)
        {
            return existingTenantUser.TenantId;
        }

        // Only customer External ID users can be
        // automatically provisioned.
        var tokenTenantId = user.GetTenantId();

        var customerTenantId =
            _configuration["CustomerAzureAd:TenantId"];

        if (!string.Equals(
                tokenTenantId,
                customerTenantId,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The current user is not associated with a tenant.");
        }

        // First-time customer:
        // create a new application tenant.
        var tenant = new Tenant
        {
            Name = $"Customer-{objectId[..8]}"
        };

        _dbContext.Tenants.Add(tenant);

        await _dbContext.SaveChangesAsync(ct);

        // Make the first customer user the tenant Admin.
        var tenantUser = new TenantUser
        {
            TenantId = tenant.Id,
            EntraObjectId = objectId,
            Role = "Admin"
        };

        _dbContext.TenantUsers.Add(tenantUser);

        await _dbContext.SaveChangesAsync(ct);

        return tenant.Id;
    }
}
