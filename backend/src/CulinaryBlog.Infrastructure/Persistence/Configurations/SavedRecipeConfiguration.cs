using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

public sealed class SavedRecipeConfiguration
    : IEntityTypeConfiguration<SavedRecipe>
{
    public void Configure(EntityTypeBuilder<SavedRecipe> builder)
    {
        builder.ToTable("SavedRecipes");

        builder.HasKey(savedRecipe => new
        {
            savedRecipe.UserId,
            savedRecipe.RecipeId
        });

        builder.Property(savedRecipe => savedRecipe.CreatedAt)
            .IsRequired();

        builder.HasIndex(savedRecipe => new
        {
            savedRecipe.UserId,
            savedRecipe.CreatedAt
        });

        builder.HasOne(savedRecipe => savedRecipe.User)
            .WithMany()
            .HasForeignKey(savedRecipe => savedRecipe.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(savedRecipe => savedRecipe.Recipe)
            .WithMany()
            .HasForeignKey(savedRecipe => savedRecipe.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
