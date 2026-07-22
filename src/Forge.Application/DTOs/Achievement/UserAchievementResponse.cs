namespace Forge.Application.DTOs.Achievement;

public record UserAchievementResponse(
    Guid Id,
    Guid UserProfileId,
    Guid AchievementId,
    DateTime UnlockedAt);
