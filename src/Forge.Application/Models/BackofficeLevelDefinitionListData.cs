namespace Forge.Application.Models;

public record BackofficeLevelDefinitionListData(
    IReadOnlyCollection<BackofficeLevelDefinitionData> Items,
    int TotalItems);
