using Forge.Application.DTOs.Backoffice.MuscleGroups;
using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Application.Validators;
using Forge.Domain.Entities;

namespace Forge.Application.Services;

public class BackofficeMuscleGroupService(
    IMuscleGroupRepository muscleGroupRepository,
    IValidator<CreateBackofficeMuscleGroupRequest> createValidator,
    IValidator<UpdateBackofficeMuscleGroupRequest> updateValidator) : IBackofficeMuscleGroupService
{
    public async Task<IReadOnlyCollection<BackofficeMuscleGroupResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var muscleGroups = await muscleGroupRepository.GetBackofficeAsync(cancellationToken);

        return muscleGroups
            .Select(ToResponse)
            .ToArray();
    }

    public async Task<BackofficeMuscleGroupResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var muscleGroup = await muscleGroupRepository.GetBackofficeByIdAsync(id, cancellationToken);

        return muscleGroup is null ? null : ToResponse(muscleGroup);
    }

    public async Task<BackofficeMuscleGroupResponse> CreateAsync(
        CreateBackofficeMuscleGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var name = NormalizeName(request.Name);
        await EnsureNameIsUniqueAsync(name, ignoredId: null, cancellationToken);

        var now = DateTime.UtcNow;
        var muscleGroup = new MuscleGroup
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayName = request.DisplayName.Trim(),
            Icon = NormalizeOptionalText(request.Icon),
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        await muscleGroupRepository.AddAsync(muscleGroup, cancellationToken);

        return new BackofficeMuscleGroupResponse(
            muscleGroup.Id,
            muscleGroup.Name,
            muscleGroup.DisplayName,
            muscleGroup.Icon,
            muscleGroup.DisplayOrder,
            muscleGroup.IsActive,
            ExerciseCount: 0);
    }

    public async Task<BackofficeMuscleGroupResponse?> UpdateAsync(
        Guid id,
        UpdateBackofficeMuscleGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        ValidateUpdateRequest(request);

        var muscleGroup = await muscleGroupRepository.GetByIdIncludingInactiveAsync(id, cancellationToken);
        if (muscleGroup is null)
        {
            return null;
        }

        var name = NormalizeName(request.Name);
        await EnsureNameIsUniqueAsync(name, id, cancellationToken);

        muscleGroup.Name = name;
        muscleGroup.DisplayName = request.DisplayName.Trim();
        muscleGroup.Icon = NormalizeOptionalText(request.Icon);
        muscleGroup.DisplayOrder = request.DisplayOrder;
        muscleGroup.UpdatedAt = DateTime.UtcNow;

        await muscleGroupRepository.UpdateAsync(muscleGroup, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<BackofficeMuscleGroupResponse?> UpdateStatusAsync(
        Guid id,
        UpdateBackofficeMuscleGroupStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var muscleGroup = await muscleGroupRepository.GetByIdIncludingInactiveAsync(id, cancellationToken);
        if (muscleGroup is null)
        {
            return null;
        }

        muscleGroup.IsActive = request.IsActive;
        muscleGroup.UpdatedAt = DateTime.UtcNow;

        await muscleGroupRepository.UpdateAsync(muscleGroup, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var muscleGroup = await muscleGroupRepository.GetByIdIncludingInactiveAsync(id, cancellationToken);
        if (muscleGroup is null)
        {
            return false;
        }

        if (MuscleGroupCatalog.Items.Any(item => item.Id == id))
        {
            throw new InvalidOperationException("Official muscle groups cannot be deleted; deactivate them instead.");
        }

        if (await muscleGroupRepository.HasExercisesAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Muscle group cannot be deleted because it is linked to exercises.");
        }

        await muscleGroupRepository.DeleteAsync(muscleGroup, cancellationToken);

        return true;
    }

    private async Task EnsureNameIsUniqueAsync(
        string name,
        Guid? ignoredId,
        CancellationToken cancellationToken)
    {
        if (await muscleGroupRepository.NameExistsAsync(name, ignoredId, cancellationToken))
        {
            throw new InvalidOperationException("Muscle group name already exists.");
        }
    }

    private void ValidateCreateRequest(CreateBackofficeMuscleGroupRequest request)
    {
        var validationResult = createValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private void ValidateUpdateRequest(UpdateBackofficeMuscleGroupRequest request)
    {
        var validationResult = updateValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private static BackofficeMuscleGroupResponse ToResponse(BackofficeMuscleGroupData muscleGroup)
    {
        return new BackofficeMuscleGroupResponse(
            muscleGroup.Id,
            muscleGroup.Name,
            muscleGroup.DisplayName,
            muscleGroup.Icon,
            muscleGroup.DisplayOrder,
            muscleGroup.IsActive,
            muscleGroup.ExerciseCount);
    }

    private static string NormalizeName(string name)
    {
        return name.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
