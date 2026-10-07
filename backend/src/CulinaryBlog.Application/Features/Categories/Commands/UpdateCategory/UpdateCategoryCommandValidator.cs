using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Validation;

namespace CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandValidator
    : IRequestValidator<UpdateCategoryCommand>
{
    public IReadOnlyList<string> Validate(UpdateCategoryCommand request)
    {
        var errors = new List<string>();

        if (request.Id == Guid.Empty)
        {
            errors.Add("A valid category id is required.");
        }

        if (request.Name is null
            && request.Description is null
            && request.ImageUrl is null
            && !request.OrderIndex.HasValue)
        {
            errors.Add("At least one field must be supplied for update.");
        }

        if (request.Name is not null)
        {
            CategoryValidationRules.ValidateName(
                request.Name,
                errors,
                required: true);
        }

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
