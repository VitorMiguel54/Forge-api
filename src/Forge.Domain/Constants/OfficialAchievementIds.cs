namespace Forge.Domain.Constants;

public static class OfficialAchievementIds
{
    public static readonly Guid FirstWorkout = Guid.Parse("11111111-1111-4111-8111-111111111111");
    public static readonly Guid FiveWorkouts = Guid.Parse("11111111-1111-4111-8111-111111111112");
    public static readonly Guid TenWorkouts = Guid.Parse("11111111-1111-4111-8111-111111111113");
    public static readonly Guid ForgeMarathoner = Guid.Parse("11111111-1111-4111-8111-111111111114");
    public static readonly Guid ConsistentWeek = Guid.Parse("11111111-1111-4111-8111-111111111115");
    public static readonly Guid PerfectWeek = Guid.Parse("11111111-1111-4111-8111-111111111116");
    public static readonly Guid FirstTon = Guid.Parse("11111111-1111-4111-8111-111111111117");
    public static readonly Guid AccumulatedStrength = Guid.Parse("11111111-1111-4111-8111-111111111118");
    public static readonly Guid HydrationOnTrack = Guid.Parse("11111111-1111-4111-8111-111111111119");
    public static readonly Guid RestorativeSleep = Guid.Parse("11111111-1111-4111-8111-111111111120");
    public static readonly Guid GuardianAwakened = Guid.Parse("11111111-1111-4111-8111-111111111121");

    public static IReadOnlySet<Guid> All { get; } = new HashSet<Guid>
    {
        FirstWorkout,
        FiveWorkouts,
        TenWorkouts,
        ForgeMarathoner,
        ConsistentWeek,
        PerfectWeek,
        FirstTon,
        AccumulatedStrength,
        HydrationOnTrack,
        RestorativeSleep,
        GuardianAwakened
    };

    public static bool Contains(Guid id)
    {
        return All.Contains(id);
    }
}