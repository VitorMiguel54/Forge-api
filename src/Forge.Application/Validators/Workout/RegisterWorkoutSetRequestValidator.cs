using Forge.Application.DTOs.Workout;

namespace Forge.Application.Validators.Workout;

public class RegisterWorkoutSetRequestValidator : IValidator<RegisterWorkoutSetRequest>
{
    public ValidationResult Validate(RegisterWorkoutSetRequest request)
    {
        var errors = new List<string>();

        if (request.WorkoutExerciseId == Guid.Empty)
        {
            errors.Add("Workout exercise id is required.");
        }

        if (request.SetNumber <= 0)
        {
            errors.Add("Set number must be greater than zero.");
        }

        if (request.Repetitions <= 0)
        {
            errors.Add("Repetitions must be greater than zero.");
        }

        if (request.Weight < 0)
        {
            errors.Add("Weight cannot be negative.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}
