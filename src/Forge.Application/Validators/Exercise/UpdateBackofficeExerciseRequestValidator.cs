using Forge.Application.DTOs.Backoffice.Exercises;

namespace Forge.Application.Validators.Exercise;

public class UpdateBackofficeExerciseRequestValidator : IValidator<UpdateBackofficeExerciseRequest>
{
    public ValidationResult Validate(UpdateBackofficeExerciseRequest request)
    {
        var errors = new List<string>();

        CreateBackofficeExerciseRequestValidator.ValidateCommonFields(
            request.Name,
            request.Description,
            request.MuscleGroupId,
            request.Difficulty,
            request.Equipment,
            request.DisplayOrder,
            request.ImageUrl,
            request.GifUrl,
            request.VideoUrl,
            request.ThumbnailUrl,
            errors);

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors.ToArray());
    }
}
