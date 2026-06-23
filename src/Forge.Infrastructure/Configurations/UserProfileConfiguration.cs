using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(userProfile => userProfile.Id);

        builder.Property(userProfile => userProfile.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(userProfile => userProfile.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(userProfile => userProfile.Email)
            .IsUnique();

        builder.Property(userProfile => userProfile.InitialWeight)
            .HasPrecision(6, 2);

        builder.Property(userProfile => userProfile.CurrentWeight)
            .HasPrecision(6, 2);

        builder.Property(userProfile => userProfile.DailyWaterGoalInLiters)
            .HasPrecision(5, 2);

        builder.Property(userProfile => userProfile.DailySleepGoalInHours)
            .HasPrecision(4, 2);

        builder.Property(userProfile => userProfile.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(userProfile => userProfile.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
