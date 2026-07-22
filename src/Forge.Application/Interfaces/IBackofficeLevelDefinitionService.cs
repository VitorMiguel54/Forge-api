using Forge.Application.DTOs.Backoffice.Levels;
using Forge.Application.DTOs.Backoffice;

namespace Forge.Application.Interfaces;

public interface IBackofficeLevelDefinitionService
{
    Task<BackofficeLevelDefinitionListResponse> GetAsync(string? search = null, Guid? rarityId = null, bool? isActive = null, string? sortBy = null, string? sortDirection = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<BackofficeLevelDefinitionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BackofficeLevelDefinitionResponse> CreateAsync(CreateBackofficeLevelDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<BackofficeLevelDefinitionResponse?> UpdateAsync(Guid id, UpdateBackofficeLevelDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<BackofficeLevelDefinitionResponse?> UpdateStatusAsync(Guid id, UpdateBackofficeLevelDefinitionStatusRequest request, CancellationToken cancellationToken = default);
    Task<AdminImageUploadResponse?> UploadGuardianImageAsync(Guid id, Stream stream, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default);
    Task<bool?> DeleteGuardianImageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
