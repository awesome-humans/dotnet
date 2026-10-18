// Shared by the checks and the site build that read house opinions — pulled in with
// `#:include House.cs`. Not a file-based app itself: it declares no top-level statements.
//
// A house opinion belongs to a house profile, and sits beside the sourced text it
// changes, never inside it (HOUSE-OPINIONS.md: How this works). In an opinion it's a block:
//
//     /// house | simon-mckenna
//
//     **House:** Treat warnings as errors on every machine, not only in CI. …
//
//     ///
//
// In a template it's a region between two comment lines, in the file's comment syntax:
//
//     <!-- house:simon-mckenna begin -->
//     …
//     <!-- house:simon-mckenna end -->
//
// The file's sources list names the profile as house:<profile>. Removing every block and
// region gives the default copy, which carries no house opinions, and keeping one profile's
// gives that profile's copy.

using System.Text.RegularExpressions;

internal static partial class House
{
    /// <summary>The prefix of a house profile's id in a sources list: house:simon-mckenna.</summary>
    internal const string Prefix = "house:";

    /// <summary>The fixed string every house opinion opens with, so grep can find it.</summary>
    internal const string Marking = "**House:**";

    /// <summary>The id of a house profile in a sources list, or null for any other id.</summary>
    internal static string? ProfileOf(string sourceId) =>
        sourceId.StartsWith(Prefix, StringComparison.Ordinal) ? sourceId[Prefix.Length..] : null;

    /// <summary>Whether a profile id is lowercase words joined by hyphens, as a roster id is.</summary>
    internal static bool IsValidProfile(string profile) => ProfileId().IsMatch(profile);

    /// <summary>
    /// The house blocks in an opinion, or the house regions in a template, in file order:
    /// each one's profile, its first and last line (the opening and closing lines included),
    /// and whether it carries the marking. A malformed opening or closing line goes to
    /// <paramref name="errors"/>, prefixed with <paramref name="path"/>.
    /// </summary>
    internal static List<Span> Spans(string path, string text, List<string> errors)
    {
        bool template = !path.EndsWith(".md", StringComparison.Ordinal);
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        List<Span> spans = [];
        string? open = null;
        int start = 0;
        string? fence = null;

        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index];

            // A fenced code block in an opinion can hold a /// line of its own, such as a C#
            // documentation comment, so markers only count outside one. Templates have no fences.
            if (!template && FenceOpening().Match(line) is { Success: true } opening)
            {
                string marker = opening.Groups["fence"].Value;
                if (fence is null)
                {
                    fence = marker;
                }
                else if (marker[0] == fence[0] && marker.Length >= fence.Length && line.Trim() == marker)
                {
                    fence = null;
                }

                continue;
            }

            if (fence is not null)
            {
                continue;
            }

            (string Profile, bool Begin)? edge = template ? RegionEdge(line) : BlockEdge(line, open is not null);
            if (edge is not { } found)
            {
                continue;
            }

            if (found.Begin)
            {
                if (open is not null)
                {
                    errors.Add($"{path}:{index + 1}: house:{found.Profile} opens inside house:{open}, which hasn't closed");
                    continue;
                }

                if (!IsValidProfile(found.Profile))
                {
                    errors.Add($"{path}:{index + 1}: '{found.Profile}' isn't a profile id: use lowercase words joined by hyphens");
                }

                open = found.Profile;
                start = index;
            }
            else if (open is null)
            {
                errors.Add($"{path}:{index + 1}: a house {(template ? "region" : "block")} closes, but none is open");
            }
            else if (template && found.Profile != open)
            {
                errors.Add($"{path}:{index + 1}: house:{found.Profile} ends, but the open region is house:{open}");
            }
            else
            {
                bool marked = lines[start..index].Any(content => content.Contains(Marking, StringComparison.Ordinal));
                spans.Add(new Span(open, start, index, marked));
                open = null;
            }
        }

        if (open is not null)
        {
            errors.Add($"{path}:{start + 1}: house:{open} never closes");
        }

        return spans;
    }

    /// <summary>
    /// The text with every house block or region removed except the given profile's, or all
    /// of them for null. A kept template region loses its two marker lines, which only staging
    /// reads. The sources list in the frontmatter or header loses every other profile's id, so
    /// a copy names only the house opinions it carries.
    /// </summary>
    internal static string ForProfile(string path, string text, string? profile)
    {
        bool template = !path.EndsWith(".md", StringComparison.Ordinal);
        List<string> lines = [.. text.ReplaceLineEndings("\n").Split('\n')];
        List<Span> spans = Spans(path, text, []);

        // From the end, so earlier line numbers stay valid.
        foreach (Span span in Enumerable.Reverse(spans))
        {
            if (span.Profile == profile)
            {
                if (template)
                {
                    lines.RemoveAt(span.End);
                    lines.RemoveAt(span.Start);
                }

                continue;
            }

            int count = span.End - span.Start + 1;

            // A removed block takes the blank line after it, so no double blank is left behind.
            if (!template && span.End + 1 < lines.Count && lines[span.End + 1].Length == 0)
            {
                count++;
            }

            lines.RemoveRange(span.Start, count);
        }

        string result = string.Join("\n", lines);
        foreach (string other in spans.Select(span => span.Profile).Where(other => other != profile).Distinct())
        {
            string id = Regex.Escape(Prefix + other);
            result = Regex.Replace(result, $@"{id}\s*,\s*", "", RegexOptions.None, TimeSpan.FromSeconds(1));
            result = Regex.Replace(result, $@",\s*{id}(?=[\s\],|]|$)", "", RegexOptions.None, TimeSpan.FromSeconds(1));

            // The profile was the only source: an empty frontmatter list, or an empty header field.
            result = Regex.Replace(result, $@"\[\s*{id}\s*\]", "[]", RegexOptions.None, TimeSpan.FromSeconds(1));
            result = Regex.Replace(result, $@"(sources:) {id}(?= \||\s*-->|$)", "$1", RegexOptions.Multiline, TimeSpan.FromSeconds(1));
        }

        return result;
    }

    // An opinion's block opens with '/// house | <profile>' and closes with a bare '///'.
    private static (string Profile, bool Begin)? BlockEdge(string line, bool open)
    {
        if (BlockOpening().Match(line) is { Success: true } opening)
        {
            return (opening.Groups["profile"].Value.Trim(), true);
        }

        return open && line.TrimEnd() == "///" ? ("", false) : null;
    }

    // A template's region opens and closes with a comment line naming the profile.
    private static (string Profile, bool Begin)? RegionEdge(string line) =>
        RegionMarker().Match(line) is { Success: true } marker
            ? (marker.Groups["profile"].Value, marker.Groups["edge"].Value == "begin")
            : null;

    /// <summary>One house block or region: its profile, its first and last line, and whether it's marked.</summary>
    internal readonly record struct Span(string Profile, int Start, int End, bool Marked);

    [GeneratedRegex(@"\A[a-z0-9]+(?:-[a-z0-9]+)*\z")]
    private static partial Regex ProfileId();

    [GeneratedRegex(@"\A/// house \|(?<profile>.*)\z")]
    private static partial Regex BlockOpening();

    [GeneratedRegex(@"\A\s*(?:<!--|#|//)\s*house:(?<profile>[^\s]+) (?<edge>begin|end)\s*(?:-->)?\s*\z")]
    private static partial Regex RegionMarker();

    [GeneratedRegex(@"\A\s*(?<fence>`{3,}|~{3,})")]
    private static partial Regex FenceOpening();
}
