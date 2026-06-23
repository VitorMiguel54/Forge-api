using Forge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class XpTransactionConfiguration : IEntityTypeConfiguration<XpTransaction>
{
    public void Configure(EntityTypeBuilder<XpTransaction> builder)
    {
        builder.ToTable("XpTransactions");

        builder.HasKey(xpTransaction => xpTransaction.Id);

        builder.Property(xpTransaction => xpTransaction.Source)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(xpTransaction => xpTransaction.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(xpTransaction => xpTransaction.UserProfileId);
        builder.HasIndex(xpTransaction => xpTransaction.CreatedAt);
        builder.HasIndex(xpTransaction => xpTransaction.Source);

        builder.HasOne(xpTransaction => xpTransaction.UserProfile)
            .WithMany(userProfile => userProfile.XpTransactions)
            .HasForeignKey(xpTransaction => xpTransaction.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(xpTransaction => xpTransaction.CreatedAt)
            .HasConversion(DateTimeUtcConverter.Instance);
    }
}
