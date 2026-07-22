using Forge.Application.Interfaces;
using Forge.Application.Models;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class MuscleGroupRepository(ForgeDbContext dbContext) : IMuscleGroupRepository
{
    public async Task<IReadOnlyCollection<MuscleGroup>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.MuscleGroups
            .AsNoTracking()
            .Where(muscleGroup => muscleGroup.IsActive)
            .OrderBy(muscleGroup => muscleGroup.DisplayOrder)
            .ThenBy(muscleGroup => muscleGroup.DisplayName)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BackofficeMuscleGroupData>> GetBackofficeAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.MuscleGroups
            .AsNoTracking()
            .OrderBy(muscleGroup => muscleGroup.DisplayOrder)
            .ThenBy(muscleGroup => muscleGroup.DisplayName)
            .Select(muscleGroup => new BackofficeMuscleGroupData(
                muscleGroup.Id,
                muscleGroup.Name,
                muscleGroup.DisplayName,
                muscleGroup.Icon,
                muscleGroup.DisplayOrder,
                muscleGroup.IsActive,
                muscleGroup.Exercises.Count))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<BackofficeMuscleGroupData?> GetBackofficeByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.MuscleGroups
            .AsNoTracking()
            .Where(muscleGroup => muscleGroup.Id == id)
            .Select(muscleGroup => new BackofficeMuscleGroupData(
                muscleGroup.Id,
                muscleGroup.Name,
                muscleGroup.DisplayName,
                muscleGroup.Icon,
                muscleGroup.DisplayOrder,
                muscleGroup.IsActive,
                muscleGroup.Exercises.Count))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MuscleGroup?> GetByIdIncludingInactiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.MuscleGroups
            .FirstOrDefaultAsync(muscleGroup => muscleGroup.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.MuscleGroups
            .AsNoTracking()
            .AnyAsync(muscleGroup => muscleGroup.Id == id && muscleGroup.IsActive, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        Guid? ignoredId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToLower();

        return await dbContext.MuscleGroups
            .AsNoTracking()
            .AnyAsync(
                muscleGroup =>
                    muscleGroup.Name.ToLower() == normalizedName
                    && (ignoredId == null || muscleGroup.Id != ignoredId),
                cancellationToken);
    }

    public async Task<bool> HasExercisesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Exercises
            .AsNoTracking()
            .AnyAsync(exercise => exercise.MuscleGroupId == id, cancellationToken);
    }

    public async Task AddAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
    {
        await dbContext.MuscleGroups.AddAsync(muscleGroup, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
    {
        dbContext.MuscleGroups.Update(muscleGroup);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
    {
        dbContext.MuscleGroups.Remove(muscleGroup);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
