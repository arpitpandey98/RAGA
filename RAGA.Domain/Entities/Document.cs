namespace RAGA.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public FileType FileType { get; set; }
        public required string FileName { get; set; }
        public required string BlobPath { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public string? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Status Status { get; set; }
    }
}
