using Forge.Domain.Constants;
using Forge.Infrastructure.Seeding;
using Xunit;

namespace Forge.Api.Tests.Services;

public class AchievementCatalogTests
{
    [Fact]
    public void All_ContainsExpectedInitialCatalog()
    {
        Assert.Equal(11, AchievementCatalog.All.Count);
        Assert.All(AchievementCatalog.All, achievement => Assert.NotEqual(Guid.Empty, achievement.Id));
        Assert.Equal(
            AchievementCatalog.All.Count,
            AchievementCatalog.All.Select(achievement => achievement.Id).Distinct().Count());
        Assert.Equal(
            AchievementCatalog.All.Count,
            AchievementCatalog.All.Select(achievement => achievement.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void All_UsesOfficialStableIds()
    {
        Assert.Equal(OfficialAchievementIds.All.Count, AchievementCatalog.All.Count);
        Assert.All(AchievementCatalog.All, achievement => Assert.Contains(achievement.Id, OfficialAchievementIds.All));
    }
}