using Forge.Application.DTOs.Workout;

namespace Forge.Application.Interfaces;

public interface IWorkoutSetService
{
    Task<IReadOnlyCollection<WorkoutSetResponse>?> GetByWorkoutExerciseAsync(
        Guid workoutExerciseId,
        CancellationToken cancellationToken = default);

    Task<WorkoutSetResponse> CreateAsync(
        Guid workoutExerciseId,
        CreateWorkoutSetRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkoutSetResponse?> UpdateAsync(
        Guid id,
        UpdateWorkoutSetRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
