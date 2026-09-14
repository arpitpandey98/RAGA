using Microsoft.EntityFrameworkCore;
using RAGA.Domain.Entities;
using Document = RAGA.Domain.Entities.Document;

namespace RAGA.Infrastructure.Data
{
    public class RAGADbContext : DbContext
    {
        public RAGADbContext(DbContextOptions<RAGADbContext> options) : base(options)
        {

        }

        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DocumentChunk>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Content)
                    .IsRequired();

                entity.Property(x => x.Embedding)
                    .HasColumnType("vector(1536)");

                entity.HasOne(x => x.Document)
                    .WithMany()
                    .HasForeignKey(x => x.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new
                {
                    x.DocumentId,
                    x.ChunkIndex
                });
            });
        }
    }
}
