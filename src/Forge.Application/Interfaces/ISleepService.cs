using Forge.Application.DTOs.Sleep;

namespace Forge.Application.Interfaces;

public interface ISleepService
{
    Task<SleepRecordResponse> RegisterAsync(CreateSleepRecordRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SleepRecordResponse>> GetByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
