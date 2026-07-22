using Forge.Application.DTOs.Sleep;

namespace Forge.Application.Validators.Sleep;

public class CreateSleepRecordRequestValidator : IValidator<CreateSleepRecordRequest>
{
    public ValidationResult Validate(CreateSleepRecordRequest request)
    {
        var errors = new List<string>();

        if (request.HoursSlept <= 0)
        {
            errors.Add("Hours slept must be greater than zero.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
