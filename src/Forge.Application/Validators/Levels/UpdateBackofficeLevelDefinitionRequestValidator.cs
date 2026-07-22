using Forge.Application.DTOs.Backoffice.Levels;

namespace Forge.Application.Validators.Levels;

public class UpdateBackofficeLevelDefinitionRequestValidator : IValidator<UpdateBackofficeLevelDefinitionRequest>
{
    public ValidationResult Validate(UpdateBackofficeLevelDefinitionRequest request)
    {
        var errors = new List<string>();
        CreateBackofficeLevelDefinitionRequestValidator.ValidateCommonFields(request.Name, request.Description, request.MinimumXp, request.DisplayOrder, request.RarityId, errors);
        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
