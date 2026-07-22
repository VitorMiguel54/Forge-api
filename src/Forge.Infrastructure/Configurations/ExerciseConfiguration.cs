using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");

        builder.HasKey(exercise => exercise.Id);

        builder.Property(exercise => exercise.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(exercise => exercise.Description)
            .HasMaxLength(500);

        builder.Property(exercise => exercise.Difficulty)
            .HasMaxLength(80);

        builder.Property(exercise => exercise.Equipment)
            .HasMaxLength(120);

        builder.Property(exercise => exercise.IsActive)
            .HasDefaultValue(true);

        builder.Property(exercise => exercise.ImageUrl)
            .HasMaxLength(500);

        builder.Property(exercise => exercise.GifUrl)
            .HasMaxLength(500);

        builder.Property(exercise => exercise.VideoUrl)
            .HasMaxLength(500);

        builder.Property(exercise => exercise.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(exercise => exercise.MuscleGroup)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(exercise => exercise.UserProfileId)
            .IsRequired(false);

        builder.HasIndex(exercise => exercise.Name);
        builder.HasIndex(exercise => exercise.MuscleGroup);
        builder.HasIndex(exercise => exercise.MuscleGroupId);
        builder.HasIndex(exercise => exercise.IsActive);
        builder.HasIndex(exercise => exercise.DisplayOrder);

        builder.HasOne(exercise => exercise.MuscleGroupEntity)
            .WithMany(muscleGroup => muscleGroup.Exercises)
            .HasForeignKey(exercise => exercise.MuscleGroupId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(exercise => exercise.UserProfile)
            .WithMany(userProfile => userProfile.Exercises)
            .HasForeignKey(exercise => exercise.UserProfileId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(exercise => exercise.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(exercise => exercise.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}

