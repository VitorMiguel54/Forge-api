using Forge.Domain.Enums;

namespace Forge.Application.Models;

public record MobileWorkoutSummaryData(
    Guid Id,
    string Name,
    DateTime WorkoutDate,
    DateTime CreatedAt,
    int DisplayOrder,
    IReadOnlyCollection<string> MuscleGroups,
    int ExerciseCount,
    int DurationMinutes,
    WorkoutStatus Status);
