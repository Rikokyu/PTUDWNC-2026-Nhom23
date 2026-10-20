using System.Text.RegularExpressions;
using FluentValidation;

namespace CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandValidator
    : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("A valid category id is required.");

        RuleFor(command => command)
            .Must(command =>
                command.Name is not null
                || command.Description is not null
                || command.ImageUrl is not null
                || command.OrderIndex.HasValue)
            .WithMessage("At least one field must be supplied for update.");

        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .When(command => command.Name is not null)
            .WithMessage("Name is required.")
            .Must(name => name!.Trim().Length is >= 2 and <= 50)
            .When(command => command.Name is not null)
            .WithMessage("Name must contain between 2 and 50 characters.")
            .Must(name => !Regex.IsMatch(
                name!.Trim(),
                "<[^>]+>",
                RegexOptions.CultureInvariant))
            .When(command => command.Name is not null)
            .WithMessage("Name must not contain HTML.");

        RuleFor(command => command.Description)
            .MaximumLength(2000);

        RuleFor(command => command.ImageUrl)
            .Must(IsHttpUrl)
            .When(command => !string.IsNullOrWhiteSpace(command.ImageUrl))
            .WithMessage(
                "ImageUrl must be a valid HTTP or HTTPS URL no longer than 500 characters.");

        RuleFor(command => command.OrderIndex)
            .GreaterThanOrEqualTo(0)
            .When(command => command.OrderIndex.HasValue);
    }

    private static bool IsHttpUrl(string? value) =>
        value is not null
        && value.Length <= 500
        && Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp
            || uri.Scheme == Uri.UriSchemeHttps);
}
