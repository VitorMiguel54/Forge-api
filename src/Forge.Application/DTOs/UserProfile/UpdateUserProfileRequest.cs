namespace Forge.Application.DTOs.UserProfile;

public record UpdateUserProfileRequest(
    string Name,
    string Email,
    decimal InitialWeight,
    decimal CurrentWeight,
    decimal DailyWaterGoalInLiters,
    decimal DailySleepGoalInHours,
    int WeeklyWorkoutGoal);
