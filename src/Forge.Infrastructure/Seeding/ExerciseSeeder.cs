using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Seeding;

public class ExerciseSeeder(ForgeDbContext dbContext)
{
    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingExercises = await dbContext.Exercises
            .Select(exercise => new ExistingExercise(exercise.Id, exercise.Name))
            .ToArrayAsync(cancellationToken);

        var existingMuscleGroupIds = await dbContext.MuscleGroups
            .Select(muscleGroup => muscleGroup.Id)
            .ToArrayAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var missingExercises = CreateMissingOfficialExercises(
                existingExercises,
                existingMuscleGroupIds,
                now)
            .ToArray();

        if (missingExercises.Length == 0)
        {
            return 0;
        }

        await dbContext.Exercises.AddRangeAsync(missingExercises, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return missingExercises.Length;
    }

    public static IReadOnlyCollection<Exercise> CreateMissingOfficialExercises(
        IEnumerable<ExistingExercise> existingExercises,
        IEnumerable<Guid> existingMuscleGroupIds,
        DateTime utcNow)
    {
        var existing = existingExercises.ToArray();
        var existingIds = existing.Select(exercise => exercise.Id).ToHashSet();
        var existingNames = existing.Select(exercise => exercise.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var muscleGroupIds = existingMuscleGroupIds.ToHashSet();

        return ExerciseCatalog.All
            .Where(exercise =>
                !existingIds.Contains(exercise.Id)
                && !existingNames.Contains(exercise.Name)
                && muscleGroupIds.Contains(exercise.MuscleGroupId))
            .Select(exercise => ToEntity(exercise, utcNow))
            .ToArray();
    }

    private static Exercise ToEntity(ExerciseSeedDefinition exercise, DateTime utcNow)
    {
        return new Exercise
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            MuscleGroup = exercise.LegacyMuscleGroup,
            MuscleGroupId = exercise.MuscleGroupId,
            Difficulty = null,
            Equipment = exercise.Equipment,
            IsCustom = false,
            IsActive = true,
            DisplayOrder = exercise.DisplayOrder,
            ImageUrl = null,
            GifUrl = null,
            VideoUrl = null,
            ThumbnailUrl = null,
            UserProfileId = null,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
}

public sealed record ExistingExercise(Guid Id, string Name);
