using Microsoft.EntityFrameworkCore;
using Server_Licenta.Controllers;

namespace Server_Licenta
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> User { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurați modelul dacă este necesar
            base.OnModelCreating(modelBuilder);
        }
    }
}