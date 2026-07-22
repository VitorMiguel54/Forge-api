namespace Forge.Application.Models;

public record BackofficeAchievementListData(
    IReadOnlyCollection<BackofficeAchievementData> Items,
    int TotalItems);