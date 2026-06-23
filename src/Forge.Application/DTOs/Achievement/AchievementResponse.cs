using Forge.Domain.Enums;

namespace Forge.Application.DTOs.Achievement;

public record AchievementResponse(
    Guid Id,
    string Name,
    string Description,
    AchievementCategory Category,
    int RequiredValue,
    bool IsSecret,
    int XpReward);
