#!/usr/bin/env dotnet
#:package YamlDotNet@18.1.0
#:include Opinions.cs
#:include Frontmatter.cs
#:property PublishAot=false
#:property Nullable=enable
#:property TreatWarningsAsErrors=true

// Stages the awesome-humans.net documentation site into site/build, ready for Zensical.
// The hand-written pages under site/pages are copied as they are. The .NET ecosystem's
// pages under /dotnet/ are generated from this repository's own files, so the site never
// carries a second copy to drift: README.md up to its layout section becomes the overview,
// opinions/ and skills/ are copied in full, the roster is split into a page per section,
// and every file under templates/ gets a page showing it. llms.txt at the root points agents
// at the raw files. Relative links are rewritten to the page they now point at, or to GitHub
// for a file the site does not carry. The navigation is generated too: the opinions take their titles and order from
// README.md's Scope list, and the skills theirs from its Maintenance via skills table, which
// validate-readme-index.cs already holds to the opinions and skills directories.
//
// Zensical requires the build output inside the directory holding its configuration, so
// the staged zensical.toml, docs/ and the built public/ all sit under site/build, which is
// gitignored. site/zensical.toml is the committed configuration this script stages, with
// the navigation added.
//
// A .NET 10 file-based app. Run it from the repository root, then build or preview the site:
//
//     dotnet run scripts/build-site.cs
//     uvx --with-requirements site/requirements.txt zensical build --strict -f site/build/zensical.toml
//     uvx --with-requirements site/requirements.txt zensical serve -f site/build/zensical.toml
//
// Exit codes: 0 staged, 1 a link or file the site needs could not be resolved, 2 not run
// from the repository root.

using System.Collections;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

const string SiteDirectory = "site";
const string PagesDirectory = "site/pages";
const string StagingDirectory = "site/build";
const string DocsDirectory = "site/build/docs";
const string DiagramsDirectory = "assets/diagrams";
const string TemplatesDirectory = "templates";
const string SkillsDirectory = "skills";
const string Ecosystem = "dotnet";
const string EcosystemName = ".NET";

if (!Directory.Exists(SiteDirectory) || !Opinions.DirectoryExists)
{
    Console.Error.WriteLine("No site/ or opinions/ directory here. Run this script from the repository root.");
    return 2;
}

List<string> errors = [];

if (Directory.Exists(DocsDirectory))
{
    Directory.Delete(DocsDirectory, recursive: true);
}

// Hand-written pages, then the diagrams they and the generated pages show.
foreach (string file in Directory.EnumerateFiles(PagesDirectory, "*", SearchOption.AllDirectories))
{
    Write(Path.GetRelativePath(PagesDirectory, file), File.ReadAllText(file));
}

foreach (string file in Directory.EnumerateFiles(DiagramsDirectory, "*.svg"))
{
    Write($"{DiagramsDirectory}/{Path.GetFileName(file)}", File.ReadAllText(file));
}

// Where each repository file lands on the site. A link to anything else goes to GitHub.
string[] templates =
[
    .. Directory.EnumerateFiles(TemplatesDirectory, "*", SearchOption.AllDirectories)
        .Select(Site.Normalise)
        .Where(path => !path.Split('/').Any(part => part is "artifacts" or "bin" or "obj"))
        .Order(StringComparer.OrdinalIgnoreCase),
];

string readme = File.ReadAllText("README.md").ReplaceLineEndings("\n");
Match[] skills = Site.SkillRow().Matches(readme).ToArray();

Dictionary<string, string> pages = new(StringComparer.Ordinal)
{
    ["README.md"] = $"{Ecosystem}/index.md",
    ["AWESOME-HUMANS.md"] = $"{Ecosystem}/awesome-humans/index.md",
    ["HOUSE-OPINIONS.md"] = $"{Ecosystem}/house-opinions.md",
    [Opinions.DirectoryName] = $"{Ecosystem}/opinions/index.md",
    [TemplatesDirectory] = $"{Ecosystem}/templates/index.md",
    [$"{TemplatesDirectory}/projects"] = $"{Ecosystem}/templates/index.md",
    [SkillsDirectory] = $"{Ecosystem}/skills/index.md",
};

foreach (Match skill in skills)
{
    string name = skill.Groups["name"].Value;
    pages[$"{SkillsDirectory}/{name}"] = $"{Ecosystem}/skills/{name}.md";
    pages[Site.SkillPath(name)] = $"{Ecosystem}/skills/{name}.md";
}

foreach (string name in Opinions.Names())
{
    pages[Opinions.PathOf(name)] = $"{Ecosystem}/opinions/{name}";
}

foreach (string template in templates)
{
    pages[template] = $"{Ecosystem}/templates/{Site.TemplatePage(template)}";
}

foreach (string file in Directory.EnumerateFiles(DiagramsDirectory, "*.svg"))
{
    pages[Site.Normalise(file)] = $"{DiagramsDirectory}/{Path.GetFileName(file)}";
}

// The roster, split so that no page carries all of it. The overview holds the admission
// criteria and the shape of a source's notes, each roster table gets a page beside the notes
// for the sources in it, and the decision log gets its own. A heading's anchor resolves to
// the page now holding it, preferring the linking page when two pages share one.
const string RosterPath = "AWESOME-HUMANS.md";
string roster = File.ReadAllText(RosterPath).ReplaceLineEndings("\n");
Dictionary<string, string> rosterPages = new(StringComparer.Ordinal)
{
    ["citable"] = $"{Ecosystem}/awesome-humans/citable.md",
    ["watch-list"] = $"{Ecosystem}/awesome-humans/watch-list.md",
    ["decision-log"] = $"{Ecosystem}/awesome-humans/decision-log.md",
};
string[] rosterSections = Site.SplitBefore(roster, "## ");
Dictionary<string, string> rosterText = new(StringComparer.Ordinal) { [pages[RosterPath]] = rosterSections[0] };
foreach (string section in rosterSections.Skip(1))
{
    if (section.StartsWith("## Admission criteria", StringComparison.Ordinal))
    {
        rosterText[pages[RosterPath]] += section;
    }
    else if (section.StartsWith("## Citable", StringComparison.Ordinal))
    {
        rosterText[rosterPages["citable"]] = section[1..] + "\n## Source notes\n\n";
    }
    else if (section.StartsWith("## Watch list", StringComparison.Ordinal))
    {
        rosterText[rosterPages["watch-list"]] = section[1..] + "\n## Source notes\n\n";
    }
    else if (section.StartsWith("## Decision log", StringComparison.Ordinal))
    {
        rosterText[rosterPages["decision-log"]] = section[1..];
    }
    else if (section.StartsWith("## Source notes", StringComparison.Ordinal))
    {
        string[] notes = Site.SplitBefore(section, "### ");
        rosterText[pages[RosterPath]] += notes[0];
        foreach (string note in notes.Skip(1))
        {
            string? bucket = note.StartsWith("### `", StringComparison.Ordinal) && note.Contains("`: Citable", StringComparison.Ordinal) ? "citable"
                : note.StartsWith("### `", StringComparison.Ordinal) && note.Contains("`: Watch list", StringComparison.Ordinal) ? "watch-list"
                : null;
            if (bucket is null || !rosterText.ContainsKey(rosterPages[bucket]))
            {
                errors.Add($"{RosterPath}: no roster page for the notes headed {note[..note.IndexOf('\n')]}");
                continue;
            }

            rosterText[rosterPages[bucket]] += note;
        }
    }
    else
    {
        errors.Add($"{RosterPath}: no site page for the section headed {section[..section.IndexOf('\n')]}; add one in build-site.cs");
    }
}

Dictionary<string, List<(string Page, bool Title)>> rosterAnchors = new(StringComparer.Ordinal);
foreach ((string page, string text) in rosterText)
{
    foreach (Match heading in Site.Heading().Matches(text))
    {
        string slug = Site.Slug(heading.Groups["text"].Value);
        if (!rosterAnchors.TryGetValue(slug, out List<(string Page, bool Title)>? found))
        {
            rosterAnchors[slug] = found = [];
        }

        found.Add((page, heading.Groups["hashes"].Value.Length == 1));
    }
}

// The overview: README.md up to its layout section, which is about the repository rather
// than the opinions, without the rule for contributors adding an opinion file.
int layout = readme.IndexOf("\n## Repository layout", StringComparison.Ordinal);
string overview = string.Join(
    "\n\n",
    readme[..layout].Split("\n\n").Where(block => !block.StartsWith("A new opinion file must appear", StringComparison.Ordinal)));
Write(pages["README.md"], Rewrite("README.md", overview + "\n", anchorsToGitHub: true));

foreach ((string page, string text) in rosterText)
{
    Write(page, Rewrite(RosterPath, text.TrimEnd('\n') + "\n", from: page));
}

Write(pages["HOUSE-OPINIONS.md"], Rewrite("HOUSE-OPINIONS.md", File.ReadAllText("HOUSE-OPINIONS.md")));

// Opinions, each with a line under its title giving the metadata from its frontmatter.
IDeserializer deserializer = new DeserializerBuilder().Build();
foreach (string name in Opinions.Names())
{
    string path = Opinions.PathOf(name);
    IDictionary? meta = Frontmatter.Read(deserializer, path, out string? error);
    if (meta is null)
    {
        errors.Add($"{path}: {error}");
        continue;
    }

    string line = string.Join(
        " · ",
        $"**Targets:** {string.Join(", ", Site.List(meta["targets"]).Select(target => $"`{target}`"))}",
        $"**Last reviewed:** {meta["last-reviewed"]}",
        $"**Sources:** {string.Join(", ", Site.List(meta["sources"]).Select(id => SourceLink(path, id)))}");
    string text = Rewrite(path, File.ReadAllText(path));
    int title = text.IndexOf("\n# ", StringComparison.Ordinal);
    int titleEnd = text.IndexOf('\n', title + 1);
    Write(pages[path], text[..titleEnd] + "\n\n" + line + text[titleEnd..]);
}

// An index of the opinions, in the order and with the summaries of README.md's Scope list.
Write(
    pages[Opinions.DirectoryName],
    "# Opinions\n\n"
    + "One topic per page. Each opinion states its recommendation first, then the rationale, then its sources, and targets the latest released versions.\n\n"
    + string.Concat(Site.ScopeBullet().Matches(readme).Select(bullet =>
        $"- **[{bullet.Groups["title"].Value}]({Path.GetFileName(bullet.Groups["path"].Value)}):**{bullet.Groups["summary"].Value}\n")));

// A page per template file, and an index of them.
List<string> index =
[
    "# Templates\n\n",
    "Copy-paste-ready files that encode the opinions. Copy one verbatim and trim it, rather than writing your own from scratch. ",
    "Each opens with a comment header giving its targets, review date and sources, except `global.json` and `example.slnf`, which as JSON have no comment syntax.\n",
];
foreach (IGrouping<string, string> group in templates.GroupBy(template => Path.GetDirectoryName(template)!.Replace('\\', '/')))
{
    index.Add($"\n## `{group.Key}/`\n\n");
    foreach (string template in group)
    {
        index.Add($"- [`{Path.GetFileName(template)}`]({Site.TemplatePage(template)})\n");

        string content = File.ReadAllText(template).ReplaceLineEndings("\n").TrimEnd('\n');
        string fence = new('`', Math.Max(3, Site.LongestBacktickRun(content) + 1));
        Write(
            pages[template],
            $"# `{Path.GetFileName(template)}`\n\n"
            + $"[View on GitHub]({Site.GitHub(template, isDirectory: false)}) · [Raw]({Site.Raw(template)})\n\n"
            + $"{fence}{Site.Language(template)}\n{content}\n{fence}\n");
    }
}

Write(pages[TemplatesDirectory], string.Concat(index));

// A page per skill, its SKILL.md without the frontmatter, and an index of them in the order
// of README.md's table, which validate-readme-index.cs holds to the skills directory.
List<string> skillIndex =
[
    "# Skills\n\n",
    "Agent skills that check a project against the opinions and keep the opinions current. ",
    "They follow the [Agent Skills specification](https://agentskills.io/specification), so any compliant agent can run them. ",
    "To run one, clone the repository and ask your agent to run the skill by name. ",
    $"If your agent only discovers skills in a directory of its own, see [AGENTS.md: Skills]({Site.GitHub("AGENTS.md", isDirectory: false)}#skills).\n\n",
    "`verify-project` checks your own project, and `research-topic` answers a .NET question from the opinions and their sources. ",
    "The others maintain this repository.\n\n",
    "| Skill | Purpose |\n| --- | --- |\n",
];
foreach (Match skill in skills)
{
    string name = skill.Groups["name"].Value;
    string path = Site.SkillPath(name);
    if (!File.Exists(path))
    {
        errors.Add($"README.md: the skills table lists {name}, but {path} does not exist");
        continue;
    }

    skillIndex.Add($"| [`{name}`]({name}.md) | {skill.Groups["purpose"].Value} |\n");

    string text = File.ReadAllText(path).ReplaceLineEndings("\n");
    int end = text.StartsWith("---\n", StringComparison.Ordinal) ? text.IndexOf("\n---\n", 4, StringComparison.Ordinal) : -1;
    if (end == -1)
    {
        errors.Add($"{path}: no frontmatter block to strip");
        continue;
    }

    string body = Rewrite(path, text[(end + "\n---\n".Length)..].TrimStart('\n'));
    int titleEnd = body.IndexOf('\n', StringComparison.Ordinal);
    Write(
        pages[path],
        body[..titleEnd] + "\n\n"
        + $"[View on GitHub]({Site.GitHub(path, isDirectory: false)}) · [Raw]({Site.Raw(path)})"
        + body[titleEnd..]);
}

Write(pages[SkillsDirectory], string.Concat(skillIndex));

// llms.txt at the site root, per llmstxt.org: a summary and links an agent can follow, to
// the raw Markdown of each opinion and the raw template files rather than rendered pages.
List<string> llms =
[
    "# Awesome Humans\n\n",
    "> Opinionated best practices for each software ecosystem, distilled from the published writing of people and publications with a proven, multi-year track record. ",
    "Each opinion states its recommendation first, then the rationale, then its sources, and targets the latest released versions.\n\n",
    $"## {EcosystemName} opinions\n\n",
    "Follow these when writing or reviewing .NET code. Target the versions in each file's `targets:` frontmatter, never an older one.\n\n",
    .. Site.ScopeBullet().Matches(readme).Select(bullet =>
        $"- [{bullet.Groups["title"].Value}]({Site.Raw(bullet.Groups["path"].Value)}): {bullet.Groups["summary"].Value.Trim()}\n"),
    $"\n## {EcosystemName} templates\n\n",
    "Copy-paste-ready starting points that encode the opinions. Copy one verbatim and trim it.\n\n",
    .. templates.Select(template => $"- [{Site.Normalise(Path.GetRelativePath(TemplatesDirectory, template))}]({Site.Raw(template)})\n"),
    $"\n## {EcosystemName} skills\n\n",
    "Agent Skills procedures. Run verify-project to check a .NET project against the opinions. The others maintain the repository.\n\n",
    .. skills.Select(skill => $"- [{skill.Groups["name"].Value}]({Site.Raw(Site.SkillPath(skill.Groups["name"].Value))}): {skill.Groups["purpose"].Value}\n"),
    "\n## Optional\n\n",
    $"- [Awesome humans]({Site.Raw("AWESOME-HUMANS.md")}): the vetted sources the opinions cite, and the admission criteria\n",
    $"- [House opinions]({Site.Raw("HOUSE-OPINIONS.md")}): the repository owner's own preferences, marked House: where they appear\n",
];
Write("llms.txt", string.Concat(llms));

// The navigation, added to the committed configuration as it is staged.
string Entry(string title, string page) => $"{{ {Site.TomlString(title)} = {Site.TomlString(page)} }}";
string Section(string title, IEnumerable<string> entries) => $"{{ {Site.TomlString(title)} = [{string.Join(", ", entries)}] }}";

List<string> opinionEntries = [];
foreach (Match bullet in Site.ScopeBullet().Matches(readme))
{
    opinionEntries.Add(Entry(bullet.Groups["title"].Value, pages[bullet.Groups["path"].Value]));
}

if (opinionEntries.Count != Opinions.Names().Length)
{
    errors.Add($"README.md: Scope lists {opinionEntries.Count} opinions but opinions/ holds {Opinions.Names().Length}; run validate-readme-index.cs");
}

string[] nav =
[
    Entry("Home", "index.md"),
    Entry("How it works", "how-it-works.md"),
    Section(EcosystemName,
    [
        Entry("Overview", pages["README.md"]),
        Section("Opinions", [Entry("Overview", pages[Opinions.DirectoryName]), .. opinionEntries]),
        Section("Templates", [Entry("Overview", pages[TemplatesDirectory]), .. templates.Select(template => Entry(Path.GetFileName(template), pages[template]))]),
        Section("Skills", [Entry("Overview", pages[SkillsDirectory]), .. skills.Select(skill => Entry(skill.Groups["name"].Value, pages[Site.SkillPath(skill.Groups["name"].Value)]))]),
        Section("Awesome humans",
        [
            Entry("Overview", pages[RosterPath]),
            Entry("Citable", rosterPages["citable"]),
            Entry("Watch list", rosterPages["watch-list"]),
            Entry("Decision log", rosterPages["decision-log"]),
        ]),
        Entry("House opinions", pages["HOUSE-OPINIONS.md"]),
    ]),
];

string config = File.ReadAllText(Path.Combine(SiteDirectory, "zensical.toml")).ReplaceLineEndings("\n");
const string ProjectTable = "[project]\n";
int project = config.IndexOf(ProjectTable, StringComparison.Ordinal);
if (project == -1)
{
    errors.Add("site/zensical.toml: no [project] table to add the navigation to");
}
else
{
    config = config.Insert(project + ProjectTable.Length, "nav = [\n  " + string.Join(",\n  ", nav) + ",\n]\n");
    File.WriteAllText(Path.Combine(StagingDirectory, "zensical.toml"), config);
}

foreach (string error in errors)
{
    Console.Error.WriteLine(error);
}

if (errors.Count > 0)
{
    return 1;
}

Console.WriteLine($"Staged to {StagingDirectory}/");
return 0;

// A source id links to its notes section in the roster, or to the citable table when it has
// nothing to record and so no section, and house links to the house opinions.
string SourceLink(string path, string id)
{
    string from = Path.GetDirectoryName(path) ?? "";
    if (id == "house")
    {
        return $"[house]({Target(path, pages[path], Site.Normalise(Path.GetRelativePath(from, "HOUSE-OPINIONS.md")), anchorsToGitHub: false)})";
    }

    Match heading = Regex.Match(roster, $@"^### (`{Regex.Escape(id)}`.*)$", RegexOptions.Multiline);
    string anchor = heading.Success ? Site.Slug(heading.Groups[1].Value) : "citable-quotable-in-opinions-and-templates";
    return $"[{id}]({Target(path, pages[path], $"{Site.Normalise(Path.GetRelativePath(from, RosterPath))}#{anchor}", anchorsToGitHub: false)})";
}

// Rewrites every relative link in a Markdown file, read from its repository path, to point
// from its page on the site, and escapes a heading's trailing # (C#, Testing F#), which
// GitHub keeps but Zensical reads as a closing sequence. Fenced code blocks are left alone.
string Rewrite(string source, string text, bool anchorsToGitHub = false, string? from = null)
{
    from ??= pages[source];
    bool fenced = false;
    IEnumerable<string> lines = text.ReplaceLineEndings("\n").Split('\n').Select(line =>
    {
        if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
        {
            fenced = !fenced;
        }

        if (fenced)
        {
            return line;
        }

        line = Site.HeadingTrailingHash().Replace(line, match => match.Groups["text"].Value + match.Groups["hashes"].Value.Replace("#", "\\#", StringComparison.Ordinal));
        return Site.MarkdownLink().Replace(line, match => $"]({Target(source, from, match.Groups["target"].Value, anchorsToGitHub)})");
    });
    return string.Join("\n", lines);
}

string Target(string source, string from, string target, bool anchorsToGitHub)
{
    if (target.Contains("://", StringComparison.Ordinal) || target.StartsWith("mailto:", StringComparison.Ordinal))
    {
        return target;
    }

    int hash = target.IndexOf('#');
    string path = hash == -1 ? target : target[..hash];
    string anchor = hash == -1 ? "" : target[hash..];
    if (path.Length == 0 && anchorsToGitHub)
    {
        return Site.GitHub(source, isDirectory: false) + anchor;
    }

    string resolved = path.Length == 0
        ? source
        : Site.Normalise(Path.GetRelativePath(".", Path.Combine(Path.GetDirectoryName(source) ?? "", path))).TrimEnd('/');
    if (resolved == RosterPath && anchor.Length > 1)
    {
        if (!rosterAnchors.TryGetValue(anchor[1..], out List<(string Page, bool Title)>? found))
        {
            errors.Add($"{source}: link to {target} names no heading in {RosterPath}");
            return target;
        }

        (string sectionPage, bool title) = found.FirstOrDefault(candidate => candidate.Page == from, found[0]);
        string relative = sectionPage == from ? "" : Site.Normalise(Path.GetRelativePath(Path.GetDirectoryName(from) ?? "", sectionPage));
        return title ? (relative.Length == 0 ? "#" : relative) : relative + anchor;
    }

    if (path.Length == 0)
    {
        return target;
    }

    if (pages.TryGetValue(resolved, out string? page))
    {
        return Site.Normalise(Path.GetRelativePath(Path.GetDirectoryName(from) ?? "", page)) + anchor;
    }

    if (File.Exists(resolved) || Directory.Exists(resolved))
    {
        return Site.GitHub(resolved, Directory.Exists(resolved)) + anchor;
    }

    errors.Add($"{source}: link to {target} resolves to {resolved}, which does not exist");
    return target;
}

void Write(string page, string content)
{
    string path = Path.Combine(DocsDirectory, page);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, content);
}

internal static partial class Site
{
    private const string Repository = "si618/dotnet-awesome-humans";

    public static string Normalise(string path) => path.Replace('\\', '/');

    public static string GitHub(string path, bool isDirectory) =>
        $"https://github.com/{Repository}/{(isDirectory ? "tree" : "blob")}/main/{path}";

    public static string SkillPath(string name) => $"skills/{name}/SKILL.md";

    public static string Raw(string path) => $"https://raw.githubusercontent.com/{Repository}/main/{path}";

    // Zensical skips dotfiles, so .editorconfig's page drops the leading dot.
    public static string TemplatePage(string template) =>
        Normalise(Path.GetRelativePath("templates", template)).Replace("/.", "/", StringComparison.Ordinal).TrimStart('.') + ".md";

    public static string Language(string path) => Path.GetExtension(path) switch
    {
        ".props" or ".csproj" or ".fsproj" or ".slnx" => "xml",
        ".json" or ".slnf" => "json",
        ".editorconfig" => "ini",
        ".cs" => "csharp",
        ".fs" => "fsharp",
        _ => "text",
    };

    // Splits Markdown before each line that starts with the given heading prefix, keeping the
    // text ahead of the first such line as the first part.
    public static string[] SplitBefore(string text, string prefix) =>
        Regex.Split(text, $"(?=^{Regex.Escape(prefix)})", RegexOptions.Multiline);

    public static int LongestBacktickRun(string text) =>
        BacktickRun().Matches(text).Select(match => match.Length).DefaultIfEmpty(0).Max();

    public static IEnumerable<string> List(object? value) =>
        value is IEnumerable<object> items ? items.Select(item => item.ToString() ?? "") : [value?.ToString() ?? ""];

    // GitHub's heading anchors, which the rest of the repository already links by: lower
    // case, punctuation dropped, spaces to hyphens.
    public static string Slug(string heading) =>
        NotSlug().Replace(heading.ToLowerInvariant(), "").Replace(' ', '-');

    public static string TomlString(string value) =>
        "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

    // A bullet in README.md's Scope list: - **[Title](opinions/file.md):** what it covers.
    [GeneratedRegex(@"^- \*\*\[(?<title>[^\]]+)\]\((?<path>opinions/[^)]+)\):\*\*(?<summary>.*)$", RegexOptions.Multiline)]
    public static partial Regex ScopeBullet();

    // A row in README.md's Maintenance via skills table: | [`name`](skills/name/SKILL.md) | purpose |.
    [GeneratedRegex(@"^\| \[`(?<name>[^`]+)`\]\(skills/[^)]+\)\s*\|\s*(?<purpose>.*?)\s*\|$", RegexOptions.Multiline)]
    public static partial Regex SkillRow();

    [GeneratedRegex(@"^(?<text>#{1,6} .*[^\s\\#])(?<hashes>#+)$")]
    public static partial Regex HeadingTrailingHash();

    [GeneratedRegex(@"^(?<hashes>#{1,6}) (?<text>.+)$", RegexOptions.Multiline)]
    public static partial Regex Heading();

    [GeneratedRegex(@"\]\((?<target>[^)\s]+)\)")]
    public static partial Regex MarkdownLink();

    [GeneratedRegex("`+")]
    private static partial Regex BacktickRun();

    [GeneratedRegex(@"[^\p{L}\p{Nd}\p{Pc} -]")]
    private static partial Regex NotSlug();
}
