using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class RarityConfiguration : IEntityTypeConfiguration<Rarity>
{
    public void Configure(EntityTypeBuilder<Rarity> builder)
    {
        builder.ToTable("Rarities");

        builder.HasKey(rarity => rarity.Id);

        builder.Property(rarity => rarity.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(rarity => rarity.PrimaryColor)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(rarity => rarity.SecondaryColor)
            .HasMaxLength(20);

        builder.Property(rarity => rarity.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(rarity => rarity.Name)
            .IsUnique();

        builder.HasIndex(rarity => rarity.DisplayOrder);

        builder.Property(rarity => rarity.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(rarity => rarity.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
