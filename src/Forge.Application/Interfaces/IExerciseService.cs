using Forge.Application.DTOs.Exercise;

namespace Forge.Application.Interfaces;

public interface IExerciseService
{
    Task<ExerciseResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ExerciseResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExerciseResponse> CreateAsync(CreateExerciseRequest request, CancellationToken cancellationToken = default);
}
