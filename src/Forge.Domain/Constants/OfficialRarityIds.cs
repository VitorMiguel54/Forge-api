namespace Forge.Domain.Constants;

public static class OfficialRarityIds
{
    public static readonly Guid Uncommon = Guid.Parse("11111111-1111-4111-8111-111111111101");
    public static readonly Guid Common = Guid.Parse("11111111-1111-4111-8111-111111111102");
    public static readonly Guid Rare = Guid.Parse("11111111-1111-4111-8111-111111111103");
    public static readonly Guid Epic = Guid.Parse("11111111-1111-4111-8111-111111111104");
    public static readonly Guid Legendary = Guid.Parse("11111111-1111-4111-8111-111111111105");

    public static IReadOnlySet<Guid> All { get; } = new HashSet<Guid>
    {
        Uncommon,
        Common,
        Rare,
        Epic,
        Legendary
    };
}
