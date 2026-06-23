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

        builder.HasIndex(workout => workout.UserProfileId);
        builder.HasIndex(workout => workout.WorkoutDate);

        builder.HasOne(workout => workout.UserProfile)
            .WithMany(userProfile => userProfile.Workouts)
            .HasForeignKey(workout => workout.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(workout => workout.WorkoutDate)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(workout => workout.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(workout => workout.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
