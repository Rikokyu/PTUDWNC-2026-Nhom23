using System.Text.RegularExpressions;

namespace CulinaryBlog.Application.Common.Validation;

public static partial class CategoryValidationRules
{
    public static void ValidateName(
        string? name,
        ICollection<string> errors,
        bool required)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            if (required)
            {
                errors.Add("Name is required.");
            }

            return;
        }

        var trimmedName = name.Trim();

        if (trimmedName.Length is < 2 or > 50)
        {
            errors.Add("Name must contain between 2 and 50 characters.");
        }

        if (HtmlTagRegex().IsMatch(trimmedName))
        {
            errors.Add("Name must not contain HTML.");
        }
    }

    public static void ValidateDescription(
        string? description,
        ICollection<string> errors)
    {
        if (description?.Length > 2000)
        {
            errors.Add("Description cannot exceed 2000 characters.");
        }
    }

    public static void ValidateImageUrl(
        string? imageUrl,
        ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        if (imageUrl.Length > 500
            || !Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp
                && uri.Scheme != Uri.UriSchemeHttps))
        {
            errors.Add("ImageUrl must be a valid HTTP or HTTPS URL no longer than 500 characters.");
        }
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTagRegex();
}
