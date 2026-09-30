using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        // Sử dụng context.Set<Category>() trực tiếp qua DbContext tổng
        if (!await context.Set<Category>().AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Id = Guid.NewGuid(), Name = "Món Khai Vị", Description = "Các món ăn nhẹ đầu bữa" },
                new() { Id = Guid.NewGuid(), Name = "Món Chính", Description = "Các món ăn đậm đà cho bữa chính" },
                new() { Id = Guid.NewGuid(), Name = "Món Tráng Miệng", Description = "Bánh ngọt, chè và trái cây" },
                new() { Id = Guid.NewGuid(), Name = "Món Chay", Description = "Thực phẩm thanh tịnh từ thực vật" },
                new() { Id = Guid.NewGuid(), Name = "Món Á", Description = "Hương vị truyền thống phương Đông" },
                new() { Id = Guid.NewGuid(), Name = "Món Âu", Description = "Phong cách ẩm thực phương Tây" },
                new() { Id = Guid.NewGuid(), Name = "Đồ Uống & Pha Chế", Description = "Trà, cà phê, sinh tố và cocktail" },
                new() { Id = Guid.NewGuid(), Name = "Món Nướng & BBQ", Description = "Các món nướng thơm ngon" },
                new() { Id = Guid.NewGuid(), Name = "Món Hấp & Tần", Description = "Các món ăn bổ dưỡng giữ trọn vị" },
                new() { Id = Guid.NewGuid(), Name = "Món Xào", Description = "Món xào nhanh đậm đà gia vị" },
                new() { Id = Guid.NewGuid(), Name = "Món Canh & Lẩu", Description = "Các món nước thanh ngọt" },
                new() { Id = Guid.NewGuid(), Name = "Bánh Ngọt & Bánh Mì", Description = "Các loại bánh nướng hấp dẫn" },
                new() { Id = Guid.NewGuid(), Name = "Ẩm Thực Mẹ Nấu", Description = "Món ăn gia đình đầm ấm" },
                new() { Id = Guid.NewGuid(), Name = "Món Ăn Vặt", Description = "Đồ ăn nhẹ đường phố" },
                new() { Id = Guid.NewGuid(), Name = "Hải Sản", Description = "Các món chế biến từ tôm, cá, mực" },
                new() { Id = Guid.NewGuid(), Name = "Món Thịt Bò", Description = "Công thức chế biến thịt bò" },
                new() { Id = Guid.NewGuid(), Name = "Món Thịt Gà", Description = "Món ngon dễ làm từ thịt gà" },
                new() { Id = Guid.NewGuid(), Name = "Món Thịt Heo", Description = "Các món ăn quen thuộc từ thịt heo" },
                new() { Id = Guid.NewGuid(), Name = "Salad & Gỏi", Description = "Các món trộn thanh mát, chống ngấy" },
                new() { Id = Guid.NewGuid(), Name = "Món Nhanh & Tiện Lợi", Description = "Công thức nấu dưới 15 phút" }
            };

            await context.Set<Category>().AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}