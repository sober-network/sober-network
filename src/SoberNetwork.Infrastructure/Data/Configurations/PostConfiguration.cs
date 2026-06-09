using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoberNetwork.Domain.Entities;

namespace SoberNetwork.Infrastructure.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Subject).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Body).IsRequired().HasMaxLength(5000);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Property(p => p.LinkUrl).HasMaxLength(500);
        builder.Property(p => p.LinkTitle).HasMaxLength(200);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(p => p.Group)
            .WithMany(g => g.Posts)
            .HasForeignKey(p => p.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.GroupId, p.DeletedAt, p.IsApproved });
        builder.HasIndex(p => new { p.AuthorId, p.DeletedAt });
        builder.HasIndex(p => p.CreatedAt);
    }
}
