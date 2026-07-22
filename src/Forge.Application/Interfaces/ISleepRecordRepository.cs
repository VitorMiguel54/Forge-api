using Forge.Domain.Entities;

namespace Forge.Application.Interfaces;

public interface ISleepRecordRepository
{
    Task<IReadOnlyCollection<SleepRecord>> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<SleepRecord?> GetLatestByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<SleepRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(SleepRecord sleepRecord, CancellationToken cancellationToken = default);
    Task DeleteAsync(SleepRecord sleepRecord, CancellationToken cancellationToken = default);
}
