using Forge.Application.DTOs.Workout;

namespace Forge.Application.Interfaces;

public interface IWorkoutExerciseService
{
    Task<IReadOnlyCollection<WorkoutExerciseResponse>?> GetByWorkoutAsync(
        Guid workoutId,
        CancellationToken cancellationToken = default);

    Task<WorkoutExerciseResponse> CreateAsync(
        Guid workoutId,
        CreateWorkoutExerciseRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid workoutId,
        Guid id,
        CancellationToken cancellationToken = default);
}
