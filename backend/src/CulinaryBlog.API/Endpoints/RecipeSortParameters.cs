using CulinaryBlog.Application.Common.Exceptions;

namespace CulinaryBlog.API.Endpoints;

internal static class RecipeSortParameters
{
    public static string Read(IQueryCollection query)
    {
        if (query.ContainsKey("sort"))
        {
            throw new ValidationException(
                "Use sortBy and sortOrder instead of sort.");
        }

        var sortByValues = query["sortBy"];
        var sortOrderValues = query["sortOrder"];
        if (sortByValues.Count > 1 || sortOrderValues.Count > 1)
        {
            throw new ValidationException(
                "Sort parameters must occur only once.");
        }

        var sortBy = sortByValues.FirstOrDefault() ?? "createdAt";
        var sortOrder = sortOrderValues.FirstOrDefault() ?? "desc";
        if (sortBy is not ("title" or "createdAt" or "cookTime")
            || sortOrder is not ("asc" or "desc"))
        {
            throw new ValidationException(
                "sortBy must be title, createdAt or cookTime; sortOrder must be asc or desc.");
        }

        return (sortOrder == "desc" ? "-" : "") + sortBy;
    }
}
