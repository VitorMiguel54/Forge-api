using Forge.Application.DTOs.Mobile.History;

namespace Forge.Application.Interfaces;

public interface IMobileHistoryService
{
    Task<MobileHistoryResponse?> GetAsync(
        Guid userProfileId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
