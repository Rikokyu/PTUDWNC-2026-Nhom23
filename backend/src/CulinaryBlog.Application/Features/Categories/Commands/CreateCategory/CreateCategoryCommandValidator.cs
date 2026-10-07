using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Validation;

namespace CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator
    : IRequestValidator<CreateCategoryCommand>
{
    public IReadOnlyList<string> Validate(CreateCategoryCommand request)
    {
        var errors = new List<string>();

        CategoryValidationRules.ValidateName(
            request.Name,
            errors,
            required: true);
        CategoryValidationRules.ValidateDescription(
            request.Description,
            errors);
        CategoryValidationRules.ValidateImageUrl(
            request.ImageUrl,
            errors);

        if (request.OrderIndex < 0)
        {
            errors.Add("OrderIndex cannot be negative.");
        }

        return errors;
    }
}
