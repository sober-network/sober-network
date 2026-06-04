using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoberNetwork.Domain.Entities;

namespace SoberNetwork.Infrastructure.Data.Configurations;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.ToTable("meetings");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Time).IsRequired().HasMaxLength(5);
        builder.Property(m => m.DurationMinutes).HasDefaultValue(60);
        builder.Property(m => m.Formats).HasColumnType("text[]").HasDefaultValueSql("ARRAY[]::text[]");

        builder.Property(m => m.MeetingType)
            .HasConversion<int>()
            .HasDefaultValue(SoberNetwork.Domain.Enums.MeetingType.InPerson);
        builder.Property(m => m.VenueName).HasMaxLength(200);
        builder.Property(m => m.Street).HasMaxLength(200);
        builder.Property(m => m.City).HasMaxLength(100);
        builder.Property(m => m.State).HasMaxLength(100);
        builder.Property(m => m.PostalCode).HasMaxLength(20);
        builder.Property(m => m.Country).HasMaxLength(100);
        builder.Property(m => m.PublicJoinUrl).HasMaxLength(500);

        builder.HasOne(m => m.Group)
            .WithMany(g => g.Meetings)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite index for the common "get active meetings for a group" query
        builder.HasIndex(m => new { m.GroupId, m.IsActive, m.DeletedAt });

        // Index for the public meeting finder (cross-group, public groups only)
        builder.HasIndex(m => new { m.IsActive, m.DeletedAt, m.MeetingType, m.DayOfWeek });
    }
}
