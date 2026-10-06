using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;

public sealed class SearchRecipesQueryHandler
	: IRequestHandler<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>
{
	private readonly IRecipeRepository _recipeRepository;

	public SearchRecipesQueryHandler(IRecipeRepository recipeRepository)
	{
		_recipeRepository = recipeRepository;
	}

	public async Task<PagedResult<RecipeSummaryDto>> Handle(
		SearchRecipesQuery request,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.SearchTerm))
		{
			throw new ValidationException("q is required.");
		}

		var result = await _recipeRepository.SearchAsync(
			request.SearchTerm.Trim(),
			request.Page,
			request.PageSize,
			request.Sort,
			cancellationToken);

		var items = result.Items
			.Select(recipe => new RecipeSummaryDto(
				recipe.Id,
				recipe.Title,
				recipe.Slug,
				recipe.Images
					.OrderByDescending(image => image.IsPrimary)
					.ThenBy(image => image.OrderIndex)
					.Select(image => image.ThumbnailUrl ?? image.MediumUrl ?? image.OriginalUrl)
					.FirstOrDefault(),
				recipe.Category.Name,
				recipe.CategoryId,
				recipe.PrepTimeMinutes,
				recipe.CookTimeMinutes,
				recipe.Servings,
				recipe.Difficulty,
				recipe.Status,
				recipe.CreatedAt))
			.ToList();

		return new PagedResult<RecipeSummaryDto>(
			items,
			result.TotalCount,
			request.Page,
			request.PageSize);
	}
}
