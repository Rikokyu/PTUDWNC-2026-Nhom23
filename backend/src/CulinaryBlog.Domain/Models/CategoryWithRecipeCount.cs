using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Domain.Models;

public sealed record CategoryWithRecipeCount(
    Category Category,
    int RecipeCount);
