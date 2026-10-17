// Shared by the checks that read the one-line headers on templates/ files — pulled in
// with `#:include CommentHeader.cs`. Not a file-based app itself: it declares no top-level
// statements, and the YamlDotNet reference it compiles beside is declared by the script
// including it.
//
// Templates cannot carry YAML frontmatter: they are XML, INI, JSON and source files that
// have to stay valid for the tools that read them, and a `---` block would break every
// one. The same metadata goes in a comment on the first line instead, and this parses it:
//
//     <!-- awesome-humans/dotnet template | targets: net10.0 | last-reviewed: … | … -->
//     # awesome-humans/dotnet template | targets: net10.0 | last-reviewed: … | …
//     // awesome-humans/dotnet template | targets: net10.0 | last-reviewed: … | …

using System.Collections;

internal static class CommentHeader
{
    /// <summary>The literal every template header opens with, inside its comment marker.</summary>
    internal const string Marker = "awesome-humans/dotnet template";

    /// <summary>The directory whose files carry these headers, as the reports quote it.</summary>
    internal const string TemplatesDirectory = "templates";

    /// <summary>
    /// Every file under <see cref="TemplatesDirectory"/> expected to carry a header, sorted,
    /// as repository-relative paths with forward slashes so a finding reads the same whichever
    /// platform ran the check. Empty when the directory is absent.
    /// </summary>
    internal static string[] Files()
    {
        if (!Directory.Exists(TemplatesDirectory))
        {
            return [];
        }

        // Smoke-testing the example projects (audit-freshness, refresh-dotnet-versions) leaves
        // gitignored build output under templates/ — generated files, not resources. Skipping the
        // same directories .gitignore does keeps a post-build working tree and a fresh checkout
        // passing identically; CI is not the only place these checks run.
        string[] buildOutput = ["artifacts", "bin", "obj"];

        // JSON carries no comment syntax, so templates/global.json and the .slnf filter cannot hold
        // a header at all. Exactly those two are exempt, the same list AGENTS.md documents, so a
        // headerless JSON file added anywhere else under templates/ fails loudly rather than
        // silently escaping validation. What the two pin is audited against the latest releases
        // instead (skills/audit-freshness/SKILL.md).
        string[] exempt = [$"{TemplatesDirectory}/global.json", $"{TemplatesDirectory}/example.slnf"];

        return
        [
            .. new DirectoryInfo(TemplatesDirectory)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Select(file =>
                    $"{TemplatesDirectory}/{Path.GetRelativePath(TemplatesDirectory, file.FullName).Replace('\\', '/')}")
                .Where(path => !path.Split('/').Any(buildOutput.Contains))
                .Where(path => !Array.Exists(exempt, entry => string.Equals(entry, path, StringComparison.Ordinal)))
                .Order(StringComparer.Ordinal),
        ];
    }

    /// <summary>
    /// Parses a template file's first-line comment header. Returns the fields, or null with
    /// <paramref name="error"/> set to the reason — deliberately the same shape as
    /// <see cref="Frontmatter.Read"/>, so one required-field check covers both syntaxes and
    /// the two report identical wording. The returned mapping is keyed ordinally.
    /// </summary>
    internal static IDictionary? Read(string path, out string? error)
    {
        // Only the first line matters, so read only that. StreamReader.ReadLine ends a line
        // on \n, \r, or \r\n, so a CRLF checkout needs no normalising here; it also consumes a
        // UTF-8 BOM, which would otherwise sit in front of the comment marker.
        string line = (File.ReadLines(path).FirstOrDefault() ?? "").Trim();

        // XML comment first: an .editorconfig header opens with '#', an example source
        // file (.fs, .cs) with '//', and nothing else opens with either.
        string body;
        if (line.StartsWith("<!--", StringComparison.Ordinal) && line.EndsWith("-->", StringComparison.Ordinal))
        {
            body = line[4..^3];
        }
        else if (line.StartsWith("//", StringComparison.Ordinal))
        {
            body = line[2..];
        }
        else if (line.StartsWith('#'))
        {
            body = line[1..];
        }
        else
        {
            error = "first line carries no comment header";
            return null;
        }

        body = body.Trim();

        if (!body.StartsWith(Marker, StringComparison.Ordinal))
        {
            error = $"header does not open with '{Marker}'";
            return null;
        }

        Dictionary<string, string> fields = new(StringComparer.Ordinal);

        string[] segments = body[Marker.Length..]
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string segment in segments)
        {
            // Split on the first colon only: `targets: net10.0, csharp-14` is one field
            // whose value happens to be a list.
            int colon = segment.IndexOf(':');
            if (colon == -1)
            {
                error = $"header segment '{segment}' is not 'key: value'";
                return null;
            }

            string key = segment[..colon].Trim();

            if (!fields.TryAdd(key, segment[(colon + 1)..].Trim()))
            {
                error = $"header repeats the field '{key}'";
                return null;
            }
        }

        error = null;
        return fields;
    }
}
