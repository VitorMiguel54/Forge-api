namespace Forge.Application.DTOs.Backoffice.Rarities;

public record BackofficeRarityListResponse(
    IReadOnlyCollection<BackofficeRarityResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
