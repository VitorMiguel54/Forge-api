using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.ToTable("UserAchievements");

        builder.HasKey(userAchievement => userAchievement.Id);

        builder.HasIndex(userAchievement => new
            {
                userAchievement.UserProfileId,
                userAchievement.AchievementId
            })
            .IsUnique();

        builder.HasOne(userAchievement => userAchievement.UserProfile)
            .WithMany(userProfile => userProfile.UserAchievements)
            .HasForeignKey(userAchievement => userAchievement.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(userAchievement => userAchievement.Achievement)
            .WithMany(achievement => achievement.UserAchievements)
            .HasForeignKey(userAchievement => userAchievement.AchievementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(userAchievement => userAchievement.UnlockedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
