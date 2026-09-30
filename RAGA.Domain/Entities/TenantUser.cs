namespace RAGA.Domain.Entities;

public class TenantUser
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public required string EntraObjectId { get; set; }

    public required string Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
}