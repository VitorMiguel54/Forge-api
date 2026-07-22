using Forge.Domain.Enums;

namespace Forge.Application.DTOs.Backoffice.Achievements;

public record BackofficeAchievementResponse(
    Guid Id,
    string Name,
    string Description,
    AchievementCategory Category,
    string CategoryName,
    int RequiredValue,
    int XpReward,
    bool IsSecret,
    bool IsActive,
    bool IsOfficial,
    string? BadgeImageUrl,
    int UnlockedCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
