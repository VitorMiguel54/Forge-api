using Forge.Domain.Enums;

namespace Forge.Application.DTOs.Backoffice.Achievements;

public record CreateBackofficeAchievementRequest(
    string Name,
    string Description,
    AchievementCategory Category,
    int RequiredValue,
    int XpReward,
    bool IsSecret,
    bool IsActive = true);