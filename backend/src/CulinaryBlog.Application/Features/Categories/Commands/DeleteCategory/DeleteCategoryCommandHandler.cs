using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Interfaces;
using MediatR;
using ConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler
	: IRequestHandler<DeleteCategoryCommand>
{
	private readonly IUnitOfWork _unitOfWork;

	public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task Handle(
		DeleteCategoryCommand request,
		CancellationToken cancellationToken)
	{
		var category = await _unitOfWork.Categories.GetByIdAsync(
			request.Id,
			cancellationToken)
			?? throw new NotFoundException("Category was not found.");

		if (await _unitOfWork.Categories.HasRecipesAsync(
				request.Id,
				cancellationToken))
		{
			throw new ConflictException("Category still contains recipes.");
		}

		category.IsDeleted = true;
		category.UpdatedAt = DateTime.UtcNow;
		_unitOfWork.Categories.Update(category);
		await _unitOfWork.SaveChangesAsync(cancellationToken);
	}
}
