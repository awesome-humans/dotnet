// Shared by the checks that read opinion frontmatter, through
// `#:include Frontmatter.cs`. Not a file-based app itself: it declares no top-level
// statements. The YamlDotNet reference it compiles against is declared by the script
// including it, so the two checks report identical wording from one implementation.

using System.Collections;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

internal static class Frontmatter
{
    /// <summary>
    /// Parses a Markdown file's YAML frontmatter. Returns the mapping, or null with
    /// <paramref name="error"/> set to the reason: the phrase the frontmatter check
    /// prints after the file path, and the reason the source check skips the file.
    /// </summary>
    internal static IDictionary? Read(IDeserializer deserializer, string path, out string? error)
    {
        string text = File.ReadAllText(path).ReplaceLineEndings("\n");

        if (!text.StartsWith("---\n", StringComparison.Ordinal))
        {
            error = "missing frontmatter block";
            return null;
        }

        int end = text.IndexOf("\n---", 4, StringComparison.Ordinal);
        if (end == -1)
        {
            error = "unterminated frontmatter block";
            return null;
        }

        object? parsed;
        try
        {
            parsed = deserializer.Deserialize<object>(text[4..end]);
        }
        catch (YamlException exception)
        {
            // The parser's line numbers are relative to the frontmatter body, which
            // starts on line 2 of the file, so offset them to point at the real line.
            error = $"frontmatter isn't valid YAML: {exception.Message} "
                + $"(line {exception.Start.Line + 1}, column {exception.Start.Column})";
            return null;
        }

        if (parsed is not IDictionary mapping)
        {
            error = "frontmatter isn't a YAML mapping";
            return null;
        }

        error = null;
        return mapping;
    }
}
