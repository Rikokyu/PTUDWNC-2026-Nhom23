using CulinaryBlog.API.Endpoints;
using CulinaryBlog.Application.Common.Exceptions;
using Xunit;

namespace CulinaryBlog.API.Tests;

public sealed class RecipeImageUploadValidatorTests
{
    [Fact]
    public void Validate_AcceptsJpegWithMatchingExtensionMimeAndSignature()
    {
        var extension = RecipeImageUploadValidator.Validate(
            "dish.jpg",
            "image/jpeg",
            3,
            1024,
            new byte[] { 0xFF, 0xD8, 0xFF });

        Assert.Equal(".jpg", extension);
    }

    [Fact]
    public void Validate_AcceptsJpegAliasExtension()
    {
        var extension = RecipeImageUploadValidator.Validate(
            "dish.jpeg",
            "image/jpeg",
            3,
            1024,
            new byte[] { 0xFF, 0xD8, 0xFF });

        Assert.Equal(".jpeg", extension);
    }

    [Fact]
    public void Validate_RejectsEmptyFile()
    {
        Assert.Throws<ValidationException>(() =>
            RecipeImageUploadValidator.Validate(
                "dish.jpg",
                "image/jpeg",
                0,
                1024,
                new byte[] { 0xFF, 0xD8, 0xFF }));
    }

    [Fact]
    public void Validate_RejectsFileExceedingConfiguredLimit()
    {
        Assert.Throws<ValidationException>(() =>
            RecipeImageUploadValidator.Validate(
                "dish.jpg",
                "image/jpeg",
                1025,
                1024,
                new byte[] { 0xFF, 0xD8, 0xFF }));
    }

    [Fact]
    public void Validate_RejectsUnsupportedMimeType()
    {
        Assert.Throws<ValidationException>(() =>
            RecipeImageUploadValidator.Validate(
                "dish.gif",
                "image/gif",
                3,
                1024,
                new byte[] { 0xFF, 0xD8, 0xFF }));
    }

    [Fact]
    public void Validate_RejectsExtensionThatDoesNotMatchMimeType()
    {
        Assert.Throws<ValidationException>(() =>
            RecipeImageUploadValidator.Validate(
                "dish.png",
                "image/jpeg",
                3,
                1024,
                new byte[] { 0xFF, 0xD8, 0xFF }));
    }

    [Fact]
    public void Validate_RejectsMismatchedMagicBytes()
    {
        Assert.Throws<ValidationException>(() =>
            RecipeImageUploadValidator.Validate(
                "dish.jpg",
                "image/jpeg",
                3,
                1024,
                new byte[] { 0x89, 0x50, 0x4E }));
    }

    [Fact]
    public void Validate_RejectsInvalidFileName()
    {
        Assert.Throws<ValidationException>(() =>
            RecipeImageUploadValidator.Validate(
                " ",
                "image/jpeg",
                3,
                1024,
                new byte[] { 0xFF, 0xD8, 0xFF }));
    }
}
