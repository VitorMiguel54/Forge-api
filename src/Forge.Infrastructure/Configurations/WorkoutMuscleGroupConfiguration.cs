using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class WorkoutMuscleGroupConfiguration : IEntityTypeConfiguration<WorkoutMuscleGroup>
{
    public void Configure(EntityTypeBuilder<WorkoutMuscleGroup> builder)
    {
        builder.ToTable("WorkoutMuscleGroups");

        builder.HasKey(workoutMuscleGroup => new
        {
            workoutMuscleGroup.WorkoutId,
            workoutMuscleGroup.MuscleGroupId
        });

        builder.HasOne(workoutMuscleGroup => workoutMuscleGroup.Workout)
            .WithMany(workout => workout.WorkoutMuscleGroups)
            .HasForeignKey(workoutMuscleGroup => workoutMuscleGroup.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(workoutMuscleGroup => workoutMuscleGroup.MuscleGroup)
            .WithMany(muscleGroup => muscleGroup.WorkoutMuscleGroups)
            .HasForeignKey(workoutMuscleGroup => workoutMuscleGroup.MuscleGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(workoutMuscleGroup => workoutMuscleGroup.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
