namespace Forge.Application.DTOs.Workout;

public record WorkoutExerciseResponse(
    Guid Id,
    Guid WorkoutId,
    Guid ExerciseId,
    int Order,
    string? Notes);
