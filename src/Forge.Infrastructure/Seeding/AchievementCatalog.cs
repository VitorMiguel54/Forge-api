using Forge.Domain.Constants;
using Forge.Domain.Enums;

namespace Forge.Infrastructure.Seeding;

public static class AchievementCatalog
{
    // Add new official achievements here with stable IDs. Never change an existing ID after release.
    public static IReadOnlyCollection<AchievementSeedDefinition> All { get; } =
    [
        new(
            OfficialAchievementIds.FirstWorkout,
            "Primeiro treino",
            "Conclua seu primeiro treino.",
            AchievementCategory.Workout,
            1,
            false,
            50),
        new(
            OfficialAchievementIds.FiveWorkouts,
            "Cinco treinos",
            "Conclua cinco treinos.",
            AchievementCategory.Workout,
            5,
            false,
            100),
        new(
            OfficialAchievementIds.TenWorkouts,
            "Dez treinos",
            "Conclua dez treinos.",
            AchievementCategory.Workout,
            10,
            false,
            150),
        new(
            OfficialAchievementIds.ForgeMarathoner,
            "Maratonista Forge",
            "Conclua vinte e cinco treinos.",
            AchievementCategory.Workout,
            25,
            false,
            300),
        new(
            OfficialAchievementIds.ConsistentWeek,
            "Semana consistente",
            "Conclua tres treinos na mesma semana.",
            AchievementCategory.Consistency,
            3,
            false,
            100),
        new(
            OfficialAchievementIds.PerfectWeek,
            "Semana perfeita",
            "Conclua cinco treinos na mesma semana.",
            AchievementCategory.Consistency,
            5,
            false,
            200),
        new(
            OfficialAchievementIds.FirstTon,
            "Primeira tonelada",
            "Movimente mil quilos em treinos concluidos.",
            AchievementCategory.Progression,
            1_000,
            false,
            100),
        new(
            OfficialAchievementIds.AccumulatedStrength,
            "Forca acumulada",
            "Movimente dez mil quilos em treinos concluidos.",
            AchievementCategory.Progression,
            10_000,
            false,
            250),
        new(
            OfficialAchievementIds.HydrationOnTrack,
            "Hidratacao em dia",
            "Atinja a meta diaria de agua por sete dias.",
            AchievementCategory.Hydration,
            7,
            false,
            100),
        new(
            OfficialAchievementIds.RestorativeSleep,
            "Sono reparador",
            "Atinja a meta diaria de sono por sete dias.",
            AchievementCategory.Sleep,
            7,
            false,
            100),
        new(
            OfficialAchievementIds.GuardianAwakened,
            "Guardiao desperto",
            "Desbloqueie um marco secreto do Forge.",
            AchievementCategory.Secret,
            1,
            true,
            250)
    ];
}
