using Forge.Application.DTOs.Water;

namespace Forge.Application.Interfaces;

public interface IWaterService
{
    Task<WaterIntakeResponse> RegisterAsync(CreateWaterIntakeRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<WaterIntakeResponse>> GetByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
