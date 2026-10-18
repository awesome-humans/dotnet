#!/usr/bin/env dotnet
#:package YamlDotNet@18.1.0
#:include Opinions.cs
#:include Frontmatter.cs
#:include CommentHeader.cs
#:include House.cs
// PublishAot is on by default for file-based apps, and YamlDotNet's reflection-based
// builder trips its analyzer (IL3050). These scripts are run, never published, so turn
// AOT off instead of suppressing an accurate diagnostic.
#:property PublishAot=false
#:property Nullable=enable
#:property TreatWarningsAsErrors=true

// Verifies that every source id in an opinion's frontmatter or a template's comment
// header resolves to the roster in AWESOME-HUMANS.md, and that it is allowed to feed
// one: watch-list sources have not been admitted yet, opted-out sources asked not to be
// cited, and discovery-only sources are channels that lead you to a primary source rather
// than being one. templates/ is held
// to the same gate as opinions/ because it is what people copy.
//
// research/ is deliberately outside the gate. A topic may cite unvetted material as long
// as the text flags it as such (research-topic, audit-freshness); the roster gate lands at
// promotion instead, where resolve-research drops or re-sources every unvetted claim.
//
// A house profile's id, house:<profile>, isn't on the roster. Instead, each one in a file's
// sources must match a house block in the opinion, or a house region in the template, and
// every house opinion must sit inside one (HOUSE-OPINIONS.md: How this works).
//
// It also holds the roster itself to its shape: each table sorted by id, and no id
// used twice. Both are conventions a reader cannot spot in a 30-row table, and the
// second is worse than untidy — the parse below is keyed by id, so a duplicate would
// silently decide a source's bucket by whichever row came last.
//
// A .NET 10 file-based app: no project, no build step, sharing its helpers with the
// other checks via #:include. Run it from the repository root, through its
// recipe as CI does, or directly:
//
//     just validate-sources
//     dotnet run scripts/validate-sources.cs
//     ./scripts/validate-sources.cs      (the shebang above)
//
// Exit codes: 0 clean, 1 findings printed, 2 not run from the repository root.

using System.Collections;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

const string RosterPath = "AWESOME-HUMANS.md";
const string DiscoveryMarker = "**Discovery-only.**";
const string CorroborateMarker = "**Corroborate.**";
const string WatchBucket = "watch";
const string OptedOutBucket = "opted-out";

if (!Opinions.DirectoryExists)
{
    Console.Error.WriteLine($"No {Opinions.DirectoryName}/ directory here — run this script from the repository root.");
    return 2;
}

if (!File.Exists(RosterPath))
{
    Console.WriteLine($"{RosterPath} not found");
    return 1;
}

// Which level-2 headings open which bucket. Anything outside them, such as the admission
// criteria and the decision log, carries no source rows.
(string Bucket, string[] Prefixes)[] sections =
[
    ("citable", ["## Citable"]),
    (WatchBucket, ["## Watch list"]),
    (OptedOutBucket, ["## Opted out"]),
];

// The roster tables carry only id, Kind, Source, Focus and Since. The evidence behind each row,
// and with it the two markings, lives under '## Source notes', one '### `id`' section per
// source. Splitting them is what keeps a diff readable: a table row is a short line, and a
// reworded sentence in a note touches that sentence rather than a 4,000-character row.
const string NotesHeading = "## Source notes";

// Parse the roster: source id -> (bucket, the source's notes section, heading included).
// A source with nothing to record has no section, which reads as unmarked, exactly as an
// empty Notes cell did before.
Dictionary<string, (string Bucket, string Notes)> roster = new(StringComparer.Ordinal);

// Notes sections in file order, for the same alphabetical check the tables get.
List<string> noteIds = [];
Dictionary<string, string> notes = new(StringComparer.Ordinal);
string? notesFor = null;

// Ids in the order they appear, one entry per table. Order is checked per table rather
// than across the file, because the watch list legitimately starts over at 'a' where the
// citable table left off at 's'. Replaced at each heading; the initial list is never filled.
List<(string Heading, List<string> Ids)> tables = [];
List<string> ids = [];

List<string> errors = [];
string? bucket = null;

// The Citable section states how many people and publications its table holds, and the
// site shows that sentence. A hand-kept count drifts with every admission, so the rows
// are counted here and the sentence has to agree.
Match? statedCount = null;
int citablePeople = 0;
int citablePublications = 0;

foreach (string line in File.ReadAllText(RosterPath).ReplaceLineEndings("\n").Split('\n'))
{
    if (line.StartsWith("### ", StringComparison.Ordinal) && notesFor is not null)
    {
        Match noteId = Patterns.NotesHeadingId().Match(line);
        if (noteId.Success)
        {
            notesFor = noteId.Groups[1].Value;
            noteIds.Add(notesFor);
            if (!notes.TryAdd(notesFor, line))
            {
                errors.Add(
                    $"{RosterPath}: source id '{notesFor}' has more than one notes section");
            }
        }

        continue;
    }

    if (line.StartsWith("## ", StringComparison.Ordinal))
    {
        // An empty string means inside '## Source notes' but before the first '### `id`'.
        notesFor = line.StartsWith(NotesHeading, StringComparison.Ordinal) ? "" : null;
        bucket = null;
        foreach ((string name, string[] prefixes) in sections)
        {
            if (Array.Exists(prefixes, prefix => line.StartsWith(prefix, StringComparison.Ordinal)))
            {
                bucket = name;
            }
        }

        if (bucket is not null)
        {
            ids = [];
            tables.Add((line[3..].Trim(), ids));
        }

        continue;
    }

    if (notesFor is { Length: > 0 })
    {
        notes[notesFor] += "\n" + line;
        continue;
    }

    if (bucket == "citable" && Patterns.CitableCount().Match(line) is { Success: true } count)
    {
        statedCount = count;
        continue;
    }

    if (bucket is null || !line.StartsWith('|'))
    {
        continue;
    }

    string[] cells = [.. line.Trim().Trim('|').Split('|').Select(cell => cell.Trim())];
    Match id = Patterns.SourceId().Match(cells[0]);
    if (!id.Success)
    {
        continue; // header or separator row
    }

    string rowId = id.Groups[1].Value;
    ids.Add(rowId);

    // The site reads the kind to decide how to take a source's name from its link, so a row
    // missing it, or one the site doesn't know, fails here rather than in the site build.
    if (cells.Length < 5)
    {
        errors.Add($"{RosterPath}: the row for '{rowId}' has {cells.Length} cells, not the 5 of id, Kind, Source, Focus and Since");
    }
    else if (cells[1] is not ("person" or "publication"))
    {
        errors.Add($"{RosterPath}: the row for '{rowId}' has Kind '{cells[1]}', not 'person' or 'publication'");
    }
    else if (bucket == "citable")
    {
        if (cells[1] == "person")
        {
            citablePeople++;
        }
        else
        {
            citablePublications++;
        }
    }

    // TryAdd rather than the indexer: two rows sharing an id would otherwise leave the
    // roster holding whichever came last, deciding a source's bucket by row order.
    if (!roster.TryAdd(rowId, (bucket, string.Empty)))
    {
        errors.Add($"{RosterPath}: source id '{rowId}' appears on more than one row — ids are unique");
    }
}

if (statedCount is null)
{
    errors.Add(
        $"{RosterPath}: '## Citable' has no sentence of the form "
        + "'The table lists N sources: N people and N publications.'");
}
else
{
    string expected = $"{citablePeople + citablePublications} sources: {citablePeople} people and {citablePublications} publications";
    string stated = $"{statedCount.Groups[1].Value} sources: {statedCount.Groups[2].Value} people and {statedCount.Groups[3].Value} publications";
    if (stated != expected)
    {
        errors.Add($"{RosterPath}: '## Citable' says it lists {stated}, but the table has {expected}");
    }
}

if (roster.Count == 0)
{
    errors.Add($"{RosterPath}: no source rows parsed — roster tables may have changed shape");
}

// Roster tables are kept alphabetical by id (vet-source, step 6) so a new row lands in
// one predictable place instead of wherever the admitting PR happened to put it. Nothing
// about a 30-row table makes a misplaced row visible, so it is checked rather than left
// to review.
foreach ((string heading, List<string> tableIds) in tables)
{
    for (int index = 1; index < tableIds.Count; index++)
    {
        if (StringComparer.Ordinal.Compare(tableIds[index - 1], tableIds[index]) > 0)
        {
            errors.Add(
                $"{RosterPath}: '{heading}' is not sorted by id "
                    + $"— '{tableIds[index]}' follows '{tableIds[index - 1]}'");
        }
    }
}

// A notes section for an id that no table row declares is a leftover from a demotion or a
// typo, and would look authoritative while applying to nothing. The reverse is
// allowed: a source with nothing worth recording has no section.
foreach (string noteId in noteIds)
{
    if (!roster.ContainsKey(noteId))
    {
        errors.Add(
            $"{RosterPath}: '{NotesHeading}' has a section for '{noteId}', "
            + "which is on no roster table");
    }
}

// An opted-out source's notes go with it: the evidence was about work it asked us to stop
// citing, and its row and the decision log are all the roster keeps.
foreach (string noteId in noteIds)
{
    if (roster.TryGetValue(noteId, out (string Bucket, string Notes) entry) && entry.Bucket == OptedOutBucket)
    {
        errors.Add(
            $"{RosterPath}: '{NotesHeading}' has a section for '{noteId}', "
            + "which has opted out — delete the section");
    }
}

// Sorted for the same reason the tables are: so a new section lands in one predictable
// place rather than wherever the admitting pull request happened to put it.
for (int index = 1; index < noteIds.Count; index++)
{
    if (StringComparer.Ordinal.Compare(noteIds[index - 1], noteIds[index]) > 0)
    {
        errors.Add(
            $"{RosterPath}: '{NotesHeading}' is not sorted by id "
                + $"— '{noteIds[index]}' follows '{noteIds[index - 1]}'");
    }
}

// The markings are read off the section, so fold it in before looking for them.
foreach ((string noteId, string body) in notes)
{
    if (roster.TryGetValue(noteId, out (string Bucket, string Notes) entry))
    {
        roster[noteId] = (entry.Bucket, body);
    }
}

HashSet<string> discovery =
[
    .. roster
        .Where(entry => entry.Value.Notes.Contains(DiscoveryMarker, StringComparison.Ordinal))
        .Select(entry => entry.Key),
];

// Rows admitted on track record but thin on depth: citable, never the only citation.
HashSet<string> corroborate =
[
    .. roster
        .Where(entry => entry.Value.Notes.Contains(CorroborateMarker, StringComparison.Ordinal))
        .Select(entry => entry.Key),
];

IDeserializer deserializer = new DeserializerBuilder().Build();

// Every resource held to the roster, as (path, declared ids).
List<(string Path, List<string> Ids)> citing = [];

foreach (string name in Opinions.Names())
{
    string path = Opinions.PathOf(name);

    // A file with no frontmatter, or unreadable frontmatter, is reported in detail by
    // validate-metadata.cs — this check stays quiet about it.
    if (Frontmatter.Read(deserializer, path, out _) is not IDictionary frontmatter)
    {
        continue;
    }

    object? declared = frontmatter.Contains("sources") ? frontmatter["sources"] : null;
    if (declared is null or "")
    {
        continue;
    }

    if (declared is not IList sources)
    {
        errors.Add($"{path}: 'sources' must be a list");
        continue;
    }

    List<string> declaredIds = [];
    foreach (object? entry in sources)
    {
        declaredIds.Add(entry?.ToString() ?? string.Empty);
    }

    citing.Add((path, declaredIds));
}

// A template carries the same ids comma-separated inside its one-line comment header.
foreach (string path in CommentHeader.Files())
{
    if (CommentHeader.Read(path, out _) is not IDictionary header || header["sources"] is not string declared)
    {
        continue;
    }

    citing.Add((
        path,
        [.. declared.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)]));
}

foreach ((string path, List<string> sourceIds) in citing)
{
    foreach (string sourceId in sourceIds)
    {
        // A house profile isn't on the roster. The pairing check below holds it to its blocks.
        if (House.ProfileOf(sourceId) is not null)
        {
            continue;
        }

        if (sourceId == "house")
        {
            errors.Add($"{path}: 'house' names no profile — write {House.Prefix}<profile>, such as {House.Prefix}simon-mckenna");
            continue;
        }

        if (!roster.TryGetValue(sourceId, out (string Bucket, string Notes) row))
        {
            errors.Add($"{path}: source '{sourceId}' is not on the roster in {RosterPath}");
            continue;
        }

        if (row.Bucket == WatchBucket)
        {
            errors.Add(
                $"{path}: source '{sourceId}' is on the watch list and may not feed opinions or templates "
                    + "— corroborate with a citable source or admit it via vet-source");
        }
        else if (row.Bucket == OptedOutBucket)
        {
            errors.Add(
                $"{path}: source '{sourceId}' has opted out and may not feed opinions or templates "
                    + "— re-source the claim with research-topic, or cut it back to what other citable sources support");
        }
        else if (discovery.Contains(sourceId))
        {
            errors.Add(
                $"{path}: source '{sourceId}' is marked discovery-only and never appears in 'sources:' "
                    + "— cite the primary source it led you to");
        }
    }

    // A Corroborate-marked source may carry the shape of a claim but never the claim alone,
    // so something unmarked has to stand beside it. A house profile does not count: it is its
    // owner's position, not a second reading of the evidence.
    bool marked = false;
    bool unmarked = false;
    foreach (string sourceId in sourceIds)
    {
        if (corroborate.Contains(sourceId))
        {
            marked = true;
        }
        else if (roster.ContainsKey(sourceId) && !discovery.Contains(sourceId))
        {
            unmarked = true;
        }
    }

    if (marked && !unmarked)
    {
        errors.Add(
            $"{path}: every citable source here is marked '{CorroborateMarker}' "
                + "— add a citable source that is not, or drop the claim");
    }

    // A house profile in the sources and its blocks or regions always appear together, and
    // every house opinion sits inside one, so removing them leaves no house opinion behind
    // (HOUSE-OPINIONS.md: How this works).
    string text = File.ReadAllText(path);
    List<House.Span> spans = House.Spans(path, text, errors);
    string kind = path.EndsWith(".md", StringComparison.Ordinal) ? "block" : "region";
    HashSet<string> declared = [.. sourceIds.Select(House.ProfileOf).OfType<string>()];
    HashSet<string> found = [.. spans.Select(span => span.Profile)];
    foreach (string profile in declared.Except(found).Order(StringComparer.Ordinal))
    {
        errors.Add($"{path}: sources name {House.Prefix}{profile}, but no house {kind} here belongs to it");
    }

    foreach (string profile in found.Except(declared).Order(StringComparer.Ordinal))
    {
        errors.Add($"{path}: a house {kind} belongs to {profile}, but sources don't name {House.Prefix}{profile}");
    }

    foreach (House.Span span in spans.Where(span => !span.Marked))
    {
        errors.Add($"{path}:{span.Start + 1}: the house {kind} for {span.Profile} doesn't carry '{House.Marking}'");
    }

    string[] lines = text.ReplaceLineEndings("\n").Split('\n');
    for (int index = 0; index < lines.Length; index++)
    {
        if (lines[index].Contains(House.Marking, StringComparison.Ordinal)
            && !spans.Any(span => span.Start < index && index < span.End))
        {
            errors.Add($"{path}:{index + 1}: '{House.Marking}' outside a house {kind}, so the default copy would carry it");
        }
    }
}

if (errors.Count > 0)
{
    Console.WriteLine("Roster and source validation failed:");
    foreach (string error in errors)
    {
        Console.WriteLine($"  - {error}");
    }

    return 1;
}

Console.WriteLine(
    $"All sources in {citing.Count} opinions and templates resolve to the roster "
    + $"({roster.Count} sources across {tables.Count} tables, {noteIds.Count} with notes, "
    + $"{discovery.Count} discovery-only, {corroborate.Count} corroborate-only), "
    + "and every table and the notes are sorted by id.");
return 0;

internal static partial class Patterns
{
    // A roster row's first cell is a bare source id in backticks: `andrew-lock`.
    [GeneratedRegex(@"\A`([a-z0-9-]+)`\z")]
    internal static partial Regex SourceId();

    // A notes heading opens with the id and then says where the source stands:
    // '### `ardalis`: Citable, **Corroborate.**'.
    [GeneratedRegex(@"\A### `([a-z0-9-]+)`")]
    internal static partial Regex NotesHeadingId();

    // The count sentence above the Citable table: 'The table lists 31 sources: 21 people and
    // 10 publications.'
    [GeneratedRegex(@"\AThe table lists (\d+) sources: (\d+) people and (\d+) publications\.\z")]
    internal static partial Regex CitableCount();
}
