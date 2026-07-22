using Forge.Application.DTOs.Backoffice.Rarities;

namespace Forge.Application.Interfaces;

public interface IBackofficeRarityService
{
    Task<BackofficeRarityListResponse> GetAsync(
        string? search = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<BackofficeRarityResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BackofficeRarityResponse> CreateAsync(CreateBackofficeRarityRequest request, CancellationToken cancellationToken = default);
    Task<BackofficeRarityResponse?> UpdateAsync(Guid id, UpdateBackofficeRarityRequest request, CancellationToken cancellationToken = default);
    Task<BackofficeRarityResponse?> UpdateStatusAsync(Guid id, UpdateBackofficeRarityStatusRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
