using Forge.Application.DTOs.Achievement;
using Forge.Domain.Entities;

namespace Forge.Application.Mappings;

public static class AchievementMappings
{
    public static AchievementResponse ToResponse(this Achievement achievement)
    {
        return new AchievementResponse(
            achievement.Id,
            achievement.Name,
            achievement.Description,
            achievement.Category,
            achievement.RequiredValue,
            achievement.IsSecret,
            achievement.XpReward);
    }

    public static UserAchievementResponse ToResponse(this UserAchievement userAchievement)
    {
        return new UserAchievementResponse(
            userAchievement.Id,
            userAchievement.UserProfileId,
            userAchievement.AchievementId,
            userAchievement.UnlockedAt);
    }
}
