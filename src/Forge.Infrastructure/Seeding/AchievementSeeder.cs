using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Seeding;

public class AchievementSeeder(ForgeDbContext dbContext)
{
    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingAchievements = await dbContext.Achievements
            .Select(achievement => new
            {
                achievement.Id,
                achievement.Name
            })
            .ToArrayAsync(cancellationToken);

        var existingIds = existingAchievements
            .Select(achievement => achievement.Id)
            .ToHashSet();
        var existingNames = existingAchievements
            .Select(achievement => achievement.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        var missingAchievements = AchievementCatalog.All
            .Where(achievement =>
                !existingIds.Contains(achievement.Id)
                && !existingNames.Contains(achievement.Name))
            .Select(achievement => ToEntity(achievement, now))
            .ToArray();

        if (missingAchievements.Length == 0)
        {
            return 0;
        }

        await dbContext.Achievements.AddRangeAsync(missingAchievements, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return missingAchievements.Length;
    }

    private static Achievement ToEntity(AchievementSeedDefinition achievement, DateTime utcNow)
    {
        return new Achievement
        {
            Id = achievement.Id,
            Name = achievement.Name,
            Description = achievement.Description,
            Category = achievement.Category,
            RequiredValue = achievement.RequiredValue,
            IsSecret = achievement.IsSecret,
            XpReward = achievement.XpReward,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
}
