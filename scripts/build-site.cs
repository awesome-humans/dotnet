#!/usr/bin/env dotnet
#:package YamlDotNet@18.1.0
#:include Opinions.cs
#:include Frontmatter.cs
#:include House.cs
#:property PublishAot=false
#:property Nullable=enable
#:property TreatWarningsAsErrors=true

// Stages the .NET ecosystem's pages of the awesome-humans.net site, under /dotnet/. They're
// generated from this repository's own files, so the site never carries a second copy to
// drift: README.md up to its layout section becomes the overview, opinions/, research/, and skills/ are
// copied in full, the roster is split into a page per section, and every file under
// templates/ gets a page showing it. llms.txt points agents at the raw files. Relative links are rewritten to the page they now point at, or to GitHub
// for a file the site does not carry. The navigation is generated too: the opinions take their titles and order from
// README.md's Scope list, and the skills theirs from its Maintenance via skills table, which
// validate-readme-index.cs already holds to the opinions and skills directories.
//
// It follows the awesome-humans/website staging contract
// (https://github.com/awesome-humans/website#staging-contract): everything goes under
// <dir>/dotnet/, with the diagrams in dotnet/assets/, the section's navigation in nav.json,
// the roster in sources.json, and this ecosystem's own llms.txt. The website repository
// adds the shared pages, the configuration, and the root llms.txt, and builds the site.
//
// House opinions stay in the pages as house blocks, and the site shows a profile's only to a
// reader who chooses it. A template with house regions shows its default copy, with each
// profile's copy in a house block below it. Agents get the same choice as files:
// llms.txt links copies of the opinions and templates with every house block and region
// removed, and llms-<profile>.txt links copies with only that profile's, under raw/.
//
// A .NET 10 file-based app. Run it from the repository root, through its recipe or
// directly. awesome-humans/website runs it directly, because it doesn't install just:
//
//     just build-site <dir>
//     dotnet run scripts/build-site.cs -- --out <dir>
//
// Exit codes: 0 staged, 1 a link or file the site needs could not be resolved, 2 not run
// from the repository root, or arguments it doesn't recognise.

using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

const string TemplatesDirectory = "templates";
const string SkillsDirectory = "skills";
const string ResearchDirectory = "research";
const string Ecosystem = "dotnet";
const string EcosystemName = ".NET";
const string SchemaBase = "https://raw.githubusercontent.com/awesome-humans/website/main/schemas";
const string SiteUrl = "https://awesome-humans.net/";

// The default profile, which carries no house opinions. Its copies go under raw/none/.
const string NoProfile = "none";

// Days after its last-reviewed date that an opinion counts as stale, from audit-freshness.
const int OpinionTolerance = 180;

// The repository's diagrams, and where they land on the site.
const string DiagramsDirectory = "assets/diagrams";

if (args is not ["--out", string docsDirectory and not ""])
{
    Console.Error.WriteLine("Usage: dotnet run scripts/build-site.cs -- --out <dir>");
    return 2;
}

string diagramsPage = $"{Ecosystem}/{DiagramsDirectory}";

if (!Opinions.DirectoryExists)
{
    Console.Error.WriteLine("No opinions/ directory here. Run this script from the repository root.");
    return 2;
}

List<string> errors = [];

// Other ecosystems may already have staged beside this one, so only this ecosystem's
// directory is cleared.
string cleared = Path.Combine(docsDirectory, Ecosystem);
if (Directory.Exists(cleared))
{
    Directory.Delete(cleared, recursive: true);
}

// The diagrams that the generated pages, and the website repository's own pages, show.
foreach (string file in Directory.EnumerateFiles(DiagramsDirectory, "*.svg"))
{
    Write($"{diagramsPage}/{Path.GetFileName(file)}", File.ReadAllText(file));
}

// Where each repository file lands on the site. A link to anything else goes to GitHub.
string[] templates =
[
    .. Directory.EnumerateFiles(TemplatesDirectory, "*", SearchOption.AllDirectories)
        .Select(Site.Normalise)
        .Where(path => !path.Split('/').Any(part => part is "artifacts" or "bin" or "obj"))
        .Order(StringComparer.OrdinalIgnoreCase),
];

// The opinions and templates that hold house opinions, and the profiles they belong to.
// validate-sources.cs holds each one to its sources list, so a malformed one is an error
// here too, rather than a page that shows the wrong thing.
Dictionary<string, string[]> housed = new(StringComparer.Ordinal);
foreach (string path in Opinions.Names().Select(Opinions.PathOf).Concat(templates))
{
    string[] found = [.. House.Spans(path, File.ReadAllText(path), errors).Select(span => span.Profile).Distinct().Order(StringComparer.Ordinal)];
    if (found.Length > 0)
    {
        housed[path] = found;
    }
}

string[] profiles = [.. housed.Values.SelectMany(found => found).Distinct().Order(StringComparer.Ordinal)];

// The unresolved research topics, ordered by title. resolve-research deletes a topic once
// it's resolved, so every file here is open.
(string Path, string Title)[] research =
[
    .. (Directory.Exists(ResearchDirectory) ? Directory.EnumerateFiles(ResearchDirectory, "*.md") : [])
        .Select(path => (Site.Normalise(path), Site.Heading().Matches(File.ReadAllText(path)).First(heading => heading.Groups["hashes"].Value.Length == 1).Groups["text"].Value))
        .OrderBy(topic => topic.Item2, StringComparer.OrdinalIgnoreCase),
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
    [ResearchDirectory] = $"{Ecosystem}/research/index.md",
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

foreach ((string path, _) in research)
{
    pages[path] = $"{Ecosystem}/research/{Path.GetFileName(path)}";
}

foreach (string template in templates)
{
    pages[template] = $"{Ecosystem}/templates/{Site.TemplatePage(template)}";
}

foreach (string file in Directory.EnumerateFiles(DiagramsDirectory, "*.svg"))
{
    pages[Site.Normalise(file)] = $"{diagramsPage}/{Path.GetFileName(file)}";
}

// The roster, split so that no page carries all of it. The overview holds the admission
// criteria and the shape of a source's notes, each roster table gets a page beside the notes
// for the sources in it, each collapsed so the page reads as a list of sources, and the
// decision log gets its own. A heading's anchor resolves to the page now holding it,
// preferring the linking page when two pages share one. A source's collapsed notes keep
// the anchor its heading had, and javascripts/open-details.js opens them when linked to.
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
    if (section.StartsWith("## Admission criteria", StringComparison.Ordinal) || section.StartsWith("## Opted out", StringComparison.Ordinal))
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

            rosterText[rosterPages[bucket]] += Site.Collapsible(note);
        }
    }
    else
    {
        errors.Add($"{RosterPath}: no site page for the section headed {section[..section.IndexOf('\n')]}; add one in build-site.cs");
    }
}

// Each id in a roster table links to the source's notes on the same page, when it has any.
foreach (string bucket in (string[])["citable", "watch-list"])
{
    string page = rosterPages[bucket];
    rosterText[page] = Site.TableId().Replace(rosterText[page], row =>
    {
        string id = row.Groups["id"].Value;
        Match note = Regex.Match(rosterText[page], $@"^<details id=""({Regex.Escape(id)}-(?:citable|watch-list)[^""]*)""", RegexOptions.Multiline);
        return note.Success ? $"| [`{id}`](#{note.Groups[1].Value}) |" : row.Value;
    });
}

Dictionary<string, List<(string Page, bool Title)>> rosterAnchors = new(StringComparer.Ordinal);
foreach ((string page, string text) in rosterText)
{
    foreach (Match heading in Site.Heading().Matches(text))
    {
        string slug = heading.Groups["id"].Success ? heading.Groups["id"].Value : Site.Slug(heading.Groups["text"].Value);
        if (!rosterAnchors.TryGetValue(slug, out List<(string Page, bool Title)>? found))
        {
            rosterAnchors[slug] = found = [];
        }

        found.Add((page, heading.Groups["hashes"].Value.Length == 1));
    }
}

// The overview: README.md up to its layout section, which is about the repository rather
// than the opinions.
int layout = readme.IndexOf("\n## Repository layout", StringComparison.Ordinal);
string overview = readme[..layout];
Write(pages["README.md"], Rewrite("README.md", overview + "\n", anchorsToGitHub: true));

foreach ((string page, string text) in rosterText)
{
    Write(page, Rewrite(RosterPath, text.TrimEnd('\n') + "\n", from: page));
}

Write(pages["HOUSE-OPINIONS.md"], Rewrite("HOUSE-OPINIONS.md", File.ReadAllText("HOUSE-OPINIONS.md")));

// Opinions, each with its metadata strip and freshness chip, which the weekly rebuild keeps
// current.
IDeserializer deserializer = new DeserializerBuilder().Build();
DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
foreach (string name in Opinions.Names())
{
    WithMeta(Opinions.PathOf(name), freshness: true, lead: "");
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

        string text = File.ReadAllText(template).ReplaceLineEndings("\n");
        string content = House.ForProfile(template, text, null).TrimEnd('\n');
        string fence = new('`', Math.Max(3, Site.LongestBacktickRun(text) + 1));
        string variants = string.Concat(housed.GetValueOrDefault(template, []).Select(profile =>
            $"\n/// house | {profile}\n\n"
            + $"**House:** With this house profile, the file reads:\n\n"
            + $"{fence}{Site.Language(template)}\n{House.ForProfile(template, text, profile).TrimEnd('\n')}\n{fence}\n\n///\n"));
        Write(
            pages[template],
            $"# `{Path.GetFileName(template)}`\n\n"
            + $"[View on GitHub]({Site.GitHub(template, isDirectory: false)}) · [Raw]({RawLink(template, null)})\n\n"
            + $"{fence}{Site.Language(template)}\n{content}\n{fence}\n"
            + variants);
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
    "`review-project` checks your own project, and `research-topic` answers a .NET question from the opinions and their sources. ",
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

// A page per research topic, with the same strip as an opinion but no freshness chip, since
// a topic is evidence waiting on resolve-research rather than guidance to keep current. An
// index lists them. llms.txt leaves them out, so agents follow only the opinions.
foreach ((string path, _) in research)
{
    WithMeta(path, freshness: false, lead: "**Unresolved research.** This topic is evidence that `research-topic` gathered, waiting on `resolve-research`. It isn't an opinion, and it can cite unvetted material. Follow the [opinions](../opinions/index.md) instead.\n\n");
}

Write(
    pages[ResearchDirectory],
    "# Research\n\n"
    + "Topics under research: the evidence `research-topic` gathered, before `resolve-research` decides what becomes an opinion. "
    + "Resolving a topic deletes it, so every topic here is still open. None of it is guidance yet.\n\n"
    + (research.Length == 0 ? "No topics are open.\n" : string.Concat(research.Select(topic => $"- [{topic.Title}]({Path.GetFileName(topic.Path)})\n"))));

// Copies of each opinion and template that holds house opinions, for agents: one with every
// house block or region removed, and one per profile it holds, with only that profile's.
// An opinion's copy ends in .md.txt, because the site renders every .md file as a page.
foreach ((string path, string[] found) in housed)
{
    string text = File.ReadAllText(path);
    foreach (string? profile in found.Prepend(null))
    {
        Write($"{Ecosystem}/{RawPage(path, profile)}", House.ForProfile(path, text, profile));
    }
}

// llms.txt, per llmstxt.org: a summary and links an agent can follow, to the raw Markdown
// of each opinion and the raw template files rather than rendered pages. It describes this
// ecosystem alone, at /dotnet/llms.txt, and the website repository's root llms.txt links to
// it. It carries no house opinions, and llms-<profile>.txt carries one profile's. The raw
// string takes this file's line endings, which a Windows checkout can make CRLF, so the
// result is normalised to LF like every file the script reads.
const string LlmsSummary = "Opinionated best practices for building with .NET, distilled from the published writing of people and publications with a proven, multi-year track record. Each opinion states its recommendation first, then the rationale, then its sources, and targets the latest released versions.";
string skillLinks = string.Join("\n", skills.Select(skill =>
    $"- [{skill.Groups["name"].Value}]({Site.Raw(Site.SkillPath(skill.Groups["name"].Value))}): {skill.Groups["purpose"].Value}"));

foreach (string? profile in profiles.Prepend(null))
{
    string opinionLinks = string.Join("\n", Site.ScopeBullet().Matches(readme).Select(bullet =>
        $"- [{bullet.Groups["title"].Value}]({RawLink(bullet.Groups["path"].Value, profile)}): {bullet.Groups["summary"].Value.Trim()}"));
    string templateLinks = string.Join("\n", templates.Select(template =>
        $"- [{Site.Normalise(Path.GetRelativePath(TemplatesDirectory, template))}]({RawLink(template, profile)})"));
    string house = profile is null
        ? $"These files carry no house opinions. For a house profile's, read its llms-<profile>.txt beside this file instead: {string.Join(", ", profiles.Select(other => $"[{other}]({SiteUrl}{Ecosystem}/llms-{other}.txt)"))}."
        : $"These files carry the {profile} house profile's opinions. Each is a block that opens with `/// house | {profile}`, starts with `**House:**`, and closes with `///`, or a template region between `house:{profile} begin` and `house:{profile} end` comments. A house opinion is its owner's preference from experience, not sourced community practice. Where it conflicts with the sourced text beside it, follow the house opinion. For the default, which carries no house opinions, read [llms.txt]({SiteUrl}{Ecosystem}/llms.txt).";
    if (profile is null && profiles.Length == 0)
    {
        house = "These files carry no house opinions.";
    }

    string llms = $"""
        # Awesome Humans of {EcosystemName}{(profile is null ? "" : $", with the {profile} house profile")}

        > {LlmsSummary}

        {house}

        ## {EcosystemName} opinions

        Follow these when writing or reviewing .NET code. Target the versions in each file's `targets:` frontmatter, never an older one.

        {opinionLinks}

        ## {EcosystemName} templates

        Copy-paste-ready starting points that encode the opinions. Copy one verbatim and trim it.

        {templateLinks}

        ## {EcosystemName} skills

        Agent Skills procedures. Run review-project to check a .NET project against the opinions. The others maintain the repository.

        {skillLinks}

        ## Optional

        - [Awesome humans]({Site.Raw("AWESOME-HUMANS.md")}): the vetted sources the opinions cite, and the admission criteria
        - [House opinions]({Site.Raw("HOUSE-OPINIONS.md")}): the house profiles, and the record of every house opinion woven in

        """;
    Write($"{Ecosystem}/{(profile is null ? "llms.txt" : $"llms-{profile}.txt")}", llms.ReplaceLineEndings("\n"));
}

// The navigation. This ecosystem's section goes to nav.json, with its paths relative to
// dotnet/, and the website repository places it.
NavItem Entry(string title, string page) => new NavPage(title, page);
NavItem Section(string title, IEnumerable<NavItem> entries) => new NavSection(title, [.. entries]);

List<NavItem> opinionEntries = [];
foreach (Match bullet in Site.ScopeBullet().Matches(readme))
{
    opinionEntries.Add(Entry(bullet.Groups["title"].Value, pages[bullet.Groups["path"].Value]));
}

if (opinionEntries.Count != Opinions.Names().Length)
{
    errors.Add($"README.md: Scope lists {opinionEntries.Count} opinions but opinions/ holds {Opinions.Names().Length}; run validate-readme-index.cs");
}

NavItem[] ecosystemNav =
[
    Entry("Overview", pages["README.md"]),
    Section("Awesome humans",
    [
        Entry("Overview", pages[RosterPath]),
        Entry("Citable", rosterPages["citable"]),
        Entry("Watch list", rosterPages["watch-list"]),
        Entry("Decision log", rosterPages["decision-log"]),
    ]),
    Section("Opinions", [Entry("Overview", pages[Opinions.DirectoryName]), .. opinionEntries]),
    Section("Templates", [Entry("Overview", pages[TemplatesDirectory]), .. templates.Select(template => Entry(Path.GetFileName(template), pages[template]))]),
    Section("Research", [Entry("Overview", pages[ResearchDirectory]), .. research.Select(topic => Entry(NavTitle(topic.Title), pages[topic.Path]))]),
    Section("Skills", [Entry("Overview", pages[SkillsDirectory]), .. skills.Select(skill => Entry(skill.Groups["name"].Value, pages[Site.SkillPath(skill.Groups["name"].Value)]))]),
    Entry("House opinions", pages["HOUSE-OPINIONS.md"]),
];

JsonSerializerOptions json = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
JsonObject navJson = new()
{
    ["$schema"] = $"{SchemaBase}/nav.schema.json",
    ["version"] = 1,
    ["nav"] = new JsonArray([.. ecosystemNav.Select(item => item.ToJson($"{Ecosystem}/"))]),
};
Write($"{Ecosystem}/nav.json", navJson.ToJsonString(json) + "\n");

JsonArray sources = [];
// An opted-out source is exported too, so intake can turn away a suggestion of it.
foreach ((string bucket, string heading) in ((string, string)[])[("citable", "## Citable"), ("watch-list", "## Watch list"), ("opted-out", "## Opted out")])
{
    foreach (Match row in Site.RosterRow().Matches(rosterSections.First(section => section.StartsWith(heading, StringComparison.Ordinal))))
    {
        JsonObject? source = Site.Source(row, bucket, roster, errors);
        if (source is not null)
        {
            sources.Add(source);
        }
    }
}

JsonObject sourcesJson = new()
{
    ["$schema"] = $"{SchemaBase}/sources.schema.json",
    ["version"] = 1,
    ["sources"] = new JsonArray([.. sources.OrderBy(source => (string)source!["id"]!, StringComparer.Ordinal).Select(source => source!.DeepClone())]),
};
Write($"{Ecosystem}/sources.json", sourcesJson.ToJsonString(json) + "\n");

foreach (string error in errors)
{
    Console.Error.WriteLine(error);
}

if (errors.Count > 0)
{
    return 1;
}

Console.WriteLine($"Staged to {Path.Combine(docsDirectory, Ecosystem)}/");
return 0;

// A research topic's sidebar label: its title up to any colon, which names the topic, without
// the ecosystem the section already sets, such as "in .NET development", or the backticks a
// label can't render.
static string NavTitle(string title)
{
    string topic = title.Split(':')[0];
    foreach (string context in (string[])[$" in {EcosystemName}", $" for {EcosystemName}"])
    {
        int at = topic.IndexOf(context, StringComparison.Ordinal);
        topic = at == -1 ? topic : topic[..at];
    }

    return topic.Replace("`", "", StringComparison.Ordinal);
}

// A source id links to its notes section in the roster, or to the citable table when it has
// nothing to record and so no section, and a house profile links to the house opinions.
// One labelled group in an opinion's metadata strip. Markdown inside the span still renders.
static string MetaItem(string label, string values) =>
    $"<span class=\"ah-meta__item\"><span class=\"ah-label\">{label}</span> {values}</span>";

// A house profile's chip carries the profile, so the site shows it only with the profile's blocks.
static string HouseAttribute(string id) =>
    House.ProfileOf(id) is string profile ? $" data-house=\"{profile}\"" : "";

// Where an opinion or template's copy for a profile lands under dotnet/, for a file that holds
// house opinions. Zensical skips dotfiles, so a template's leading dot is dropped, as on its page.
static string RawPage(string path, string? profile)
{
    string name = path.EndsWith(".md", StringComparison.Ordinal) ? path + ".txt" : path.Replace("/.", "/", StringComparison.Ordinal);
    return $"raw/{profile ?? NoProfile}/{name}";
}

// The raw file an agent reads for a profile, or for the default with null. A file without
// house opinions is the same for everyone, so it's GitHub's raw copy. A file with them is
// this profile's copy, or the default copy when none of its house opinions are this profile's.
string RawLink(string path, string? profile) =>
    !housed.TryGetValue(path, out string[]? found) ? Site.Raw(path)
        : $"{SiteUrl}{Ecosystem}/{RawPage(path, profile is not null && found.Contains(profile) ? profile : null)}";

// Writes an opinion or research topic's page, with the design system's OpinionMeta strip
// under its title, from its frontmatter: a paragraph marked { .ah-meta }, as the staging
// contract defines, then the lead, if any. The freshness chip turns stale past
// audit-freshness's 180-day tolerance, counted from the day the site is staged.
void WithMeta(string path, bool freshness, string lead)
{
    IDictionary? meta = Frontmatter.Read(deserializer, path, out string? error);
    if (meta is null)
    {
        errors.Add($"{path}: {error}");
        return;
    }

    if (!DateOnly.TryParseExact($"{meta["last-reviewed"]}", "yyyy-MM-dd", out DateOnly reviewed))
    {
        errors.Add($"{path}: last-reviewed isn't a YYYY-MM-DD date");
        return;
    }

    string state = today.DayNumber - reviewed.DayNumber > OpinionTolerance ? "stale" : "fresh";
    string chip = freshness ? $" <span class=\"ah-chip ah-chip--{state} ah-chip--dot\">{state}</span>" : "";
    string line = string.Concat(
        MetaItem("Targets", string.Join(" ", Site.List(meta["targets"]).Select(target => $"`{target}`"))),
        MetaItem("Last reviewed", $"{reviewed:yyyy-MM-dd}{chip}"),
        MetaItem("Sources", string.Join(" ", Site.List(meta["sources"]).Select(id => $"{SourceLink(path, id)}{{ .ah-chip{HouseAttribute(id)} }}"))))
        + "\n{ .ah-meta }";
    string text = Rewrite(path, File.ReadAllText(path));
    int title = text.IndexOf("\n# ", StringComparison.Ordinal);
    int titleEnd = text.IndexOf('\n', title + 1);
    Write(pages[path], text[..titleEnd] + "\n\n" + line + (lead.Length == 0 ? "" : "\n\n" + lead.TrimEnd('\n')) + text[titleEnd..]);
}

string SourceLink(string path, string id)
{
    string from = Path.GetDirectoryName(path) ?? "";
    if (House.ProfileOf(id) is not null)
    {
        return $"[{id}]({Target(path, pages[path], Site.Normalise(Path.GetRelativePath(from, "HOUSE-OPINIONS.md")), anchorsToGitHub: false)})";
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
    string path = Path.Combine(docsDirectory, page);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, content);
}

// A navigation entry, written as JSON into the contract's nav.json, where the paths drop the
// ecosystem's own prefix.
internal abstract record NavItem(string Title)
{
    public abstract JsonNode ToJson(string prefix);
}

internal sealed record NavPage(string Title, string Page) : NavItem(Title)
{
    public override JsonNode ToJson(string prefix) =>
        new JsonObject { [Title] = Page.StartsWith(prefix, StringComparison.Ordinal) ? Page[prefix.Length..] : Page };
}

internal sealed record NavSection(string Title, NavItem[] Items) : NavItem(Title)
{
    public override JsonNode ToJson(string prefix) =>
        new JsonObject { [Title] = new JsonArray([.. Items.Select(item => item.ToJson(prefix))]) };
}

internal static partial class Site
{
    // One source in the contract's sources.json, from its roster row and the heading of its
    // notes, which carries the marking. A person's name is the start of their Source link
    // text, before ": ", " (", or "'s", as AWESOME-HUMANS.md: How the roster is organised
    // requires. A publication's is the whole link text. Since keeps only its leading year,
    // so "~2016 (docs.microsoft.com era)" exports as "~2016".
    public static JsonObject? Source(Match row, string bucket, string roster, List<string> errors)
    {
        string id = row.Groups["id"].Value;
        string kind = row.Groups["kind"].Value;
        Match link = FirstLink().Match(row.Groups["source"].Value);
        if (!link.Success)
        {
            errors.Add($"AWESOME-HUMANS.md: the Source cell for '{id}' has no link");
            return null;
        }

        string text = link.Groups["text"].Value;
        string name = kind == "person" ? PersonName().Match(text).Groups["name"].Value : text;
        string since = row.Groups["since"].Value.Trim();
        Match year = Year().Match(since);
        if (!year.Success && since != "—")
        {
            errors.Add($"AWESOME-HUMANS.md: the Since cell for '{id}' is '{since}', not a year, a ~year, or —");
        }

        Match heading = Regex.Match(roster, $@"^### `{Regex.Escape(id)}`:(?<rest>.*)$", RegexOptions.Multiline);
        string? marking = heading.Groups["rest"].Value switch
        {
            string rest when rest.Contains("**Corroborate.**", StringComparison.Ordinal) => "corroborate",
            string rest when rest.Contains("**Discovery-only.**", StringComparison.Ordinal) => "discovery-only",
            _ => null,
        };

        JsonObject source = new()
        {
            ["id"] = id,
            ["name"] = name,
            ["kind"] = kind,
            ["url"] = link.Groups["url"].Value,
            ["standing"] = bucket,
        };
        if (marking is not null)
        {
            source["marking"] = marking;
        }

        source["focus"] = row.Groups["focus"].Value.Trim();
        source["since"] = year.Success ? year.Value : null;
        return source;
    }

    private const string Repository = "awesome-humans/dotnet";

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
    // Turns a source's notes, headed '### `id`: Citable', into a collapsed block whose id is
    // the anchor the heading had, so links to it still land.
    public static string Collapsible(string note)
    {
        int end = note.IndexOf('\n');
        string heading = note[4..end];
        string summary = Strong().Replace(Code().Replace(System.Net.WebUtility.HtmlEncode(heading), "<code>$1</code>"), "<strong>$1</strong>");
        return $"<details id=\"{Slug(heading)}\" markdown=\"1\">\n<summary>{summary}</summary>\n\n{note[(end + 1)..].Trim('\n')}\n\n</details>\n\n";
    }

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

    // A bullet in README.md's Scope list: - **[Title](opinions/file.md):** what it covers.
    [GeneratedRegex(@"^- \*\*\[(?<title>[^\]]+)\]\((?<path>opinions/[^)]+)\):\*\*(?<summary>.*)$", RegexOptions.Multiline)]
    public static partial Regex ScopeBullet();

    // A row in README.md's Maintenance via skills table: | [`name`](skills/name/SKILL.md) | purpose |.
    [GeneratedRegex(@"^\| \[`(?<name>[^`]+)`\]\(skills/[^)]+\)\s*\|\s*(?<purpose>.*?)\s*\|$", RegexOptions.Multiline)]
    public static partial Regex SkillRow();

    [GeneratedRegex(@"^(?<text>#{1,6} .*[^\s\\#])(?<hashes>#+)$")]
    public static partial Regex HeadingTrailingHash();

    // A heading, or the opening of a source's collapsed notes, which stands in for one.
    [GeneratedRegex(@"^(?:(?<hashes>#{1,6}) (?<text>.+)|<details id=""(?<id>[^""]+)"".*)$", RegexOptions.Multiline)]
    public static partial Regex Heading();

    // The id cell that opens a roster table row: | `id` |.
    [GeneratedRegex(@"^\| `(?<id>[a-z0-9-]+)` +\|", RegexOptions.Multiline)]
    public static partial Regex TableId();

    // A whole roster table row: | `id` | Kind | Source | Focus | Since |.
    [GeneratedRegex(@"^\| `(?<id>[a-z0-9-]+)` +\| (?<kind>person|publication) +\| (?<source>[^|]+)\| (?<focus>[^|]+)\| (?<since>[^|]+)\|$", RegexOptions.Multiline)]
    public static partial Regex RosterRow();

    [GeneratedRegex(@"\[(?<text>[^\]]+)\]\((?<url>https://[^)\s]+)\)")]
    private static partial Regex FirstLink();

    [GeneratedRegex(@"^(?<name>.+?)(?::\s| \(|'s\s|$)")]
    private static partial Regex PersonName();

    [GeneratedRegex(@"^~?\d{4}")]
    private static partial Regex Year();

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex Code();

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex Strong();

    [GeneratedRegex(@"\]\((?<target>[^)\s]+)\)")]
    public static partial Regex MarkdownLink();

    [GeneratedRegex("`+")]
    private static partial Regex BacktickRun();

    [GeneratedRegex(@"[^\p{L}\p{Nd}\p{Pc} -]")]
    private static partial Regex NotSlug();
}
