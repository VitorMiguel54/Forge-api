using Forge.Application.Interfaces;
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
            .OrderBy(exercise => exercise.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Exercises
            .FirstOrDefaultAsync(exercise => exercise.Id == id, cancellationToken);
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
}
