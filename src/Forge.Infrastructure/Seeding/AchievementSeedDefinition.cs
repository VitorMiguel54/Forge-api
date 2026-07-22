using Forge.Domain.Enums;

namespace Forge.Infrastructure.Seeding;

public sealed record AchievementSeedDefinition(
    Guid Id,
    string Name,
    string Description,
    AchievementCategory Category,
    int RequiredValue,
    bool IsSecret,
    int XpReward);
