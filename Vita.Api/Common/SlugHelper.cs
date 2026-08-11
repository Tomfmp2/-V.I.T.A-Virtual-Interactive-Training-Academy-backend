using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Vita.Api.Common;

public static class SlugHelper
{
    private static readonly Regex NonSlugChars = new(@"[^a-z0-9\s-]", RegexOptions.Compiled);
    private static readonly Regex WhitespaceAndHyphens = new(@"[\s-]+", RegexOptions.Compiled);

    public static string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var withoutDiacritics = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        var cleaned = NonSlugChars.Replace(withoutDiacritics, string.Empty);
        var collapsed = WhitespaceAndHyphens.Replace(cleaned, "-").Trim('-');

        return collapsed;
    }
}
