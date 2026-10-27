namespace CulinaryBlog.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsAuthor { get; }

    bool IsAdmin { get; }
}