using Forge.Application.DTOs.Backoffice;
using Forge.Application.DTOs.Backoffice.Achievements;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Validators;
using Forge.Domain.Constants;
using Forge.Domain.Entities;
using Forge.Domain.Enums;

namespace Forge.Application.Services;

public class BackofficeAchievementService(
    IAchievementRepository achievementRepository,
    IAdminImageStorage imageStorage,
    IValidator<CreateBackofficeAchievementRequest> createValidator,
    IValidator<UpdateBackofficeAchievementRequest> updateValidator) : IBackofficeAchievementService
{
    private const int MaxPageSize = 100;

    public async Task<BackofficeAchievementListResponse> GetAsync(
        string? search = null,
        AchievementCategory? category = null,
        bool? isActive = null,
        bool? isSecret = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var safeCategory = category.HasValue && Enum.IsDefined(typeof(AchievementCategory), category.Value)
            ? category
            : null;
        var query = new BackofficeAchievementListQuery(
            search,
            safeCategory,
            isActive,
            isSecret,
            sortBy,
            sortDirection,
            safePage,
            safePageSize);
        var data = await achievementRepository.GetBackofficeAsync(query, cancellationToken);
        var totalPages = data.TotalItems == 0
            ? 0
            : (int)Math.Ceiling(data.TotalItems / (double)safePageSize);

        return new BackofficeAchievementListResponse(
            data.Items.Select(ToResponse).ToArray(),
            safePage,
            safePageSize,
            data.TotalItems,
            totalPages);
    }

    public async Task<BackofficeAchievementResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var achievement = await achievementRepository.GetBackofficeByIdAsync(id, cancellationToken);

        return achievement is null ? null : ToResponse(achievement);
    }

    public async Task<BackofficeAchievementResponse> CreateAsync(
        CreateBackofficeAchievementRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name, ignoredId: null, cancellationToken);

        var now = DateTime.UtcNow;
        var achievement = new Achievement
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = request.Description.Trim(),
            Category = request.Category,
            RequiredValue = request.RequiredValue,
            IsSecret = request.IsSecret,
            IsActive = request.IsActive,
            XpReward = request.XpReward,
            CreatedAt = now,
            UpdatedAt = now
        };

        await achievementRepository.AddAsync(achievement, cancellationToken);

        return await GetByIdAsync(achievement.Id, cancellationToken)
            ?? throw new InvalidOperationException("Achievement was created but could not be loaded.");
    }

    public async Task<BackofficeAchievementResponse?> UpdateAsync(
        Guid id,
        UpdateBackofficeAchievementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        ValidateUpdateRequest(request);

        var achievement = await achievementRepository.GetByIdAsync(id, cancellationToken);
        if (achievement is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name, id, cancellationToken);

        achievement.Name = name;
        achievement.Description = request.Description.Trim();
        achievement.Category = request.Category;
        achievement.RequiredValue = request.RequiredValue;
        achievement.XpReward = request.XpReward;
        achievement.IsSecret = request.IsSecret;
        achievement.UpdatedAt = DateTime.UtcNow;

        await achievementRepository.UpdateAsync(achievement, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<BackofficeAchievementResponse?> UpdateStatusAsync(
        Guid id,
        UpdateBackofficeAchievementStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var achievement = await achievementRepository.GetByIdAsync(id, cancellationToken);
        if (achievement is null)
        {
            return null;
        }

        achievement.IsActive = request.IsActive;
        achievement.UpdatedAt = DateTime.UtcNow;

        await achievementRepository.UpdateAsync(achievement, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<AdminImageUploadResponse?> UploadBadgeImageAsync(
        Guid id,
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        AdminImageUploadValidator.Validate(fileName, contentType, fileSize, stream);

        var achievement = await achievementRepository.GetByIdAsync(id, cancellationToken);
        if (achievement is null)
        {
            return null;
        }

        var previousStorageKey = achievement.BadgeImageStorageKey;
        var storedFile = await imageStorage.UploadAsync(
            stream,
            fileName,
            contentType,
            $"achievements/{id}/badge",
            cancellationToken);

        try
        {
            achievement.BadgeImageUrl = storedFile.PublicUrl;
            achievement.BadgeImageStorageKey = storedFile.StorageKey;
            achievement.UpdatedAt = DateTime.UtcNow;

            await achievementRepository.UpdateAsync(achievement, cancellationToken);
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

        return new AdminImageUploadResponse(
            achievement.Id,
            nameof(Achievement.BadgeImageUrl),
            storedFile.PublicUrl,
            storedFile.ContentType,
            storedFile.FileSize,
            achievement.UpdatedAt);
    }

    public async Task<bool?> DeleteBadgeImageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var achievement = await achievementRepository.GetByIdAsync(id, cancellationToken);
        if (achievement is null)
        {
            return null;
        }

        var previousStorageKey = achievement.BadgeImageStorageKey;
        achievement.BadgeImageUrl = null;
        achievement.BadgeImageStorageKey = null;
        achievement.UpdatedAt = DateTime.UtcNow;

        await achievementRepository.UpdateAsync(achievement, cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousStorageKey))
        {
            await imageStorage.DeleteAsync(previousStorageKey, cancellationToken);
        }

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var achievement = await achievementRepository.GetByIdAsync(id, cancellationToken);
        if (achievement is null)
        {
            return false;
        }

        if (IsOfficial(id))
        {
            throw new InvalidOperationException("Official achievements cannot be deleted. Deactivate them instead.");
        }

        if (await achievementRepository.HasUserAchievementsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Achievement cannot be deleted because it has already been unlocked by users. Deactivate it instead.");
        }

        if (await achievementRepository.HasXpTransactionsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Achievement cannot be deleted because it is linked to XP history. Deactivate it instead.");
        }

        await achievementRepository.DeleteAsync(achievement, cancellationToken);

        return true;
    }

    private async Task EnsureNameIsUniqueAsync(
        string name,
        Guid? ignoredId,
        CancellationToken cancellationToken)
    {
        if (await achievementRepository.NameExistsAsync(name, ignoredId, cancellationToken))
        {
            throw new InvalidOperationException("Achievement name already exists.");
        }
    }

    private void ValidateCreateRequest(CreateBackofficeAchievementRequest request)
    {
        var validationResult = createValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private void ValidateUpdateRequest(UpdateBackofficeAchievementRequest request)
    {
        var validationResult = updateValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private static BackofficeAchievementResponse ToResponse(BackofficeAchievementData achievement)
    {
        return new BackofficeAchievementResponse(
            achievement.Id,
            achievement.Name,
            achievement.Description,
            achievement.Category,
            achievement.Category.ToString(),
            achievement.RequiredValue,
            achievement.XpReward,
            achievement.IsSecret,
            achievement.IsActive,
            IsOfficial(achievement.Id),
            achievement.BadgeImageUrl,
            achievement.UnlockedCount,
            achievement.CreatedAt,
            achievement.UpdatedAt);
    }

    private static bool IsOfficial(Guid id)
    {
        return OfficialAchievementIds.Contains(id);
    }
}
