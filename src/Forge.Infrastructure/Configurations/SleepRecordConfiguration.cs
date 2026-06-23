using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class SleepRecordConfiguration : IEntityTypeConfiguration<SleepRecord>
{
    public void Configure(EntityTypeBuilder<SleepRecord> builder)
    {
        builder.ToTable("SleepRecords");

        builder.HasKey(sleepRecord => sleepRecord.Id);

        builder.Property(sleepRecord => sleepRecord.HoursSlept)
            .HasPrecision(4, 2);

        builder.Property(sleepRecord => sleepRecord.GoalInHours)
            .HasPrecision(4, 2);

        builder.HasIndex(sleepRecord => sleepRecord.UserProfileId);
        builder.HasIndex(sleepRecord => sleepRecord.SleepDate);

        builder.HasOne(sleepRecord => sleepRecord.UserProfile)
            .WithMany(userProfile => userProfile.SleepRecords)
            .HasForeignKey(sleepRecord => sleepRecord.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(sleepRecord => sleepRecord.SleepDate)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(sleepRecord => sleepRecord.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(sleepRecord => sleepRecord.UpdatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
