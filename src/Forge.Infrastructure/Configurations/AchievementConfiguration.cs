using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("Achievements");

        builder.HasKey(achievement => achievement.Id);

        builder.Property(achievement => achievement.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(achievement => achievement.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(achievement => achievement.Category)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(achievement => achievement.IsActive)
            .HasDefaultValue(true);

        builder.Property(achievement => achievement.BadgeImageUrl)
            .HasMaxLength(500);

        builder.Property(achievement => achievement.BadgeImageStorageKey)
            .HasMaxLength(500);

        builder.Property(achievement => achievement.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(achievement => achievement.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
