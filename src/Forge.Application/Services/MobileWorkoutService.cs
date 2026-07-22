using Forge.Application.DTOs.Mobile.Workouts;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Domain.Enums;

namespace Forge.Application.Services;

public class MobileWorkoutService(
    IUserProfileRepository userProfileRepository,
    IWorkoutRepository workoutRepository) : IMobileWorkoutService
{
    public async Task<MobileWorkoutsResponse?> GetAsync(
        Guid userProfileId,
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

        var workouts = await workoutRepository.GetMobileSummariesByUserProfileAsync(
            userProfileId,
            cancellationToken);

        var activeWorkout = workouts
            .Where(workout => workout.Status == WorkoutStatus.InProgress)
            .OrderByDescending(workout => workout.WorkoutDate)
            .ThenByDescending(workout => workout.CreatedAt)
            .FirstOrDefault();

        var savedWorkouts = workouts
            .Where(workout => workout.Id != activeWorkout?.Id)
            .Where(workout => workout.Status == WorkoutStatus.Draft)
            .OrderBy(workout => workout.DisplayOrder)
            .ThenBy(workout => workout.CreatedAt)
            .Select(ToSavedWorkoutResponse)
            .ToArray();

        return new MobileWorkoutsResponse(
            activeWorkout is null ? null : ToActiveWorkoutResponse(activeWorkout),
            savedWorkouts);
    }

    private static MobileWorkoutSummaryResponse ToActiveWorkoutResponse(MobileWorkoutSummaryData workout)
    {
        return ToResponse(workout, MobileWorkoutStatus.InProgress);
    }

    private static MobileWorkoutSummaryResponse ToSavedWorkoutResponse(MobileWorkoutSummaryData workout)
    {
        return ToResponse(workout, ToMobileStatus(workout.Status));
    }

    private static MobileWorkoutSummaryResponse ToResponse(
        MobileWorkoutSummaryData workout,
        string status)
    {
        return new MobileWorkoutSummaryResponse(
            workout.Id,
            workout.Name,
            workout.MuscleGroups,
            workout.ExerciseCount,
            workout.DurationMinutes,
            workout.DurationMinutes,
            workout.DisplayOrder,
            status);
    }

    private static string ToMobileStatus(WorkoutStatus status)
    {
        return status switch
        {
            WorkoutStatus.Completed => MobileWorkoutStatus.Completed,
            WorkoutStatus.InProgress => MobileWorkoutStatus.InProgress,
            WorkoutStatus.Cancelled => MobileWorkoutStatus.Cancelled,
            _ => MobileWorkoutStatus.Available
        };
    }

}

internal static class MobileWorkoutStatus
{
    public const string InProgress = "inProgress";
    public const string Available = "available";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";
}
