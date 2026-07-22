using Forge.Application.DTOs.Backoffice.Achievements;
using Forge.Domain.Enums;

namespace Forge.Application.Validators.Achievements;

public class CreateBackofficeAchievementRequestValidator : IValidator<CreateBackofficeAchievementRequest>
{
    public ValidationResult Validate(CreateBackofficeAchievementRequest request)
    {
        var errors = new List<string>();

        ValidateCommonFields(
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

    internal static void ValidateCommonFields(
        string name,
        string description,
        AchievementCategory category,
        int requiredValue,
        int xpReward,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Achievement name is required.");
        }
        else if (name.Trim().Length > 150)
        {
            errors.Add("Achievement name must have at most 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            errors.Add("Achievement description is required.");
        }
        else if (description.Trim().Length > 500)
        {
            errors.Add("Achievement description must have at most 500 characters.");
        }

        if (!Enum.IsDefined(typeof(AchievementCategory), category))
        {
            errors.Add("Achievement category is invalid.");
        }

        if (requiredValue <= 0)
        {
            errors.Add("Achievement required value must be greater than zero.");
        }

        if (xpReward < 0)
        {
            errors.Add("Achievement XP reward must be greater than or equal to zero.");
        }
    }
}