using System.Globalization;
using System.Text;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands;

public sealed class RecipeCommandHandler :
    IRequestHandler<CreateRecipeCommand, RecipeMutationResult>,
    IRequestHandler<UpdateRecipeCommand>,
    IRequestHandler<SetRecipeStatusCommand>,
    IRequestHandler<DeleteRecipeCommand>,
    IRequestHandler<AddRecipeIngredientCommand, Guid>,
    IRequestHandler<UpdateRecipeIngredientCommand>,
    IRequestHandler<DeleteRecipeIngredientCommand>,
    IRequestHandler<AddRecipeStepCommand, Guid>,
    IRequestHandler<UpdateRecipeStepCommand>,
    IRequestHandler<DeleteRecipeStepCommand>,
    IRequestHandler<AddRecipeImageCommand, Guid>,
    IRequestHandler<UpdateRecipeImageCommand>,
    IRequestHandler<DeleteRecipeImageCommand>
{
    private readonly IRecipeRepository _recipes;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<Recipe> _recipeStore;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public RecipeCommandHandler(
        IRecipeRepository recipes,
        IRepository<Category> categories,
        IRepository<Recipe> recipeStore,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _recipes = recipes;
        _categories = categories;
        _recipeStore = recipeStore;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<RecipeMutationResult> Handle(
        CreateRecipeCommand request,
        CancellationToken cancellationToken)
    {
        EnsureCanCreateRecipe();
        ValidateRecipeFields(
            request.Title,
            request.Description,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.Servings,
            request.Difficulty);
        await EnsureCategoryExistsAsync(
            request.CategoryId,
            cancellationToken);

        var ingredients = request.Ingredients
            .Select((item, index) => CreateIngredient(item, index))
            .ToList();
        var steps = request.Steps
            .Select((item, index) => CreateStep(item, index + 1))
            .ToList();
        var slug = await CreateUniqueSlugAsync(
            request.Title,
            cancellationToken);

        var recipe = new Recipe
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description.Trim(),
            Instructions = request.Instructions?.Trim() ?? string.Empty,
            CategoryId = request.CategoryId,
            PrepTimeMinutes = request.PrepTimeMinutes,
            CookTimeMinutes = request.CookTimeMinutes,
            Servings = request.Servings,
            Difficulty = request.Difficulty,
            Status = RecipeStatus.Draft,
            AuthorId = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow,
            Nutrition = ToNutrition(request.Nutrition),
            Ingredients = ingredients,
            Steps = steps
        };

        foreach (var ingredient in ingredients)
        {
            ingredient.Recipe = recipe;
        }

        foreach (var step in steps)
        {
            step.Recipe = recipe;
        }

        await _recipeStore.AddAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RecipeMutationResult(recipe.Id, recipe.Slug);
    }

    public async Task Handle(
        UpdateRecipeCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        ValidateRecipeFields(
            request.Title,
            request.Description,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.Servings,
            request.Difficulty);
        await EnsureCategoryExistsAsync(
            request.CategoryId,
            cancellationToken);

        recipe.Title = request.Title.Trim();
        recipe.Description = request.Description.Trim();
        recipe.Instructions = request.Instructions?.Trim() ?? string.Empty;
        recipe.CategoryId = request.CategoryId;
        recipe.PrepTimeMinutes = request.PrepTimeMinutes;
        recipe.CookTimeMinutes = request.CookTimeMinutes;
        recipe.Servings = request.Servings;
        recipe.Difficulty = request.Difficulty;
        recipe.Nutrition = ToNutrition(request.Nutrition);
        recipe.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(
        SetRecipeStatusCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);

        if (request.Status == RecipeStatus.Published
            && (recipe.Ingredients.Count == 0 || recipe.Steps.Count == 0))
        {
            throw new ValidationException(
                "A recipe needs at least one ingredient and one step before publishing.");
        }

        recipe.Status = request.Status;
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(
        DeleteRecipeCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        recipe.IsDeleted = true;
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> Handle(
        AddRecipeIngredientCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        var ingredient = CreateIngredient(
            new RecipeIngredientInput(
                request.Name,
                request.Quantity,
                request.Unit,
                request.Notes),
            recipe.Ingredients.Count);
        ingredient.Recipe = recipe;
        recipe.Ingredients.Add(ingredient);
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ingredient.Id;
    }

    public async Task Handle(
        UpdateRecipeIngredientCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        var ingredient = recipe.Ingredients
            .FirstOrDefault(item => item.Id == request.IngredientId)
            ?? throw new NotFoundException("Recipe ingredient was not found.");
        ValidateIngredient(request.Name, request.Quantity, request.Unit, request.Notes);

        ingredient.Name = request.Name.Trim();
        ingredient.Quantity = request.Quantity?.Trim();
        ingredient.Unit = request.Unit?.Trim();
        ingredient.Notes = request.Notes?.Trim();
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(
        DeleteRecipeIngredientCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        var ingredient = recipe.Ingredients
            .FirstOrDefault(item => item.Id == request.IngredientId)
            ?? throw new NotFoundException("Recipe ingredient was not found.");
        EnsurePublishedRecipeKeepsOne(recipe, recipe.Ingredients.Count);

        recipe.Ingredients.Remove(ingredient);
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> Handle(
        AddRecipeStepCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        var step = CreateStep(
            new RecipeStepInput(
                request.Title,
                request.Description,
                request.TimerMinutes,
                request.ImageUrl),
            recipe.Steps.Count + 1);
        step.Recipe = recipe;
        recipe.Steps.Add(step);
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return step.Id;
    }

    public async Task Handle(
        UpdateRecipeStepCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        var step = recipe.Steps
            .FirstOrDefault(item => item.Id == request.StepId)
            ?? throw new NotFoundException("Recipe step was not found.");
        ValidateStep(request.Description, request.TimerMinutes);

        step.Title = request.Title.Trim();
        step.Description = request.Description.Trim();
        step.TimerMinutes = request.TimerMinutes;
        step.ImageUrl = request.ImageUrl?.Trim();
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(
        DeleteRecipeStepCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        var step = recipe.Steps
            .FirstOrDefault(item => item.Id == request.StepId)
            ?? throw new NotFoundException("Recipe step was not found.");
        EnsurePublishedRecipeKeepsOne(recipe, recipe.Steps.Count);

        recipe.Steps.Remove(step);
        var remainingSteps = recipe.Steps
            .OrderBy(item => item.StepNumber)
            .ToList();
        foreach (var remainingStep in remainingSteps)
        {
            remainingStep.StepNumber = -remainingStep.StepNumber;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < remainingSteps.Count; index++)
        {
            remainingSteps[index].StepNumber = index + 1;
        }

        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> Handle(
        AddRecipeImageCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        if (string.IsNullOrWhiteSpace(request.OriginalUrl))
        {
            throw new ValidationException("originalUrl is required.");
        }

        var makePrimary = request.IsPrimary || recipe.Images.Count == 0;
        if (makePrimary)
        {
            foreach (var image in recipe.Images)
            {
                image.IsPrimary = false;
            }
        }

        var imageEntity = new RecipeImage
        {
            Recipe = recipe,
            OriginalUrl = request.OriginalUrl.Trim(),
            AltText = request.AltText?.Trim(),
            IsPrimary = makePrimary,
            OrderIndex = recipe.Images.Count,
            CreatedAt = DateTime.UtcNow
        };
        recipe.Images.Add(imageEntity);
        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return imageEntity.Id;
    }

    public async Task Handle(
        UpdateRecipeImageCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        var image = recipe.Images
            .FirstOrDefault(item => item.Id == request.ImageId)
            ?? throw new NotFoundException("Recipe image was not found.");

        if (request.IsPrimary == false && image.IsPrimary)
        {
            throw new ValidationException(
                "Select another primary image before unsetting the current one.");
        }

        image.AltText = request.AltText?.Trim();
        if (request.IsPrimary == true)
        {
            foreach (var recipeImage in recipe.Images)
            {
                recipeImage.IsPrimary = recipeImage.Id == image.Id;
            }
        }

        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(
        DeleteRecipeImageCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await GetOwnedRecipeAsync(
            request.RecipeId,
            cancellationToken);
        var image = recipe.Images
            .FirstOrDefault(item => item.Id == request.ImageId)
            ?? throw new NotFoundException("Recipe image was not found.");
        var wasPrimary = image.IsPrimary;
        recipe.Images.Remove(image);

        if (wasPrimary)
        {
            var replacement = recipe.Images
                .OrderBy(item => item.OrderIndex)
                .FirstOrDefault();
            if (replacement != null)
            {
                replacement.IsPrimary = true;
            }
        }

        recipe.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Recipe> GetOwnedRecipeAsync(
        Guid recipeId,
        CancellationToken cancellationToken)
    {
        var recipe = await _recipes.GetByIdWithDetailsAsync(
            recipeId,
            cancellationToken)
            ?? throw new NotFoundException("Recipe was not found.");

        if (!_currentUser.IsAuthenticated
            || (!_currentUser.IsAdmin
                && (!_currentUser.UserId.HasValue
                    || recipe.AuthorId != _currentUser.UserId)))
        {
            throw new ForbiddenException(
                "You do not have permission to modify this recipe.");
        }

        return recipe;
    }

    private async Task EnsureCategoryExistsAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        if (await _categories.GetByIdAsync(categoryId, cancellationToken) == null)
        {
            throw new ValidationException(
                "Recipe category was not found.");
        }
    }

    private async Task<string> CreateUniqueSlugAsync(
        string title,
        CancellationToken cancellationToken)
    {
        var baseSlug = Slugify(title);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            throw new ValidationException("title must contain letters or numbers.");
        }

        var slug = baseSlug;
        var suffix = 2;
        while (await _recipes.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private static string Slugify(string value)
    {
        var normalized = value
            .Trim()
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var slug = new StringBuilder();

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character)
                == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                slug.Append(character);
            }
            else if (slug.Length > 0 && slug[^1] != '-')
            {
                slug.Append('-');
            }
        }

        return slug.ToString().Trim('-');
    }

    private static void ValidateRecipeFields(
        string title,
        string description,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int servings,
        DifficultyLevel difficulty)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200)
        {
            throw new ValidationException("title is required and must be at most 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ValidationException("description is required.");
        }

        if (prepTimeMinutes <= 0 || cookTimeMinutes < 0 || servings <= 0)
        {
            throw new ValidationException(
                "prepTimeMinutes must be positive; cookTimeMinutes must be non-negative; servings must be positive.");
        }

        if (!Enum.IsDefined(difficulty))
        {
            throw new ValidationException("difficulty is invalid.");
        }
    }

    private static RecipeIngredient CreateIngredient(
        RecipeIngredientInput input,
        int orderIndex)
    {
        ValidateIngredient(input.Name, input.Quantity, input.Unit, input.Notes);
        return new RecipeIngredient
        {
            Name = input.Name.Trim(),
            Quantity = input.Quantity?.Trim(),
            Unit = input.Unit?.Trim(),
            Notes = input.Notes?.Trim(),
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void ValidateIngredient(
        string name,
        string? quantity,
        string? unit,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200)
        {
            throw new ValidationException("ingredient name is required and must be at most 200 characters.");
        }

        if (quantity?.Trim().Length > 50
            || unit?.Trim().Length > 50
            || notes?.Trim().Length > 500)
        {
            throw new ValidationException(
                "ingredient quantity and unit must be at most 50 characters; notes must be at most 500 characters.");
        }

        if (string.IsNullOrWhiteSpace(quantity))
        {
            if (!string.IsNullOrWhiteSpace(unit)
                || string.IsNullOrWhiteSpace(notes))
            {
                throw new ValidationException(
                    "An ingredient without quantity must omit unit and provide notes.");
            }
        }
        else if (!decimal.TryParse(
                     quantity,
                     NumberStyles.Number,
                     CultureInfo.InvariantCulture,
                     out var parsedQuantity)
                 || parsedQuantity <= 0
                 || string.IsNullOrWhiteSpace(unit))
        {
            throw new ValidationException(
                "quantity must be a positive number and requires a unit.");
        }
    }

    private static RecipeStep CreateStep(
        RecipeStepInput input,
        int stepNumber)
    {
        if (input.Title?.Trim().Length > 200)
        {
            throw new ValidationException(
                "step title must be at most 200 characters.");
        }

        ValidateStep(input.Description, input.TimerMinutes);
        return new RecipeStep
        {
            StepNumber = stepNumber,
            Title = input.Title?.Trim() ?? string.Empty,
            Description = input.Description.Trim(),
            TimerMinutes = input.TimerMinutes,
            ImageUrl = input.ImageUrl?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void ValidateStep(
        string description,
        int? timerMinutes)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ValidationException("step description is required.");
        }

        if (timerMinutes < 0)
        {
            throw new ValidationException("timerMinutes must be non-negative.");
        }
    }

    private static RecipeNutrition? ToNutrition(RecipeNutritionDto? dto)
    {
        if (dto == null)
        {
            return null;
        }

        var values = new[]
        {
            dto.Calories,
            dto.Protein,
            dto.Carbs,
            dto.Fat,
            dto.Fiber,
            dto.Sodium
        };

        if (values.Any(value => value < 0))
        {
            throw new ValidationException("Nutrition values must be non-negative.");
        }

        if (values.Any(value => value > 99999999.99m))
        {
            throw new ValidationException(
                "Nutrition values exceed the supported precision.");
        }

        return new RecipeNutrition
        {
            Calories = dto.Calories,
            Protein = dto.Protein,
            Carbs = dto.Carbs,
            Fat = dto.Fat,
            Fiber = dto.Fiber,
            Sodium = dto.Sodium
        };
    }

    private void EnsureSignedIn()
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new ForbiddenException("Sign in to manage recipes.");
        }
    }

    private void EnsureCanCreateRecipe()
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            throw new ForbiddenException("Sign in to manage recipes.");
        }

        if (!_currentUser.IsAuthor && !_currentUser.IsAdmin)
        {
            throw new ForbiddenException(
                "Only authors and administrators can create recipes.");
        }
    }

    private static void EnsurePublishedRecipeKeepsOne(
        Recipe recipe,
        int itemCount)
    {
        if (recipe.Status == RecipeStatus.Published && itemCount <= 1)
        {
            throw new ValidationException(
                "A published recipe must keep at least one ingredient and one step.");
        }
    }
}