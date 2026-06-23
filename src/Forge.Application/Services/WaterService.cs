using Forge.Application.DTOs.Water;
using Forge.Application.Interfaces;

namespace Forge.Application.Services;

public class WaterService : IWaterService
{
    public Task<WaterIntakeResponse> RegisterAsync(CreateWaterIntakeRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<WaterIntakeResponse>> GetByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
