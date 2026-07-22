using Forge.Application.DTOs.Sleep;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Application.Validators;
using Forge.Domain.Entities;

namespace Forge.Application.Services;

public class SleepService(
    ISleepRecordRepository sleepRecordRepository,
    IUserProfileRepository userProfileRepository,
    IValidator<CreateSleepRecordRequest> createSleepRecordValidator) : ISleepService
{
    public async Task<SleepRecordResponse> CreateAsync(
        Guid userProfileId,
        CreateSleepRecordRequest request,
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
        var sleepRecord = new SleepRecord
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            SleepDate = request.SleepDate,
            HoursSlept = request.HoursSlept,
            GoalInHours = userProfile.DailySleepGoalInHours,
            GoalAchieved = request.HoursSlept >= userProfile.DailySleepGoalInHours,
            CreatedAt = now,
            UpdatedAt = now
        };

        await sleepRecordRepository.AddAsync(sleepRecord, cancellationToken);

        return sleepRecord.ToResponse();
    }

    public async Task<IReadOnlyCollection<SleepRecordResponse>?> GetByUserProfileAsync(
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

        var sleepRecords = await sleepRecordRepository.GetByUserProfileAsync(
            userProfileId,
            cancellationToken);

        return sleepRecords
            .Select(sleepRecord => sleepRecord.ToResponse())
            .ToArray();
    }

    public async Task<SleepRecordResponse?> GetLatestByUserProfileAsync(
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

        var sleepRecord = await sleepRecordRepository.GetLatestByUserProfileAsync(
            userProfileId,
            cancellationToken);

        return sleepRecord?.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var sleepRecord = await sleepRecordRepository.GetByIdAsync(id, cancellationToken);
        if (sleepRecord is null)
        {
            return false;
        }

        await sleepRecordRepository.DeleteAsync(sleepRecord, cancellationToken);

        return true;
    }

    private void ValidateCreateRequest(CreateSleepRecordRequest request)
    {
        var validationResult = createSleepRecordValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }
}
