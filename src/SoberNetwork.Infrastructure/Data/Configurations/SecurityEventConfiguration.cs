using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoberNetwork.Domain.Entities;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Infrastructure.Data.Configurations;

public class SecurityEventConfiguration : IEntityTypeConfiguration<SecurityEvent>
{
    public void Configure(EntityTypeBuilder<SecurityEvent> builder)
    {
        builder.ToTable("security_events");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(e => e.IpAddress).HasMaxLength(45);   // IPv6 max length
        builder.Property(e => e.UserAgent).HasMaxLength(512);
        builder.Property(e => e.Details).HasMaxLength(1024);
        builder.Property(e => e.UserId);  // Guid — no MaxLength needed

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.EventType);

        // Nullable FK — events can exist without a user (e.g., failed login for unknown email)
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
