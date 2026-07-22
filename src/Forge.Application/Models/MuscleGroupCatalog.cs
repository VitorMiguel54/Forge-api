using Forge.Domain.Constants;
using Forge.Domain.Entities;
using MuscleGroupEnum = Forge.Domain.Enums.MuscleGroup;

namespace Forge.Application.Models;

public record MuscleGroupDefinition(
    Guid Id,
    string Name,
    string DisplayName,
    string Icon,
    int DisplayOrder,
    MuscleGroupEnum LegacyGroup);

public static class MuscleGroupCatalog
{
    public static readonly IReadOnlyCollection<MuscleGroupDefinition> Items = new[]
    {
        new MuscleGroupDefinition(MuscleGroupIds.Chest, "Chest", "Peito", "dumbbell", 10, MuscleGroupEnum.Chest),
        new MuscleGroupDefinition(MuscleGroupIds.Back, "Back", "Costas", "body", 20, MuscleGroupEnum.Back),
        new MuscleGroupDefinition(MuscleGroupIds.Shoulders, "Shoulders", "Ombro", "shield", 30, MuscleGroupEnum.Shoulders),
        new MuscleGroupDefinition(MuscleGroupIds.Biceps, "Biceps", "Bíceps", "dumbbell", 40, MuscleGroupEnum.Arms),
        new MuscleGroupDefinition(MuscleGroupIds.Triceps, "Triceps", "Tríceps", "dumbbell", 50, MuscleGroupEnum.Arms),
        new MuscleGroupDefinition(MuscleGroupIds.Forearms, "Forearms", "Antebraço", "grip", 60, MuscleGroupEnum.Arms),
        new MuscleGroupDefinition(MuscleGroupIds.Core, "Core", "Abdômen", "core", 70, MuscleGroupEnum.Core),
        new MuscleGroupDefinition(MuscleGroupIds.LowerBack, "LowerBack", "Lombar", "body", 80, MuscleGroupEnum.Back),
        new MuscleGroupDefinition(MuscleGroupIds.Glutes, "Glutes", "Glúteo", "body", 90, MuscleGroupEnum.Glutes),
        new MuscleGroupDefinition(MuscleGroupIds.Quadriceps, "Quadriceps", "Quadríceps", "leg", 100, MuscleGroupEnum.Legs),
        new MuscleGroupDefinition(MuscleGroupIds.Hamstrings, "Hamstrings", "Posterior", "leg", 110, MuscleGroupEnum.Legs),
        new MuscleGroupDefinition(MuscleGroupIds.Calves, "Calves", "Panturrilha", "leg", 120, MuscleGroupEnum.Legs),
        new MuscleGroupDefinition(MuscleGroupIds.FullBody, "FullBody", "Corpo inteiro", "body", 130, MuscleGroupEnum.FullBody),
        new MuscleGroupDefinition(MuscleGroupIds.Cardio, "Cardio", "Cardio", "heart", 140, MuscleGroupEnum.Cardio),
        new MuscleGroupDefinition(MuscleGroupIds.Other, "Other", "Outros", "circle", 150, MuscleGroupEnum.Other),
    };

    public static Guid GetDefaultId(MuscleGroupEnum legacyGroup)
    {
        return legacyGroup switch
        {
            MuscleGroupEnum.Chest => MuscleGroupIds.Chest,
            MuscleGroupEnum.Back => MuscleGroupIds.Back,
            MuscleGroupEnum.Shoulders => MuscleGroupIds.Shoulders,
            MuscleGroupEnum.Arms => MuscleGroupIds.Biceps,
            MuscleGroupEnum.Legs => MuscleGroupIds.Quadriceps,
            MuscleGroupEnum.Glutes => MuscleGroupIds.Glutes,
            MuscleGroupEnum.Core => MuscleGroupIds.Core,
            MuscleGroupEnum.FullBody => MuscleGroupIds.FullBody,
            MuscleGroupEnum.Cardio => MuscleGroupIds.Cardio,
            _ => MuscleGroupIds.Other
        };
    }

    public static IReadOnlyCollection<MuscleGroup> CreateEntities(DateTime utcNow)
    {
        return Items
            .Select(item => new MuscleGroup
            {
                Id = item.Id,
                Name = item.Name,
                DisplayName = item.DisplayName,
                Icon = item.Icon,
                DisplayOrder = item.DisplayOrder,
                IsActive = true,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            })
            .ToArray();
    }
}
