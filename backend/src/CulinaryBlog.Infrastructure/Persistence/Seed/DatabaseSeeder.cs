using Bogus;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        CulinaryBlogDbContext context)
    {
        await context.Database.MigrateAsync();

        Randomizer.Seed = new Random(12345);

        if (!await context.Categories.AnyAsync())
        {
            var categories = GenerateCategories();

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            var recipes = GenerateRecipes(categories);

            await context.Recipes.AddRangeAsync(recipes);
            await context.SaveChangesAsync();
        }

        await EnsureRecipeDataAsync(context);
    }

    private static List<Category> GenerateCategories()
    {
        var faker = new Faker("vi");

        var categories = new List<Category>();

        for (int i = 1; i <= 20; i++)
        {
            categories.Add(
                new Category
                {
                    Id = Guid.NewGuid(),

                    Name =
                        $"Danh mục món ăn {i}",

                    Slug =
                        $"danh-muc-mon-an-{i}",

                    Description =
                        faker.Lorem.Sentence(),

                    OrderIndex = i,

                    CreatedAt =
                        DateTime.UtcNow
                });
        }

        return categories;
    }

    private static List<Recipe> GenerateRecipes(
        List<Category> categories)
    {
        var recipes = new List<Recipe>();

        var faker = new Faker("vi");

        for (int i = 1; i <= 100; i++)
        {
            var recipeId = Guid.NewGuid();

            var recipe =
                new Recipe
                {
                    Id = recipeId,

                    Title =
                        $"Công thức món ăn {i}",

                    Slug =
                        $"cong-thuc-mon-an-{i}",

                    Description =
                        faker.Lorem.Paragraph(),

                    Instructions =
                        faker.Lorem.Paragraphs(2),

                    PrepTimeMinutes =
                        faker.Random.Int(5, 60),

                    CookTimeMinutes =
                        faker.Random.Int(10, 180),

                    Servings =
                        faker.Random.Int(1, 8),

                    Difficulty =
                        faker.PickRandom<DifficultyLevel>(),

                    Status =
                        RecipeStatus.Published,

                    CategoryId =
                        faker.PickRandom(categories).Id,

                    Nutrition =
                        new RecipeNutrition
                        {
                            Calories =
                                faker.Random.Decimal(100, 900),

                            Protein =
                                faker.Random.Decimal(5, 80),

                            Carbs =
                                faker.Random.Decimal(10, 120),

                            Fat =
                                faker.Random.Decimal(5, 60),

                            Fiber =
                                faker.Random.Decimal(1, 20),

                            Sodium =
                                faker.Random.Decimal(10, 1500)
                        },

                    CreatedAt =
                        DateTime.UtcNow
                };

            // 10 ingredients
            for (int j = 1; j <= 10; j++)
            {
                recipe.Ingredients.Add(
                    new RecipeIngredient
                    {
                        Id = Guid.NewGuid(),

                        RecipeId = recipeId,

                        Name =
                            $"Nguyên liệu {j}",

                        Quantity =
                            faker.Random
                                .Int(1, 500)
                                .ToString(),

                        Unit =
                            faker.PickRandom(
                                "g",
                                "kg",
                                "ml",
                                "l",
                                "quả",
                                "cái",
                                "thìa"),

                        Notes =
                            faker.Lorem.Sentence(),

                        OrderIndex = j,

                        CreatedAt =
                            DateTime.UtcNow
                    });
            }

            // 5 steps
            for (int j = 1; j <= 5; j++)
            {
                recipe.Steps.Add(
                    new RecipeStep
                    {
                        Id = Guid.NewGuid(),

                        RecipeId = recipeId,

                        StepNumber = j,

                        Title =
                            $"Bước thực hiện {j}",

                        Description =
                            faker.Lorem.Paragraph(),

                        TimerMinutes =
                            faker.Random.Int(1, 30),

                        CreatedAt =
                            DateTime.UtcNow
                    });
            }

            recipes.Add(recipe);
        }

        return recipes;
    }

private static async Task EnsureRecipeDataAsync(
    CulinaryBlogDbContext context)
{
    var hasRecipes =
        await context.Recipes.AnyAsync();

    if (hasRecipes)
    {
        return;
    }

    var recipes =
        await context.Recipes
            .Include(x => x.Nutrition)
            .Include(x => x.Images)
            .ToListAsync();

    foreach (var recipe in recipes)
    {
        if (recipe.Nutrition == null)
        {
            recipe.Nutrition =
                new RecipeNutrition
                {
                    Calories = 450,
                    Protein = 25,
                    Carbs = 50,
                    Fat = 18,
                    Fiber = 6,
                    Sodium = 600
                };
        }

        if (!recipe.Images.Any())
        {
            recipe.Images.Add(
                new RecipeImage
                {
                    Id = Guid.NewGuid(),

                    RecipeId = recipe.Id,

                    OriginalUrl =
                        $"https://picsum.photos/seed/recipe-{recipe.Id}/1200/800",

                    MediumUrl =
                        $"https://picsum.photos/seed/recipe-{recipe.Id}/800/600",

                    ThumbnailUrl =
                        $"https://picsum.photos/seed/recipe-{recipe.Id}/300/300",

                    AltText =
                        recipe.Title,

                    IsPrimary = true,

                    OrderIndex = 1,

                    CreatedAt =
                        DateTime.UtcNow
                });
        }
    }

    await context.SaveChangesAsync();
}}