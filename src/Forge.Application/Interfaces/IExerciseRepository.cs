using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface IExerciseRepository
{
    Task<IReadOnlyCollection<Exercise>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Exercise exercise, CancellationToken cancellationToken = default);
    Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken = default);
    Task DeleteAsync(Exercise exercise, CancellationToken cancellationToken = default);
}
