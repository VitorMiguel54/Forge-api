namespace Forge.Application.DTOs.Workout;

public record RegisterWorkoutSetRequest(
    Guid WorkoutExerciseId,
    int SetNumber,
    int Repetitions,
    decimal Weight);
