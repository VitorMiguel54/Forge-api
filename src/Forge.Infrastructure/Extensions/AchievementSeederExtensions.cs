using Forge.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace Forge.Infrastructure.Extensions;

public static class AchievementSeederExtensions
{
    public static async Task<int> SeedAchievementsAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AchievementSeeder>();

        return await seeder.SeedAsync(cancellationToken);
    }
}
