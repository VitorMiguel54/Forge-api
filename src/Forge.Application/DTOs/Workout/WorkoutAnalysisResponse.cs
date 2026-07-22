namespace Forge.Application.DTOs.Workout;

public record WorkoutAnalysisResponse(
    Guid Id,
    string Name,
    DateTime WorkoutDate,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    int DurationMinutes,
    decimal TotalVolume,
    int TotalExercises,
    int TotalSets,
    int TotalRepetitions,
    string Status,
    IReadOnlyCollection<WorkoutAnalysisExerciseResponse> Exercises);

public record WorkoutAnalysisExerciseResponse(
    Guid WorkoutExerciseId,
    Guid ExerciseId,
    string Name,
    string MuscleGroup,
    string? Equipment,
    int Order,
    string? Notes,
    int TotalSets,
    int TotalRepetitions,
    decimal BestWeight,
    decimal TotalVolume,
    decimal? PreviousBestWeight,
    decimal? WeightDifference,
    decimal? WeightDifferencePercentage,
    IReadOnlyCollection<WorkoutAnalysisSetResponse> Sets);

public record WorkoutAnalysisSetResponse(
    Guid Id,
    int SetNumber,
    int Repetitions,
    decimal Weight,
    decimal Volume,
    string? Notes);
