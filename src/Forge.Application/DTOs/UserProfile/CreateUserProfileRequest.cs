namespace Forge.Application.DTOs.UserProfile;

public record CreateUserProfileRequest(
    string Name,
    string Email,
    decimal InitialWeight,
    decimal DailyWaterGoalInLiters,
    decimal DailySleepGoalInHours,
    int WeeklyWorkoutGoal);
