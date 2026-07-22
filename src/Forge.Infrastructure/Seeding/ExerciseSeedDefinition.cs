using MuscleGroupEnum = Forge.Domain.Enums.MuscleGroup;

namespace Forge.Infrastructure.Seeding;

public sealed record ExerciseSeedDefinition(
    Guid Id,
    string Name,
    string Description,
    Guid MuscleGroupId,
    MuscleGroupEnum LegacyMuscleGroup,
    string? Equipment,
    int DisplayOrder);
