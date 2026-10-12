using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    private const string DevelopmentPassword = "Password@123";

    public static async Task SeedAsync(CulinaryBlogDbContext context)
    {
        await context.Database.MigrateAsync();

        var users = await EnsureUsersAsync(context);
        var categories = await EnsureCategoriesAsync(context);
        await EnsureRecipesAsync(context, users, categories);
    }

    private static async Task<Dictionary<string, ApplicationUser>> EnsureUsersAsync(
        CulinaryBlogDbContext context)
    {
        var seeds = new[]
        {
            new UserSeed(
                "admin@culinary.local",
                "admin",
                "Quản trị Culinary",
                "Admin",
                "Quản trị viên hệ thống Culinary Blog.",
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=200&q=80"),
            new UserSeed(
                "lan.nguyen@culinary.local",
                "lannguyen",
                "Nguyễn Ngọc Lan",
                "Author",
                "Yêu ẩm thực Việt và những bữa cơm gia đình.",
                "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=200&q=80"),
            new UserSeed(
                "minh.tran@culinary.local",
                "minhtran",
                "Trần Hoàng Minh",
                "Author",
                "Đầu bếp tại gia, thích các món nhanh và lành mạnh.",
                "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=200&q=80")
        };

        var existing = await context.ApplicationUsers.ToListAsync();
        var usersByName = existing.ToDictionary(
            user => user.UserName,
            StringComparer.OrdinalIgnoreCase);
        var hasher = new PasswordHasher<ApplicationUser>();

        foreach (var seed in seeds)
        {
            if (usersByName.ContainsKey(seed.UserName))
            {
                continue;
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = seed.Email,
                UserName = seed.UserName,
                DisplayName = seed.DisplayName,
                AvatarUrl = seed.AvatarUrl,
                Bio = seed.Bio,
                Role = seed.Role,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = hasher.HashPassword(user, DevelopmentPassword);

            context.ApplicationUsers.Add(user);
            usersByName[user.UserName] = user;
        }

        await context.SaveChangesAsync();
        return usersByName;
    }

    private static async Task<Dictionary<string, Category>> EnsureCategoriesAsync(
        CulinaryBlogDbContext context)
    {
        var seeds = new[]
        {
            new CategorySeed("Món Việt", "mon-viet", "Các món ăn truyền thống từ ba miền Việt Nam.", 1),
            new CategorySeed("Món Á", "mon-a", "Ẩm thực đa dạng từ các quốc gia châu Á.", 2),
            new CategorySeed("Món Âu", "mon-au", "Những món ăn mang phong cách châu Âu.", 3),
            new CategorySeed("Món chay", "mon-chay", "Món chay thanh đạm, cân bằng và giàu dinh dưỡng.", 4),
            new CategorySeed("Tráng miệng", "trang-mieng", "Bánh và món ngọt dùng sau bữa ăn.", 5),
            new CategorySeed("Đồ uống", "do-uong", "Đồ uống nóng, lạnh và thức uống giải khát.", 6),
            new CategorySeed("Bữa sáng", "bua-sang", "Món ăn nhanh gọn cho một ngày mới.", 7),
            new CategorySeed("Món nhanh", "mon-nhanh", "Công thức đơn giản, phù hợp ngày bận rộn.", 8)
        };

        var existing = await context.Categories.ToListAsync();
        var categoriesBySlug = existing.ToDictionary(
            category => category.Slug,
            StringComparer.OrdinalIgnoreCase);

        foreach (var seed in seeds)
        {
            if (categoriesBySlug.ContainsKey(seed.Slug))
            {
                continue;
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = seed.Name,
                Slug = seed.Slug,
                Description = seed.Description,
                ImageUrl = "/images/image 1.png",
                OrderIndex = seed.OrderIndex,
                CreatedAt = DateTime.UtcNow
            };

            context.Categories.Add(category);
            categoriesBySlug[category.Slug] = category;
        }

        await context.SaveChangesAsync();
        return categoriesBySlug;
    }

    private static async Task EnsureRecipesAsync(
        CulinaryBlogDbContext context,
        IReadOnlyDictionary<string, ApplicationUser> users,
        IReadOnlyDictionary<string, Category> categories)
    {
        var seeds = GetRecipeSeeds();
        var existingSlugs = await context.Recipes
            .Select(recipe => recipe.Slug)
            .ToHashSetAsync();

        var createdIndex = 0;
        foreach (var seed in seeds)
        {
            if (existingSlugs.Contains(seed.Slug))
            {
                continue;
            }

            var recipeId = Guid.NewGuid();
            var recipe = new Recipe
            {
                Id = recipeId,
                Title = seed.Title,
                Slug = seed.Slug,
                Description = seed.Description,
                Instructions = seed.Instructions,
                PrepTimeMinutes = seed.PrepTimeMinutes,
                CookTimeMinutes = seed.CookTimeMinutes,
                Servings = seed.Servings,
                Difficulty = seed.Difficulty,
                Status = seed.Status,
                CategoryId = categories[seed.CategorySlug].Id,
                AuthorId = users[seed.AuthorUserName].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-(seeds.Count - createdIndex)),
                Nutrition = new RecipeNutrition
                {
                    Calories = seed.Calories,
                    Protein = seed.Protein,
                    Carbs = seed.Carbs,
                    Fat = seed.Fat,
                    Fiber = seed.Fiber,
                    Sodium = seed.Sodium
                }
            };

            for (var index = 0; index < seed.Ingredients.Count; index++)
            {
                var ingredient = seed.Ingredients[index];
                recipe.Ingredients.Add(new RecipeIngredient
                {
                    Id = Guid.NewGuid(),
                    RecipeId = recipeId,
                    Name = ingredient.Name,
                    Quantity = ingredient.Quantity,
                    Unit = ingredient.Unit,
                    Notes = ingredient.Notes,
                    OrderIndex = index,
                    CreatedAt = recipe.CreatedAt
                });
            }

            for (var index = 0; index < seed.Steps.Count; index++)
            {
                var step = seed.Steps[index];
                recipe.Steps.Add(new RecipeStep
                {
                    Id = Guid.NewGuid(),
                    RecipeId = recipeId,
                    StepNumber = index + 1,
                    Title = step.Title,
                    Description = step.Description,
                    TimerMinutes = step.TimerMinutes,
                    CreatedAt = recipe.CreatedAt
                });
            }

            recipe.Images.Add(new RecipeImage
            {
                Id = Guid.NewGuid(),
                RecipeId = recipeId,
                OriginalUrl = seed.ImageUrl,
                MediumUrl = seed.ImageUrl,
                ThumbnailUrl = seed.ImageUrl,
                AltText = seed.Title,
                IsPrimary = true,
                OrderIndex = 0,
                CreatedAt = recipe.CreatedAt
            });

            context.Recipes.Add(recipe);
            existingSlugs.Add(recipe.Slug);
            createdIndex++;
        }

        await context.SaveChangesAsync();
    }

    private static IReadOnlyList<RecipeSeed> GetRecipeSeeds() =>
    [
        new RecipeSeed(
            "Phở bò Hà Nội", "pho-bo-ha-noi",
            "Nước dùng trong, thơm mùi quế hồi, ăn cùng bánh phở và thịt bò mềm.",
            "Ninh xương lấy nước dùng, nêm gia vị, chần bánh phở rồi xếp thịt bò và chan nước dùng nóng.",
            "mon-viet", "lannguyen", 30, 150, 4,
            DifficultyLevel.Medium, RecipeStatus.Published,
            520, 32, 58, 16, 4, 920,
            "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?auto=format&fit=crop&w=1200&q=80",
            [
                new("Xương bò", "1", "kg", "Rửa sạch và chần sơ"),
                new("Bánh phở", "600", "g", null),
                new("Thịt bò", "400", "g", "Thái mỏng"),
                new("Hành tây", "1", "củ", "Nướng thơm"),
                new("Quế và hoa hồi", null, null, "Dùng lượng vừa đủ để tạo hương")
            ],
            [
                new("Ninh xương", "Chần xương, rửa sạch rồi ninh lửa nhỏ để lấy nước dùng trong.", 120),
                new("Tạo hương", "Nướng hành, gừng, quế và hoa hồi rồi cho vào nồi nước dùng.", 15),
                new("Chuẩn bị tô", "Chần bánh phở, xếp thịt bò và hành lá vào tô.", 5),
                new("Hoàn thiện", "Chan nước dùng đang sôi, dùng ngay với rau thơm và chanh.", 2)
            ]),
        new RecipeSeed(
            "Cơm tấm sườn nướng", "com-tam-suon-nuong",
            "Cơm tấm Sài Gòn với sườn nướng đậm vị, mỡ hành và nước mắm chua ngọt.",
            "Ướp sườn, nướng vàng hai mặt, nấu cơm tấm và pha nước mắm để dùng kèm.",
            "mon-viet", "minhtran", 25, 35, 4,
            DifficultyLevel.Medium, RecipeStatus.Published,
            680, 38, 76, 24, 3, 1050,
            "https://images.unsplash.com/photo-1547592180-85f173990554?auto=format&fit=crop&w=1200&q=80",
            [
                new("Sườn cốt lết", "600", "g", null),
                new("Gạo tấm", "400", "g", null),
                new("Sả băm", "2", "thìa", null),
                new("Nước mắm", "4", "thìa", null),
                new("Hành lá", "50", "g", "Thái nhỏ")
            ],
            [
                new("Ướp sườn", "Ướp sườn với sả, nước mắm, đường và tiêu.", 20),
                new("Nấu cơm", "Vo gạo tấm và nấu với lượng nước vừa đủ.", 25),
                new("Nướng sườn", "Nướng sườn đến khi vàng cạnh và chín đều.", 15),
                new("Trình bày", "Cho cơm ra đĩa, thêm sườn, mỡ hành và nước mắm.", 5)
            ]),
        new RecipeSeed(
            "Gỏi cuốn tôm thịt", "goi-cuon-tom-thit",
            "Gỏi cuốn thanh mát với tôm, thịt, bún và rau sống, ăn cùng sốt đậu phộng.",
            "Luộc tôm thịt, chuẩn bị rau bún rồi cuốn chặt tay bằng bánh tráng đã làm mềm.",
            "mon-viet", "lannguyen", 25, 20, 4,
            DifficultyLevel.Easy, RecipeStatus.Published,
            310, 22, 38, 8, 5, 540,
            "https://images.unsplash.com/photo-1559314809-0d155014e29e?auto=format&fit=crop&w=1200&q=80",
            [
                new("Tôm", "300", "g", "Luộc chín và bóc vỏ"),
                new("Thịt ba chỉ", "250", "g", "Luộc chín và thái mỏng"),
                new("Bún tươi", "300", "g", null),
                new("Bánh tráng", "12", "cái", null),
                new("Rau sống", null, null, "Xà lách, hẹ và rau thơm")
            ],
            [
                new("Luộc nguyên liệu", "Luộc tôm và thịt, để nguội rồi thái vừa ăn.", 15),
                new("Chuẩn bị", "Rửa rau, để ráo và chia bún thành từng phần.", 10),
                new("Cuốn", "Làm mềm bánh tráng, xếp nguyên liệu và cuốn chặt tay.", 10),
                new("Pha sốt", "Khuấy sốt đậu phộng với chút nước ấm đến khi sánh.", 5)
            ]),
        new RecipeSeed(
            "Mì xào rau củ", "mi-xao-rau-cu",
            "Món mì xào nhanh với rau củ giòn ngọt, phù hợp cho bữa tối bận rộn.",
            "Trụng mì, xào rau trên lửa lớn rồi cho mì và sốt vào đảo đều.",
            "mon-a", "minhtran", 15, 15, 2,
            DifficultyLevel.Easy, RecipeStatus.Published,
            430, 14, 68, 12, 8, 710,
            "https://images.unsplash.com/photo-1612927601601-6638404737ce?auto=format&fit=crop&w=1200&q=80",
            [
                new("Mì trứng", "250", "g", null),
                new("Cải thìa", "200", "g", null),
                new("Cà rốt", "1", "củ", "Thái sợi"),
                new("Nấm", "150", "g", "Thái lát"),
                new("Nước tương", "2", "thìa", null)
            ],
            [
                new("Trụng mì", "Trụng mì vừa mềm, xả nhanh với nước mát và để ráo.", 4),
                new("Xào rau", "Làm nóng chảo, xào cà rốt, nấm và cải thìa trên lửa lớn.", 6),
                new("Trộn mì", "Cho mì và nước tương vào chảo, đảo đều.", 3),
                new("Hoàn thiện", "Nêm lại gia vị và dùng khi còn nóng.", 2)
            ]),
        new RecipeSeed(
            "Salad đậu hũ mè rang", "salad-dau-hu-me-rang",
            "Salad chay giàu đạm với đậu hũ áp chảo, rau xanh và sốt mè rang.",
            "Áp chảo đậu hũ, trộn rau và rưới sốt mè ngay trước khi dùng.",
            "mon-chay", "lannguyen", 15, 10, 2,
            DifficultyLevel.Easy, RecipeStatus.Published,
            280, 16, 22, 15, 7, 430,
            "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=1200&q=80",
            [
                new("Đậu hũ", "300", "g", "Cắt miếng vuông"),
                new("Xà lách", "150", "g", null),
                new("Cà chua bi", "120", "g", "Cắt đôi"),
                new("Bắp ngọt", "100", "g", null),
                new("Sốt mè rang", "3", "thìa", null)
            ],
            [
                new("Áp chảo đậu", "Áp chảo đậu hũ đến khi vàng các mặt.", 8),
                new("Chuẩn bị rau", "Rửa rau, để thật ráo rồi cho vào tô lớn.", 5),
                new("Trộn salad", "Thêm đậu hũ, bắp, cà chua và sốt mè rang.", 2)
            ]),
        new RecipeSeed(
            "Bánh flan caramel", "banh-flan-caramel",
            "Bánh flan mềm mịn, thơm trứng sữa với lớp caramel óng đẹp.",
            "Nấu caramel, pha hỗn hợp trứng sữa rồi hấp cách thủy ở lửa nhỏ.",
            "trang-mieng", "minhtran", 20, 35, 6,
            DifficultyLevel.Medium, RecipeStatus.Published,
            220, 7, 32, 8, 0, 95,
            "https://images.unsplash.com/photo-1578985545062-69928b1d9587?auto=format&fit=crop&w=1200&q=80",
            [
                new("Trứng gà", "4", "quả", null),
                new("Sữa tươi", "500", "ml", null),
                new("Đường", "120", "g", null),
                new("Vani", "1", "thìa", null)
            ],
            [
                new("Nấu caramel", "Đun đường với ít nước đến màu cánh gián rồi đổ vào khuôn.", 8),
                new("Pha trứng sữa", "Khuấy nhẹ trứng với sữa ấm và vani, sau đó lọc mịn.", 10),
                new("Hấp bánh", "Đổ hỗn hợp vào khuôn và hấp lửa nhỏ.", 30),
                new("Làm lạnh", "Để nguội rồi làm lạnh ít nhất hai giờ trước khi dùng.", null)
            ]),
        new RecipeSeed(
            "Cà phê sữa đá", "ca-phe-sua-da",
            "Cà phê phin đậm vị kết hợp sữa đặc và đá lạnh theo phong cách Việt Nam.",
            "Tráng phin, ủ cà phê, pha với sữa đặc rồi thêm đá.",
            "do-uong", "lannguyen", 5, 7, 1,
            DifficultyLevel.Easy, RecipeStatus.Published,
            160, 4, 26, 5, 0, 70,
            "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?auto=format&fit=crop&w=1200&q=80",
            [
                new("Cà phê xay", "25", "g", "Loại rang vừa hoặc rang đậm"),
                new("Sữa đặc", "35", "ml", null),
                new("Nước nóng", "80", "ml", null),
                new("Đá viên", null, null, "Dùng đủ đầy một ly")
            ],
            [
                new("Tráng phin", "Tráng phin bằng nước nóng rồi cho cà phê vào.", 1),
                new("Ủ cà phê", "Rót ít nước nóng, ủ rồi châm phần nước còn lại.", 6),
                new("Pha sữa", "Khuấy cà phê với sữa đặc.", 1),
                new("Thêm đá", "Rót hỗn hợp vào ly đá và dùng ngay.", 1)
            ]),
        new RecipeSeed(
            "Bánh mì trứng ốp la", "banh-mi-trung-op-la",
            "Bữa sáng nhanh với bánh mì giòn, trứng lòng đào và rau dưa tươi.",
            "Ốp trứng, làm nóng bánh mì rồi kẹp cùng rau, dưa leo và tương ớt.",
            "bua-sang", "minhtran", 5, 8, 2,
            DifficultyLevel.Easy, RecipeStatus.Published,
            390, 18, 46, 15, 4, 620,
            "https://images.unsplash.com/photo-1525351484163-7529414344d8?auto=format&fit=crop&w=1200&q=80",
            [
                new("Bánh mì", "2", "ổ", null),
                new("Trứng gà", "2", "quả", null),
                new("Dưa leo", "1", "quả", "Thái lát"),
                new("Rau mùi", null, null, "Rửa sạch và để ráo"),
                new("Tương ớt", "2", "thìa", null)
            ],
            [
                new("Làm nóng bánh", "Nướng bánh mì đến khi vỏ giòn nhẹ.", 3),
                new("Ốp trứng", "Chiên trứng đến độ chín mong muốn.", 4),
                new("Hoàn thiện", "Kẹp trứng, dưa leo, rau mùi và tương ớt vào bánh.", 2)
            ])
    ];

    private sealed record UserSeed(
        string Email,
        string UserName,
        string DisplayName,
        string Role,
        string Bio,
        string AvatarUrl);

    private sealed record CategorySeed(
        string Name,
        string Slug,
        string Description,
        int OrderIndex);

    private sealed record IngredientSeed(
        string Name,
        string? Quantity,
        string? Unit,
        string? Notes);

    private sealed record StepSeed(
        string Title,
        string Description,
        int? TimerMinutes);

    private sealed record RecipeSeed(
        string Title,
        string Slug,
        string Description,
        string Instructions,
        string CategorySlug,
        string AuthorUserName,
        int PrepTimeMinutes,
        int CookTimeMinutes,
        int Servings,
        DifficultyLevel Difficulty,
        RecipeStatus Status,
        decimal Calories,
        decimal Protein,
        decimal Carbs,
        decimal Fat,
        decimal Fiber,
        decimal Sodium,
        string ImageUrl,
        IReadOnlyList<IngredientSeed> Ingredients,
        IReadOnlyList<StepSeed> Steps);
}
