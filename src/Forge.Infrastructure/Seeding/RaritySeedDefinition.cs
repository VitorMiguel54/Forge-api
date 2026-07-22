namespace Forge.Infrastructure.Seeding;

public sealed record RaritySeedDefinition(
    Guid Id,
    string Name,
    string PrimaryColor,
    string? SecondaryColor,
    int DisplayOrder);
