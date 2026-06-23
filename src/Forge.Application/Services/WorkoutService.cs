using Forge.Application.DTOs.Workout;
using Forge.Application.Interfaces;

namespace Forge.Application.Services;

public class WorkoutService : IWorkoutService
{
    public Task<WorkoutResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkoutResponse> CreateAsync(CreateWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkoutExerciseResponse> AddExerciseAsync(AddWorkoutExerciseRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkoutSetResponse> RegisterSetAsync(RegisterWorkoutSetRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
