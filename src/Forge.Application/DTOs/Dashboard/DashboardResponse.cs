using Forge.Application.DTOs.Sleep;
using Forge.Application.DTOs.Workout;

namespace Forge.Application.DTOs.Dashboard;

public record DashboardResponse(
    DashboardUserResponse User,
    DashboardWeightResponse Weight,
    DashboardWorkoutSummaryResponse Workouts,
    DashboardWaterResponse Water,
    DashboardSleepResponse Sleep,
    DashboardGamificationResponse Gamification);

public record DashboardUserResponse(
    string Name);

public record DashboardWeightResponse(
    decimal InitialWeight,
    decimal CurrentWeight,
    decimal Difference);

public record DashboardWorkoutSummaryResponse(
    int CompletedCount,
    WorkoutResponse? LatestWorkout,
    decimal TotalVolumeMoved);

public record DashboardWaterResponse(
    decimal TodayConsumption,
    decimal DailyGoal);

public record DashboardSleepResponse(
    SleepRecordResponse? LatestRecord,
    decimal DailyGoal,
    bool GoalAchieved);

public record DashboardGamificationResponse(
    int Level,
    int Xp,
    int XpToNextLevel,
    string? LevelName = null,
    string? LevelDescription = null,
    string? LevelBadgeImageUrl = null,
    string? GuardianImageUrl = null,
    string? Rarity = null,
    string? NextLevelName = null,
    decimal ProgressPercentage = 0,
    bool IsMaximumLevel = false);
