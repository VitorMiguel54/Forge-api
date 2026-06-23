using Forge.Application.DTOs.Exercise;

namespace Forge.Application.Validators.Exercise;

public class UpdateExerciseRequestValidator : IValidator<UpdateExerciseRequest>
{
    public ValidationResult Validate(UpdateExerciseRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("Exercise name is required.");
        }

        if (request.IsCustom && request.UserProfileId is null)
        {
            errors.Add("Custom exercises must be linked to a user profile.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
