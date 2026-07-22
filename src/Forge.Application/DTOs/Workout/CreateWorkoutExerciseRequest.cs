namespace Forge.Application.DTOs.Workout;

public record CreateWorkoutExerciseRequest(
    Guid ExerciseId,
    int Order,
    string? Notes);
