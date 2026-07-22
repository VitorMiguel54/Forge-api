namespace Forge.Application.Models;

public record BackofficeLevelDefinitionListQuery(
    string? Search,
    Guid? RarityId,
    bool? IsActive,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);
