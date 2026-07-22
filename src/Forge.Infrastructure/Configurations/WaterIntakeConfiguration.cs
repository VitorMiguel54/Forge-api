using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class WaterIntakeConfiguration : IEntityTypeConfiguration<WaterIntake>
{
    public void Configure(EntityTypeBuilder<WaterIntake> builder)
    {
        builder.ToTable("WaterIntakes");

        builder.HasKey(waterIntake => waterIntake.Id);

        builder.Property(waterIntake => waterIntake.Liters)
            .HasPrecision(5, 2);

        builder.Property(waterIntake => waterIntake.GoalInLiters)
            .HasPrecision(5, 2);

        builder.HasIndex(waterIntake => waterIntake.UserProfileId);
        builder.HasIndex(waterIntake => waterIntake.IntakeDate);

        builder.HasOne(waterIntake => waterIntake.UserProfile)
            .WithMany(userProfile => userProfile.WaterIntakes)
            .HasForeignKey(waterIntake => waterIntake.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(waterIntake => waterIntake.IntakeDate)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(waterIntake => waterIntake.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(waterIntake => waterIntake.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
