using Microsoft.EntityFrameworkCore;
using Document = RAGA.Domain.Entities.Document;

namespace RAGA.Infrastructure.Data
{
    public class RAGADbContext : DbContext
    {
        public RAGADbContext(DbContextOptions<RAGADbContext> options) : base(options)
        {
            
        }

        public DbSet<Document> Documents { get; set; } = null!;
    }
}
