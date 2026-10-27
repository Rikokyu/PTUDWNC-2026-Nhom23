namespace CulinaryBlog.Domain.Constants;

public static class UserRoles
{
    public const string Author = "Author";

    public const string Admin = "Admin";

    public static bool IsSupported(string role) =>
        role is Author or Admin;
}
