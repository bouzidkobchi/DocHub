using DocHub.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DocHub.Api.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<PathsPair> HashPathsPairs { get; set; }
        public DbSet<AppUser> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=mydb.db");
        }
    }
}
