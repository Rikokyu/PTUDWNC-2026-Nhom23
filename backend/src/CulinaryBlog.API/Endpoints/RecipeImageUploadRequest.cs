using System.ComponentModel.DataAnnotations;

namespace CulinaryBlog.API.Endpoints;

public sealed class RecipeImageUploadRequest
{
    [Required]
    public IFormFile File { get; init; } = null!;

    public string? AltText { get; init; }

    public bool IsPrimary { get; init; }
}
