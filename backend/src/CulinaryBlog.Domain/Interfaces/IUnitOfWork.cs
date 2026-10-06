using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Domain.Interfaces;

public interface IUnitOfWork
{
    IRecipeRepository Recipes { get; }

    ICategoryRepository Categories { get; }

    void AddRecipeImage(RecipeImage image);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
