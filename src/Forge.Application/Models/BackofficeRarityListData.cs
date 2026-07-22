namespace Forge.Application.Models;

public record BackofficeRarityListData(
    IReadOnlyCollection<BackofficeRarityData> Items,
    int TotalItems);
