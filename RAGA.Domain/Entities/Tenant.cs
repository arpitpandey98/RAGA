namespace RAGA.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<TenantUser> Users { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Document> Documents { get; set; } = [];

    public ICollection<Conversation> Conversations { get; set; } = [];
}