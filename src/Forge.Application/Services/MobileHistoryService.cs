using Forge.Application.DTOs.Mobile.History;
using Forge.Application.Interfaces;
using Forge.Application.Models;

namespace Forge.Application.Services;

public class MobileHistoryService(
    IUserProfileRepository userProfileRepository,
    IWorkoutRepository workoutRepository) : IMobileHistoryService
{
    private const int MaxPageSize = 50;

    public async Task<MobileHistoryResponse?> GetAsync(
        Guid userProfileId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return null;
        }

        if (!await userProfileRepository.ExistsAsync(userProfileId, cancellationToken))
        {
            return null;
        }

        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var skip = (normalizedPage - 1) * normalizedPageSize;
        var utcWeekStart = GetUtcWeekStart(DateTime.UtcNow.Date);

        var totalWorkouts = await workoutRepository.CountCompletedByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var totalDurationMinutes = await workoutRepository.SumCompletedDurationMinutesByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var weeklyVolume = await workoutRepository.SumCompletedVolumeByUserProfileSinceAsync(
            userProfileId,
            utcWeekStart,
            cancellationToken);

        var workouts = await workoutRepository.GetMobileHistoryWorkoutsByUserProfileAsync(
            userProfileId,
            skip,
            normalizedPageSize,
            cancellationToken);

        return new MobileHistoryResponse(
            new MobileHistorySummaryResponse(
                totalWorkouts,
                totalDurationMinutes,
                weeklyVolume),
            new MobileHistoryPageResponse(
                normalizedPage,
                normalizedPageSize,
                totalWorkouts,
                CalculateTotalPages(totalWorkouts, normalizedPageSize)),
            workouts.Select(ToResponse).ToArray());
    }

    private static DateTime GetUtcWeekStart(DateTime utcDate)
    {
        var daysSinceMonday = ((int)utcDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

        return utcDate.AddDays(-daysSinceMonday);
    }

    private static MobileHistoryWorkoutResponse ToResponse(MobileHistoryWorkoutData workout)
    {
        return new MobileHistoryWorkoutResponse(
            workout.Id,
            workout.Name,
            workout.WorkoutDate,
            workout.DurationMinutes,
            workout.TotalVolume,
            workout.ExerciseCount,
            workout.MuscleGroups ?? Array.Empty<string>());
    }

    private static int CalculateTotalPages(int totalItems, int pageSize)
    {
        if (totalItems <= 0)
        {
            return 0;
        }

        return (int)Math.Ceiling((double)totalItems / pageSize);
    }
}
