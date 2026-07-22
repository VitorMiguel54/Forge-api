namespace Forge.Application.Interfaces;

public interface IWorkoutMuscleGroupRepository
{
    Task SyncFromWorkoutExercisesAsync(Guid workoutId, CancellationToken cancellationToken = default);
}
