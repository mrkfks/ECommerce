using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            // SQLite için connection string
            optionsBuilder.UseSqlite("Data Source=ECommerce.db");
            
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
