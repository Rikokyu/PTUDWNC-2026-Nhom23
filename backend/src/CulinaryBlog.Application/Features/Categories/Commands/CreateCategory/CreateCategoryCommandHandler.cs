using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Interfaces;
using MediatR;
using ConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler
	: IRequestHandler<CreateCategoryCommand, CategoryDto>
{
	private readonly IUnitOfWork _unitOfWork;

	public CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<CategoryDto> Handle(
		CreateCategoryCommand request,
		CancellationToken cancellationToken)
	{
		var name = request.Name.Trim();
		Validate(name, request.Description, request.ImageUrl, request.OrderIndex);
		var slug = CreateSlug(name);
		var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);

		if (categories.Any(category =>
				string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(category.Slug, slug, StringComparison.OrdinalIgnoreCase)))
		{
			throw new ConflictException("A category with this name already exists.");
		}

		var category = new Category
		{
			Id = Guid.NewGuid(),
			Name = name,
			Slug = slug,
			Description = NormalizeOptional(request.Description),
			ImageUrl = NormalizeOptional(request.ImageUrl),
			OrderIndex = request.OrderIndex,
			CreatedAt = DateTime.UtcNow
		};

		await _unitOfWork.Categories.AddAsync(category, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);
		return ToDto(category);
	}

	internal static string CreateSlug(string value)
	{
		var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
		var slug = new System.Text.StringBuilder(normalized.Length);
		var pendingSeparator = false;

		foreach (var character in normalized)
		{
			if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character)
				== System.Globalization.UnicodeCategory.NonSpacingMark)
			{
				continue;
			}

			if (char.IsLetterOrDigit(character))
			{
				if (pendingSeparator && slug.Length > 0)
				{
					slug.Append('-');
				}

				slug.Append(char.ToLowerInvariant(character));
				pendingSeparator = false;
			}
			else
			{
				pendingSeparator = true;
			}
		}

		return slug.ToString();
	}

	internal static CategoryDto ToDto(Category category) =>
		new(category.Id, category.Name, category.Slug, category.Description,
			category.ImageUrl, category.OrderIndex);

	private static void Validate(
		string name,
		string? description,
		string? imageUrl,
		int orderIndex)
	{
		if (name.Length is < 2 or > 100)
		{
			throw new ValidationException("name must be 2-100 characters.");
		}

		if (description?.Length > 2000)
		{
			throw new ValidationException("description cannot exceed 2000 characters.");
		}

		if (imageUrl is not null
			&& (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)
				|| uri.Scheme is not ("http" or "https")))
		{
			throw new ValidationException("imageUrl must be a valid HTTP or HTTPS URL.");
		}

		if (orderIndex < 0)
		{
			throw new ValidationException("orderIndex cannot be negative.");
		}
	}

	private static string? NormalizeOptional(string? value) =>
		string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
