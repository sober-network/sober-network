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

        builder.HasOne(m => m.Group)
            .WithMany(g => g.Meetings)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite index for the common "get active meetings for a group" query
        builder.HasIndex(m => new { m.GroupId, m.IsActive, m.DeletedAt });
    }
}
