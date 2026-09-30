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
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Tenant)
                    .WithMany(x => x.Documents)
                    .HasForeignKey(x => x.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


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

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.Property(x => x.UpdatedAt)
                    .IsRequired();

                entity.HasOne(x => x.Tenant)
                    .WithMany(x => x.Conversations)
                    .HasForeignKey(x => x.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Role)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.Content)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasOne(x => x.Conversation)
                    .WithMany(x => x.Messages)
                    .HasForeignKey(x => x.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new
                {
                    x.ConversationId,
                    x.CreatedAt
                });
            });

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(x => x.Name)
                    .IsUnique();

                entity.HasMany(x => x.Users)
                    .WithOne(x => x.Tenant)
                    .HasForeignKey(x => x.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Documents)
                    .WithOne(x => x.Tenant)
                    .HasForeignKey(x => x.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Conversations)
                    .WithOne(x => x.Tenant)
                    .HasForeignKey(x => x.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TenantUser>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.EntraObjectId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Role)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(x => new { x.TenantId, x.EntraObjectId })
                    .IsUnique();
            });
        }
    }
}
