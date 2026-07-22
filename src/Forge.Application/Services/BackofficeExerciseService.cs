using Forge.Application.DTOs.Backoffice.Exercises;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Validators;
using Forge.Domain.Entities;
using MuscleGroupEnum = Forge.Domain.Enums.MuscleGroup;

namespace Forge.Application.Services;

public class BackofficeExerciseService(
    IExerciseRepository exerciseRepository,
    IMuscleGroupRepository muscleGroupRepository,
    IValidator<CreateBackofficeExerciseRequest> createValidator,
    IValidator<UpdateBackofficeExerciseRequest> updateValidator) : IBackofficeExerciseService
{
    private const int MaxPageSize = 100;

    public async Task<BackofficeExerciseListResponse> GetAsync(
        string? search = null,
        Guid? muscleGroupId = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDirection = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var query = new BackofficeExerciseListQuery(
            search,
            muscleGroupId == Guid.Empty ? null : muscleGroupId,
            isActive,
            sortBy,
            sortDirection,
            safePage,
            safePageSize);
        var data = await exerciseRepository.GetBackofficeAsync(query, cancellationToken);
        var totalPages = data.TotalItems == 0
            ? 0
            : (int)Math.Ceiling(data.TotalItems / (double)safePageSize);

        return new BackofficeExerciseListResponse(
            data.Items.Select(ToResponse).ToArray(),
            safePage,
            safePageSize,
            data.TotalItems,
            totalPages);
    }

    public async Task<BackofficeExerciseResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var exercise = await exerciseRepository.GetBackofficeByIdAsync(id, cancellationToken);

        return exercise is null ? null : ToResponse(exercise);
    }

    public async Task<BackofficeExerciseResponse> CreateAsync(
        CreateBackofficeExerciseRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);
        await ValidateMuscleGroupAsync(request.MuscleGroupId, cancellationToken);

        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name, ignoredId: null, cancellationToken);

        var now = DateTime.UtcNow;
        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = NormalizeOptionalText(request.Description),
            MuscleGroup = ResolveLegacyMuscleGroup(request.MuscleGroupId),
            MuscleGroupId = request.MuscleGroupId,
            Difficulty = NormalizeOptionalText(request.Difficulty),
            Equipment = NormalizeOptionalText(request.Equipment),
            IsCustom = request.IsCustom,
            IsActive = true,
            DisplayOrder = request.DisplayOrder,
            ImageUrl = NormalizeOptionalText(request.ImageUrl),
            GifUrl = NormalizeOptionalText(request.GifUrl),
            VideoUrl = NormalizeOptionalText(request.VideoUrl),
            ThumbnailUrl = NormalizeOptionalText(request.ThumbnailUrl),
            UserProfileId = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        await exerciseRepository.AddAsync(exercise, cancellationToken);

        return await GetByIdAsync(exercise.Id, cancellationToken)
            ?? throw new InvalidOperationException("Exercise was created but could not be loaded.");
    }

    public async Task<BackofficeExerciseResponse?> UpdateAsync(
        Guid id,
        UpdateBackofficeExerciseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        ValidateUpdateRequest(request);
        await ValidateMuscleGroupAsync(request.MuscleGroupId, cancellationToken);

        var exercise = await exerciseRepository.GetByIdAsync(id, cancellationToken);
        if (exercise is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name, id, cancellationToken);

        exercise.Name = name;
        exercise.Description = NormalizeOptionalText(request.Description);
        exercise.MuscleGroup = ResolveLegacyMuscleGroup(request.MuscleGroupId);
        exercise.MuscleGroupId = request.MuscleGroupId;
        exercise.Difficulty = NormalizeOptionalText(request.Difficulty);
        exercise.Equipment = NormalizeOptionalText(request.Equipment);
        exercise.IsCustom = request.IsCustom;
        exercise.DisplayOrder = request.DisplayOrder;
        exercise.ImageUrl = NormalizeOptionalText(request.ImageUrl);
        exercise.GifUrl = NormalizeOptionalText(request.GifUrl);
        exercise.VideoUrl = NormalizeOptionalText(request.VideoUrl);
        exercise.ThumbnailUrl = NormalizeOptionalText(request.ThumbnailUrl);
        exercise.UserProfileId = null;
        exercise.UpdatedAt = DateTime.UtcNow;

        await exerciseRepository.UpdateAsync(exercise, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<BackofficeExerciseResponse?> UpdateStatusAsync(
        Guid id,
        UpdateBackofficeExerciseStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var exercise = await exerciseRepository.GetByIdAsync(id, cancellationToken);
        if (exercise is null)
        {
            return null;
        }

        exercise.IsActive = request.IsActive;
        exercise.UpdatedAt = DateTime.UtcNow;

        await exerciseRepository.UpdateAsync(exercise, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var exercise = await exerciseRepository.GetByIdAsync(id, cancellationToken);
        if (exercise is null)
        {
            return false;
        }

        if (await exerciseRepository.IsInUseAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Exercise cannot be deleted because it is linked to a workout.");
        }

        await exerciseRepository.DeleteAsync(exercise, cancellationToken);

        return true;
    }

    private async Task ValidateMuscleGroupAsync(Guid muscleGroupId, CancellationToken cancellationToken)
    {
        if (!await muscleGroupRepository.ExistsAsync(muscleGroupId, cancellationToken))
        {
            throw new ArgumentException("Muscle group does not exist.");
        }
    }

    private async Task EnsureNameIsUniqueAsync(
        string name,
        Guid? ignoredId,
        CancellationToken cancellationToken)
    {
        if (await exerciseRepository.NameExistsAsync(name, ignoredId, cancellationToken))
        {
            throw new InvalidOperationException("Exercise name already exists.");
        }
    }

    private void ValidateCreateRequest(CreateBackofficeExerciseRequest request)
    {
        var validationResult = createValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private void ValidateUpdateRequest(UpdateBackofficeExerciseRequest request)
    {
        var validationResult = updateValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private static BackofficeExerciseResponse ToResponse(BackofficeExerciseData exercise)
    {
        return new BackofficeExerciseResponse(
            exercise.Id,
            exercise.Name,
            exercise.Description,
            exercise.MuscleGroupId ?? Guid.Empty,
            exercise.MuscleGroupName,
            exercise.Difficulty,
            exercise.Equipment,
            exercise.IsCustom,
            exercise.IsActive,
            exercise.DisplayOrder,
            exercise.ImageUrl,
            exercise.GifUrl,
            exercise.VideoUrl,
            exercise.ThumbnailUrl);
    }

    private static MuscleGroupEnum ResolveLegacyMuscleGroup(Guid muscleGroupId)
    {
        return MuscleGroupCatalog.Items
            .FirstOrDefault(item => item.Id == muscleGroupId)
            ?.LegacyGroup
            ?? MuscleGroupEnum.Other;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
