using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class ExerciseRepository(ForgeDbContext dbContext) : IExerciseRepository
{
    public async Task<IReadOnlyCollection<Exercise>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Exercises
            .AsNoTracking()
            .Include(exercise => exercise.MuscleGroupEntity)
            .Where(exercise => exercise.IsActive)
            .OrderBy(exercise => exercise.MuscleGroupEntity == null ? int.MaxValue : exercise.MuscleGroupEntity.DisplayOrder)
            .ThenBy(exercise => exercise.DisplayOrder ?? int.MaxValue)
            .ThenBy(exercise => exercise.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Exercises
            .Include(exercise => exercise.MuscleGroupEntity)
            .FirstOrDefaultAsync(exercise => exercise.Id == id, cancellationToken);
    }

    public async Task<BackofficeExerciseListData> GetBackofficeAsync(
        BackofficeExerciseListQuery query,
        CancellationToken cancellationToken = default)
    {
        var exercises = dbContext.Exercises
            .AsNoTracking()
            .Include(exercise => exercise.MuscleGroupEntity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            exercises = exercises.Where(exercise => exercise.Name.Contains(search));
        }

        if (query.MuscleGroupId is not null && query.MuscleGroupId != Guid.Empty)
        {
            exercises = exercises.Where(exercise => exercise.MuscleGroupId == query.MuscleGroupId);
        }

        if (query.IsActive is not null)
        {
            exercises = exercises.Where(exercise => exercise.IsActive == query.IsActive);
        }

        exercises = ApplyOrdering(exercises, query.SortBy, query.SortDirection);

        var totalItems = await exercises.CountAsync(cancellationToken);
        var skip = (query.Page - 1) * query.PageSize;
        var items = await exercises
            .Skip(skip)
            .Take(query.PageSize)
            .Select(exercise => new BackofficeExerciseData(
                exercise.Id,
                exercise.Name,
                exercise.Description,
                exercise.MuscleGroupId,
                exercise.MuscleGroupEntity == null ? null : exercise.MuscleGroupEntity.DisplayName,
                exercise.Difficulty,
                exercise.Equipment,
                exercise.IsCustom,
                exercise.IsActive,
                exercise.DisplayOrder,
                exercise.ImageUrl,
                exercise.GifUrl,
                exercise.VideoUrl,
                exercise.ThumbnailUrl))
            .ToArrayAsync(cancellationToken);

        return new BackofficeExerciseListData(items, totalItems);
    }

    public async Task<BackofficeExerciseData?> GetBackofficeByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Exercises
            .AsNoTracking()
            .Where(exercise => exercise.Id == id)
            .Select(exercise => new BackofficeExerciseData(
                exercise.Id,
                exercise.Name,
                exercise.Description,
                exercise.MuscleGroupId,
                exercise.MuscleGroupEntity == null ? null : exercise.MuscleGroupEntity.DisplayName,
                exercise.Difficulty,
                exercise.Equipment,
                exercise.IsCustom,
                exercise.IsActive,
                exercise.DisplayOrder,
                exercise.ImageUrl,
                exercise.GifUrl,
                exercise.VideoUrl,
                exercise.ThumbnailUrl))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        Guid? ignoredId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToLower();

        return await dbContext.Exercises
            .AsNoTracking()
            .AnyAsync(
                exercise =>
                    exercise.Name.ToLower() == normalizedName
                    && (ignoredId == null || exercise.Id != ignoredId),
                cancellationToken);
    }

    public async Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkoutExercises
            .AnyAsync(workoutExercise => workoutExercise.ExerciseId == id, cancellationToken);
    }

    public async Task AddAsync(Exercise exercise, CancellationToken cancellationToken = default)
    {
        await dbContext.Exercises.AddAsync(exercise, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken = default)
    {
        dbContext.Exercises.Update(exercise);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Exercise exercise, CancellationToken cancellationToken = default)
    {
        dbContext.Exercises.Remove(exercise);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Exercise> ApplyOrdering(
        IQueryable<Exercise> exercises,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "displayorder" => descending
                ? exercises.OrderByDescending(exercise => exercise.DisplayOrder ?? int.MaxValue).ThenBy(exercise => exercise.Name)
                : exercises.OrderBy(exercise => exercise.DisplayOrder ?? int.MaxValue).ThenBy(exercise => exercise.Name),
            "musclegroup" => descending
                ? exercises.OrderByDescending(exercise => exercise.MuscleGroupEntity == null ? null : exercise.MuscleGroupEntity.DisplayName).ThenBy(exercise => exercise.Name)
                : exercises.OrderBy(exercise => exercise.MuscleGroupEntity == null ? null : exercise.MuscleGroupEntity.DisplayName).ThenBy(exercise => exercise.Name),
            "status" => descending
                ? exercises.OrderByDescending(exercise => exercise.IsActive).ThenBy(exercise => exercise.Name)
                : exercises.OrderBy(exercise => exercise.IsActive).ThenBy(exercise => exercise.Name),
            "createdat" => descending
                ? exercises.OrderByDescending(exercise => exercise.CreatedAt).ThenBy(exercise => exercise.Name)
                : exercises.OrderBy(exercise => exercise.CreatedAt).ThenBy(exercise => exercise.Name),
            _ => descending
                ? exercises.OrderByDescending(exercise => exercise.Name)
                : exercises.OrderBy(exercise => exercise.Name)
        };
    }
}
