using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Application.Common.Validation;
using ApplicationValidationException =
    CulinaryBlog.Application.Common.Exceptions.ValidationException;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class RecipeWorkflowTests
{
    [Fact]
    public void Publish_RequiresOneStepButDoesNotRequireIngredients()
    {
        var recipe = CreateRecipe(RecipeStatus.Draft);
        recipe.Steps.Add(new RecipeStep
        {
            Description = "Bake until golden."
        });

        recipe.Publish();

        Assert.Equal(RecipeStatus.Published, recipe.Status);
        Assert.Empty(recipe.Ingredients);
    }

    [Fact]
    public void Publish_RejectsDraftWithoutSteps()
    {
        var recipe = CreateRecipe(RecipeStatus.Draft);

        Assert.Throws<ValidationException>(recipe.Publish);
    }

    [Fact]
    public void PublishAndUnpublishAreIdempotentForTheirTargetStates()
    {
        var published = CreateRecipe(RecipeStatus.Published);
        var draft = CreateRecipe(RecipeStatus.Draft);

        published.Publish();
        draft.Unpublish();

        Assert.Equal(RecipeStatus.Published, published.Status);
        Assert.Equal(RecipeStatus.Draft, draft.Status);
    }

    [Fact]
    public void Unpublish_RejectsArchivedRecipe()
    {
        var recipe = CreateRecipe(RecipeStatus.Archived);

        Assert.Throws<ValidationException>(recipe.Unpublish);
    }

    [Fact]
    public void Publish_RejectsArchivedRecipe()
    {
        var recipe = CreateRecipe(RecipeStatus.Archived);
        recipe.Steps.Add(new RecipeStep
        {
            Description = "Bake until golden."
        });

        Assert.Throws<ValidationException>(recipe.Publish);
    }

    [Fact]
    public void SoftDelete_LeavesRelatedRecipeDataAvailableForRecovery()
    {
        var recipe = CreateRecipe(RecipeStatus.Published);
        recipe.Steps.Add(new RecipeStep { Description = "Mix." });
        recipe.Ingredients.Add(new RecipeIngredient { Name = "Flour" });
        recipe.Images.Add(new RecipeImage { OriginalUrl = "/uploads/recipe.png" });

        recipe.SoftDelete();

        Assert.True(recipe.IsDeleted);
        Assert.Single(recipe.Steps);
        Assert.Single(recipe.Ingredients);
        Assert.Single(recipe.Images);
        Assert.Equal("/uploads/recipe.png", recipe.Images.Single().OriginalUrl);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("-0.5")]
    [InlineData("-1/2")]
    [InlineData("1/-2")]
    [InlineData("about -2 cups")]
    public void IngredientQuantity_RejectsNegativeAmounts(string quantity)
    {
        Assert.Throws<ApplicationValidationException>(
            () => RecipeIngredientQuantityValidator.Validate(quantity));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.5")]
    [InlineData("1/2")]
    [InlineData("3/4")]
    [InlineData("vừa đủ")]
    [InlineData("")]
    public void IngredientQuantity_AllowsNonNegativeAndFreeTextAmounts(
        string quantity)
    {
        RecipeIngredientQuantityValidator.Validate(quantity);
    }

    [Fact]
    public void RecipeRowVersion_UsesNpgsqlXminAsGeneratedConcurrencyToken()
    {
        using var context = CreateContext(new SharedRowVersion());
        var entityType = context.Model.FindEntityType(typeof(Recipe))!;
        var property = entityType.FindProperty(nameof(Recipe.RowVersion))!;
        var table = StoreObjectIdentifier.Table("Recipes", null);

        Assert.True(property.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        Assert.Equal("xmin", property.GetColumnName(table));
    }

    [Fact]
    public async Task ConcurrentUpdatesWithSameXmin_OnlyOneSaveSucceeds()
    {
        var state = new SharedRowVersion();
        await using var firstContext = CreateContext(state);
        await using var secondContext = CreateContext(state);
        var firstUnitOfWork = new UnitOfWork(firstContext, null!, null!);
        var secondUnitOfWork = new UnitOfWork(secondContext, null!, null!);

        var outcomes = await Task.WhenAll(
            TrySaveAsync(firstUnitOfWork),
            TrySaveAsync(secondUnitOfWork));

        Assert.Equal(1, outcomes.Count(succeeded => succeeded));
        Assert.Equal(1, outcomes.Count(succeeded => !succeeded));
    }

    private static Recipe CreateRecipe(RecipeStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Test recipe",
            Slug = "test-recipe",
            Description = "A recipe.",
            Instructions = "Prepare the recipe.",
            CategoryId = Guid.NewGuid(),
            Status = status
        };

    private static ConcurrentWriteDbContext CreateContext(
        SharedRowVersion state)
    {
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseNpgsql(
                "Host=127.0.0.1;Database=unused;Username=unused;Password=unused")
            .Options;

        return new ConcurrentWriteDbContext(options, state);
    }

    private static async Task<bool> TrySaveAsync(UnitOfWork unitOfWork)
    {
        try
        {
            await unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (BusinessRuleException)
        {
            return false;
        }
    }

    private sealed class SharedRowVersion
    {
        private int _value = 1;

        public bool TryUpdate(uint originalVersion) =>
            originalVersion == 1
            && Interlocked.CompareExchange(ref _value, 2, 1) == 1;
    }

    private sealed class ConcurrentWriteDbContext(
        DbContextOptions<CulinaryBlogDbContext> options,
        SharedRowVersion rowVersion)
        : CulinaryBlogDbContext(options)
    {
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            if (rowVersion.TryUpdate(originalVersion: 1))
            {
                return 1;
            }

            throw new DbUpdateConcurrencyException(
                "The simulated xmin no longer matches.");
        }
    }
}
