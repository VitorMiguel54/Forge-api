using Forge.Application.DTOs.Exercise;

namespace Forge.Application.Validators.Exercise;

public class CreateExerciseRequestValidator : IValidator<CreateExerciseRequest>
{
    public ValidationResult Validate(CreateExerciseRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("Exercise name is required.");
        }

        if (request.IsCustom && (request.UserProfileId is null || request.UserProfileId == Guid.Empty))
        {
            errors.Add("Custom exercises must be linked to a user profile.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
