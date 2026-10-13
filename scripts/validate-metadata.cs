#!/usr/bin/env dotnet
#:package YamlDotNet@18.1.0
#:include Opinions.cs
#:include Frontmatter.cs
#:include CommentHeader.cs
// PublishAot is on by default for file-based apps, and YamlDotNet's reflection-based
// builder trips its analyzer (IL3050). These scripts are run, never published, so turn
// AOT off instead of suppressing an accurate diagnostic.
#:property PublishAot=false
#:property Nullable=enable
#:property TreatWarningsAsErrors=true

// Verifies that every resource carries the metadata fields AGENTS.md requires
// (targets, last-reviewed, sources, plus last-used outside research/) and that the
// dates are ISO 8601.
//
// Three resource kinds carry them two ways. opinions/ and research/ use YAML
// frontmatter; templates/ use a first-line comment header, because an XML or INI file
// cannot open with a `---` block and stay valid for the tools that read it. One
// required set spans both syntaxes, so they cannot drift apart.
//
// A .NET 10 file-based app: no project, no build step, sharing its helpers with the
// other checks via #:include. Run it from the repository root:
//
//     dotnet run scripts/validate-metadata.cs
//     ./scripts/validate-metadata.cs      (the shebang above)
//
// Exit codes: 0 clean, 1 findings printed, 2 not run from the repository root.

using System.Collections;
using System.Globalization;
using YamlDotNet.Serialization;

if (!Opinions.DirectoryExists)
{
    Console.Error.WriteLine($"No {Opinions.DirectoryName}/ directory here — run this script from the repository root.");
    return 2;
}

string[] required = ["targets", "last-reviewed", "last-used", "sources"];

// A research topic carries no last-used: the only way to consult one is to build on it,
// which re-verifies it, so its two dates would always move together (research-topic).
string[] researchRequired = ["targets", "last-reviewed", "sources"];

// ...and two fields it must not carry. A last-used or status key on a research topic is
// the retired frontmatter shape coming back. A topic on disk is unresolved by
// definition, and deletion is the only promotion signal (AGENTS.md), so these are
// rejected rather than ignored.
string[] researchForbidden = ["last-used", "status"];

string[] dateFields = ["last-reviewed", "last-used"];

List<string> errors = [];
IDeserializer deserializer = new DeserializerBuilder().Build();

List<(string Path, IDictionary? Fields, string? Error, string[] Required, string[] Forbidden)> resources = [];

string[] opinions = Opinions.Names();
if (opinions.Length == 0)
{
    errors.Add($"no markdown files found under {Opinions.DirectoryName}/");
}

foreach (string name in opinions)
{
    string path = Opinions.PathOf(name);
    resources.Add((path, Frontmatter.Read(deserializer, path, out string? error), error, required, []));
}

// research/ is staging, and legitimately empty once every topic has been resolved —
// resolve-research deletes the file it finishes with. So an empty or absent directory
// is not a finding.
string[] research = MarkdownIn("research");

foreach (string path in research)
{
    resources.Add((path, Frontmatter.Read(deserializer, path, out string? error), error, researchRequired, researchForbidden));
}

string[] templates = CommentHeader.Files();
if (templates.Length == 0)
{
    errors.Add($"no header-carrying files found under {CommentHeader.TemplatesDirectory}/");
}

foreach (string path in templates)
{
    resources.Add((path, CommentHeader.Read(path, out string? error), error, required, []));
}

foreach ((string path, IDictionary? fields, string? readError, string[] fieldsRequired, string[] fieldsForbidden) in resources)
{
    if (fields is null)
    {
        errors.Add($"{path}: {readError}");
        continue;
    }

    foreach (string field in fieldsRequired)
    {
        if (!fields.Contains(field) || IsEmpty(fields[field]))
        {
            errors.Add($"{path}: missing or empty metadata field '{field}'");
        }
    }

    foreach (string field in fieldsForbidden)
    {
        if (fields.Contains(field))
        {
            errors.Add($"{path}: field '{field}' is not allowed on a research topic");
        }
    }

    foreach (string field in dateFields)
    {
        // YamlDotNet may hand back an already-parsed date for an unquoted YAML scalar;
        // a comment header always arrives as text. Only text needs the format check —
        // a value YAML read as a date is one that was written as one.
        if (fields.Contains(field) && fields[field] is string text && !IsIsoDate(text))
        {
            errors.Add($"{path}: '{field}' is '{text}', not an ISO 8601 date (YYYY-MM-DD)");
        }
    }
}

if (errors.Count > 0)
{
    Console.WriteLine("Resource metadata validation failed:");
    foreach (string error in errors)
    {
        Console.WriteLine($"  - {error}");
    }

    return 1;
}

Console.WriteLine(
    $"All {resources.Count} resources carry valid metadata "
    + $"({opinions.Length} opinions, {research.Length} research, {templates.Length} templates).");
return 0;

// A key that is present but carries nothing (null, "", []) is as much a gap as
// a missing key, so both fail the same way.
static bool IsEmpty(object? value) => value switch
{
    null => true,
    string text => text.Length == 0,
    ICollection collection => collection.Count == 0,
    _ => false,
};

// AGENTS.md: dates are ISO 8601, always absolute, never relative. An exact parse is
// what rejects "yesterday" and "August 2026" as well as 20-08-2026.
static bool IsIsoDate(string text) =>
    DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

static string[] MarkdownIn(string directory)
{
    if (!Directory.Exists(directory))
    {
        return [];
    }

    return
    [
        .. new DirectoryInfo(directory)
            .EnumerateFiles("*.md")
            .Select(file => $"{directory}/{file.Name}")
            .Order(StringComparer.Ordinal),
    ];
}
