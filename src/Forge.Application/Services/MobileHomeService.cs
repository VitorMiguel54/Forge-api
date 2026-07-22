using Forge.Application.DTOs.Mobile.Home;
using Forge.Application.Interfaces;
using Forge.Domain.Entities;

namespace Forge.Application.Services;

public class MobileHomeService(
    IUserProfileRepository userProfileRepository,
    IWorkoutRepository workoutRepository,
    IWaterIntakeRepository waterIntakeRepository,
    ISleepRecordRepository sleepRecordRepository,
    ILevelProgressionService levelProgressionService) : IMobileHomeService
{

    public async Task<MobileHomeResponse?> GetAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return null;
        }

        var userProfile = await userProfileRepository.GetByIdAsync(userProfileId, cancellationToken);
        if (userProfile is null)
        {
            return null;
        }

        var utcToday = DateTime.UtcNow.Date;
        var utcWeekStart = GetUtcWeekStart(utcToday);

        var completedWorkouts = await workoutRepository.CountCompletedByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var weeklyCompletedWorkouts = await workoutRepository.CountCompletedByUserProfileSinceAsync(
            userProfileId,
            utcWeekStart,
            cancellationToken);

        var totalVolumeMoved = await workoutRepository.SumCompletedVolumeByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var weeklyVolumeMoved = await workoutRepository.SumCompletedVolumeByUserProfileSinceAsync(
            userProfileId,
            utcWeekStart,
            cancellationToken);

        var todayWaterConsumption = await waterIntakeRepository.SumTodayByUserProfileAsync(
            userProfileId,
            utcToday,
            cancellationToken);

        var latestSleepRecord = await sleepRecordRepository.GetLatestByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var activeWorkout = await workoutRepository.GetLatestInProgressByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var progression = await levelProgressionService.GetProgressionAsync(userProfile.TotalXp, cancellationToken);

        return new MobileHomeResponse(
            new MobileHomeUserResponse(
                userProfile.Id,
                userProfile.Name),
            new MobileHomeGamificationResponse(
                progression.NumericLevel,
                userProfile.TotalXp,
                progression.XpToNextLevel,
                progression.CurrentLevel?.Name,
                progression.CurrentLevel?.Description,
                progression.CurrentLevel?.BadgeImageUrl,
                progression.CurrentLevel?.GuardianImageUrl,
                progression.CurrentLevel?.RarityName,
                progression.NextLevel?.Name,
                progression.ProgressPercentage,
                progression.IsMaximumLevel),
            new MobileHomeWeightResponse(
                userProfile.InitialWeight,
                userProfile.CurrentWeight,
                userProfile.CurrentWeight - userProfile.InitialWeight),
            new MobileHomeWaterResponse(
                todayWaterConsumption,
                userProfile.DailyWaterGoalInLiters,
                todayWaterConsumption >= userProfile.DailyWaterGoalInLiters),
            new MobileHomeSleepResponse(
                latestSleepRecord?.HoursSlept,
                userProfile.DailySleepGoalInHours,
                latestSleepRecord?.GoalAchieved ?? false),
            new MobileHomeWeeklyProgressResponse(
                weeklyCompletedWorkouts,
                userProfile.WeeklyWorkoutGoal,
                CalculateProgressPercent(weeklyCompletedWorkouts, userProfile.WeeklyWorkoutGoal)),
            activeWorkout is null ? null : ToActiveWorkoutResponse(activeWorkout),
            new MobileHomeMetricsSummaryResponse(
                completedWorkouts,
                totalVolumeMoved,
                weeklyVolumeMoved,
                todayWaterConsumption,
                latestSleepRecord?.HoursSlept));
    }

    private static DateTime GetUtcWeekStart(DateTime utcDate)
    {
        var daysSinceMonday = ((int)utcDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

        return utcDate.AddDays(-daysSinceMonday);
    }

    private static decimal CalculateProgressPercent(int current, int target)
    {
        if (target <= 0 || current <= 0)
        {
            return 0;
        }

        return Math.Min((decimal)current / target * 100, 100);
    }

    private static MobileHomeActiveWorkoutResponse ToActiveWorkoutResponse(Workout workout)
    {
        return new MobileHomeActiveWorkoutResponse(
            workout.Id,
            workout.Name,
            workout.WorkoutDate,
            workout.WorkoutExercises.Count,
            CalculateDurationMinutes(workout.StartedAt, DateTime.UtcNow),
            workout.WorkoutExercises
                .SelectMany(workoutExercise => workoutExercise.WorkoutSets)
                .Sum(workoutSet => workoutSet.Weight * workoutSet.Repetitions));
    }

    private static int CalculateDurationMinutes(DateTime? startedAt, DateTime utcNow)
    {
        if (startedAt is null || utcNow <= startedAt)
        {
            return 0;
        }

        return (int)Math.Floor((utcNow - startedAt.Value).TotalMinutes);
    }
}
