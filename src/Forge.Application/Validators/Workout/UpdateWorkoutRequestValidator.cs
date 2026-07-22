using Forge.Application.DTOs.Workout;

namespace Forge.Application.Validators.Workout;

public class UpdateWorkoutRequestValidator : IValidator<UpdateWorkoutRequest>
{
    public ValidationResult Validate(UpdateWorkoutRequest request)
    {
        var errors = new List<string>();

        if (request.UserProfileId == Guid.Empty)
        {
            errors.Add("User profile id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("Workout name is required.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
