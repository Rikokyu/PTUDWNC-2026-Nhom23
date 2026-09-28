using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;

public sealed class GetRecipeBySlugQueryHandler
    : IRequestHandler<
        GetRecipeBySlugQuery,
        RecipeDetailDto>
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ICurrentUser _currentUser;

    public GetRecipeBySlugQueryHandler(
        IRecipeRepository recipeRepository,
        ICurrentUser currentUser)
    {
        _recipeRepository = recipeRepository;
        _currentUser = currentUser;
    }

    public async Task<RecipeDetailDto> Handle(
        GetRecipeBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var recipe =
            await _recipeRepository
                .GetBySlugWithDetailsAsync(
                    request.Slug,
                    cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(
                $"Recipe with slug '{request.Slug}' was not found.");
        }

        // Draft / Archived authorization
        if (recipe.Status != RecipeStatus.Published)
        {
            var isOwner =
                _currentUser.UserId.HasValue
                && recipe.AuthorId.HasValue
                && recipe.AuthorId.Value ==
                   _currentUser.UserId.Value;

            if (!_currentUser.IsAdmin && !isOwner)
            {
                throw new ForbiddenException(
                    "You do not have permission to view this recipe.");
            }
        }

        var ingredients =
            recipe.Ingredients
                .OrderBy(x => x.OrderIndex)
                .Select(x =>
                    new RecipeIngredientDto(
                        x.Id,
                        x.Name,
                        x.Quantity,
                        x.Unit,
                        x.Notes,
                        x.OrderIndex))
                .ToList();

        var steps =
            recipe.Steps
                .OrderBy(x => x.StepNumber)
                .Select(x =>
                    new RecipeStepDto(
                        x.Id,
                        x.StepNumber,
                        x.Title,
                        x.Description,
                        x.TimerMinutes,
                        x.ImageUrl))
                .ToList();

        var images =
            recipe.Images
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.OrderIndex)
                .Select(x =>
                    new RecipeImageDto(
                        x.Id,
                        x.OriginalUrl,
                        x.MediumUrl,
                        x.ThumbnailUrl,
                        x.AltText,
                        x.IsPrimary,
                        x.OrderIndex))
                .ToList();

        RecipeAuthorDto? author = null;

        if (recipe.Author != null)
        {
            author =
                new RecipeAuthorDto(
                    recipe.Author.Id,
                    recipe.Author.DisplayName,
                    recipe.Author.Email);
        }

        RecipeNutritionDto? nutrition = null;

        if (recipe.Nutrition != null)
        {
            nutrition =
                new RecipeNutritionDto(
                    recipe.Nutrition.Calories,
                    recipe.Nutrition.Protein,
                    recipe.Nutrition.Carbs,
                    recipe.Nutrition.Fat,
                    recipe.Nutrition.Fiber,
                    recipe.Nutrition.Sodium);
        }

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