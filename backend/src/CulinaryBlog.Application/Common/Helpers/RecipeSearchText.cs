using System.Text;
using System.Text.RegularExpressions;

namespace CulinaryBlog.Application.Common.Helpers;

public static partial class RecipeSearchText
{
    public const int MaxLength = 200;

    public static string Normalize(string value) => value.Trim().Normalize(NormalizationForm.FormC);

    // Never pass user-supplied tsquery operators to PostgreSQL. Each word is a
    // literal prefix and all words must match; accent folding happens in the DB.
    public static string BuildPrefixQuery(string value) => string.Join(" & ",
        WordPattern().Matches(Normalize(value))
            .Select(match => match.Value.ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .Select(word => $"{word}:*"));

    [GeneratedRegex(@"[\p{L}\p{N}]+", RegexOptions.CultureInvariant)]
    private static partial Regex WordPattern();
}
