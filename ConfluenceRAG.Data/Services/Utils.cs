using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ConfluenceRAG.Data.Services;

public static partial class Utils
{
    // Block-level elements → newline
    [GeneratedRegex(@"(?is)</?(?:p|div|li|h[1-6])\b[^>]*>|<br\s*/?>")]
    private static partial Regex BlockTagsRegex();

    // Collapse spaces/tabs/non-breaking spaces
    [GeneratedRegex(@"[ \t\u00A0]+")]
    private static partial Regex SpaceRegex();

    // Normalize single newlines
    [GeneratedRegex(@"\s*\n\s*")]
    private static partial Regex NewlineRegex();

    // Collapse 3+ newlines into 2
    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex MultiNewlineRegex();

    public static string CleanHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Insert newlines for block tags
        string pre = BlockTagsRegex().Replace(html, "\n");

        var doc = new HtmlDocument();
        doc.LoadHtml(pre);

        // Extract plain text
        var raw = doc.DocumentNode.InnerText;

        // Decode HTML entities (&mdash; -> —, &nbsp; -> space, etc.)
        var text = WebUtility.HtmlDecode(raw);

        // Normalize whitespace/newlines
        text = SpaceRegex().Replace(text, " ");
        text = NewlineRegex().Replace(text, "\n");
        text = MultiNewlineRegex().Replace(text, "\n\n");

        return text.Trim();
    }
}