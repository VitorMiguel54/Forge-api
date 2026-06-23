namespace Forge.Application.DTOs.Workout;

public record AddWorkoutExerciseRequest(
    Guid WorkoutId,
    Guid ExerciseId,
    int Order,
    string? Notes);
