using Forge.Application.DTOs.Weight;

namespace Forge.Application.Interfaces;

public interface IWeightService
{
    Task<WeightRecordResponse> CreateAsync(
        Guid userProfileId,
        CreateWeightRecordRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WeightRecordResponse>?> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<WeightRecordResponse?> GetLatestByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
