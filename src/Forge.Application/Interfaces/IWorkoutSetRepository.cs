using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface IWorkoutSetRepository
{
    Task<IReadOnlyCollection<WorkoutSet>> GetByWorkoutExerciseAsync(
        Guid workoutExerciseId,
        CancellationToken cancellationToken = default);

    Task<WorkoutSet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(WorkoutSet workoutSet, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkoutSet workoutSet, CancellationToken cancellationToken = default);
    Task DeleteAsync(WorkoutSet workoutSet, CancellationToken cancellationToken = default);
}
