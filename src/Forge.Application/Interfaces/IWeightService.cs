using Forge.Application.DTOs.Weight;

namespace Forge.Application.Interfaces;

public interface IWeightService
{
    Task<WeightRecordResponse> RegisterAsync(CreateWeightRecordRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<WeightRecordResponse>> GetByUserAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
