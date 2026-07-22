using Forge.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace Forge.Infrastructure.Extensions;

public static class RaritySeederExtensions
{
    public static async Task<int> SeedRaritiesAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<RaritySeeder>();

        return await seeder.SeedAsync(cancellationToken);
    }
}
