using Forge.Application.DTOs.Sleep;

namespace Forge.Application.Validators.Sleep;

public class CreateSleepRecordRequestValidator : IValidator<CreateSleepRecordRequest>
{
    public ValidationResult Validate(CreateSleepRecordRequest request)
    {
        var errors = new List<string>();

        if (request.UserProfileId == Guid.Empty)
        {
            errors.Add("User profile id is required.");
        }

        if (request.HoursSlept < 0)
        {
            errors.Add("Hours slept cannot be negative.");
        }

        if (request.GoalInHours <= 0)
        {
            errors.Add("Sleep goal must be greater than zero.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
