using Forge.Application.DTOs.Workout;
using Forge.Domain.Entities;

namespace Forge.Application.Mappings;

public static class WorkoutMappings
{
    public static WorkoutResponse ToResponse(this Workout workout)
    {
        return new WorkoutResponse(
            workout.Id,
            workout.UserProfileId,
            workout.Name,
            workout.WorkoutDate,
            workout.Location,
            workout.Notes,
            workout.TotalVolume);
    }

    public static WorkoutExerciseResponse ToResponse(this WorkoutExercise workoutExercise)
    {
        return new WorkoutExerciseResponse(
            workoutExercise.Id,
            workoutExercise.WorkoutId,
            workoutExercise.ExerciseId,
            workoutExercise.Order,
            workoutExercise.Notes);
    }

    public static WorkoutSetResponse ToResponse(this WorkoutSet workoutSet)
    {
        return new WorkoutSetResponse(
            workoutSet.Id,
            workoutSet.WorkoutExerciseId,
            workoutSet.SetNumber,
            workoutSet.Repetitions,
            workoutSet.Weight,
            workoutSet.Volume);
    }
}
