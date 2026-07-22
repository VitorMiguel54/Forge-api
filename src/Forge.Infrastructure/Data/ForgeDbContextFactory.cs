using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Forge.Infrastructure.Data;

public class ForgeDbContextFactory : IDesignTimeDbContextFactory<ForgeDbContext>
{
    public ForgeDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ForgeDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new ForgeDbContext(optionsBuilder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiProjectPath = ResolveApiProjectPath();

        return new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();
    }

    private static string ResolveApiProjectPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            Path.Combine(currentDirectory, "src", "Forge.Api"),
            Path.Combine(currentDirectory, "..", "Forge.Api"),
            Path.Combine(currentDirectory, "..", "..", "Forge.Api"),
            currentDirectory
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(Path.Combine(fullPath, "appsettings.json")))
            {
                return fullPath;
            }
        }

        throw new FileNotFoundException("Could not find Forge.Api appsettings.json.");
    }
}
