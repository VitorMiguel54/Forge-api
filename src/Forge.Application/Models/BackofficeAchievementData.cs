using Forge.Domain.Enums;

namespace Forge.Application.Models;

public record BackofficeAchievementData(
    Guid Id,
    string Name,
    string Description,
    AchievementCategory Category,
    int RequiredValue,
    int XpReward,
    bool IsSecret,
    bool IsActive,
    string? BadgeImageUrl,
    int UnlockedCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
