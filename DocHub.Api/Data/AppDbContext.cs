using DocHub.Api.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DocHub.Api.Data
{
    public class AppDbContext : IdentityDbContext<AppUser , IdentityRole<string> , string>
    {
        public DbSet<PathsPair> HashPathsPairs { get; set; }
        public DbSet<AppUser> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DocHub;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;");
        }
    }
}
