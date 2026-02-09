using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ErpOnlineOrder.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ErpOnlineOrderDbContext>
    {
        public ErpOnlineOrderDbContext CreateDbContext(string[] args)
        {
            // ??c configuration t? appsettings.json c?a WebAPI project
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ErpOnlineOrder.WebAPI"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ErpOnlineOrderDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ErpOnlineOrderDbContext(optionsBuilder.Options);
        }
    }
}
