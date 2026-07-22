using Forge.Application.DTOs.Workout;

namespace Forge.Application.Validators.Workout;

public class CreateWorkoutExerciseRequestValidator : IValidator<CreateWorkoutExerciseRequest>
{
    public ValidationResult Validate(CreateWorkoutExerciseRequest request)
    {
        var errors = new List<string>();

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
