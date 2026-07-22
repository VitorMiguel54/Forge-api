using Forge.Application.Interfaces;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class WorkoutSetRepository(ForgeDbContext dbContext) : IWorkoutSetRepository
{
    public async Task<IReadOnlyCollection<WorkoutSet>> GetByWorkoutExerciseAsync(
        Guid workoutExerciseId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkoutSets
            .AsNoTracking()
            .Where(workoutSet => workoutSet.WorkoutExerciseId == workoutExerciseId)
            .OrderBy(workoutSet => workoutSet.SetNumber)
            .ThenBy(workoutSet => workoutSet.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<WorkoutSet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkoutSets
            .FirstOrDefaultAsync(workoutSet => workoutSet.Id == id, cancellationToken);
    }

    public async Task AddAsync(WorkoutSet workoutSet, CancellationToken cancellationToken = default)
    {
        await dbContext.WorkoutSets.AddAsync(workoutSet, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WorkoutSet workoutSet, CancellationToken cancellationToken = default)
    {
        dbContext.WorkoutSets.Update(workoutSet);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WorkoutSet workoutSet, CancellationToken cancellationToken = default)
    {
        dbContext.WorkoutSets.Remove(workoutSet);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
