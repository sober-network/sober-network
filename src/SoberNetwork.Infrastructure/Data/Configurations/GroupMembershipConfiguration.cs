using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoberNetwork.Domain.Entities;

namespace SoberNetwork.Infrastructure.Data.Configurations;

public class GroupMembershipConfiguration : IEntityTypeConfiguration<GroupMembership>
{
    public void Configure(EntityTypeBuilder<GroupMembership> builder)
    {
        builder.ToTable("group_memberships");
        builder.HasKey(m => m.Id);

        // Unique: one membership record per user per group
        builder.HasIndex(m => new { m.UserId, m.GroupId }).IsUnique();

        // Index GroupId alone for efficient "get all members of group X" queries
        builder.HasIndex(m => m.GroupId);

        // Composite index for active-member counts/listings (group_id + status + soft-delete filter)
        builder.HasIndex(m => new { m.GroupId, m.Status, m.DeletedAt });

        // Index created_at for chronological queries and reporting
        builder.HasIndex(m => m.CreatedAt);

        builder.HasOne(m => m.User)
            .WithMany(u => u.GroupMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Group)
            .WithMany(g => g.Memberships)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(m => m.Role).HasConversion<string>();
        builder.Property(m => m.Status).HasConversion<string>();
    }
}
