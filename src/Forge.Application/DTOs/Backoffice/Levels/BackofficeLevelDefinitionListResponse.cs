namespace Forge.Application.DTOs.Backoffice.Levels;

public record BackofficeLevelDefinitionListResponse(
    IReadOnlyCollection<BackofficeLevelDefinitionResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
