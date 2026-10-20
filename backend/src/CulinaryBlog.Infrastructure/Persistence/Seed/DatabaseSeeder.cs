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

        await EnsureVietnameseSampleRecipesAsync(context);
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

    private static async Task EnsureVietnameseSampleRecipesAsync(
        CulinaryBlogDbContext context)
    {
        var categoryId = await context.Categories
            .OrderBy(x => x.OrderIndex)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (categoryId == Guid.Empty)
        {
            return;
        }

        var sampleRecipes = new[]
        {
            new
            {
                Title = "Phở bò",
                Slug = "pho-bo",
                Description = "Món phở bò thơm ngon, nước dùng đậm đà, bánh phở mềm, thịt bò tươi, rau thơm và gia vị đặc trưng.",
                Instructions = "1. Đun sôi nước với xương bò, gừng, hành, quế và các gia vị. 2. Nấu khoảng 3 giờ cho nước dùng ngọt và đậm vị. 3. Thêm nêm muối, đường, hạt nêm cho vừa ăn. 4. Chuẩn bị bánh phở, thịt bò, hành, rau thơm và chanh. 5. Cho bánh phở vào tô, xếp thịt bò, chan nước dùng nóng và thưởng thức.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 180,
                Servings = 3,
                Difficulty = DifficultyLevel.Medium,
                Status = RecipeStatus.Published,
                CategoryId = categoryId
            },
            new
            {
                Title = "Phở gà",
                Slug = "pho-ga",
                Description = "Phở gà thanh đạm, nước dùng trong và thơm, ăn cùng bánh phở, hành, ngò gai và chanh.",
                Instructions = "1. Luộc gà với gừng và hành cho thơm. 2. Nấu nước dùng với xương gà và gia vị. 3. Chắt lấy nước trong và nêm lại cho vừa. 4. Cắt gà thành miếng. 5. Cho bánh phở vào tô, xếp thịt gà, chan nước dùng và thêm rau thơm.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 150,
                Servings = 3,
                Difficulty = DifficultyLevel.Easy,
                Status = RecipeStatus.Published,
                CategoryId = categoryId
            },
            new
            {
                Title = "Bún bò Huế",
                Slug = "bun-bo-hue",
                Description = "Bún bò Huế cay nồng, đậm vị với nước dùng sả, ruốc, thịt bò và chả cua.",
                Instructions = "1. Nấu xương bò với sả, nước mắm, muối và các gia vị. 2. Thêm tiết và chả cua cho đậm vị. 3. Chuẩn bị bún, thịt bò, rau sống. 4. Chan nước dùng nóng lên bún và thịt. 5. Thưởng thức kèm chanh, ớt và rau thơm.",
                PrepTimeMinutes = 25,
                CookTimeMinutes = 170,
                Servings = 4,
                Difficulty = DifficultyLevel.Hard,
                Status = RecipeStatus.Published,
                CategoryId = categoryId
            },
            new
            {
                Title = "Gà nướng",
                Slug = "ga-nuong",
                Description = "Gà nướng thơm lừng, vàng ươm, được tẩm ướp gia vị đặc trưng và nướng tới khi chín mềm.",
                Instructions = "1. Ướp gà với tỏi, mật ong, dầu hào, muối, tiêu và gia vị. 2. Để gà ngấm gia vị trong 30 phút. 3. Nướng ở lửa vừa đến khi vàng và chín. 4. Quay đều để gà chín đều. 5. Thưởng thức cùng rau sống và nước chấm.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 80,
                Servings = 4,
                Difficulty = DifficultyLevel.Medium,
                Status = RecipeStatus.Published,
                CategoryId = categoryId
            },
            new
            {
                Title = "Canh chua cá",
                Slug = "canh-chua-ca",
                Description = "Canh chua cá thanh mát, chua ngọt hài hòa với dứa, cà chua, rau thơm và cá tươi.",
                Instructions = "1. Làm nóng nước với cà chua, dứa và nước mắm. 2. Thêm cá vào nấu chín vừa. 3. Cho me, đường, muối để cân bằng vị chua ngọt. 4. Nấu thêm vài phút cho cá mềm. 5. Cho rau thơm và thưởng thức nóng.",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 45,
                Servings = 3,
                Difficulty = DifficultyLevel.Easy,
                Status = RecipeStatus.Published,
                CategoryId = categoryId
            }
        };

        foreach (var sampleRecipe in sampleRecipes)
        {
            var exists = await context.Recipes.AnyAsync(x =>
                x.Title == sampleRecipe.Title || x.Slug == sampleRecipe.Slug);

            if (exists)
            {
                continue;
            }

            var recipe = new Recipe
            {
                Id = Guid.NewGuid(),
                Title = sampleRecipe.Title,
                Slug = sampleRecipe.Slug,
                Description = sampleRecipe.Description,
                Instructions = sampleRecipe.Instructions,
                PrepTimeMinutes = sampleRecipe.PrepTimeMinutes,
                CookTimeMinutes = sampleRecipe.CookTimeMinutes,
                Servings = sampleRecipe.Servings,
                Difficulty = sampleRecipe.Difficulty,
                Status = sampleRecipe.Status,
                CategoryId = sampleRecipe.CategoryId,
                CreatedAt = DateTime.UtcNow,
                Nutrition = new RecipeNutrition
                {
                    Calories = 450,
                    Protein = 25,
                    Carbs = 50,
                    Fat = 18,
                    Fiber = 6,
                    Sodium = 600
                },
                Images =
                {
                    new RecipeImage
                    {
                        Id = Guid.NewGuid(),
                        RecipeId = Guid.Empty,
                        OriginalUrl = $"https://picsum.photos/seed/{sampleRecipe.Slug}/1200/800",
                        MediumUrl = $"https://picsum.photos/seed/{sampleRecipe.Slug}/800/600",
                        ThumbnailUrl = $"https://picsum.photos/seed/{sampleRecipe.Slug}/300/300",
                        AltText = sampleRecipe.Title,
                        IsPrimary = true,
                        OrderIndex = 1,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            recipe.Images.First().RecipeId = recipe.Id;

            await context.Recipes.AddAsync(recipe);
        }

        await context.SaveChangesAsync();
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
    }
}