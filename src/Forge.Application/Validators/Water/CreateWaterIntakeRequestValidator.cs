using Forge.Application.DTOs.Water;

namespace Forge.Application.Validators.Water;

public class CreateWaterIntakeRequestValidator : IValidator<CreateWaterIntakeRequest>
{
    public ValidationResult Validate(CreateWaterIntakeRequest request)
    {
        var errors = new List<string>();

        if (request.Liters <= 0)
        {
            errors.Add("Water intake must be greater than zero.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
