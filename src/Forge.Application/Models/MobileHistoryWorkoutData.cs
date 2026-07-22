namespace Forge.Application.Models;

public record MobileHistoryWorkoutData(
    Guid Id,
    string Name,
    DateTime WorkoutDate,
    int DurationMinutes,
    decimal TotalVolume,
    int ExerciseCount,
    IReadOnlyCollection<string> MuscleGroups = null!);
