#!/usr/bin/env dotnet
#:include CommentHeader.cs
#:property Nullable=enable
#:property TreatWarningsAsErrors=true

// Verifies that the interchangeable entries in each MSBuild template are sorted by name,
// ignoring case, within their ItemGroup: PackageVersion in Directory.Packages.props, and
// PackageReference in Directory.Build.props and the project files. Reordering them doesn't
// change what MSBuild does, so one order means a reader finds a package where they expect
// it, and two edits that add the same package land on the same line. The ItemGroup is the
// group: its Label says what the entries have in common, and the order restarts in each.
//
// Order that carries meaning isn't checked. F# compiles the Compile items in the order the
// project lists them, and a PropertyGroup reads top to bottom. See awesome-humans/website:
// Writing style, Order.
//
// A .NET 10 file-based app: no project, no build step. Run it from the repository root,
// through its recipe as CI does, or directly:
//
//     just validate-template-order
//     dotnet run scripts/validate-template-order.cs
//     ./scripts/validate-template-order.cs      (the shebang above)
//
// Exit codes: 0 clean, 1 findings printed, 2 not run from the repository root.

using System.Xml;
using System.Xml.Linq;

if (!Directory.Exists(CommentHeader.TemplatesDirectory))
{
    Console.Error.WriteLine(
        $"No {CommentHeader.TemplatesDirectory}/ directory here — run this script from the repository root.");
    return 2;
}

string[] extensions = [".props", ".targets", ".csproj", ".fsproj"];
string[] sortedItems = ["PackageVersion", "PackageReference"];

string[] templates =
[
    .. CommentHeader.Files()
        .Where(path => extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)),
];

List<string> errors = [];
int groups = 0;

foreach (string path in templates)
{
    XDocument document;
    try
    {
        document = XDocument.Load(path, LoadOptions.SetLineInfo);
    }
    catch (XmlException exception)
    {
        errors.Add($"{path}: not well-formed XML: {exception.Message}");
        continue;
    }

    foreach (XElement group in document.Descendants("ItemGroup"))
    {
        foreach (string item in sortedItems)
        {
            string[] names =
            [
                .. group.Elements(item)
                    // NuGet accepts Update in place of Include, as on a PackageVersion
                    // override, and both name the package.
                    .Select(element =>
                        (string?)element.Attribute("Include") ?? (string?)element.Attribute("Update") ?? ""),
            ];
            if (names.Length < 2)
            {
                continue;
            }

            groups++;
            string[] sorted = [.. names.Order(StringComparer.OrdinalIgnoreCase)];
            if (!names.SequenceEqual(sorted, StringComparer.Ordinal))
            {
                int line = ((IXmlLineInfo)group).LineNumber;
                string label = (string?)group.Attribute("Label") is string text ? $" '{text}'" : "";
                errors.Add(
                    $"{path}:{line}: the {item} entries in ItemGroup{label} are not sorted; "
                    + $"expected {string.Join(", ", sorted)}");
            }
        }
    }
}

if (errors.Count > 0)
{
    Console.WriteLine("Template order validation failed:");
    foreach (string error in errors)
    {
        Console.WriteLine($"  - {error}");
    }

    return 1;
}

Console.WriteLine($"All {groups} sortable item lists in {templates.Length} MSBuild templates are in order.");
return 0;
