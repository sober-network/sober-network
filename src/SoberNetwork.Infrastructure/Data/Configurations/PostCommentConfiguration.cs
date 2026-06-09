using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoberNetwork.Domain.Entities;

namespace SoberNetwork.Infrastructure.Data.Configurations;

public class PostCommentConfiguration : IEntityTypeConfiguration<PostComment>
{
    public void Configure(EntityTypeBuilder<PostComment> builder)
    {
        builder.ToTable("post_comments");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Body).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.LinkUrl).HasMaxLength(500);
        builder.Property(c => c.LinkTitle).HasMaxLength(200);

        builder.HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing: replies belong to a parent comment
        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.PostId);
        builder.HasIndex(c => c.ParentCommentId);
    }
}
