using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class WeightRecordConfiguration : IEntityTypeConfiguration<WeightRecord>
{
    public void Configure(EntityTypeBuilder<WeightRecord> builder)
    {
        builder.ToTable("WeightRecords");

        builder.HasKey(weightRecord => weightRecord.Id);

        builder.Property(weightRecord => weightRecord.Weight)
            .HasPrecision(6, 2);

        builder.HasIndex(weightRecord => weightRecord.UserProfileId);
        builder.HasIndex(weightRecord => weightRecord.RecordDate);

        builder.HasOne(weightRecord => weightRecord.UserProfile)
            .WithMany(userProfile => userProfile.WeightRecords)
            .HasForeignKey(weightRecord => weightRecord.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(weightRecord => weightRecord.RecordDate)
            .HasConversion(DateTimeUtcConverter.Instance);

        builder.Property(weightRecord => weightRecord.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
