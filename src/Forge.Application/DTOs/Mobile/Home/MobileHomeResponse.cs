namespace Forge.Application.DTOs.Mobile.Home;

public record MobileHomeResponse(
    MobileHomeUserResponse User,
    MobileHomeGamificationResponse Gamification,
    MobileHomeWeightResponse Weight,
    MobileHomeWaterResponse Water,
    MobileHomeSleepResponse Sleep,
    MobileHomeWeeklyProgressResponse WeeklyProgress,
    MobileHomeActiveWorkoutResponse? ActiveWorkout,
    MobileHomeMetricsSummaryResponse MetricsSummary);

public record MobileHomeUserResponse(
    Guid Id,
    string Name);

public record MobileHomeGamificationResponse(
    int Level,
    int CurrentXp,
    int XpToNextLevel,
    string? LevelName = null,
    string? LevelDescription = null,
    string? LevelBadgeImageUrl = null,
    string? GuardianImageUrl = null,
    string? Rarity = null,
    string? NextLevelName = null,
    decimal ProgressPercentage = 0,
    bool IsMaximumLevel = false);

public record MobileHomeWeightResponse(
    decimal InitialWeight,
    decimal CurrentWeight,
    decimal Difference);

public record MobileHomeWaterResponse(
    decimal TodayConsumption,
    decimal DailyGoal,
    bool GoalAchieved);

public record MobileHomeSleepResponse(
    decimal? LatestHours,
    decimal DailyGoal,
    bool GoalAchieved);

public record MobileHomeWeeklyProgressResponse(
    int CompletedWorkouts,
    int WorkoutGoal,
    decimal ProgressPercent);

public record MobileHomeActiveWorkoutResponse(
    Guid Id,
    string Name,
    DateTime WorkoutDate,
    int ExerciseCount,
    int DurationMinutes,
    decimal CurrentVolume);

public record MobileHomeMetricsSummaryResponse(
    int CompletedWorkouts,
    decimal TotalVolumeMoved,
    decimal WeeklyVolumeMoved,
    decimal TodayWaterConsumption,
    decimal? LatestSleepHours);
