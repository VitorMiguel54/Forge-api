using Forge.Domain.Constants;

namespace Forge.Infrastructure.Seeding;

public sealed record LevelDefinitionSeedDefinition(Guid Id, string Name, string Description, int MinimumXp, int DisplayOrder, string RarityName);
