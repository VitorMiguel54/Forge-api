namespace Forge.Application.DTOs.Workout;

public record WorkoutSetResponse(
    Guid Id,
    Guid WorkoutExerciseId,
    int SetNumber,
    int Repetitions,
    decimal Weight,
    decimal Volume,
    string? Notes);
