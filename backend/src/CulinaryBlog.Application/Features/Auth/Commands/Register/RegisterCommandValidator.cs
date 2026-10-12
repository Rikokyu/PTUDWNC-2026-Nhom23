using System.Net.Mail;
using System.Text.RegularExpressions;
using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.Application.Features.Auth.Commands.Register;

public sealed partial class RegisterCommandValidator
    : IFieldRequestValidator<RegisterCommand>
{
    public IReadOnlyDictionary<string, string[]> ValidateFields(
        RegisterCommand request)
    {
        var errors = new Dictionary<string, List<string>>(
            StringComparer.OrdinalIgnoreCase);

        AddIf(
            errors,
            "fullName",
            string.IsNullOrWhiteSpace(request.FullName),
            "Họ và tên không được để trống.");
        AddIf(
            errors,
            "fullName",
            request.FullName?.Trim().Length > 100,
            "Họ và tên không được vượt quá 100 ký tự.");

        AddIf(
            errors,
            "email",
            string.IsNullOrWhiteSpace(request.Email),
            "Email không được để trống.");
        AddIf(
            errors,
            "email",
            !string.IsNullOrWhiteSpace(request.Email)
                && (!MailAddress.TryCreate(request.Email.Trim(), out var address)
                    || !string.Equals(
                        address.Address,
                        request.Email.Trim(),
                        StringComparison.OrdinalIgnoreCase)),
            "Email không đúng định dạng.");
        AddIf(
            errors,
            "email",
            request.Email?.Trim().Length > 320,
            "Email không được vượt quá 320 ký tự.");

        AddIf(
            errors,
            "userName",
            string.IsNullOrWhiteSpace(request.UserName),
            "Tên đăng nhập không được để trống.");
        AddIf(
            errors,
            "userName",
            !string.IsNullOrWhiteSpace(request.UserName)
                && !UserNamePattern().IsMatch(request.UserName.Trim()),
            "Tên đăng nhập chỉ được chứa chữ cái và chữ số.");
        AddIf(
            errors,
            "userName",
            request.UserName?.Trim().Length is > 0 and < 3,
            "Tên đăng nhập phải có ít nhất 3 ký tự.");
        AddIf(
            errors,
            "userName",
            request.UserName?.Trim().Length > 50,
            "Tên đăng nhập không được vượt quá 50 ký tự.");

        AddIf(
            errors,
            "password",
            string.IsNullOrEmpty(request.Password),
            "Mật khẩu không được để trống.");
        AddIf(
            errors,
            "password",
            request.Password?.Length is > 0 and < 8,
            "Mật khẩu phải có ít nhất 8 ký tự.");
        AddIf(
            errors,
            "password",
            !string.IsNullOrEmpty(request.Password)
                && !request.Password.Any(char.IsUpper),
            "Mật khẩu phải có ít nhất 1 chữ hoa.");
        AddIf(
            errors,
            "password",
            !string.IsNullOrEmpty(request.Password)
                && !request.Password.Any(char.IsDigit),
            "Mật khẩu phải có ít nhất 1 chữ số.");
        AddIf(
            errors,
            "password",
            !string.IsNullOrEmpty(request.Password)
                && !request.Password.Any(character => !char.IsLetterOrDigit(character)),
            "Mật khẩu phải có ít nhất 1 ký tự đặc biệt.");
        AddIf(
            errors,
            "password",
            request.Password?.Length > 128,
            "Mật khẩu không được vượt quá 128 ký tự.");

        return errors.ToDictionary(
            error => error.Key,
            error => error.Value.Distinct().ToArray(),
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> Validate(RegisterCommand request) =>
        ValidateFields(request)
            .SelectMany(field => field.Value)
            .ToArray();

    private static void AddIf(
        IDictionary<string, List<string>> errors,
        string field,
        bool condition,
        string message)
    {
        if (!condition)
        {
            return;
        }

        if (!errors.TryGetValue(field, out var fieldErrors))
        {
            fieldErrors = [];
            errors[field] = fieldErrors;
        }

        fieldErrors.Add(message);
    }

    [GeneratedRegex("^[A-Za-z0-9]+$", RegexOptions.CultureInvariant)]
    private static partial Regex UserNamePattern();
}
