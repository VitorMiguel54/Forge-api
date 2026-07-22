using Forge.Domain.Constants;

namespace Forge.Infrastructure.Seeding;

public static class RarityCatalog
{
    public static IReadOnlyCollection<RaritySeedDefinition> All { get; } =
    [
        new(OfficialRarityIds.Uncommon, "Incomum", "#8A8A92", null, 1),
        new(OfficialRarityIds.Common, "Comum", "#22C55E", null, 2),
        new(OfficialRarityIds.Rare, "Raro", "#3B82F6", null, 3),
        new(OfficialRarityIds.Epic, "Épico", "#A855F7", null, 4),
        new(OfficialRarityIds.Legendary, "Lendário", "#F59E0B", "#FDE68A", 5)
    ];
}
