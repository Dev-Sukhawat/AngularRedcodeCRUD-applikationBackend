using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;

namespace LibraryApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Detta gör att .NET förstår att vi vill ha en tabell som heter "Users"
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Quote> Quotes { get; set; }
    }
}