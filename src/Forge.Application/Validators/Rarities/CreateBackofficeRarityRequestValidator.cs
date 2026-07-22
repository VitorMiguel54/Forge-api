using Forge.Application.DTOs.Backoffice.Rarities;
using System.Text.RegularExpressions;

namespace Forge.Application.Validators.Rarities;

public partial class CreateBackofficeRarityRequestValidator : IValidator<CreateBackofficeRarityRequest>
{
    public ValidationResult Validate(CreateBackofficeRarityRequest request)
    {
        var errors = new List<string>();

        ValidateCommonFields(
            request.Name,
            request.PrimaryColor,
            request.SecondaryColor,
            request.DisplayOrder,
            errors);

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors.ToArray());
    }

    internal static void ValidateCommonFields(
        string name,
        string primaryColor,
        string? secondaryColor,
        int displayOrder,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Rarity name is required.");
        }
        else if (name.Trim().Length > 80)
        {
            errors.Add("Rarity name must have at most 80 characters.");
        }

        ValidateColor(primaryColor, "primary", required: true, errors);
        ValidateColor(secondaryColor, "secondary", required: false, errors);

        if (displayOrder <= 0)
        {
            errors.Add("Rarity display order must be greater than zero.");
        }
    }

    private static void ValidateColor(string? color, string fieldName, bool required, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            if (required)
            {
                errors.Add($"Rarity {fieldName} color is required.");
            }

            return;
        }

        var trimmedColor = color.Trim();
        if (trimmedColor.Length > 20)
        {
            errors.Add($"Rarity {fieldName} color must have at most 20 characters.");
            return;
        }

        if (!HexColorRegex().IsMatch(trimmedColor))
        {
            errors.Add($"Rarity {fieldName} color must be a valid hexadecimal color.");
        }
    }

    [GeneratedRegex("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    private static partial Regex HexColorRegex();
}
