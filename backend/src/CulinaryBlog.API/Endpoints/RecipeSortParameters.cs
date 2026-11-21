using CulinaryBlog.Application.Common.Exceptions;

namespace CulinaryBlog.API.Endpoints;

internal static class RecipeSortParameters
{
    public static string Read(IQueryCollection query, bool isSearch = false)
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

        var sortBy = sortByValues.FirstOrDefault() ?? (isSearch ? "relevance" : "createdAt");
        var sortOrder = sortOrderValues.FirstOrDefault() ?? "desc";
        if ((sortBy is not ("title" or "createdAt" or "cookTime")
                && !(isSearch && sortBy == "relevance"))
            || sortOrder is not ("asc" or "desc"))
        {
            throw new ValidationException(
                "Invalid sortBy or sortOrder.");
        }

        if (sortBy == "relevance")
        {
            if (sortOrder == "asc")
                throw new ValidationException("Relevance can only be sorted descending.");
            return "relevance";
        }

        return (sortOrder == "desc" ? "-" : "") + sortBy;
    }
}
