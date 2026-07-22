using Forge.Application.DTOs.Water;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Application.Validators;
using Forge.Domain.Entities;

namespace Forge.Application.Services;

public class WaterService(
    IWaterIntakeRepository waterIntakeRepository,
    IUserProfileRepository userProfileRepository,
    IValidator<CreateWaterIntakeRequest> createWaterIntakeValidator) : IWaterService
{
    public async Task<WaterIntakeResponse> CreateAsync(
        Guid userProfileId,
        CreateWaterIntakeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            throw new ArgumentException("User profile id is required.");
        }

        ValidateCreateRequest(request);

        var userProfile = await userProfileRepository.GetByIdAsync(userProfileId, cancellationToken);
        if (userProfile is null)
        {
            throw new ArgumentException("User profile does not exist.");
        }

        var now = DateTime.UtcNow;
        var waterIntake = new WaterIntake
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            IntakeDate = request.IntakeDate,
            Liters = request.Liters,
            GoalInLiters = userProfile.DailyWaterGoalInLiters,
            GoalAchieved = request.Liters >= userProfile.DailyWaterGoalInLiters,
            CreatedAt = now,
            UpdatedAt = now
        };

        await waterIntakeRepository.AddAsync(waterIntake, cancellationToken);

        return waterIntake.ToResponse();
    }

    public async Task<IReadOnlyCollection<WaterIntakeResponse>?> GetByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return null;
        }

        if (!await userProfileRepository.ExistsAsync(userProfileId, cancellationToken))
        {
            return null;
        }

        var waterIntakes = await waterIntakeRepository.GetByUserProfileAsync(
            userProfileId,
            cancellationToken);

        return waterIntakes
            .Select(waterIntake => waterIntake.ToResponse())
            .ToArray();
    }

    public async Task<WaterIntakeResponse?> GetTodayByUserProfileAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return null;
        }

        if (!await userProfileRepository.ExistsAsync(userProfileId, cancellationToken))
        {
            return null;
        }

        var waterIntake = await waterIntakeRepository.GetTodayByUserProfileAsync(
            userProfileId,
            DateTime.UtcNow.Date,
            cancellationToken);

        return waterIntake?.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var waterIntake = await waterIntakeRepository.GetByIdAsync(id, cancellationToken);
        if (waterIntake is null)
        {
            return false;
        }

        await waterIntakeRepository.DeleteAsync(waterIntake, cancellationToken);

        return true;
    }

    private void ValidateCreateRequest(CreateWaterIntakeRequest request)
    {
        var validationResult = createWaterIntakeValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }
}
