using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoberNetwork.Domain.Entities;

namespace SoberNetwork.Infrastructure.Data.Configurations;

/// <summary>Configures persistence for group service role assignments.</summary>
public class GroupServiceRoleConfiguration : IEntityTypeConfiguration<GroupServiceRole>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GroupServiceRole> builder)
    {
        builder.ToTable("group_service_roles");
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.GroupId);
        builder.HasIndex(r => new { r.GroupId, r.UserId });
        builder.HasIndex(r => new { r.GroupId, r.RoleType });

        builder.Property(r => r.RoleType).HasConversion<string>();
        builder.Property(r => r.CustomTitle).HasMaxLength(100);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Group)
            .WithMany(g => g.ServiceRoles)
            .HasForeignKey(r => r.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
