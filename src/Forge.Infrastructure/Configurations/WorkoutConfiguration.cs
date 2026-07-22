using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.ToTable("Workouts");

        builder.HasKey(workout => workout.Id);

        builder.Property(workout => workout.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(workout => workout.Location)
            .HasMaxLength(150);

        builder.Property(workout => workout.Notes)
            .HasMaxLength(1000);

        builder.Property(workout => workout.TotalVolume)
            .HasPrecision(10, 2);

        builder.Property(workout => workout.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(workout => workout.DisplayOrder)
            .IsRequired();

        builder.Property(workout => workout.IsArchived)
            .HasDefaultValue(false);

        builder.Property(workout => workout.StartedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(workout => workout.FinishedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.HasIndex(workout => workout.UserProfileId);
        builder.HasIndex(workout => workout.WorkoutDate);
        builder.HasIndex(workout => workout.Status);
        builder.HasIndex(workout => workout.TemplateWorkoutId);
        builder.HasIndex(workout => workout.IsArchived);
        builder.HasIndex(workout => new { workout.UserProfileId, workout.DisplayOrder });
        builder.HasIndex(workout => workout.StartedAt);
        builder.HasIndex(workout => workout.FinishedAt);

        builder.HasOne(workout => workout.UserProfile)
            .WithMany(userProfile => userProfile.Workouts)
            .HasForeignKey(workout => workout.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(workout => workout.TemplateWorkout)
            .WithMany(workout => workout.Executions)
            .HasForeignKey(workout => workout.TemplateWorkoutId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(workout => workout.WorkoutDate)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(workout => workout.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(workout => workout.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
