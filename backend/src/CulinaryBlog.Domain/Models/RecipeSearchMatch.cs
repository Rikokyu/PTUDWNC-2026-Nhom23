using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Domain.Models;

public sealed record RecipeSearchMatch(Recipe Recipe, float RelevanceScore);
