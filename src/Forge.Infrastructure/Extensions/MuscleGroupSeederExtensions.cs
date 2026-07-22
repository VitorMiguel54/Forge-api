using Forge.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace Forge.Infrastructure.Extensions;

public static class MuscleGroupSeederExtensions
{
    public static async Task<int> SeedMuscleGroupsAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<MuscleGroupSeeder>();

        return await seeder.SeedAsync(cancellationToken);
    }
}
