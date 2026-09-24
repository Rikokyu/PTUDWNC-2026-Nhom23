using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence;

public class CulinaryBlogDbContext : DbContext
{
    public CulinaryBlogDbContext(
        DbContextOptions<CulinaryBlogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Recipe> Recipes => Set<Recipe>();

    public DbSet<RecipeIngredient> RecipeIngredients
        => Set<RecipeIngredient>();

    public DbSet<RecipeStep> RecipeSteps
        => Set<RecipeStep>();

    public DbSet<RecipeImage> RecipeImages
        => Set<RecipeImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CulinaryBlogDbContext).Assembly);
    }
}