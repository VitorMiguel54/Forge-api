using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class LevelDefinitionConfiguration : IEntityTypeConfiguration<LevelDefinition>
{
    public void Configure(EntityTypeBuilder<LevelDefinition> builder)
    {
        builder.ToTable("LevelDefinitions");
        builder.HasKey(level => level.Id);

        builder.Property(level => level.Name).IsRequired().HasMaxLength(120);
        builder.Property(level => level.Description).IsRequired().HasMaxLength(500);
        builder.Property(level => level.BadgeImageUrl).HasMaxLength(500);
        builder.Property(level => level.GuardianImageUrl).HasMaxLength(500);
        builder.Property(level => level.GuardianImageStorageKey).HasMaxLength(500);
        builder.Property(level => level.IsActive).HasDefaultValue(true);

        builder.HasOne(level => level.Rarity)
            .WithMany(rarity => rarity.LevelDefinitions)
            .HasForeignKey(level => level.RarityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(level => level.Name).IsUnique();
        builder.HasIndex(level => level.DisplayOrder).IsUnique();
        builder.HasIndex(level => level.MinimumXp).IsUnique();
        builder.HasIndex(level => level.RarityId);

        builder.Property(level => level.CreatedAt).HasConversion(DateTimeUtcConverter.Instance);
        builder.Property(level => level.UpdatedAt).HasConversion(DateTimeUtcConverter.Instance);
    }
}
