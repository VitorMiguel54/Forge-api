namespace Forge.Application.DTOs.Mobile.Workouts;

public record MobileWorkoutsResponse(
    MobileWorkoutSummaryResponse? ActiveWorkout,
    IReadOnlyCollection<MobileWorkoutSummaryResponse> SavedWorkouts);

public record MobileWorkoutSummaryResponse(
    Guid Id,
    string Name,
    IReadOnlyCollection<string> MuscleGroups,
    int ExerciseCount,
    int DurationMinutes,
    int EstimatedDurationMinutes,
    int DisplayOrder,
    string Status);
