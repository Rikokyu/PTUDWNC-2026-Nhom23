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
        var tokens = Regex.Matches(normalized.ToString(), "[a-z0-9]+")
            .Select(m => m.Value).Distinct().ToArray();
        // Prefixes help while typing one word or an incomplete short word.
        // For a submitted phrase, exact full words avoid "cha" matching
        // "chao" and "chat", which made unrelated dishes appear.
        return string.Join(" & ", tokens.Select(token =>
            tokens.Length == 1 || token.Length < 3 ? token + ":*" : token));
    }
}
