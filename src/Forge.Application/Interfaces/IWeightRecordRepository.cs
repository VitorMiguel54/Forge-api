using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface IWeightRecordRepository
{
    Task<IReadOnlyCollection<WeightRecord>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<WeightRecord?> GetLatestByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<WeightRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(WeightRecord weightRecord, CancellationToken cancellationToken = default);
    Task DeleteAsync(WeightRecord weightRecord, CancellationToken cancellationToken = default);
}
