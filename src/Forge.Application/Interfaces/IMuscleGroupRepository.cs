using Forge.Application.Models;
using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface IMuscleGroupRepository
{
    Task<IReadOnlyCollection<MuscleGroup>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BackofficeMuscleGroupData>> GetBackofficeAsync(CancellationToken cancellationToken = default);
    Task<BackofficeMuscleGroupData?> GetBackofficeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MuscleGroup?> GetByIdIncludingInactiveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? ignoredId = null, CancellationToken cancellationToken = default);
    Task<bool> HasExercisesAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default);
    Task UpdateAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default);
    Task DeleteAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default);
}
