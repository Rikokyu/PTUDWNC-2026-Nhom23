using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

public class RecipeConfiguration
    : IEntityTypeConfiguration<Recipe>
{
    public void Configure(
        EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(220)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Instructions)
            .IsRequired();

        // The trigger owns this infrastructure-only value, keeping Npgsql out
        // of the domain entity and preventing EF from overwriting the vector.
        var searchVector = builder.Property<NpgsqlTsVector>("SearchVector")
            .HasColumnType("tsvector")
            .ValueGeneratedOnAddOrUpdate();
        searchVector.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        searchVector.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.HasIndex("SearchVector").HasMethod("GIN");

        builder.Property(x => x.Difficulty)
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.HasIndex(x => x.CategoryId);

        builder.HasIndex(x => new
        {
            x.Status,
            x.CreatedAt
        });

        builder.HasMany(x => x.Ingredients)
            .WithOne(x => x.Recipe)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Steps)
            .WithOne(x => x.Recipe)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Recipe)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Author)
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(
            x => x.Nutrition,
            nutrition =>
            {
                nutrition.Property(x => x.Calories)
                    .HasPrecision(10, 2);

                nutrition.Property(x => x.Protein)
                    .HasPrecision(10, 2);

                nutrition.Property(x => x.Carbs)
                    .HasPrecision(10, 2);

                nutrition.Property(x => x.Fat)
                    .HasPrecision(10, 2);

                nutrition.Property(x => x.Fiber)
                    .HasPrecision(10, 2);

                nutrition.Property(x => x.Sodium)
                    .HasPrecision(10, 2);
            });
    }
}
