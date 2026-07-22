using Forge.Application.DTOs.Backoffice.Rarities;

namespace Forge.Application.Validators.Rarities;

public class UpdateBackofficeRarityRequestValidator : IValidator<UpdateBackofficeRarityRequest>
{
    public ValidationResult Validate(UpdateBackofficeRarityRequest request)
    {
        var errors = new List<string>();

        CreateBackofficeRarityRequestValidator.ValidateCommonFields(
            request.Name,
            request.PrimaryColor,
            request.SecondaryColor,
            request.DisplayOrder,
            errors);

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors.ToArray());
    }
}
