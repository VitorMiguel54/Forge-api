using Forge.Application.DTOs.Backoffice;
using Forge.Application.DTOs.Backoffice.Levels;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Validators;
using Forge.Domain.Constants;
using Forge.Domain.Entities;

namespace Forge.Application.Services;

public class BackofficeLevelDefinitionService(
    ILevelDefinitionRepository levelDefinitionRepository,
    IAdminImageStorage imageStorage,
    IValidator<CreateBackofficeLevelDefinitionRequest> createValidator,
    IValidator<UpdateBackofficeLevelDefinitionRequest> updateValidator) : IBackofficeLevelDefinitionService
{
    private const int MaxPageSize = 100;

    public async Task<BackofficeLevelDefinitionListResponse> GetAsync(string? search = null, Guid? rarityId = null, bool? isActive = null, string? sortBy = null, string? sortDirection = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var query = new BackofficeLevelDefinitionListQuery(search, rarityId == Guid.Empty ? null : rarityId, isActive, sortBy, sortDirection, safePage, safePageSize);
        var data = await levelDefinitionRepository.GetBackofficeAsync(query, cancellationToken);
        var totalPages = data.TotalItems == 0 ? 0 : (int)Math.Ceiling(data.TotalItems / (double)safePageSize);
        return new BackofficeLevelDefinitionListResponse(data.Items.Select(LevelResponseFactory.ToBackofficeResponse).ToArray(), safePage, safePageSize, data.TotalItems, totalPages);
    }

    public async Task<BackofficeLevelDefinitionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return null;
        var level = await levelDefinitionRepository.GetBackofficeByIdAsync(id, cancellationToken);
        return level is null ? null : LevelResponseFactory.ToBackofficeResponse(level);
    }

    public async Task<BackofficeLevelDefinitionResponse> CreateAsync(CreateBackofficeLevelDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);
        await ValidateBusinessRulesAsync(request.Name.Trim(), request.MinimumXp, request.DisplayOrder, request.RarityId, ignoredId: null, cancellationToken);
        var now = DateTime.UtcNow;
        var level = new LevelDefinition
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            MinimumXp = request.MinimumXp,
            DisplayOrder = request.DisplayOrder,
            RarityId = request.RarityId,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };
        await levelDefinitionRepository.AddAsync(level, cancellationToken);
        return await GetByIdAsync(level.Id, cancellationToken) ?? throw new InvalidOperationException("Level definition was created but could not be loaded.");
    }

    public async Task<BackofficeLevelDefinitionResponse?> UpdateAsync(Guid id, UpdateBackofficeLevelDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return null;
        ValidateUpdateRequest(request);
        var level = await levelDefinitionRepository.GetByIdAsync(id, cancellationToken);
        if (level is null) return null;
        await ValidateBusinessRulesAsync(request.Name.Trim(), request.MinimumXp, request.DisplayOrder, request.RarityId, id, cancellationToken);
        if (level.IsActive && level.MinimumXp == 0 && request.MinimumXp != 0 && !await levelDefinitionRepository.ExistsActiveInitialLevelAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("At least one active level with minimum XP zero is required.");
        }
        level.Name = request.Name.Trim();
        level.Description = request.Description.Trim();
        level.MinimumXp = request.MinimumXp;
        level.DisplayOrder = request.DisplayOrder;
        level.RarityId = request.RarityId;
        level.UpdatedAt = DateTime.UtcNow;
        await levelDefinitionRepository.UpdateAsync(level, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<BackofficeLevelDefinitionResponse?> UpdateStatusAsync(Guid id, UpdateBackofficeLevelDefinitionStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return null;
        var level = await levelDefinitionRepository.GetByIdAsync(id, cancellationToken);
        if (level is null) return null;
        if (!request.IsActive && level.MinimumXp == 0)
        {
            throw new InvalidOperationException("The initial level cannot be deactivated.");
        }
        level.IsActive = request.IsActive;
        level.UpdatedAt = DateTime.UtcNow;
        await levelDefinitionRepository.UpdateAsync(level, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<AdminImageUploadResponse?> UploadGuardianImageAsync(
        Guid id,
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return null;

        AdminImageUploadValidator.Validate(fileName, contentType, fileSize, stream);

        var level = await levelDefinitionRepository.GetByIdAsync(id, cancellationToken);
        if (level is null) return null;

        var previousStorageKey = level.GuardianImageStorageKey;
        var storedFile = await imageStorage.UploadAsync(stream, fileName, contentType, $"levels/{id}/guardian", cancellationToken);

        try
        {
            level.GuardianImageUrl = storedFile.PublicUrl;
            level.GuardianImageStorageKey = storedFile.StorageKey;
            level.UpdatedAt = DateTime.UtcNow;

            await levelDefinitionRepository.UpdateAsync(level, cancellationToken);
        }
        catch
        {
            await imageStorage.DeleteAsync(storedFile.StorageKey, cancellationToken);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(previousStorageKey))
        {
            await imageStorage.DeleteAsync(previousStorageKey, cancellationToken);
        }

        return new AdminImageUploadResponse(level.Id, nameof(LevelDefinition.GuardianImageUrl), storedFile.PublicUrl, storedFile.ContentType, storedFile.FileSize, level.UpdatedAt);
    }

    public async Task<bool?> DeleteGuardianImageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return null;

        var level = await levelDefinitionRepository.GetByIdAsync(id, cancellationToken);
        if (level is null) return null;

        var previousStorageKey = level.GuardianImageStorageKey;
        level.GuardianImageUrl = null;
        level.GuardianImageStorageKey = null;
        level.UpdatedAt = DateTime.UtcNow;

        await levelDefinitionRepository.UpdateAsync(level, cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousStorageKey))
        {
            await imageStorage.DeleteAsync(previousStorageKey, cancellationToken);
        }

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return false;
        var level = await levelDefinitionRepository.GetByIdAsync(id, cancellationToken);
        if (level is null) return false;
        if (OfficialLevelDefinitionIds.Contains(id))
        {
            throw new InvalidOperationException("Official levels cannot be deleted. Deactivate them instead.");
        }
        if (level.IsActive && level.MinimumXp == 0 && !await levelDefinitionRepository.ExistsActiveInitialLevelAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("At least one active level with minimum XP zero is required.");
        }
        await levelDefinitionRepository.DeleteAsync(level, cancellationToken);
        return true;
    }

    private async Task ValidateBusinessRulesAsync(string name, int minimumXp, int displayOrder, Guid rarityId, Guid? ignoredId, CancellationToken cancellationToken)
    {
        if (!await levelDefinitionRepository.RarityExistsAsync(rarityId, cancellationToken)) throw new InvalidOperationException("Level rarity does not exist.");
        if (await levelDefinitionRepository.NameExistsAsync(name, ignoredId, cancellationToken)) throw new InvalidOperationException("Level name already exists.");
        if (await levelDefinitionRepository.DisplayOrderExistsAsync(displayOrder, ignoredId, cancellationToken)) throw new InvalidOperationException("Level display order already exists.");
        if (await levelDefinitionRepository.MinimumXpExistsAsync(minimumXp, ignoredId, cancellationToken)) throw new InvalidOperationException("Level minimum XP already exists.");
    }

    private void ValidateCreateRequest(CreateBackofficeLevelDefinitionRequest request)
    {
        var result = createValidator.Validate(request);
        if (!result.IsValid) throw new ArgumentException(string.Join(' ', result.Errors));
    }

    private void ValidateUpdateRequest(UpdateBackofficeLevelDefinitionRequest request)
    {
        var result = updateValidator.Validate(request);
        if (!result.IsValid) throw new ArgumentException(string.Join(' ', result.Errors));
    }
}
