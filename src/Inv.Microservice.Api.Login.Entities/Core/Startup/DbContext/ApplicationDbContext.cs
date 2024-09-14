using Microsoft.EntityFrameworkCore;

namespace Inv.Microservice.Api.Login.Entities.Core.Startup.DbContext
{
    public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = "Server=localhost;Database=inventory_db;User=root;Password=;";
                var serverVersion = ServerVersion.AutoDetect(connectionString);

                optionsBuilder.UseMySql(connectionString, serverVersion);
            }
        }
        public DbSet<User> user { get; set; }
        public DbSet<AdminUser> Adminuser { get; set; }
        public DbSet<Product> product { get; set; }
        public DbSet<Venta> Ventas { get; set; }
    }
}
