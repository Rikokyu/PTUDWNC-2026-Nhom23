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

    public DbSet<ApplicationUser> ApplicationUsers =>
        Set<ApplicationUser>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    public DbSet<Category> Categories =>
        Set<Category>();

    public DbSet<Recipe> Recipes =>
        Set<Recipe>();

    public DbSet<RecipeIngredient> RecipeIngredients =>
        Set<RecipeIngredient>();

    public DbSet<RecipeStep> RecipeSteps =>
        Set<RecipeStep>();

    public DbSet<RecipeImage> RecipeImages =>
        Set<RecipeImage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CulinaryBlogDbContext).Assembly);

        if (Database.IsNpgsql())
        {
            var builder = modelBuilder.Entity<Recipe>();
        builder.Property<NpgsqlTypes.NpgsqlTsVector>("SearchVector")
            .HasColumnType("tsvector").ValueGeneratedOnAddOrUpdate();
        builder.HasIndex("SearchVector").HasMethod("GIN");

        }
    }
}
