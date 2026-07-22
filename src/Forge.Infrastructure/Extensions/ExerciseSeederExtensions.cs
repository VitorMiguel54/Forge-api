using Forge.Infrastructure.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace Forge.Infrastructure.Extensions;

public static class ExerciseSeederExtensions
{
    public static async Task<int> SeedExercisesAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<ExerciseSeeder>();

        return await seeder.SeedAsync(cancellationToken);
    }
}
