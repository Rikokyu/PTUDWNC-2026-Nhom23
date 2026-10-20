using Bogus;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    private const int MinimumCategoryCount = 20;
    private const int MinimumRecipeCount = 100;

    public static async Task SeedAsync(
        CulinaryBlogDbContext context,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        Randomizer.Seed = new Random(12345);

        var categories = await EnsureCategoriesAsync(
            context,
            cancellationToken);
        await EnsureRecipesAsync(
            context,
            categories,
            cancellationToken);
    }

    private static async Task<List<Category>> EnsureCategoriesAsync(
        CulinaryBlogDbContext context,
        CancellationToken cancellationToken)
    {
        var categories = await context.Categories
            .OrderBy(category => category.OrderIndex)
            .ToListAsync(cancellationToken);
        var names = categories
            .Select(category => category.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var slugs = categories
            .Select(category => category.Slug)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var nextNumber = 1;
        var faker = new Faker("vi");

        while (categories.Count < MinimumCategoryCount)
        {
            var name = $"Danh mục món ăn {nextNumber}";
            var slug = $"danh-muc-mon-an-{nextNumber++}";
            if (!names.Add(name) || !slugs.Add(slug))
            {
                continue;
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug,
                Description = faker.Lorem.Sentence(),
                OrderIndex = categories.Count + 1,
                CreatedAt = DateTime.UtcNow
            };

            categories.Add(category);
            context.Categories.Add(category);
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return categories;
    }

    private static async Task EnsureRecipesAsync(
        CulinaryBlogDbContext context,
        IReadOnlyList<Category> categories,
        CancellationToken cancellationToken)
    {
        var existingSlugs = await context.Recipes
            .Select(recipe => recipe.Slug)
            .ToListAsync(cancellationToken);
        var slugs = existingSlugs.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingCount = MinimumRecipeCount - existingSlugs.Count;
        if (missingCount <= 0)
        {
            return;
        }

        var recipes = GenerateRecipes(
            categories,
            missingCount,
            slugs);
        await context.Recipes.AddRangeAsync(recipes, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<Recipe> GenerateRecipes(
        IReadOnlyList<Category> categories,
        int count,
        HashSet<string> existingSlugs)
    {
        var recipes = new List<Recipe>(count);
        var faker = new Faker("vi");
        var recipeNumber = 1;

        while (recipes.Count < count)
        {
            var slug = $"cong-thuc-mon-an-{recipeNumber}";
            var currentNumber = recipeNumber++;
            if (!existingSlugs.Add(slug))
            {
                continue;
            }

            var recipeId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var recipe = new Recipe
            {
                Id = recipeId,
                Title = $"Công thức món ăn {currentNumber}",
                Slug = slug,
                Description = faker.Lorem.Paragraph(),
                Instructions = faker.Lorem.Paragraphs(2),
                PrepTimeMinutes = faker.Random.Int(5, 60),
                CookTimeMinutes = faker.Random.Int(10, 180),
                Servings = faker.Random.Int(1, 8),
                Difficulty = faker.PickRandom<DifficultyLevel>(),
                Status = RecipeStatus.Published,
                CategoryId = categories[
                    faker.Random.Int(0, categories.Count - 1)].Id,
                CreatedAt = now,
                Nutrition = new RecipeNutrition
                {
                    Calories = faker.Random.Decimal(100, 900),
                    Protein = faker.Random.Decimal(5, 80),
                    Carbs = faker.Random.Decimal(10, 120),
                    Fat = faker.Random.Decimal(5, 60),
                    Fiber = faker.Random.Decimal(1, 20),
                    Sodium = faker.Random.Decimal(10, 1500)
                },
                Images =
                [
                    new RecipeImage
                    {
                        Id = Guid.NewGuid(),
                        RecipeId = recipeId,
                        OriginalUrl = $"https://picsum.photos/seed/recipe-{recipeId}/1200/800",
                        MediumUrl = $"https://picsum.photos/seed/recipe-{recipeId}/800/600",
                        ThumbnailUrl = $"https://picsum.photos/seed/recipe-{recipeId}/300/300",
                        AltText = $"Công thức món ăn {currentNumber}",
                        IsPrimary = true,
                        OrderIndex = 1,
                        CreatedAt = now
                    }
                ]
            };

            for (var index = 1; index <= 10; index++)
            {
                recipe.Ingredients.Add(new RecipeIngredient
                {
                    Id = Guid.NewGuid(),
                    RecipeId = recipeId,
                    Name = $"Nguyên liệu {index}",
                    Quantity = faker.Random.Int(1, 500).ToString(),
                    Unit = faker.PickRandom("g", "kg", "ml", "l", "quả", "cái", "thìa"),
                    Notes = faker.Lorem.Sentence(),
                    OrderIndex = index,
                    CreatedAt = now
                });
            }

            for (var stepNumber = 1; stepNumber <= 5; stepNumber++)
            {
                recipe.Steps.Add(new RecipeStep
                {
                    Id = Guid.NewGuid(),
                    RecipeId = recipeId,
                    StepNumber = stepNumber,
                    Title = $"Bước thực hiện {stepNumber}",
                    Description = faker.Lorem.Paragraph(),
                    TimerMinutes = faker.Random.Int(1, 30),
                    CreatedAt = now
                });
            }

            recipes.Add(recipe);
        }

        return recipes;
    }
}
