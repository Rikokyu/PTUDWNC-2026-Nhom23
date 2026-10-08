using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Features.Recipes;

internal static class RecipeDtoMapper
{
    public static RecipeDetailDto ToDetailDto(Recipe recipe)
    {
        var ingredients = recipe.Ingredients
            .OrderBy(ingredient => ingredient.OrderIndex)
            .Select(ingredient => new RecipeIngredientDto(
                ingredient.Id,
                ingredient.Name,
                ingredient.Quantity,
                ingredient.Unit,
                ingredient.Notes,
                ingredient.OrderIndex))
            .ToList();
        var steps = recipe.Steps
            .OrderBy(step => step.StepNumber)
            .Select(step => new RecipeStepDto(
                step.Id,
                step.StepNumber,
                step.Title,
                step.Description,
                step.TimerMinutes,
                step.ImageUrl))
            .ToList();
        var images = recipe.Images
            .OrderByDescending(image => image.IsPrimary)
            .ThenBy(image => image.OrderIndex)
            .Select(image => new RecipeImageDto(
                image.Id,
                image.OriginalUrl,
                image.MediumUrl,
                image.ThumbnailUrl,
                image.AltText,
                image.IsPrimary,
                image.OrderIndex))
            .ToList();
        var author = recipe.Author is null
            ? null
            : new RecipeAuthorDto(
                recipe.Author.Id,
                recipe.Author.DisplayName,
                recipe.Author.Email);
        var nutrition = recipe.Nutrition is null
            ? null
            : new RecipeNutritionDto(
                recipe.Nutrition.Calories,
                recipe.Nutrition.Protein,
                recipe.Nutrition.Carbs,
                recipe.Nutrition.Fat,
                recipe.Nutrition.Fiber,
                recipe.Nutrition.Sodium);

        return new RecipeDetailDto(
            recipe.Id,
            recipe.Title,
            recipe.Slug,
            recipe.Description,
            recipe.Instructions,
            recipe.PrepTimeMinutes,
            recipe.CookTimeMinutes,
            recipe.Servings,
            recipe.Difficulty,
            recipe.Status,
            recipe.CreatedAt,
            new RecipeCategoryDto(
                recipe.Category.Id,
                recipe.Category.Name,
                recipe.Category.Slug),
            author,
            ingredients,
            steps,
            images,
            nutrition);
    }
}
