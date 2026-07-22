using Forge.Application.Models;
using Forge.Domain.Constants;
using Forge.Infrastructure.Seeding;
using Xunit;

namespace Forge.Api.Tests.Services;

public class ExerciseCatalogTests
{
    [Fact]
    public void All_HasAtLeastTenOfficialExercisesForEachOfficialMuscleGroup()
    {
        var officialMuscleGroupIds = MuscleGroupCatalog.Items
            .Select(group => group.Id)
            .ToHashSet();

        Assert.All(ExerciseCatalog.All, exercise => Assert.Contains(exercise.MuscleGroupId, officialMuscleGroupIds));

        foreach (var muscleGroup in MuscleGroupCatalog.Items)
        {
            Assert.True(
                ExerciseCatalog.All.Count(exercise => exercise.MuscleGroupId == muscleGroup.Id) >= 10,
                $"Expected at least 10 exercises for {muscleGroup.DisplayName}.");
        }
    }

    [Fact]
    public void All_DoesNotContainDuplicateNamesOrIds()
    {
        Assert.Equal(
            ExerciseCatalog.All.Count,
            ExerciseCatalog.All.Select(exercise => exercise.Id).Distinct().Count());
        Assert.Equal(
            ExerciseCatalog.All.Count,
            ExerciseCatalog.All.Select(exercise => exercise.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void All_UsesStableOfficialIds()
    {
        var supinoReto = ExerciseCatalog.All.Single(exercise => exercise.Name == "Supino reto");

        Assert.Equal(OfficialExerciseIds.FromCatalogKey("chest-supino-reto"), supinoReto.Id);
        Assert.All(ExerciseCatalog.All, exercise => Assert.NotEqual(Guid.Empty, exercise.Id));
    }

    [Fact]
    public void All_IsOrderedByDisplayOrder()
    {
        var displayOrders = ExerciseCatalog.All.Select(exercise => exercise.DisplayOrder).ToArray();

        Assert.Equal(displayOrders.OrderBy(displayOrder => displayOrder).ToArray(), displayOrders);
    }

    [Fact]
    public void ExerciseSeeder_CreateMissingOfficialExercises_CreatesInitialCatalog()
    {
        var muscleGroupIds = MuscleGroupCatalog.Items.Select(group => group.Id);

        var missing = ExerciseSeeder.CreateMissingOfficialExercises([], muscleGroupIds, DateTime.UtcNow);

        Assert.Equal(ExerciseCatalog.All.Count, missing.Count);
        Assert.All(missing, exercise =>
        {
            Assert.False(exercise.IsCustom);
            Assert.True(exercise.IsActive);
            Assert.NotNull(exercise.MuscleGroupId);
            Assert.Null(exercise.UserProfileId);
        });
    }

    [Fact]
    public void ExerciseSeeder_CreateMissingOfficialExercises_IsIdempotent()
    {
        var existing = ExerciseCatalog.All
            .Select(exercise => new ExistingExercise(exercise.Id, exercise.Name))
            .ToArray();
        var muscleGroupIds = MuscleGroupCatalog.Items.Select(group => group.Id);

        var missing = ExerciseSeeder.CreateMissingOfficialExercises(existing, muscleGroupIds, DateTime.UtcNow);

        Assert.Empty(missing);
    }

    [Fact]
    public void ExerciseSeeder_CreateMissingOfficialExercises_DoesNotDuplicateAdministrativeName()
    {
        var existing = new[]
        {
            new ExistingExercise(Guid.NewGuid(), "Supino reto")
        };
        var muscleGroupIds = MuscleGroupCatalog.Items.Select(group => group.Id);

        var missing = ExerciseSeeder.CreateMissingOfficialExercises(existing, muscleGroupIds, DateTime.UtcNow);

        Assert.DoesNotContain(missing, exercise => exercise.Name == "Supino reto");
        Assert.Equal(ExerciseCatalog.All.Count - 1, missing.Count);
    }
}
