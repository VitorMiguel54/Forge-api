using Forge.Application.Models;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Seeding;

public class MuscleGroupSeeder(ForgeDbContext dbContext)
{
    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingGroups = await dbContext.MuscleGroups
            .Select(muscleGroup => new
            {
                muscleGroup.Id,
                muscleGroup.Name
            })
            .ToArrayAsync(cancellationToken);

        var existingIds = existingGroups
            .Select(muscleGroup => muscleGroup.Id)
            .ToHashSet();
        var existingNames = existingGroups
            .Select(muscleGroup => muscleGroup.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        var missingGroups = MuscleGroupCatalog.CreateEntities(now)
            .Where(muscleGroup =>
                !existingIds.Contains(muscleGroup.Id)
                && !existingNames.Contains(muscleGroup.Name))
            .ToArray();

        if (missingGroups.Length == 0)
        {
            return 0;
        }

        await dbContext.MuscleGroups.AddRangeAsync(missingGroups, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return missingGroups.Length;
    }
}
