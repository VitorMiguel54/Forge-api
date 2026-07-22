using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet>
{
    public void Configure(EntityTypeBuilder<WorkoutSet> builder)
    {
        builder.ToTable("WorkoutSets");

        builder.HasKey(workoutSet => workoutSet.Id);

        builder.Property(workoutSet => workoutSet.Weight)
            .HasPrecision(7, 2);

        builder.Property(workoutSet => workoutSet.Volume)
            .HasPrecision(10, 2);

        builder.Property(workoutSet => workoutSet.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(workoutSet => workoutSet.WorkoutExerciseId);

        builder.HasOne(workoutSet => workoutSet.WorkoutExercise)
            .WithMany(workoutExercise => workoutExercise.WorkoutSets)
            .HasForeignKey(workoutSet => workoutSet.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(workoutSet => workoutSet.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(workoutSet => workoutSet.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
