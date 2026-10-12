using System.Text.RegularExpressions;
using FluentValidation;

namespace CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator
    : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => name.Trim().Length is >= 2 and <= 50)
            .WithMessage("Name must contain between 2 and 50 characters.")
            .Must(name => !Regex.IsMatch(
                name.Trim(),
                "<[^>]+>",
                RegexOptions.CultureInvariant))
            .WithMessage("Name must not contain HTML.");

        RuleFor(command => command.Description)
            .MaximumLength(2000);

        RuleFor(command => command.ImageUrl)
            .Must(IsHttpUrl)
            .When(command => !string.IsNullOrWhiteSpace(command.ImageUrl))
            .WithMessage(
                "ImageUrl must be a valid HTTP or HTTPS URL no longer than 500 characters.");

        RuleFor(command => command.OrderIndex)
            .GreaterThanOrEqualTo(0);
    }

    private static bool IsHttpUrl(string? value) =>
        value is not null
        && value.Length <= 500
        && Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp
            || uri.Scheme == Uri.UriSchemeHttps);
}
