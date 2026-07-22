using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class MuscleGroupConfiguration : IEntityTypeConfiguration<MuscleGroup>
{
    public void Configure(EntityTypeBuilder<MuscleGroup> builder)
    {
        builder.ToTable("MuscleGroups");

        builder.HasKey(muscleGroup => muscleGroup.Id);

        builder.Property(muscleGroup => muscleGroup.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(muscleGroup => muscleGroup.DisplayName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(muscleGroup => muscleGroup.Icon)
            .HasMaxLength(80);

        builder.HasIndex(muscleGroup => muscleGroup.Name)
            .IsUnique();

        builder.HasIndex(muscleGroup => muscleGroup.DisplayOrder);

        builder.Property(muscleGroup => muscleGroup.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(muscleGroup => muscleGroup.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
