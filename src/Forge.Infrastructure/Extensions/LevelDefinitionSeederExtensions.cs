using Forge.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace Forge.Infrastructure.Extensions;

public static class LevelDefinitionSeederExtensions
{
    public static async Task<int> SeedLevelDefinitionsAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<LevelDefinitionSeeder>();
        return await seeder.SeedAsync(cancellationToken);
    }
}
