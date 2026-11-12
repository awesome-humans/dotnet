// Shared by the checks in this directory, through `#:include Opinions.cs`.
// Not a file-based app itself: it declares no top-level statements, so `dotnet run`
// on this file does nothing useful. It has no `#:package` directive either,
// because a `#:include`d file contributes source, and dependencies stay declared by
// the script that is actually being run.

internal static class Opinions
{
    internal const string DirectoryName = "opinions";

    internal static bool DirectoryExists => Directory.Exists(DirectoryName);

    /// <summary>The opinion file names in sorted order, empty when the directory is absent.</summary>
    internal static string[] Names()
    {
        if (!DirectoryExists)
        {
            return [];
        }

        return
        [
            .. new DirectoryInfo(DirectoryName)
                .EnumerateFiles("*.md")
                .Select(file => file.Name)
                .Order(StringComparer.Ordinal),
        ];
    }

    /// <summary>The repository-relative path of an opinion file, as the reports quote it.</summary>
    internal static string PathOf(string name) => $"{DirectoryName}/{name}";
}
