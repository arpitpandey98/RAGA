namespace RAGA.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public FileType FileType { get; set; }
        public required string FileName { get; set; }
        public required string BlobPath { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Status Status { get; set; }
    }
}
