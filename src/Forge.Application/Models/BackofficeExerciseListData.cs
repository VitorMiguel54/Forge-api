namespace Forge.Application.Models;

public record BackofficeExerciseListData(
    IReadOnlyCollection<BackofficeExerciseData> Items,
    int TotalItems);
