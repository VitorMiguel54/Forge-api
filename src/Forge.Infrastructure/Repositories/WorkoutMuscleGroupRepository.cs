using Forge.Application.Interfaces;
using Forge.Domain.Entities;
using Forge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure.Repositories;

public class WorkoutMuscleGroupRepository(ForgeDbContext dbContext) : IWorkoutMuscleGroupRepository
{
    public async Task SyncFromWorkoutExercisesAsync(Guid workoutId, CancellationToken cancellationToken = default)
    {
        var existingGroups = await dbContext.WorkoutMuscleGroups
            .Where(workoutMuscleGroup => workoutMuscleGroup.WorkoutId == workoutId)
            .ToArrayAsync(cancellationToken);

        if (existingGroups.Length > 0)
        {
            dbContext.WorkoutMuscleGroups.RemoveRange(existingGroups);
        }

        var muscleGroupIds = await dbContext.WorkoutExercises
            .Where(workoutExercise => workoutExercise.WorkoutId == workoutId)
            .Select(workoutExercise => workoutExercise.Exercise.MuscleGroupId)
            .Where(muscleGroupId => muscleGroupId != null)
            .Select(muscleGroupId => muscleGroupId!.Value)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var nextGroups = muscleGroupIds
            .Select(muscleGroupId => new WorkoutMuscleGroup
            {
                WorkoutId = workoutId,
                MuscleGroupId = muscleGroupId,
                CreatedAt = now
            })
            .ToArray();

        if (nextGroups.Length > 0)
        {
            await dbContext.WorkoutMuscleGroups.AddRangeAsync(nextGroups, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}


