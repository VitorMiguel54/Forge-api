using Forge.Application.DTOs.Achievement;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Domain.Entities;
using Forge.Domain.Enums;

namespace Forge.Application.Services;

public class AchievementService(
    IAchievementRepository achievementRepository,
    IWorkoutRepository workoutRepository,
    IUserProfileRepository userProfileRepository,
    IXpService xpService) : IAchievementService
{
    public async Task<IReadOnlyCollection<AchievementResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var achievements = await achievementRepository.GetActiveAsync(cancellationToken);

        return achievements
            .Select(achievement => achievement.ToResponse())
            .ToArray();
    }

    public async Task<IReadOnlyCollection<UserAchievementResponse>> GetUnlockedByUserAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return Array.Empty<UserAchievementResponse>();
        }

        var userAchievements = await achievementRepository.GetUnlockedByUserProfileAsync(
            userProfileId,
            cancellationToken);

        return userAchievements
            .Select(userAchievement => userAchievement.ToResponse())
            .ToArray();
    }

    public async Task<IReadOnlyCollection<UserAchievementResponse>> EvaluateWorkoutCompletedAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return Array.Empty<UserAchievementResponse>();
        }

        if (!await userProfileRepository.ExistsAsync(userProfileId, cancellationToken))
        {
            return Array.Empty<UserAchievementResponse>();
        }

        var achievements = await achievementRepository.GetActiveAsync(cancellationToken);
        if (achievements.Count == 0)
        {
            return Array.Empty<UserAchievementResponse>();
        }

        var values = await GetAchievementValuesAsync(userProfileId, cancellationToken);
        var unlockedAchievements = new List<UserAchievementResponse>();

        foreach (var achievement in achievements)
        {
            if (!IsSupportedForWorkoutEvaluation(achievement.Category))
            {
                continue;
            }

            if (!values.TryGetValue(achievement.Category, out var currentValue)
                || currentValue < achievement.RequiredValue)
            {
                continue;
            }

            if (await achievementRepository.IsUnlockedAsync(
                    userProfileId,
                    achievement.Id,
                    cancellationToken))
            {
                continue;
            }

            var userAchievement = new UserAchievement
            {
                Id = Guid.NewGuid(),
                UserProfileId = userProfileId,
                AchievementId = achievement.Id,
                UnlockedAt = DateTime.UtcNow
            };

            await achievementRepository.AddUnlockedAsync(userAchievement, cancellationToken);
            await xpService.AwardAchievementUnlockedAsync(userProfileId, achievement, cancellationToken);
            unlockedAchievements.Add(userAchievement.ToResponse());
        }

        return unlockedAchievements;
    }

    private async Task<IReadOnlyDictionary<AchievementCategory, int>> GetAchievementValuesAsync(
        Guid userProfileId,
        CancellationToken cancellationToken)
    {
        var utcToday = DateTime.UtcNow.Date;
        var utcWeekStart = GetUtcWeekStart(utcToday);
        var completedWorkouts = await workoutRepository.CountCompletedByUserProfileAsync(
            userProfileId,
            cancellationToken);
        var weeklyCompletedWorkouts = await workoutRepository.CountCompletedByUserProfileSinceAsync(
            userProfileId,
            utcWeekStart,
            cancellationToken);
        var totalVolume = await workoutRepository.SumCompletedVolumeByUserProfileAsync(
            userProfileId,
            cancellationToken);

        return new Dictionary<AchievementCategory, int>
        {
            [AchievementCategory.Workout] = completedWorkouts,
            [AchievementCategory.Consistency] = weeklyCompletedWorkouts,
            [AchievementCategory.Progression] = (int)Math.Floor(totalVolume)
        };
    }

    private static bool IsSupportedForWorkoutEvaluation(AchievementCategory category)
    {
        return category is AchievementCategory.Workout
            or AchievementCategory.Consistency
            or AchievementCategory.Progression;
    }

    private static DateTime GetUtcWeekStart(DateTime utcDate)
    {
        var daysSinceMonday = ((int)utcDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

        return utcDate.AddDays(-daysSinceMonday);
    }
}
