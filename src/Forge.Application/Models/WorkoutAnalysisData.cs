namespace Forge.Application.Models;

public record WorkoutAnalysisData(
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
    IReadOnlyCollection<WorkoutAnalysisExerciseData> Exercises);

public record WorkoutAnalysisExerciseData(
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
    IReadOnlyCollection<WorkoutAnalysisSetData> Sets);

public record WorkoutAnalysisSetData(
    Guid Id,
    int SetNumber,
    int Repetitions,
    decimal Weight,
    decimal Volume,
    string? Notes);
