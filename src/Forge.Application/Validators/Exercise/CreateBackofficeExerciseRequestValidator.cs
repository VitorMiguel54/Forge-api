using Forge.Application.DTOs.Backoffice.Exercises;

namespace Forge.Application.Validators.Exercise;

public class CreateBackofficeExerciseRequestValidator : IValidator<CreateBackofficeExerciseRequest>
{
    public ValidationResult Validate(CreateBackofficeExerciseRequest request)
    {
        var errors = new List<string>();

        ValidateCommonFields(
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

    internal static void ValidateCommonFields(
        string name,
        string? description,
        Guid muscleGroupId,
        string? difficulty,
        string? equipment,
        int? displayOrder,
        string? imageUrl,
        string? gifUrl,
        string? videoUrl,
        string? thumbnailUrl,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Exercise name is required.");
        }
        else if (name.Trim().Length > 150)
        {
            errors.Add("Exercise name must have at most 150 characters.");
        }

        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 500)
        {
            errors.Add("Exercise description must have at most 500 characters.");
        }

        if (muscleGroupId == Guid.Empty)
        {
            errors.Add("Muscle group is required.");
        }

        if (!string.IsNullOrWhiteSpace(difficulty) && difficulty.Trim().Length > 80)
        {
            errors.Add("Exercise difficulty must have at most 80 characters.");
        }

        if (!string.IsNullOrWhiteSpace(equipment) && equipment.Trim().Length > 120)
        {
            errors.Add("Exercise equipment must have at most 120 characters.");
        }

        if (displayOrder is <= 0)
        {
            errors.Add("Exercise display order must be greater than zero.");
        }

        ValidateUrlLength(imageUrl, "image URL", errors);
        ValidateUrlLength(gifUrl, "GIF URL", errors);
        ValidateUrlLength(videoUrl, "video URL", errors);
        ValidateUrlLength(thumbnailUrl, "thumbnail URL", errors);
    }

    private static void ValidateUrlLength(string? value, string label, List<string> errors)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > 500)
        {
            errors.Add($"Exercise {label} must have at most 500 characters.");
        }
    }
}
