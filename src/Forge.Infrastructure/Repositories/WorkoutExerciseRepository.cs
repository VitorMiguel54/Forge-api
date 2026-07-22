using Forge.Application.Interfaces;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class WorkoutExerciseRepository(ForgeDbContext dbContext) : IWorkoutExerciseRepository
{
    public async Task<IReadOnlyCollection<WorkoutExercise>> GetByWorkoutAsync(
        Guid workoutId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkoutExercises
            .AsNoTracking()
            .Where(workoutExercise => workoutExercise.WorkoutId == workoutId)
            .OrderBy(workoutExercise => workoutExercise.Order)
            .ThenBy(workoutExercise => workoutExercise.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<WorkoutExercise?> GetByWorkoutAndIdAsync(
        Guid workoutId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkoutExercises
            .FirstOrDefaultAsync(
                workoutExercise => workoutExercise.WorkoutId == workoutId && workoutExercise.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExerciseExistsInWorkoutAsync(
        Guid workoutId,
        Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkoutExercises
            .AsNoTracking()
            .AnyAsync(
                workoutExercise =>
                    workoutExercise.WorkoutId == workoutId
                    && workoutExercise.ExerciseId == exerciseId,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkoutExercises
            .AsNoTracking()
            .AnyAsync(workoutExercise => workoutExercise.Id == id, cancellationToken);
    }

    public async Task AddAsync(WorkoutExercise workoutExercise, CancellationToken cancellationToken = default)
    {
        await dbContext.WorkoutExercises.AddAsync(workoutExercise, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WorkoutExercise workoutExercise, CancellationToken cancellationToken = default)
    {
        dbContext.WorkoutExercises.Remove(workoutExercise);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
