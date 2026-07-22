using Forge.Application.DTOs.Backoffice.Levels;

namespace Forge.Application.Validators.Levels;

public class CreateBackofficeLevelDefinitionRequestValidator : IValidator<CreateBackofficeLevelDefinitionRequest>
{
    public ValidationResult Validate(CreateBackofficeLevelDefinitionRequest request)
    {
        var errors = new List<string>();
        ValidateCommonFields(request.Name, request.Description, request.MinimumXp, request.DisplayOrder, request.RarityId, errors);
        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }

    internal static void ValidateCommonFields(string name, string description, int minimumXp, int displayOrder, Guid rarityId, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(name)) errors.Add("Level name is required.");
        else if (name.Trim().Length > 120) errors.Add("Level name must have at most 120 characters.");

        if (string.IsNullOrWhiteSpace(description)) errors.Add("Level description is required.");
        else if (description.Trim().Length > 500) errors.Add("Level description must have at most 500 characters.");

        if (minimumXp < 0) errors.Add("Level minimum XP must be greater than or equal to zero.");
        if (displayOrder <= 0) errors.Add("Level display order must be greater than zero.");
        if (rarityId == Guid.Empty) errors.Add("Level rarity is required.");
    }
}
