using System.Globalization;
using System.Text;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;

namespace CulinaryBlog.Application.Features.Recipes.Commands;

public sealed class RecipeManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IFileStorageService _fileStorage;

    public RecipeManagementService(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<RecipeDetailDto> CreateAsync(
        CreateRecipeRequest request,
        CancellationToken cancellationToken)
    {
        EnsureAuthenticatedAuthor();
        ValidateRecipe(request.Title, request.Description, request.PrepTimeMinutes,
            request.CookTimeMinutes, request.Servings, request.Difficulty, request.Nutrition);

        var category = await _unitOfWork.Categories.GetByIdAsync(
            request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category was not found.");
        var now = DateTime.UtcNow;
        var title = request.Title.Trim();
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = await CreateUniqueSlugAsync(title, cancellationToken),
            Description = request.Description.Trim(),
            Instructions = string.Empty,
            PrepTimeMinutes = request.PrepTimeMinutes,
            CookTimeMinutes = request.CookTimeMinutes,
            Servings = request.Servings,
            Difficulty = request.Difficulty,
            Status = RecipeStatus.Draft,
            CategoryId = category.Id,
            Category = category,
            AuthorId = _currentUser.UserId,
            Nutrition = MapNutrition(request.Nutrition),
            CreatedAt = now
        };

        foreach (var input in request.Ingredients ?? [])
        {
            ValidateIngredient(input);
            recipe.Ingredients.Add(new RecipeIngredient
            {
                Id = Guid.NewGuid(),
                Name = input.Name.Trim(),
                Quantity = Normalize(input.Quantity),
                Unit = Normalize(input.Unit),
                Notes = Normalize(input.Notes),
                OrderIndex = input.OrderIndex ?? recipe.Ingredients.Count,
                CreatedAt = now
            });
        }

        foreach (var input in request.Steps ?? [])
        {
            ValidateStep(input);
            recipe.Steps.Add(new RecipeStep
            {
                Id = Guid.NewGuid(),
                StepNumber = recipe.Steps.Count + 1,
                Title = input.Title.Trim(),
                Description = input.Description.Trim(),
                TimerMinutes = input.TimerMinutes,
                ImageUrl = Normalize(input.ImageUrl),
                CreatedAt = now
            });
        }

        await _unitOfWork.Recipes.AddAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDetail(recipe);
    }

    public async Task<RecipeDetailDto> UpdateAsync(
        Guid recipeId,
        UpdateRecipeRequest request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        if (request.Title is not null)
        {
            if (request.Title.Trim().Length is < 3 or > 200)
            {
                throw new ValidationException("title must be 3-200 characters.");
            }

            recipe.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                throw new ValidationException("description cannot be empty.");
            }

            recipe.Description = request.Description.Trim();
        }

        if (request.CategoryId.HasValue)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(
                request.CategoryId.Value, cancellationToken)
                ?? throw new NotFoundException("Category was not found.");
            recipe.CategoryId = category.Id;
            recipe.Category = category;
        }

        if (request.PrepTimeMinutes.HasValue)
        {
            if (request.PrepTimeMinutes < 0)
            {
                throw new ValidationException("prepTimeMinutes cannot be negative.");
            }

            recipe.PrepTimeMinutes = request.PrepTimeMinutes.Value;
        }

        if (request.CookTimeMinutes.HasValue)
        {
            if (request.CookTimeMinutes < 0)
            {
                throw new ValidationException("cookTimeMinutes cannot be negative.");
            }

            recipe.CookTimeMinutes = request.CookTimeMinutes.Value;
        }

        if (request.Servings.HasValue)
        {
            if (request.Servings < 1)
            {
                throw new ValidationException("servings must be greater than zero.");
            }

            recipe.Servings = request.Servings.Value;
        }

        if (request.Difficulty.HasValue)
        {
            EnsureDifficulty(request.Difficulty.Value);
            recipe.Difficulty = request.Difficulty.Value;
        }

        if (request.Nutrition is not null)
        {
            ValidateNutrition(request.Nutrition);
            recipe.Nutrition = MapNutrition(request.Nutrition);
        }

        recipe.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Recipes.Update(recipe);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDetail(recipe);
    }

    public async Task<RecipeDetailDto> SetStatusAsync(
        Guid recipeId,
        RecipeStatus status,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        if (status == RecipeStatus.Published && recipe.Steps.Count == 0)
        {
            throw new ValidationException("A recipe must have at least one step before publishing.");
        }

        recipe.Status = status;
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDetail(recipe);
    }

    public async Task DeleteAsync(Guid recipeId, CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        recipe.IsDeleted = true;
        recipe.UpdatedAt = DateTime.UtcNow;
        foreach (var item in recipe.Ingredients) item.IsDeleted = true;
        foreach (var item in recipe.Steps) item.IsDeleted = true;
        foreach (var item in recipe.Images) item.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<RecipeIngredientDto> AddIngredientAsync(
        Guid recipeId,
        RecipeIngredientInput input,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        ValidateIngredient(input);
        var ingredient = new RecipeIngredient
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            Name = input.Name.Trim(),
            Quantity = Normalize(input.Quantity),
            Unit = Normalize(input.Unit),
            Notes = Normalize(input.Notes),
            OrderIndex = input.OrderIndex
                ?? recipe.Ingredients.Select(item => item.OrderIndex).DefaultIfEmpty(-1).Max() + 1,
            CreatedAt = DateTime.UtcNow
        };
        recipe.Ingredients.Add(ingredient);
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToIngredientDto(ingredient);
    }

    public async Task<RecipeIngredientDto> UpdateIngredientAsync(
        Guid recipeId,
        Guid ingredientId,
        RecipeIngredientInput input,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        var ingredient = recipe.Ingredients.FirstOrDefault(item => item.Id == ingredientId)
            ?? throw new NotFoundException("Recipe ingredient was not found.");
        ValidateIngredient(input);
        ingredient.Name = input.Name.Trim();
        ingredient.Quantity = Normalize(input.Quantity);
        ingredient.Unit = Normalize(input.Unit);
        ingredient.Notes = Normalize(input.Notes);
        if (input.OrderIndex.HasValue) ingredient.OrderIndex = input.OrderIndex.Value;
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToIngredientDto(ingredient);
    }

    public async Task DeleteIngredientAsync(
        Guid recipeId,
        Guid ingredientId,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        var ingredient = recipe.Ingredients.FirstOrDefault(item => item.Id == ingredientId)
            ?? throw new NotFoundException("Recipe ingredient was not found.");
        ingredient.IsDeleted = true;
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<RecipeStepDto> AddStepAsync(
        Guid recipeId,
        RecipeStepInput input,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        ValidateStep(input);
        var step = new RecipeStep
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            StepNumber = recipe.Steps.Select(item => item.StepNumber).DefaultIfEmpty(0).Max() + 1,
            Title = input.Title.Trim(),
            Description = input.Description.Trim(),
            TimerMinutes = input.TimerMinutes,
            ImageUrl = Normalize(input.ImageUrl),
            CreatedAt = DateTime.UtcNow
        };
        recipe.Steps.Add(step);
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToStepDto(step);
    }

    public async Task<RecipeStepDto> UpdateStepAsync(
        Guid recipeId,
        Guid stepId,
        RecipeStepInput input,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        var step = recipe.Steps.FirstOrDefault(item => item.Id == stepId)
            ?? throw new NotFoundException("Recipe step was not found.");
        ValidateStep(input);
        step.Title = input.Title.Trim();
        step.Description = input.Description.Trim();
        step.TimerMinutes = input.TimerMinutes;
        step.ImageUrl = Normalize(input.ImageUrl);
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToStepDto(step);
    }

    public async Task DeleteStepAsync(
        Guid recipeId,
        Guid stepId,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        var step = recipe.Steps.FirstOrDefault(item => item.Id == stepId)
            ?? throw new NotFoundException("Recipe step was not found.");
        step.IsDeleted = true;
        var remaining = recipe.Steps.Where(item => item.Id != stepId)
            .OrderBy(item => item.StepNumber).ToList();
        for (var index = 0; index < remaining.Count; index++)
        {
            remaining[index].StepNumber = index + 1;
        }

        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<RecipeImageDto> UploadImageAsync(
        Guid recipeId,
        Stream content,
        string objectKey,
        long size,
        string contentType,
        string? altText,
        bool makePrimary,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        ValidateAltText(altText);
        var publicUrl = await _fileStorage.UploadAsync(
            content, objectKey, size, contentType, cancellationToken);
        var isPrimary = makePrimary || recipe.Images.Count == 0;
        if (isPrimary)
        {
            foreach (var existingImage in recipe.Images)
            {
                existingImage.IsPrimary = false;
            }
        }

        var image = new RecipeImage
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            OriginalUrl = publicUrl,
            ObjectKey = objectKey,
            AltText = Normalize(altText),
            IsPrimary = isPrimary,
            OrderIndex = recipe.Images.Select(item => item.OrderIndex)
                .DefaultIfEmpty(-1).Max() + 1,
            CreatedAt = DateTime.UtcNow
        };
        recipe.Images.Add(image);
        _unitOfWork.Recipes.AddImage(image);
        recipe.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _fileStorage.DeleteAsync(objectKey, cancellationToken);
            throw;
        }

        return ToImageDto(image);
    }

    public async Task<RecipeImageDto> SetPrimaryImageAsync(
        Guid recipeId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        var image = recipe.Images.FirstOrDefault(item => item.Id == imageId)
            ?? throw new NotFoundException("Recipe image was not found.");
        foreach (var existingImage in recipe.Images)
        {
            existingImage.IsPrimary = existingImage.Id == image.Id;
        }

        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToImageDto(image);
    }

    public async Task<RecipeImageDto> UpdateImageMetadataAsync(
        Guid recipeId,
        Guid imageId,
        RecipeImageMetadataInput input,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        var image = recipe.Images.FirstOrDefault(item => item.Id == imageId)
            ?? throw new NotFoundException("Recipe image was not found.");
        ValidateAltText(input.AltText);
        if (input.OrderIndex < 0)
        {
            throw new ValidationException("orderIndex cannot be negative.");
        }

        if (input.AltText is not null) image.AltText = Normalize(input.AltText);
        if (input.OrderIndex.HasValue) image.OrderIndex = input.OrderIndex.Value;
        if (input.IsPrimary == true)
        {
            foreach (var existingImage in recipe.Images)
            {
                existingImage.IsPrimary = existingImage.Id == image.Id;
            }
        }
        else if (input.IsPrimary == false)
        {
            image.IsPrimary = false;
        }

        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToImageDto(image);
    }

    public async Task DeleteImageAsync(
        Guid recipeId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var recipe = await GetManageableRecipeAsync(recipeId, cancellationToken);
        var image = recipe.Images.FirstOrDefault(item => item.Id == imageId)
            ?? throw new NotFoundException("Recipe image was not found.");
        if (image.ObjectKey is not null)
        {
            await _fileStorage.DeleteAsync(image.ObjectKey, cancellationToken);
        }

        var wasPrimary = image.IsPrimary;
        image.IsDeleted = true;
        image.IsPrimary = false;
        if (wasPrimary)
        {
            var nextPrimary = recipe.Images
                .Where(item => item.Id != imageId)
                .OrderBy(item => item.OrderIndex)
                .FirstOrDefault();
            if (nextPrimary is not null) nextPrimary.IsPrimary = true;
        }

        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Recipe> GetManageableRecipeAsync(
        Guid recipeId,
        CancellationToken cancellationToken)
    {
        var recipe = await _unitOfWork.Recipes.GetByIdWithDetailsForUpdateAsync(
            recipeId, cancellationToken)
            ?? throw new NotFoundException("Recipe was not found.");
        if (!_currentUser.IsAdmin
            && (!_currentUser.UserId.HasValue || recipe.AuthorId != _currentUser.UserId.Value))
        {
            throw new ForbiddenException("You do not have permission to manage this recipe.");
        }

        return recipe;
    }

    private void EnsureAuthenticatedAuthor()
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            throw new UnauthorizedException("Authentication is required.");
        }
    }

    private async Task<string> CreateUniqueSlugAsync(
        string title,
        CancellationToken cancellationToken)
    {
        var baseSlug = CreateSlug(title);
        if (baseSlug.Length == 0) baseSlug = "recipe";
        var slug = baseSlug;
        for (var suffix = 2; await _unitOfWork.Recipes.SlugExistsAsync(slug, cancellationToken); suffix++)
        {
            slug = $"{baseSlug}-{suffix}";
        }

        return slug;
    }

    private static string CreateSlug(string value)
    {
        var normalized = value.Replace('đ', 'd').Replace('Đ', 'D')
            .Normalize(NormalizationForm.FormD);
        var slug = new StringBuilder();
        var separatorPending = false;
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(character))
            {
                if (separatorPending && slug.Length > 0) slug.Append('-');
                slug.Append(char.ToLowerInvariant(character));
                separatorPending = false;
            }
            else
            {
                separatorPending = true;
            }
        }

        return slug.ToString();
    }

    private static void ValidateRecipe(
        string title,
        string description,
        int prepTime,
        int cookTime,
        int servings,
        DifficultyLevel difficulty,
        RecipeNutritionInput? nutrition)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200
            || string.IsNullOrWhiteSpace(description)
            || prepTime < 0 || cookTime < 0 || servings < 1)
            throw new ValidationException("Recipe fields are invalid.");
        EnsureDifficulty(difficulty);
        if (nutrition is not null) ValidateNutrition(nutrition);
    }

    private static void EnsureDifficulty(DifficultyLevel difficulty)
    {
        if (!Enum.IsDefined(difficulty))
            throw new ValidationException("difficulty is invalid.");
    }

    private static void ValidateNutrition(RecipeNutritionInput value)
    {
        if (new[] { value.Calories, value.Protein, value.Carbs, value.Fat, value.Fiber, value.Sodium }
            .Any(number => number < 0))
            throw new ValidationException("Nutrition values cannot be negative.");
    }

    private static void ValidateIngredient(RecipeIngredientInput value)
    {
        if (string.IsNullOrWhiteSpace(value.Name) || value.Name.Trim().Length > 200
            || value.Quantity?.Length > 100 || value.Unit?.Length > 50
            || value.Notes?.Length > 500 || value.OrderIndex < 0)
            throw new ValidationException("Recipe ingredient fields are invalid.");
    }

    private static void ValidateStep(RecipeStepInput value)
    {
        if (string.IsNullOrWhiteSpace(value.Title) || value.Title.Trim().Length > 200
            || string.IsNullOrWhiteSpace(value.Description)
            || value.Description.Trim().Length > 2000 || value.TimerMinutes < 0)
            throw new ValidationException("Recipe step fields are invalid.");
        if (value.ImageUrl is not null
            && (!Uri.TryCreate(value.ImageUrl, UriKind.Absolute, out var uri)
                || uri.Scheme is not ("http" or "https")))
            throw new ValidationException("imageUrl must be an HTTP or HTTPS URL.");
    }

    private static void ValidateAltText(string? altText)
    {
        if (altText?.Length > 200)
        {
            throw new ValidationException("altText cannot exceed 200 characters.");
        }
    }

    private static RecipeNutrition? MapNutrition(RecipeNutritionInput? value) =>
        value is null ? null : new RecipeNutrition
        {
            Calories = value.Calories,
            Protein = value.Protein,
            Carbs = value.Carbs,
            Fat = value.Fat,
            Fiber = value.Fiber,
            Sodium = value.Sodium
        };

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static RecipeIngredientDto ToIngredientDto(RecipeIngredient value) =>
        new(value.Id, value.Name, value.Quantity, value.Unit, value.Notes, value.OrderIndex);

    private static RecipeStepDto ToStepDto(RecipeStep value) =>
        new(value.Id, value.StepNumber, value.Title, value.Description, value.TimerMinutes, value.ImageUrl);

    private static RecipeImageDto ToImageDto(RecipeImage value) =>
        new(value.Id, value.OriginalUrl, value.MediumUrl, value.ThumbnailUrl,
            value.AltText, value.IsPrimary, value.OrderIndex);

    private static RecipeDetailDto ToDetail(Recipe recipe) => new(
        recipe.Id, recipe.Title, recipe.Slug, recipe.Description, recipe.Instructions,
        recipe.PrepTimeMinutes, recipe.CookTimeMinutes, recipe.Servings, recipe.Difficulty,
        recipe.Status, recipe.CreatedAt,
        new RecipeCategoryDto(recipe.Category.Id, recipe.Category.Name, recipe.Category.Slug),
        recipe.Author is null ? null : new RecipeAuthorDto(
            recipe.Author.Id, recipe.Author.DisplayName, recipe.Author.Email),
        recipe.Ingredients.Where(item => !item.IsDeleted).OrderBy(item => item.OrderIndex)
            .Select(ToIngredientDto).ToList(),
        recipe.Steps.Where(item => !item.IsDeleted).OrderBy(item => item.StepNumber)
            .Select(ToStepDto).ToList(),
        recipe.Images.Where(item => !item.IsDeleted).OrderByDescending(item => item.IsPrimary)
            .ThenBy(item => item.OrderIndex).Select(item => new RecipeImageDto(
                item.Id, item.OriginalUrl, item.MediumUrl, item.ThumbnailUrl,
                item.AltText, item.IsPrimary, item.OrderIndex)).ToList(),
        recipe.Nutrition is null ? null : new RecipeNutritionDto(
            recipe.Nutrition.Calories, recipe.Nutrition.Protein, recipe.Nutrition.Carbs,
            recipe.Nutrition.Fat, recipe.Nutrition.Fiber, recipe.Nutrition.Sodium));
}