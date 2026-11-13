using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public static class VietnameseSearchQuery
{
    public static string Build(string term)
    {
        var normalized = new StringBuilder();
        foreach (var c in term.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                normalized.Append(c is '\u0111' or '\u0110' ? 'd' : char.ToLowerInvariant(c));
        // Only literal alphanumeric lexemes reach tsquery; operators are never accepted.
        return string.Join(" & ", Regex.Matches(normalized.ToString(), "[a-z0-9]+")
            .Select(m => m.Value).Distinct().Select(token => token + ":*"));
    }
}
