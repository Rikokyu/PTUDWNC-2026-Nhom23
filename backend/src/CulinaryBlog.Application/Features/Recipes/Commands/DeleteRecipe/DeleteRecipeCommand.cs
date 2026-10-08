using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands.DeleteRecipe;

public sealed record DeleteRecipeCommand(Guid Id) : IRequest;