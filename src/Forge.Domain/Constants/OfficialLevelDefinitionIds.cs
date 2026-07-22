namespace Forge.Domain.Constants;

public static class OfficialLevelDefinitionIds
{
    public static readonly Guid Forgotten = Guid.Parse("22222222-2222-4222-8222-222222222221");
    public static readonly Guid Forged = Guid.Parse("22222222-2222-4222-8222-222222222222");
    public static readonly Guid Guard = Guid.Parse("22222222-2222-4222-8222-222222222223");
    public static readonly Guid Sentinel = Guid.Parse("22222222-2222-4222-8222-222222222224");
    public static readonly Guid ForgeGuardian = Guid.Parse("22222222-2222-4222-8222-222222222225");

    public static IReadOnlySet<Guid> All { get; } = new HashSet<Guid>
    {
        Forgotten,
        Forged,
        Guard,
        Sentinel,
        ForgeGuardian
    };

    public static bool Contains(Guid id)
    {
        return All.Contains(id);
    }
}
