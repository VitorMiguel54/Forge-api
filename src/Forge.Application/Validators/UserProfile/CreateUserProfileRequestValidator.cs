using Forge.Application.DTOs.UserProfile;

namespace Forge.Application.Validators.UserProfile;

public class CreateUserProfileRequestValidator : IValidator<CreateUserProfileRequest>
{
    public ValidationResult Validate(CreateUserProfileRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("User profile name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("User profile email is required.");
        }
        else if (!IsValidEmail(request.Email))
        {
            errors.Add("User profile email is invalid.");
        }

        if (request.InitialWeight <= 0)
        {
            errors.Add("Initial weight must be greater than zero.");
        }

        if (request.DailyWaterGoalInLiters <= 0)
        {
            errors.Add("Daily water goal must be greater than zero.");
        }

        if (request.DailySleepGoalInHours <= 0)
        {
            errors.Add("Daily sleep goal must be greater than zero.");
        }

        if (request.WeeklyWorkoutGoal <= 0)
        {
            errors.Add("Weekly workout goal must be greater than zero.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }

    private static bool IsValidEmail(string email)
    {
        return email.Contains('@', StringComparison.Ordinal) && email.Contains('.', StringComparison.Ordinal);
    }
}
