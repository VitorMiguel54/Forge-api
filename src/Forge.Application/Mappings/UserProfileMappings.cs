using Forge.Application.DTOs.UserProfile;
using Forge.Domain.Entities;

namespace Forge.Application.Mappings;

public static class UserProfileMappings
{
    public static UserProfileResponse ToResponse(this UserProfile userProfile)
    {
        return new UserProfileResponse(
            userProfile.Id,
            userProfile.Name,
            userProfile.Email,
            userProfile.InitialWeight,
            userProfile.CurrentWeight,
            userProfile.Level,
            userProfile.TotalXp,
            userProfile.DailyWaterGoalInLiters,
            userProfile.DailySleepGoalInHours,
            userProfile.WeeklyWorkoutGoal,
            userProfile.CreatedAt,
            userProfile.UpdatedAt);
    }
}
