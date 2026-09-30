using Microsoft.EntityFrameworkCore;
using RAGA.Application.Interfaces;
using RAGA.Infrastructure.Data;

namespace RAGA.Infrastructure.Services;

public class TenantBootstrapService : ITenantBootstrapService
{
    private readonly RAGADbContext _dbContext;

    public TenantBootstrapService(RAGADbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureAdminTenantAsync(
        string ownerEntraObjectId,
        CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants
            .SingleOrDefaultAsync(x => x.Name == "RAGA Admin", ct);

        if (tenant == null)
        {
            tenant = new Domain.Entities.Tenant
            {
                Name = "RAGA Admin"
            };

            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync(ct);
        }

        var ownerExists = await _dbContext.TenantUsers
            .AnyAsync(
                x => x.TenantId == tenant.Id &&
                     x.EntraObjectId == ownerEntraObjectId,
                ct);

        if (!ownerExists)
        {
            _dbContext.TenantUsers.Add(new Domain.Entities.TenantUser
            {
                TenantId = tenant.Id,
                EntraObjectId = ownerEntraObjectId,
                Role = "Owner"
            });

            await _dbContext.SaveChangesAsync(ct);
        }
    }
}