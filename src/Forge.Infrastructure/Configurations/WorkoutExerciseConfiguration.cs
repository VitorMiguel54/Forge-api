using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
    {
        builder.ToTable("WorkoutExercises");

        builder.HasKey(workoutExercise => workoutExercise.Id);

        builder.Property(workoutExercise => workoutExercise.Notes)
            .HasMaxLength(1000);

        builder.HasOne(workoutExercise => workoutExercise.Workout)
            .WithMany(workout => workout.WorkoutExercises)
            .HasForeignKey(workoutExercise => workoutExercise.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(workoutExercise => workoutExercise.Exercise)
            .WithMany(exercise => exercise.WorkoutExercises)
            .HasForeignKey(workoutExercise => workoutExercise.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(workoutExercise => workoutExercise.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(workoutExercise => workoutExercise.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
