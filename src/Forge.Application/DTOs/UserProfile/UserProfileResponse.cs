namespace Forge.Application.DTOs.UserProfile;

public record UserProfileResponse(
    Guid Id,
    string Name,
    string Email,
    decimal InitialWeight,
    decimal CurrentWeight,
    int Level,
    int TotalXp,
    decimal DailyWaterGoalInLiters,
    decimal DailySleepGoalInHours,
    int WeeklyWorkoutGoal,
    DateTime CreatedAt,
    DateTime UpdatedAt);
