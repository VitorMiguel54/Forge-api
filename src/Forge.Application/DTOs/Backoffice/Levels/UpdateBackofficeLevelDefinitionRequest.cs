namespace Forge.Application.DTOs.Backoffice.Levels;

public record UpdateBackofficeLevelDefinitionRequest(
    string Name,
    string Description,
    int MinimumXp,
    int DisplayOrder,
    Guid RarityId);
