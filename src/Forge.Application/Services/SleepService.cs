using Forge.Application.DTOs.Sleep;
using Forge.Application.Interfaces;

namespace Forge.Application.Services;

public class SleepService : ISleepService
{
    public Task<SleepRecordResponse> RegisterAsync(CreateSleepRecordRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<SleepRecordResponse>> GetByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
