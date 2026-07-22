namespace Forge.Application.DTOs.Backoffice.Rarities;

public record CreateBackofficeRarityRequest(
    string Name,
    string PrimaryColor,
    string? SecondaryColor,
    int DisplayOrder,
    bool IsActive = true);
