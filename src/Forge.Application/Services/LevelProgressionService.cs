using Forge.Application.Interfaces;
using Forge.Application.Models;

namespace Forge.Application.Services;

public class LevelProgressionService(ILevelDefinitionRepository levelDefinitionRepository) : ILevelProgressionService
{
    private const int FallbackXpPerLevel = 500;

    public async Task<LevelProgressionData> GetProgressionAsync(int totalXp, CancellationToken cancellationToken = default)
    {
        var safeTotalXp = Math.Max(0, totalXp);
        var levels = (await levelDefinitionRepository.GetActiveAsync(cancellationToken))
            .OrderBy(level => level.MinimumXp)
            .ThenBy(level => level.DisplayOrder)
            .ToArray();

        if (levels.Length == 0)
        {
            return BuildFallbackProgression(safeTotalXp);
        }

        var current = levels.LastOrDefault(level => level.MinimumXp <= safeTotalXp) ?? levels.First();
        var currentIndex = Array.IndexOf(levels, current);
        var next = currentIndex >= 0 && currentIndex < levels.Length - 1 ? levels[currentIndex + 1] : null;
        var numericLevel = current.DisplayOrder;

        if (next is null)
        {
            return new LevelProgressionData(current, null, safeTotalXp, numericLevel, Math.Max(0, safeTotalXp - current.MinimumXp), 0, 100, true);
        }

        var range = Math.Max(1, next.MinimumXp - current.MinimumXp);
        var xpInCurrentLevel = Math.Clamp(safeTotalXp - current.MinimumXp, 0, range);
        var xpToNextLevel = Math.Max(0, next.MinimumXp - safeTotalXp);
        var progressPercentage = Math.Round((decimal)xpInCurrentLevel / range * 100, 2);

        return new LevelProgressionData(current, next, safeTotalXp, numericLevel, xpInCurrentLevel, xpToNextLevel, progressPercentage, false);
    }

    private static LevelProgressionData BuildFallbackProgression(int totalXp)
    {
        var numericLevel = totalXp <= 0 ? 1 : (totalXp / FallbackXpPerLevel) + 1;
        var xpToNext = FallbackXpPerLevel - (totalXp % FallbackXpPerLevel);
        return new LevelProgressionData(null, null, totalXp, numericLevel, totalXp % FallbackXpPerLevel, xpToNext == 0 ? FallbackXpPerLevel : xpToNext, 0, false);
    }
}
