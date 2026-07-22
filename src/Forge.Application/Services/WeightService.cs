using Forge.Application.DTOs.Weight;
using Forge.Application.Interfaces;
using Forge.Application.Mappings;
using Forge.Application.Validators;
using Forge.Domain.Entities;

namespace Forge.Application.Services;

public class WeightService(
    IWeightRecordRepository weightRecordRepository,
    IUserProfileRepository userProfileRepository,
    IValidator<CreateWeightRecordRequest> createWeightRecordValidator) : IWeightService
{
    public async Task<WeightRecordResponse> CreateAsync(
        Guid userProfileId,
        CreateWeightRecordRequest request,
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
        var weightRecord = new WeightRecord
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            Weight = request.Weight,
            RecordDate = request.RecordDate,
            CreatedAt = now
        };

        userProfile.CurrentWeight = request.Weight;
        userProfile.UpdatedAt = now;

        await weightRecordRepository.AddAsync(weightRecord, cancellationToken);
        await userProfileRepository.UpdateAsync(userProfile, cancellationToken);

        return weightRecord.ToResponse();
    }

    public async Task<IReadOnlyCollection<WeightRecordResponse>?> GetByUserProfileAsync(
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

        var weightRecords = await weightRecordRepository.GetByUserProfileAsync(
            userProfileId,
            cancellationToken);

        return weightRecords
            .Select(weightRecord => weightRecord.ToResponse())
            .ToArray();
    }

    public async Task<WeightRecordResponse?> GetLatestByUserProfileAsync(
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

        var weightRecord = await weightRecordRepository.GetLatestByUserProfileAsync(
            userProfileId,
            cancellationToken);

        return weightRecord?.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var weightRecord = await weightRecordRepository.GetByIdAsync(id, cancellationToken);
        if (weightRecord is null)
        {
            return false;
        }

        await weightRecordRepository.DeleteAsync(weightRecord, cancellationToken);

        return true;
    }

    private void ValidateCreateRequest(CreateWeightRecordRequest request)
    {
        var validationResult = createWeightRecordValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(' ', validationResult.Errors));
        }
    }
}
