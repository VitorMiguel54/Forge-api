using Forge.Application.DTOs.UserProfile;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Application.Validators;
using Forge.Application.Validators.UserProfile;
using Forge.Domain.Entities;

namespace Forge.Application.Services;

public class UserProfileService(
    IUserProfileRepository userProfileRepository,
    IValidator<CreateUserProfileRequest> createUserProfileValidator,
    IValidator<UpdateUserProfileRequest> updateUserProfileValidator) : IUserProfileService
{
    public async Task<UserProfileResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var userProfile = await userProfileRepository.GetByIdAsync(id, cancellationToken);

        return userProfile?.ToResponse();
    }

    public async Task<IReadOnlyCollection<UserProfileResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userProfiles = await userProfileRepository.GetAllAsync(cancellationToken);

        return userProfiles
            .Select(userProfile => userProfile.ToResponse())
            .ToArray();
    }

    public async Task<UserProfileResponse> CreateAsync(
        CreateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var email = NormalizeEmail(request.Email);
        await ValidateEmailIsUniqueAsync(email, null, cancellationToken);

        var now = DateTime.UtcNow;
        var userProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            InitialWeight = request.InitialWeight,
            CurrentWeight = request.InitialWeight,
            Level = 1,
            TotalXp = 0,
            DailyWaterGoalInLiters = request.DailyWaterGoalInLiters,
            DailySleepGoalInHours = request.DailySleepGoalInHours,
            WeeklyWorkoutGoal = request.WeeklyWorkoutGoal,
            CreatedAt = now,
            UpdatedAt = now
        };

        await userProfileRepository.AddAsync(userProfile, cancellationToken);

        return userProfile.ToResponse();
    }

    public async Task<UserProfileResponse?> UpdateAsync(
        Guid id,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        ValidateUpdateRequest(request);

        var userProfile = await userProfileRepository.GetByIdAsync(id, cancellationToken);
        if (userProfile is null)
        {
            return null;
        }

        var email = NormalizeEmail(request.Email);
        await ValidateEmailIsUniqueAsync(email, id, cancellationToken);

        userProfile.Name = request.Name.Trim();
        userProfile.Email = email;
        userProfile.InitialWeight = request.InitialWeight;
        userProfile.CurrentWeight = request.CurrentWeight;
        userProfile.DailyWaterGoalInLiters = request.DailyWaterGoalInLiters;
        userProfile.DailySleepGoalInHours = request.DailySleepGoalInHours;
        userProfile.WeeklyWorkoutGoal = request.WeeklyWorkoutGoal;
        userProfile.UpdatedAt = DateTime.UtcNow;

        await userProfileRepository.UpdateAsync(userProfile, cancellationToken);

        return userProfile.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var userProfile = await userProfileRepository.GetByIdAsync(id, cancellationToken);
        if (userProfile is null)
        {
            return false;
        }

        if (await userProfileRepository.HasCustomExercisesAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("User profile cannot be deleted because it has custom exercises.");
        }

        await userProfileRepository.DeleteAsync(userProfile, cancellationToken);

        return true;
    }

    private void ValidateCreateRequest(CreateUserProfileRequest request)
    {
        var validationResult = createUserProfileValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private void ValidateUpdateRequest(UpdateUserProfileRequest request)
    {
        var validationResult = updateUserProfileValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }

    private async Task ValidateEmailIsUniqueAsync(
        string email,
        Guid? ignoredUserProfileId,
        CancellationToken cancellationToken)
    {
        if (await userProfileRepository.EmailExistsAsync(email, ignoredUserProfileId, cancellationToken))
        {
            throw new ArgumentException("User profile email already exists.");
        }
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
