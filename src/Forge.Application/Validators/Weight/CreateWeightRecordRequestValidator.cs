using Forge.Application.DTOs.Weight;

namespace Forge.Application.Validators.Weight;

public class CreateWeightRecordRequestValidator : IValidator<CreateWeightRecordRequest>
{
    public ValidationResult Validate(CreateWeightRecordRequest request)
    {
        var errors = new List<string>();

        if (request.Weight <= 0)
        {
            errors.Add("Weight must be greater than zero.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
