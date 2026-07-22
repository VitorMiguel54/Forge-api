using Forge.Application.DTOs.Workout;

namespace Forge.Application.Validators.Workout;

public class AddWorkoutExerciseRequestValidator : IValidator<AddWorkoutExerciseRequest>
{
    public ValidationResult Validate(AddWorkoutExerciseRequest request)
    {
        var errors = new List<string>();

        if (request.WorkoutId == Guid.Empty)
        {
            errors.Add("Workout id is required.");
        }

        if (request.ExerciseId == Guid.Empty)
        {
            errors.Add("Exercise id is required.");
        }

        if (request.Order <= 0)
        {
            errors.Add("Exercise order must be greater than zero.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
