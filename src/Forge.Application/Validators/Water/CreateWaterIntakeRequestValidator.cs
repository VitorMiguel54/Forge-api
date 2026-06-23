using Forge.Application.DTOs.Water;

namespace Forge.Application.Validators.Water;

public class CreateWaterIntakeRequestValidator : IValidator<CreateWaterIntakeRequest>
{
    public ValidationResult Validate(CreateWaterIntakeRequest request)
    {
        var errors = new List<string>();

        if (request.UserProfileId == Guid.Empty)
        {
            errors.Add("User profile id is required.");
        }

        if (request.Liters < 0)
        {
            errors.Add("Water intake cannot be negative.");
        }

        if (request.GoalInLiters <= 0)
        {
            errors.Add("Water goal must be greater than zero.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
