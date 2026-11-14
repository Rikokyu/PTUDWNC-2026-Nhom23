using CulinaryBlog.Application.DTOs.Recipes;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetAuthorProfile;

public sealed record GetAuthorProfileQuery(Guid? AuthorId, int Page, int PageSize)
    : IRequest<AuthorProfileDto>;
