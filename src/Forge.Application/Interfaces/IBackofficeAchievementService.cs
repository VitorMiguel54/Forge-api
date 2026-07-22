using Forge.Application.DTOs.Backoffice.Achievements;
using Forge.Application.DTOs.Backoffice;
using Forge.Domain.Enums;

namespace Forge.Application.Interfaces;

public interface IBackofficeAchievementService
{
    Task<BackofficeAchievementListResponse> GetAsync(
        string? search = null,
        AchievementCategory? category = null,
        bool? isActive = null,
        bool? isSecret = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<BackofficeAchievementResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BackofficeAchievementResponse> CreateAsync(CreateBackofficeAchievementRequest request, CancellationToken cancellationToken = default);
    Task<BackofficeAchievementResponse?> UpdateAsync(Guid id, UpdateBackofficeAchievementRequest request, CancellationToken cancellationToken = default);
    Task<BackofficeAchievementResponse?> UpdateStatusAsync(Guid id, UpdateBackofficeAchievementStatusRequest request, CancellationToken cancellationToken = default);
    Task<AdminImageUploadResponse?> UploadBadgeImageAsync(Guid id, Stream stream, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default);
    Task<bool?> DeleteBadgeImageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
