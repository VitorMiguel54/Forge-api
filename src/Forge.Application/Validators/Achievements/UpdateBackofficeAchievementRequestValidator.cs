using Forge.Application.DTOs.Backoffice.Achievements;

namespace Forge.Application.Validators.Achievements;

public class UpdateBackofficeAchievementRequestValidator : IValidator<UpdateBackofficeAchievementRequest>
{
    public ValidationResult Validate(UpdateBackofficeAchievementRequest request)
    {
        var errors = new List<string>();

        CreateBackofficeAchievementRequestValidator.ValidateCommonFields(
            request.Name,
            request.Description,
            request.Category,
            request.RequiredValue,
            request.XpReward,
            errors);

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors.ToArray());
    }
}