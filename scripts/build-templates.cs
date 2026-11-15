#!/usr/bin/env dotnet
#:include House.cs
#:property PublishAot=false
#:property Nullable=enable
#:property TreatWarningsAsErrors=true

// Builds the files under templates/ the way a consumer who copied them would. The examples
// there aren't a solution: the project and source files sit flat under templates/projects/,
// so nothing compiles them, and a template that stops building stays unnoticed until
// someone copies it. This assembles them into the layout example.slnx describes, once
// for the default copy (every house region removed) and once for each house profile's, and
// runs the pipeline opinions/ci.md prescribes on each: restore, build, test, and the format
// check. It also builds through example.slnf, so the filter's paths are checked too.
//
// Where a file goes:
//
//   - A file directly under templates/ goes to the root of the assembled repository.
//   - A project file goes to the path example.slnx gives the project of that name.
//   - An F# source file goes beside the project whose Compile items list it.
//   - A C# source file goes beside the project its second line names: "Belongs to <project>."
//
// A file under templates/projects/ that none of these place fails the run, so a new
// example can't go unbuilt.
//
// A .NET 11 file-based app. Run it from the repository root, through its recipe or directly:
//
//     just build-templates
//     dotnet run scripts/build-templates.cs -- --out <dir>
//     ./scripts/build-templates.cs      (the shebang above)
//
// Exit codes: 0 every copy built, 1 a file had no place or a stage failed, 2 not run from
// the repository root, or arguments it doesn't recognise.

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

const string TemplatesDirectory = "templates";
const string ProjectsDirectory = "templates/projects";
const string Solution = "example.slnx";
const string Filter = "example.slnf";

// The default profile, which has no house opinions, as build-site.cs names it.
const string NoProfile = "none";

string outDirectory = "artifacts/templates";
if (args is ["--out", string given and not ""])
{
    outDirectory = given;
}
else if (args.Length > 0)
{
    Console.Error.WriteLine("Usage: dotnet run scripts/build-templates.cs -- [--out <dir>]");
    return 2;
}

if (!File.Exists($"{TemplatesDirectory}/{Solution}"))
{
    Console.Error.WriteLine($"No {TemplatesDirectory}/{Solution} here: run this script from the repository root.");
    return 2;
}

// Build output from smoke-testing by hand, which .gitignore also skips.
string[] buildOutput = ["artifacts", "bin", "obj"];

string[] rootFiles = [.. Directory.EnumerateFiles(TemplatesDirectory).Order(StringComparer.Ordinal)];
string[] projectFiles =
[
    .. Directory.EnumerateFiles(ProjectsDirectory, "*", SearchOption.AllDirectories)
        .Where(path => !Path.GetRelativePath(ProjectsDirectory, path).Split(Path.DirectorySeparatorChar).Any(buildOutput.Contains))
        .Order(StringComparer.Ordinal),
];

List<string> errors = [];

// Where each example file goes, relative to the assembled root.
Dictionary<string, string> placed = new(StringComparer.Ordinal);

// The directory of each project the solution lists, by project name.
Dictionary<string, string> projectDirectories = new(StringComparer.Ordinal);

XDocument solution = XDocument.Load($"{TemplatesDirectory}/{Solution}");
foreach (string projectPath in solution.Descendants("Project").Select(project => (string?)project.Attribute("Path") ?? ""))
{
    string fileName = Path.GetFileName(projectPath);
    string source = $"{ProjectsDirectory}/{fileName}";
    if (!File.Exists(source))
    {
        errors.Add($"{TemplatesDirectory}/{Solution}: lists {projectPath}, but {source} doesn't exist");
        continue;
    }

    string directory = Path.GetDirectoryName(projectPath)!;
    projectDirectories[Path.GetFileNameWithoutExtension(fileName)] = directory;
    placed[source] = projectPath;

    // F# compiles the files a project lists, in order, so the list says whose they are.
    foreach (string include in XDocument.Load(source).Descendants("Compile").Select(item => (string?)item.Attribute("Include") ?? ""))
    {
        placed[$"{ProjectsDirectory}/{include}"] = Path.Combine(directory, include);
    }
}

foreach (string path in projectFiles.Where(path => !placed.ContainsKey(Normalise(path))))
{
    string name = Normalise(path);
    string secondLine = File.ReadLines(path).Skip(1).FirstOrDefault() ?? "";
    if (Belongs().Match(secondLine) is { Success: true } match
        && projectDirectories.TryGetValue(match.Groups["project"].Value, out string? directory))
    {
        placed[name] = Path.Combine(directory, Path.GetFileName(path));
    }
    else
    {
        errors.Add(
            $"{name}: no project claims it. List it in a project's Compile items, or name a project "
            + $"from {Solution} on its second line: \"Belongs to <project>.\"");
    }
}

if (errors.Count > 0)
{
    return Fail("Template assembly failed:", errors);
}

// Every profile with a region in any template, after the default.
string[] profiles =
[
    NoProfile,
    .. rootFiles.Concat(projectFiles)
        .Where(HasComments)
        .SelectMany(path => House.Spans(Normalise(path), File.ReadAllText(path), errors))
        .Select(span => span.Profile)
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal),
];

if (errors.Count > 0)
{
    return Fail("Template assembly failed:", errors);
}

foreach (string profile in profiles)
{
    string root = Path.GetFullPath(Path.Combine(outDirectory, profile));
    if (Directory.Exists(root))
    {
        Directory.Delete(root, recursive: true);
    }

    foreach (string path in rootFiles)
    {
        Copy(path, Path.Combine(root, Path.GetFileName(path)), profile);
    }

    foreach ((string source, string destination) in placed)
    {
        Copy(source, Path.Combine(root, destination), profile);
    }

    Console.WriteLine($"== {profile}: assembled in {Path.GetRelativePath(Directory.GetCurrentDirectory(), root)}");

    // The stages of opinions/ci.md, in its order, then the format check it adds and a build
    // through the filter. The solution and the filter sit side by side here, so every stage
    // names the file it means.
    string[][] stages =
    [
        ["restore", Solution],
        ["build", Solution, "--no-restore", "--configuration", "Release", "-warnaserror"],
        ["test", "--solution", Solution, "--no-build", "--configuration", "Release"],
        ["format", Solution, "--verify-no-changes", "--no-restore"],
        ["build", Filter, "--no-restore", "--configuration", "Release", "-warnaserror"],
    ];

    foreach (string[] stage in stages)
    {
        Console.WriteLine($"== {profile}: dotnet {string.Join(' ', stage)}");
        int exitCode = Run(root, stage);
        if (exitCode != 0)
        {
            return Fail(
                "Template build failed:",
                [$"dotnet {string.Join(' ', stage)} exited with code {exitCode} for the {profile} copy, in {root}"]);
        }
    }
}

Console.WriteLine(
    $"The templates restore, build, test, and pass the format check as {profiles.Length} copies: {string.Join(", ", profiles)}.");
return 0;

static int Fail(string heading, List<string> errors)
{
    Console.WriteLine(heading);
    foreach (string error in errors)
    {
        Console.WriteLine($"  - {error}");
    }

    return 1;
}

static string Normalise(string path) => path.Replace('\\', '/');

// JSON has no comment syntax, so a JSON template has no regions to remove.
static bool HasComments(string path) =>
    Path.GetExtension(path) is not (".json" or ".slnf");

// Writes one profile's copy of a template: every other profile's regions removed.
static void Copy(string source, string destination, string profile)
{
    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
    if (!HasComments(source))
    {
        File.Copy(source, destination);
        return;
    }

    string text = File.ReadAllText(source);
    File.WriteAllText(destination, House.ForProfile(Normalise(source), text, profile == NoProfile ? null : profile));
}

// Runs one dotnet command in the assembled repository, so its own global.json picks the SDK
// and the test runner.
static int Run(string directory, string[] arguments)
{
    ProcessStartInfo start = new("dotnet", arguments) { WorkingDirectory = directory, UseShellExecute = false };
    using Process process = Process.Start(start)!;
    process.WaitForExit();
    return process.ExitCode;
}

internal static partial class Program
{
    [GeneratedRegex(@"\bBelongs to (?<project>[A-Za-z0-9_.]+?)\.(?:\s|\z)")]
    private static partial Regex Belongs();
}
