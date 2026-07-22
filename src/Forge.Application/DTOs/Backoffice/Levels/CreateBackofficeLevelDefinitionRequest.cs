namespace Forge.Application.DTOs.Backoffice.Levels;

public record CreateBackofficeLevelDefinitionRequest(
    string Name,
    string Description,
    int MinimumXp,
    int DisplayOrder,
    Guid RarityId,
    bool IsActive = true);
