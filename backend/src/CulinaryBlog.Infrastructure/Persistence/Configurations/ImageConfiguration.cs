using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

public class RecipeImageConfiguration
    : IEntityTypeConfiguration<RecipeImage>
{
    public void Configure(
        EntityTypeBuilder<RecipeImage> builder)
    {
        builder.ToTable("RecipeImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.MediumUrl)
            .HasMaxLength(500);

        builder.Property(x => x.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(x => x.AltText)
            .HasMaxLength(200);

        builder.HasIndex(x => x.RecipeId);
    }
}