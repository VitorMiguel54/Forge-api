using Forge.Application.DTOs.Sleep;

namespace Forge.Application.Interfaces;

public interface ISleepService
{
    Task<SleepRecordResponse> CreateAsync(
        Guid userProfileId,
        CreateSleepRecordRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SleepRecordResponse>?> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<SleepRecordResponse?> GetLatestByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
