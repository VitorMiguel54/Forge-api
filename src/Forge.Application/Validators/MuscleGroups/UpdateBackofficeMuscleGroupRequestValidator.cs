using Forge.Application.DTOs.Backoffice.MuscleGroups;

namespace Forge.Application.Validators.MuscleGroups;

public class UpdateBackofficeMuscleGroupRequestValidator : IValidator<UpdateBackofficeMuscleGroupRequest>
{
    public ValidationResult Validate(UpdateBackofficeMuscleGroupRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("Muscle group name is required.");
        }
        else if (request.Name.Trim().Length > 80)
        {
            errors.Add("Muscle group name must have at most 80 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors.Add("Muscle group display name is required.");
        }
        else if (request.DisplayName.Trim().Length > 120)
        {
            errors.Add("Muscle group display name must have at most 120 characters.");
        }

        if (!string.IsNullOrWhiteSpace(request.Icon) && request.Icon.Trim().Length > 80)
        {
            errors.Add("Muscle group icon must have at most 80 characters.");
        }

        if (request.DisplayOrder <= 0)
        {
            errors.Add("Muscle group display order must be greater than zero.");
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors.ToArray());
    }
}
