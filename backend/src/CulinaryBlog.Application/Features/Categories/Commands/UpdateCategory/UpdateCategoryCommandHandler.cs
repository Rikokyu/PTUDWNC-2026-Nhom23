using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Domain.Interfaces;
using MediatR;
using ConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
	: IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
	private readonly IUnitOfWork _unitOfWork;

	public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<CategoryDto> Handle(
		UpdateCategoryCommand request,
		CancellationToken cancellationToken)
	{
		var category = await _unitOfWork.Categories.GetByIdAsync(
			request.Id,
			cancellationToken)
			?? throw new NotFoundException("Category was not found.");
		var name = request.Name.Trim();

		if (name.Length is < 2 or > 100 || request.Description?.Length > 2000
			|| request.OrderIndex < 0)
		{
			throw new ValidationException("Category fields are invalid.");
		}

		if (request.ImageUrl is not null
			&& (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var imageUri)
				|| imageUri.Scheme is not ("http" or "https")))
		{
			throw new ValidationException("imageUrl must be a valid HTTP or HTTPS URL.");
		}

		var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
		var slug = CreateCategoryCommandHandler.CreateSlug(name);
		if (categories.Any(candidate => candidate.Id != category.Id
				&& (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(candidate.Slug, slug, StringComparison.OrdinalIgnoreCase))))
		{
			throw new ConflictException("A category with this name already exists.");
		}

		category.Name = name;
		category.Slug = slug;
		category.Description = string.IsNullOrWhiteSpace(request.Description)
			? null
			: request.Description.Trim();
		category.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl)
			? null
			: request.ImageUrl.Trim();
		category.OrderIndex = request.OrderIndex;
		category.UpdatedAt = DateTime.UtcNow;

		_unitOfWork.Categories.Update(category);
		await _unitOfWork.SaveChangesAsync(cancellationToken);
		return CreateCategoryCommandHandler.ToDto(category);
	}
}
