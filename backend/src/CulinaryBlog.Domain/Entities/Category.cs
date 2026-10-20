namespace CulinaryBlog.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int OrderIndex { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<Recipe> Recipes { get; set; }
        = new List<Recipe>();

    public static Category Create(
        string name,
        string slug,
        string? description,
        string? imageUrl,
        int orderIndex)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = description,
            ImageUrl = imageUrl,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string? name,
        bool updateDescription,
        string? description,
        bool updateImageUrl,
        string? imageUrl,
        int? orderIndex)
    {
        if (name is not null)
        {
            Name = name;
        }

        if (updateDescription)
        {
            Description = description;
        }

        if (updateImageUrl)
        {
            ImageUrl = imageUrl;
        }

        if (orderIndex.HasValue)
        {
            OrderIndex = orderIndex.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
