using Forge.Application.DTOs.Exercise;
using Forge.Application.Interfaces;

namespace Forge.Application.Services;

public class ExerciseService : IExerciseService
{
    public Task<ExerciseResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<ExerciseResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ExerciseResponse> CreateAsync(CreateExerciseRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
