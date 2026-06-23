namespace Forge.Application.Validators;

public sealed class ValidationResult
{
    private ValidationResult(IReadOnlyCollection<string> errors)
    {
        Errors = errors;
    }

    public IReadOnlyCollection<string> Errors { get; }
    public bool IsValid => Errors.Count == 0;

    public static ValidationResult Success()
    {
        return new ValidationResult(Array.Empty<string>());
    }

    public static ValidationResult Failure(params string[] errors)
    {
        return new ValidationResult(errors);
    }
}
