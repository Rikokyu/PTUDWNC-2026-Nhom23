using CulinaryBlog.Application.Features.Auth.Commands.Login;
using CulinaryBlog.Application.Features.Auth.Commands.Register;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class AuthCommandValidatorTests
{
    [Fact]
    public void RegisterValidator_AcceptsValidRequest()
    {
        var validator = new RegisterCommandValidator();

        var result = validator.Validate(
            new RegisterCommand(
                "Nguyễn Đức Hoàng",
                "hoang@example.com",
                "SecurePass1!"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("invalid-email", "SecurePass1!")]
    [InlineData("hoang@example.com", "weakpass")]
    [InlineData("hoang@example.com", "nouppercase1!")]
    [InlineData("hoang@example.com", "NOLOWERCASE1!")]
    [InlineData("hoang@example.com", "NoDigitPass!")]
    [InlineData("hoang@example.com", "NoSpecial1")]
    public void RegisterValidator_RejectsInvalidEmailOrWeakPassword(
        string email,
        string password)
    {
        var validator = new RegisterCommandValidator();

        var result = validator.Validate(
            new RegisterCommand("Nguyễn Đức Hoàng", email, password));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LoginValidator_RequiresEmailAndPassword()
    {
        var validator = new LoginCommandValidator();

        var result = validator.Validate(new LoginCommand("", ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Email");
        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }
}
