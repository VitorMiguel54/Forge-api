using Forge.Application.DTOs.Backoffice.Rarities;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Validators;
using Forge.Domain.Entities;

namespace Forge.Application.Services;

public class BackofficeRarityService(
    IRarityRepository rarityRepository,
    IValidator<CreateBackofficeRarityRequest> createValidator,
    IValidator<UpdateBackofficeRarityRequest> updateValidator) : IBackofficeRarityService
{
    private const int MaxPageSize = 100;

    public async Task<BackofficeRarityListResponse> GetAsync(
        string? search = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var query = new BackofficeRarityListQuery(
            search,
            isActive,
            sortBy,
            sortDirection,
            safePage,
            safePageSize);

        var data = await rarityRepository.GetBackofficeAsync(query, cancellationToken);
        var totalPages = data.TotalItems == 0
            ? 0
            : (int)Math.Ceiling(data.TotalItems / (double)safePageSize);

        return new BackofficeRarityListResponse(
            data.Items.Select(ToResponse).ToArray(),
            safePage,
            safePageSize,
            data.TotalItems,
            totalPages);
    }

    public async Task<BackofficeRarityResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var rarity = await rarityRepository.GetBackofficeByIdAsync(id, cancellationToken);

        return rarity is null ? null : ToResponse(rarity);
    }

    public async Task<BackofficeRarityResponse> CreateAsync(
        CreateBackofficeRarityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name, ignoredId: null, cancellationToken);

        var now = DateTime.UtcNow;
        var rarity = new Rarity
        {
            Id = Guid.NewGuid(),
            Name = name,
            PrimaryColor = NormalizeColor(request.PrimaryColor),
            SecondaryColor = NormalizeOptionalColor(request.SecondaryColor),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await rarityRepository.AddAsync(rarity, cancellationToken);

        return await GetByIdAsync(rarity.Id, cancellationToken)
            ?? throw new InvalidOperationException("Rarity was created but could not be loaded.");
    }

    public async Task<BackofficeRarityResponse?> UpdateAsync(
        Guid id,
        UpdateBackofficeRarityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        ValidateUpdateRequest(request);

        var rarity = await rarityRepository.GetByIdAsync(id, cancellationToken);
        if (rarity is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name, id, cancellationToken);

        rarity.Name = name;
        rarity.PrimaryColor = NormalizeColor(request.PrimaryColor);
        rarity.SecondaryColor = NormalizeOptionalColor(request.SecondaryColor);
        rarity.DisplayOrder = request.DisplayOrder;
        rarity.UpdatedAt = DateTime.UtcNow;

        await rarityRepository.UpdateAsync(rarity, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<BackofficeRarityResponse?> UpdateStatusAsync(
        Guid id,
        UpdateBackofficeRarityStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var rarity = await rarityRepository.GetByIdAsync(id, cancellationToken);
        if (rarity is null)
        {
            return null;
        }

        rarity.IsActive = request.IsActive;
        rarity.UpdatedAt = DateTime.UtcNow;

        await rarityRepository.UpdateAsync(rarity, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var rarity = await rarityRepository.GetByIdAsync(id, cancellationToken);
        if (rarity is null)
        {
            return false;
        }

        if (await rarityRepository.HasAchievementsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Rarity cannot be deleted because it is used by achievements. Deactivate it instead.");
        }

        if (await rarityRepository.HasLevelsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Rarity cannot be deleted because it is used by levels. Deactivate it instead.");
        }

        await rarityRepository.DeleteAsync(rarity, cancellationToken);

        return true;
    }

    private async Task EnsureNameIsUniqueAsync(
        string name,
        Guid? ignoredId,
        CancellationToken cancellationToken)
    {
        if (await rarityRepository.NameExistsAsync(name, ignoredId, cancellationToken))
        {
            throw new InvalidOperationException("Rarity name already exists.");
        }
    }

    private void ValidateCreateRequest(CreateBackofficeRarityRequest request)
    {
        var validationResult = createValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private void ValidateUpdateRequest(UpdateBackofficeRarityRequest request)
    {
        var validationResult = updateValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private static BackofficeRarityResponse ToResponse(BackofficeRarityData rarity)
    {
        return new BackofficeRarityResponse(
            rarity.Id,
            rarity.Name,
            rarity.PrimaryColor,
            rarity.SecondaryColor,
            rarity.DisplayOrder,
            rarity.IsActive,
            rarity.AchievementCount,
            rarity.LevelCount,
            rarity.CreatedAt,
            rarity.UpdatedAt);
    }

    private static string NormalizeColor(string color)
    {
        return color.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptionalColor(string? color)
    {
        return string.IsNullOrWhiteSpace(color)
            ? null
            : NormalizeColor(color);
    }
}
