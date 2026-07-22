namespace Forge.Application.Models;

public record BackofficeExerciseListQuery(
    string? Search,
    Guid? MuscleGroupId,
    bool? IsActive,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);
