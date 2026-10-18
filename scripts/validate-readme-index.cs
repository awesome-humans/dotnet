#!/usr/bin/env dotnet
#:include Opinions.cs
#:property Nullable=enable
#:property TreatWarningsAsErrors=true

// Verifies that README.md indexes the repository in both directions: every file
// under opinions/ has a linked Scope bullet and a Repository layout entry, every
// directory under skills/ has a row in the Maintenance via skills table, and none
// of those three places points at something that no longer exists. All three are
// also held to their order: Scope by title, the tree by file name, and the skills
// table by skill name.
//
// A .NET 10 file-based app: no project, no build step, sharing its helpers with the
// other checks via #:include. Run it from the repository root:
//
//     dotnet run scripts/validate-readme-index.cs
//     ./scripts/validate-readme-index.cs      (the shebang above)
//
// Exit codes: 0 clean, 1 findings printed.

using System.Text.RegularExpressions;

const string ReadmePath = "README.md";
const string SkillsDirectory = "skills";

if (!File.Exists(ReadmePath))
{
    Console.WriteLine("README.md not found");
    return 1;
}

string readme = File.ReadAllText(ReadmePath).ReplaceLineEndings("\n");
List<string> errors = [];

string? scope = Section(readme, "Scope");
string? layout = Section(readme, "Repository layout");
string? skillsTable = Section(readme, "Maintenance via skills");

(string Name, string? Body)[] required =
[
    ("Scope", scope),
    ("Repository layout", layout),
    ("Maintenance via skills", skillsTable),
];

foreach ((string name, string? body) in required)
{
    if (body is null)
    {
        errors.Add($"README.md: '## {name}' section not found — the index checks below cannot run");
    }
}

string[] opinions = Opinions.Names();

if (opinions.Length == 0)
{
    errors.Add($"no markdown files found under {Opinions.DirectoryName}/");
}

HashSet<string> names = [.. opinions];
HashSet<string> paths = [.. opinions.Select(Opinions.PathOf)];

if (scope is not null)
{
    // Opinion paths the Scope section links to, in any Markdown link style.
    HashSet<string> linked =
    [
        .. Patterns.InlineOpinionLink().Matches(scope).Select(match => match.Groups[1].Value),
        .. Patterns.ReferenceOpinionLink().Matches(scope).Select(match => match.Groups[1].Value),
    ];

    foreach (string path in paths)
    {
        if (!linked.Contains(path))
        {
            errors.Add($"README.md: Scope has no bullet linking to {path} — add one when adding an opinion");
        }
    }

    foreach (string stale in linked.Except(paths).Order(StringComparer.Ordinal))
    {
        errors.Add($"README.md: Scope links to {stale}, which no longer exists — drop or repoint the bullet");
    }

    // Scope is kept alphabetical so a new bullet lands in one predictable place. By title,
    // not file name ("Application architecture" is architecture.md), and ignoring case, so
    // "ASP.NET Core" sorts as a reader expects. An unlinked bullet (Libraries) is a summary
    // across the files above it rather than a topic of its own, so it stays last.
    string? previous = null;
    string? unlinked = null;

    foreach ((string title, bool isLink) in ScopeBullets(scope))
    {
        if (!isLink)
        {
            unlinked ??= title;
            continue;
        }

        if (unlinked is not null)
        {
            errors.Add(
                $"README.md: Scope bullet '{title}' follows the unlinked '{unlinked}' bullet "
                    + "— unlinked bullets stay last");
        }
        else if (previous is not null && StringComparer.OrdinalIgnoreCase.Compare(previous, title) > 0)
        {
            errors.Add($"README.md: Scope is not sorted by title — '{title}' follows '{previous}'");
        }

        previous = title;
    }
}

if (layout is not null)
{
    List<string> listed = TreeOpinions(layout);

    if (listed.Count == 0)
    {
        errors.Add(
            $"README.md: Repository layout tree has no `{Opinions.DirectoryName}/` node — the tree check cannot run");
    }

    foreach (string missing in names.Except(listed).Order(StringComparer.Ordinal))
    {
        errors.Add($"README.md: Repository layout tree does not list {Opinions.PathOf(missing)}");
    }

    foreach (string stale in listed.Except(names).Order(StringComparer.Ordinal))
    {
        errors.Add(
            $"README.md: Repository layout tree lists {Opinions.PathOf(stale)}, which no longer exists");
    }

    // Sorted by file name, the order Opinions.Names() and a directory listing both use.
    for (int index = 1; index < listed.Count; index++)
    {
        if (StringComparer.Ordinal.Compare(listed[index - 1], listed[index]) > 0)
        {
            errors.Add(
                $"README.md: Repository layout tree is not sorted under `{Opinions.DirectoryName}/` "
                    + $"— {listed[index]} follows {listed[index - 1]}");
        }
    }
}

HashSet<string> skills = [];
if (Directory.Exists(SkillsDirectory))
{
    skills = [.. new DirectoryInfo(SkillsDirectory).EnumerateDirectories().Select(directory => directory.Name)];
}

if (skills.Count == 0)
{
    errors.Add($"no skill directories found under {SkillsDirectory}/");
}

if (skillsTable is not null)
{
    List<string> rows = TableIds(skillsTable);

    if (rows.Count == 0)
    {
        errors.Add("README.md: no skill rows parsed from the 'Maintenance via skills' table");
    }

    foreach (string missing in skills.Except(rows).Order(StringComparer.Ordinal))
    {
        errors.Add($"README.md: 'Maintenance via skills' table has no row for `{missing}`");
    }

    foreach (string stale in rows.Except(skills).Order(StringComparer.Ordinal))
    {
        errors.Add(
            $"README.md: 'Maintenance via skills' table has a row for `{stale}`, which is not a skill directory");
    }

    // Sorted by skill name, the order a directory listing of skills/ uses, so a new row lands in
    // one predictable place. The pipeline diagram above the table shows the workflow order.
    for (int index = 1; index < rows.Count; index++)
    {
        if (StringComparer.Ordinal.Compare(rows[index - 1], rows[index]) > 0)
        {
            errors.Add(
                $"README.md: 'Maintenance via skills' table is not sorted by skill name "
                    + $"— `{rows[index]}` follows `{rows[index - 1]}`");
        }
    }
}

if (errors.Count > 0)
{
    Console.WriteLine("README index validation failed:");
    foreach (string error in errors)
    {
        Console.WriteLine($"  - {error}");
    }

    return 1;
}

Console.WriteLine(
    $"README indexes all {opinions.Length} opinion files and {skills.Count} skills, in order, with no stale entries.");
return 0;

// The body of a level-2 section, up to the next level-2 heading.
static string? Section(string text, string heading)
{
    // Every level-2 heading in one pass: the one whose text matches starts the
    // section, and the heading after it ends it. Matching headings generically
    // keeps the pattern constant, so it can be source-generated — interpolating
    // the wanted heading into the pattern could not (SYSLIB1045).
    MatchCollection headings = Patterns.SectionHeading().Matches(text);

    for (int index = 0; index < headings.Count; index++)
    {
        if (!headings[index].Groups[1].Value.Trim().Equals(heading, StringComparison.Ordinal))
        {
            continue;
        }

        int start = headings[index].Index + headings[index].Length;
        return index + 1 < headings.Count
            ? text[start..headings[index + 1].Index]
            : text[start..];
    }

    return null;
}

// Scope bullets in file order: each bullet's bold title, and whether that title is a link.
static List<(string Title, bool Linked)> ScopeBullets(string scope) =>
[
    .. Patterns.ScopeBullet().Matches(scope).Select(match => match.Groups[1].Success
        ? (match.Groups[1].Value, true)
        : (match.Groups[2].Value, false)),
];

// Files listed under the `opinions/` node of the repository layout tree, in tree order.
static List<string> TreeOpinions(string layout)
{
    List<string> listed = [];
    bool inside = false;

    foreach (string line in layout.Split('\n'))
    {
        if (Patterns.TreeOpinionsNode().IsMatch(line))
        {
            inside = true;
            continue;
        }

        if (!inside)
        {
            continue;
        }

        if (Patterns.TreeNode().IsMatch(line) || line.StartsWith("```", StringComparison.Ordinal))
        {
            break;
        }

        Match match = Patterns.MarkdownFileName().Match(line);
        if (match.Success)
        {
            listed.Add(match.Groups[1].Value);
        }
    }

    return listed;
}

// First-cell `code` ids from the Markdown table rows in a section, in table order.
static List<string> TableIds(string body)
{
    List<string> ids = [];

    foreach (string line in body.Split('\n'))
    {
        if (!line.StartsWith('|'))
        {
            continue;
        }

        string first = line.Trim().Trim('|').Split('|')[0].Trim();
        Match match = Patterns.TableId().Match(first);
        if (match.Success)
        {
            ids.Add(match.Groups[1].Value);
        }
    }

    return ids;
}

internal static partial class Patterns
{
    // A level-2 heading line; group 1 is the heading text.
    [GeneratedRegex(@"^## (.*)$", RegexOptions.Multiline)]
    internal static partial Regex SectionHeading();

    // Inline link to an opinion file, with an optional #anchor and optional "title".
    [GeneratedRegex(@"]\(\s*(opinions/[\w.-]+\.md)(?:#[^)\s]*)?(?:\s+""[^""]*"")?\s*\)")]
    internal static partial Regex InlineOpinionLink();

    // Reference-style link definition: [label]: opinions/x.md
    [GeneratedRegex(@"^\s*\[[^\]]+]:\s*(opinions/[\w.-]+\.md)", RegexOptions.Multiline)]
    internal static partial Regex ReferenceOpinionLink();

    // A Scope bullet's bold title: group 1 when it is a link, as in **[Title](...):**, and
    // group 2 when it is plain text, as in **Libraries:**.
    [GeneratedRegex(@"^- \*\*(?:\[([^\]]+)]|([^*]+?):?\*\*)", RegexOptions.Multiline)]
    internal static partial Regex ScopeBullet();

    [GeneratedRegex(@"^[├└]── opinions/")]
    internal static partial Regex TreeOpinionsNode();

    [GeneratedRegex(@"^[├└]── ")]
    internal static partial Regex TreeNode();

    [GeneratedRegex(@"([\w.-]+\.md)")]
    internal static partial Regex MarkdownFileName();

    // A first cell is a bare id, `vet-source`, or a linked one:
    // [`vet-source`](skills/vet-source/SKILL.md).
    [GeneratedRegex(@"\A(?:\[\s*)?`([\w.-]+)`(?:\s*\]\([^)]*\))?\z")]
    internal static partial Regex TableId();
}
