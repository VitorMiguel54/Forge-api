namespace Forge.Application.Models;

public record BackofficeRarityListQuery(
    string? Search,
    bool? IsActive,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);
