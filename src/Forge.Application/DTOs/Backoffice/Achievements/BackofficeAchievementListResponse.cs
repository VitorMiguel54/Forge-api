namespace Forge.Application.DTOs.Backoffice.Achievements;

public record BackofficeAchievementListResponse(
    IReadOnlyCollection<BackofficeAchievementResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);