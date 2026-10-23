using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DzentoPOS2.Data.Context
{
    public class AppDbContextFactory
        : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "../DzentoPOS2");

            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(
                        "appsettings.json",
                        optional: false)
                    .Build();

            var connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection");

            var optionsBuilder =
                new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlite(connectionString);

            return new AppDbContext(
                optionsBuilder.Options);
        }
    }
}