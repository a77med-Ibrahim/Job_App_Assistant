using Microsoft.EntityFrameworkCore;
using backend.Models;
namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Record> Records { get; set; }
        public DbSet<JobMatch> JobMatches { get; set; }
        public DbSet<CoverLetter> CoverLetters { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }

    }
}