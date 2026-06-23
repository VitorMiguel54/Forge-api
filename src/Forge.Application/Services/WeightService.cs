using Forge.Application.DTOs.Weight;
using Forge.Application.Interfaces;

namespace Forge.Application.Services;

public class WeightService : IWeightService
{
    public Task<WeightRecordResponse> RegisterAsync(CreateWeightRecordRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<WeightRecordResponse>> GetByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
