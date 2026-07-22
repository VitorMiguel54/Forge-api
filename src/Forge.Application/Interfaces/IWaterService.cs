using Forge.Application.DTOs.Water;

namespace Forge.Application.Interfaces;

public interface IWaterService
{
    Task<WaterIntakeResponse> CreateAsync(
        Guid userProfileId,
        CreateWaterIntakeRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WaterIntakeResponse>?> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<WaterIntakeResponse?> GetTodayByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
