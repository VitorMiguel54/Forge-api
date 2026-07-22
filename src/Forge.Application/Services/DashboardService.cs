using Forge.Application.DTOs.Dashboard;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;

namespace Forge.Application.Services;

public class DashboardService(
    IUserProfileRepository userProfileRepository,
    IWorkoutRepository workoutRepository,
    IWaterIntakeRepository waterIntakeRepository,
    ISleepRecordRepository sleepRecordRepository,
    ILevelProgressionService levelProgressionService) : IDashboardService
{

    public async Task<DashboardResponse?> GetAsync(
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

        var completedWorkoutsCount = await workoutRepository.CountCompletedByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var latestWorkout = await workoutRepository.GetLatestCompletedByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var totalVolumeMoved = await workoutRepository.SumCompletedVolumeByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var todayWaterConsumption = await waterIntakeRepository.SumTodayByUserProfileAsync(
            userProfileId,
            DateTime.UtcNow.Date,
            cancellationToken);

        var latestSleepRecord = await sleepRecordRepository.GetLatestByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var progression = await levelProgressionService.GetProgressionAsync(userProfile.TotalXp, cancellationToken);

        return new DashboardResponse(
            new DashboardUserResponse(userProfile.Name),
            new DashboardWeightResponse(
                userProfile.InitialWeight,
                userProfile.CurrentWeight,
                userProfile.CurrentWeight - userProfile.InitialWeight),
            new DashboardWorkoutSummaryResponse(
                completedWorkoutsCount,
                latestWorkout?.ToResponse(),
                totalVolumeMoved),
            new DashboardWaterResponse(
                todayWaterConsumption,
                userProfile.DailyWaterGoalInLiters),
            new DashboardSleepResponse(
                latestSleepRecord?.ToResponse(),
                userProfile.DailySleepGoalInHours,
                latestSleepRecord?.GoalAchieved ?? false),
            new DashboardGamificationResponse(
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
                progression.IsMaximumLevel));
    }
}
