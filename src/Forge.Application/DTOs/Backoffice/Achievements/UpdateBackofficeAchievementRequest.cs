using Forge.Domain.Enums;

namespace Forge.Application.DTOs.Backoffice.Achievements;

public record UpdateBackofficeAchievementRequest(
    string Name,
    string Description,
    AchievementCategory Category,
    int RequiredValue,
    int XpReward,
    bool IsSecret);