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

        if (await context.Categories.AnyAsync())
            return;

        Randomizer.Seed = new Random(12345);

        var categories = GenerateCategories();

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var recipes = GenerateRecipes(categories);

        await context.Recipes.AddRangeAsync(recipes);
        await context.SaveChangesAsync();
    }

    private static List<Category> GenerateCategories()
    {
        var faker = new Faker("vi");

        var categories = new List<Category>();

        for (int i = 1; i <= 20; i++)
        {
            categories.Add(new Category
            {
                Id = Guid.NewGuid(),

                Name = $"Danh mục món ăn {i}",

                Slug = $"danh-muc-mon-an-{i}",

                Description =
                    faker.Lorem.Sentence(),

                OrderIndex = i,

                CreatedAt = DateTime.UtcNow
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

            var recipe = new Recipe
            {
                Id = recipeId,

                Title = $"Công thức món ăn {i}",

                Slug = $"cong-thuc-mon-an-{i}",

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

                        Name = $"Nguyên liệu {j}",

                        Quantity =
                            faker.Random.Int(1, 500).ToString(),

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
}