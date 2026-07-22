using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface IWorkoutExerciseRepository
{
    Task<IReadOnlyCollection<WorkoutExercise>> GetByWorkoutAsync(
        Guid workoutId,
        CancellationToken cancellationToken = default);

    Task<WorkoutExercise?> GetByWorkoutAndIdAsync(
        Guid workoutId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExerciseExistsInWorkoutAsync(
        Guid workoutId,
        Guid exerciseId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(WorkoutExercise workoutExercise, CancellationToken cancellationToken = default);
    Task DeleteAsync(WorkoutExercise workoutExercise, CancellationToken cancellationToken = default);
}
