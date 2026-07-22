namespace Forge.Application.DTOs.Backoffice.Exercises;

public record BackofficeExerciseListResponse(
    IReadOnlyCollection<BackofficeExerciseResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
