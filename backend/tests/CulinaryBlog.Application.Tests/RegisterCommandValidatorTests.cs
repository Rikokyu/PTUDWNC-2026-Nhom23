using CulinaryBlog.Application.Features.Auth.Commands.Register;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void ValidateFields_AcceptsValidRegistration()
    {
        var errors = _validator.ValidateFields(new RegisterCommand(
            "Nguyễn Văn A",
            "nguyenvana@example.com",
            "nguyenvana",
            "Password@123"));

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateFields_ReturnsErrorsForEveryInvalidField()
    {
        var errors = _validator.ValidateFields(new RegisterCommand(
            "",
            "invalid-email",
            "user@name",
            "123"));

        Assert.Contains("fullName", errors.Keys);
        Assert.Contains("email", errors.Keys);
        Assert.Contains("userName", errors.Keys);
        Assert.Contains("password", errors.Keys);
        Assert.True(errors["password"].Length >= 3);
    }
}
