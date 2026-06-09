using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoberNetwork.Domain.Entities;

namespace SoberNetwork.Infrastructure.Data.Configurations;

public class PostMediaConfiguration : IEntityTypeConfiguration<PostMedia>
{
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
        builder.ToTable("post_media");
        
        builder.HasKey(p => p.Id);
        
        builder.HasIndex(p => p.GroupId);
        builder.HasIndex(p => p.PostId);
        builder.HasIndex(p => new { p.GroupId, p.DeletedAt });  // Soft delete filter
        
        builder.Property(p => p.MediaType)
            .IsRequired()
            .HasMaxLength(20);  // "image" or "video"
        
        builder.Property(p => p.FileName)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(p => p.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(p => p.ContentType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(p => p.ThumbnailPath)
            .HasMaxLength(1000);
        
        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("NOW()");
        
        // Relationships
        builder.HasOne(p => p.Group)
            .WithMany()
            .HasForeignKey(p => p.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.Post)
            .WithOne(p => p.Media)
            .HasForeignKey<PostMedia>(p => p.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
