using System.Net.Mail;
using System.Text.RegularExpressions;
using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.Application.Features.Auth.Commands.Register;

public sealed partial class RegisterCommandValidator
    : IRequestValidator<RegisterCommand>
{
    public IReadOnlyList<string> Validate(RegisterCommand request)
    {
        var errors = new List<string>();
        var fullName = request.FullName?.Trim();
        var email = request.Email?.Trim();
        var userName = request.UserName?.Trim();

        if (string.IsNullOrWhiteSpace(fullName)
            || fullName.Length is < 2 or > 100)
        {
            errors.Add("FullName must contain between 2 and 100 characters.");
        }
        else if (HtmlTagRegex().IsMatch(fullName))
        {
            errors.Add("FullName must not contain HTML.");
        }

        if (string.IsNullOrWhiteSpace(email)
            || email.Length > 320
            || !MailAddress.TryCreate(email, out _))
        {
            errors.Add("Email must be a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(userName)
            || userName.Length is < 3 or > 50
            || !UserNameRegex().IsMatch(userName))
        {
            errors.Add("UserName must contain 3 to 50 letters, numbers or underscores.");
        }

        if (string.IsNullOrEmpty(request.Password)
            || request.Password.Length < 8
            || !request.Password.Any(char.IsUpper)
            || !request.Password.Any(char.IsDigit)
            || !request.Password.Any(character =>
                !char.IsLetterOrDigit(character)))
        {
            errors.Add("Password must be at least 8 characters and contain an uppercase letter, a number and a special character.");
        }

        return errors;
    }

    [GeneratedRegex("^[A-Za-z0-9_]+$", RegexOptions.CultureInvariant)]
    private static partial Regex UserNameRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTagRegex();
}
