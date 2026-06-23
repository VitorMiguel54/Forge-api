using Forge.Application.DTOs.Workout;

namespace Forge.Application.Interfaces;

public interface IWorkoutService
{
    Task<WorkoutResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkoutResponse> CreateAsync(CreateWorkoutRequest request, CancellationToken cancellationToken = default);
    Task<WorkoutExerciseResponse> AddExerciseAsync(AddWorkoutExerciseRequest request, CancellationToken cancellationToken = default);
    Task<WorkoutSetResponse> RegisterSetAsync(RegisterWorkoutSetRequest request, CancellationToken cancellationToken = default);
}
