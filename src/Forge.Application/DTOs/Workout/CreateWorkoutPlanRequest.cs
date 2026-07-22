namespace Forge.Application.DTOs.Workout;

public record CreateWorkoutPlanRequest(
    Guid UserProfileId,
    string Name,
    DateTime WorkoutDate,
    string? Location,
    string? Notes,
    IReadOnlyCollection<CreateWorkoutPlanExerciseRequest>? Exercises);

public record CreateWorkoutPlanExerciseRequest(
    Guid ExerciseId,
    int Order,
    string? Notes);
