#!/usr/bin/env dotnet
#:property Nullable=enable
#:property TreatWarningsAsErrors=true

// Draws this repository's diagrams as SVG into assets/diagrams: the maintenance pipeline,
// which shows each skill from what starts it to where its result lands, source standing,
// which shows how vet-source sorts a candidate, and the research lifecycle. The output is
// committed; rerun this script after changing a diagram here, never edit the SVG by hand.
// CI reruns it and fails when the committed SVGs differ from what it draws.
//
// Each diagram is data at the top of its class, rows or nodes with their labels, and layout
// code below. Colours and fonts come from the Awesome Humans design system, so these match the
// diagrams on awesome-humans.net. Text names IBM Plex where the reader has it installed and
// falls back to system fonts, so a diagram makes no third-party requests. Colours follow the
// reader's light or dark preference through prefers-color-scheme. Every diagram
// paints its own background, because GitHub can show a README in a theme other than the
// operating system's, and a transparent diagram would then put light text on a light page.
//
// A .NET 10 file-based app. Run it from the repository root, through its
// recipe as CI does, or directly:
//
//     just export-diagrams
//     dotnet run scripts/export-diagrams.cs
//     ./scripts/export-diagrams.cs      (the shebang above)
//
// Exit codes: 0 exported, 2 not run from the repository root.

using System.Globalization;

const string OutputDirectory = "assets/diagrams";

if (!Directory.Exists("skills"))
{
    Console.Error.WriteLine("No skills/ directory here. Run this script from the repository root.");
    return 2;
}

Directory.CreateDirectory(OutputDirectory);
File.WriteAllText(Path.Combine(OutputDirectory, "maintenance-pipeline.svg"), Pipeline.Build());
File.WriteAllText(Path.Combine(OutputDirectory, "source-standing.svg"), Standing.Build());
File.WriteAllText(Path.Combine(OutputDirectory, "research-lifecycle.svg"), Research.Build());

Console.WriteLine($"Exported to {OutputDirectory}/");
return 0;

// One skill: what starts it, the branch it works on, what the owner does at the human gate
// and where its result lands, which is "roster", "research", "opinions" or "report". A
// report-only skill has no branch and opens no pull request, so it passes neither gate.
internal sealed record Row(
    string Lane,
    string[] PicksUp,
    string Skill,
    string? Branch,
    string HumanGate,
    string LandsIn);

internal static class Pipeline
{
    private const string Title = "Maintenance pipeline";

    private const string Description =
        "Swimlane diagram of how this repository maintains itself: vet-source, research-topic, resolve-research, harvest-sources, refresh-dotnet-versions " +
        "and weave-house-opinion each work on their own branch, pass CI and the owner's review, and land in the roster, research/ or the opinions and templates; " +
        "audit-freshness and review-project only report. Their findings, the release watch and the harvest reminder start the next run.";

    private static readonly (string Key, string Name)[] Lanes =
    [
        ("sources", "Sources"),
        ("research", "Research"),
        ("opinions", "Opinions"),
        ("reports", "Reports"),
    ];

    private static readonly Row[] Rows =
    [
        new("sources", ["candidate source,", "or a change in standing"], "vet-source", "vet/<source-id>", "check evidence", "roster"),
        new("research", ["a question", "about <topic>"], "research-topic", "research/<topic>", "review", "research"),
        new("research", ["topic saved in", "research/"], "resolve-research", "resolve/<topic>", "approve weave", "opinions"),
        new("opinions", ["monthly harvest", "reminder issue"], "harvest-sources", "harvest/<date>", "review", "opinions"),
        new("opinions", ["release-watch PR,", "or a newer pin"], "refresh-dotnet-versions", "refresh/dotnet-<version>", "review", "opinions"),
        new("opinions", ["HOUSE-OPINIONS.md", "inbox entry"], "weave-house-opinion", "house/<slug>", "confirm wording", "opinions"),
        new("reports", ["periodic cadence"], "audit-freshness", null, "", "report"),
        new("reports", ["a project path", "or repository URL"], "review-project", null, "", "report"),
    ];

    // Columns, left to right, and the height of one row.
    private const double XBand = 12, WBand = 96;
    private const double XBracket = 118;
    private const double XPicksUp = 128, WPicksUp = 192;
    private const double XSkill = 344, WSkill = 236;
    private const double XAgentGate = 604, WAgentGate = 116;
    private const double GateHalf = 20;
    private const double XHumanGate = 764;
    private const double XLandsIn = 910, WLandsIn = 184;
    private const double Width = 1106;
    private const double RowHeight = 66;

    public static string Build()
    {
        // Rows stack with a gap between lanes.
        List<double> ys = [];
        double y = 44;
        string? previous = null;
        foreach (Row row in Rows)
        {
            if (previous is not null && row.Lane != previous)
            {
                y += 18;
            }

            ys.Add(y);
            y += RowHeight + 8;
            previous = row.Lane;
        }

        double rowsBottom = ys[^1] + RowHeight;
        List<string> s = [];

        foreach ((double x, string label) in new (double, string)[]
        {
            (XBand, "lane"), (XPicksUp, "picks up"), (XSkill, "skill"), (XAgentGate, "agent gate"),
            (XHumanGate - GateHalf, "human gate"), (XLandsIn, "lands in"),
        })
        {
            s.Add(Svg.Text(x, 24, label, "th"));
        }

        foreach ((string key, string name) in Lanes)
        {
            int[] rows = [.. Enumerable.Range(0, Rows.Length).Where(i => Rows[i].Lane == key)];
            double top = ys[rows[0]], bottom = ys[rows[^1]] + RowHeight;
            s.Add($"<rect x=\"{Svg.N(XBand)}\" y=\"{Svg.N(top)}\" width=\"{Svg.N(WBand)}\" height=\"{Svg.N(bottom - top)}\" rx=\"6\" fill=\"var(--{key})\" opacity=\".14\"/>");
            s.Add($"<rect x=\"{Svg.N(XBand)}\" y=\"{Svg.N(top)}\" width=\"4\" height=\"{Svg.N(bottom - top)}\" rx=\"2\" fill=\"var(--{key})\"/>");
            s.Add(Svg.Text(XBand + 14, ((top + bottom) / 2) + 6, name, "tb"));
        }

        for (int i = 0; i < Rows.Length; i++)
        {
            Row row = Rows[i];
            double y0 = ys[i];
            double cy = y0 + (RowHeight / 2);

            s.Add(Svg.Rect(XPicksUp, y0 + 6, WPicksUp, RowHeight - 12, 6, "chip"));
            s.Add(Svg.Lines(XPicksUp + 12, cy, row.PicksUp));
            s.Add(Svg.Arrow(XPicksUp + WPicksUp, cy, XSkill - 2, cy));

            s.Add(Svg.Rect(XSkill, y0 + 4, WSkill, RowHeight - 8, 6, "box"));
            s.Add($"<rect x=\"{Svg.N(XSkill)}\" y=\"{Svg.N(y0 + 4)}\" width=\"4\" height=\"{Svg.N(RowHeight - 8)}\" rx=\"2\" fill=\"var(--{row.Lane})\"/>");
            s.Add(Svg.Text(XSkill + 14, cy - 4, row.Skill, "tm") + Svg.Text(XSkill + 14, cy + 16, row.Branch ?? "report only", "ts"));

            if (row.Branch is not null)
            {
                s.Add(Svg.Arrow(XSkill + WSkill, cy, XAgentGate - 2, cy));
                s.Add(Svg.Rect(XAgentGate, cy - GateHalf, WAgentGate, GateHalf * 2, 6, "check"));
                s.Add(Svg.Text(XAgentGate + (WAgentGate / 2), cy - 3, "CI: metadata,", "check-t", "middle"));
                s.Add(Svg.Text(XAgentGate + (WAgentGate / 2), cy + 13, "sources, links", "check-t", "middle"));
                s.Add(Svg.Arrow(XAgentGate + WAgentGate, cy, XHumanGate - GateHalf - 2, cy));

                // The human gate is a diamond the height of the agent gate's box.
                s.Add(Svg.Diamond(XHumanGate, cy, GateHalf, "gate"));
                s.Add(Svg.Arrow(XHumanGate + GateHalf, cy, XLandsIn - 2, cy));
                s.Add(Svg.Text(XHumanGate + GateHalf + 8, cy - 9, row.HumanGate, "gate-t"));
            }
            else
            {
                s.Add(Svg.Arrow(XSkill + WSkill, cy, XLandsIn - 2, cy, dash: true));
                s.Add(Svg.Text((XAgentGate + XLandsIn) / 2, cy - 8, "no pull request", "ts", "middle"));
            }
        }

        // Each destination is one store spanning the rows that land in it.
        foreach ((string key, string cls, string[] lines, string subtitle) in new (string, string, string[], string)[]
        {
            ("roster", "store", ["AWESOME-HUMANS.md"], "the roster"),
            ("research", "store", ["research/"], "staged, unresolved"),
            ("opinions", "store", ["opinions/", "templates/"], "distilled guidance"),
            ("report", "ext", ["a report"], "never saved here"),
        })
        {
            int[] rows = [.. Enumerable.Range(0, Rows.Length).Where(i => Rows[i].LandsIn == key)];
            double top = ys[rows[0]] + 4, bottom = ys[rows[^1]] + RowHeight - 4;
            s.Add(Svg.Rect(XLandsIn, top, WLandsIn, bottom - top, 6, cls));
            double ty = ((top + bottom) / 2) - (lines.Length * 11) + 5;
            for (int l = 0; l < lines.Length; l++)
            {
                s.Add(Svg.Text(XLandsIn + 14, ty + (l * 22), lines[l], key == "report" ? "tb" : "tm"));
            }

            s.Add(Svg.Text(XLandsIn + 14, ty + (lines.Length * 22), subtitle, "ts"));
        }

        // The loop: a report's findings, with the scheduled workflows and the owner, start
        // the next run in whichever row it belongs to.
        double loopY = rowsBottom + 56;
        const double StartX = 460, StartWidth = 402;
        s.Add(Svg.PathArrow($"M{Svg.N(XLandsIn + (WLandsIn / 2))} {Svg.N(rowsBottom - 4)}V{Svg.N(loopY)}H{Svg.N(StartX + StartWidth + 2)}"));
        s.Add(Svg.Text(XLandsIn + (WLandsIn / 2) - 8, loopY - 8, "findings", "ts", "end"));
        s.Add(Svg.Rect(StartX, loopY - 34, StartWidth, 68, 6, "box"));
        s.Add(Svg.Text(StartX + 16, loopY - 5, "What starts a run", "tb"));
        s.Add(Svg.Text(StartX + 16, loopY + 17, "findings, the release watch, the harvest reminder, you", "ts"));
        s.Add($"<path d=\"M{Svg.N(StartX)} {Svg.N(loopY)}H{Svg.N(XBracket)}V{Svg.N(ys[0] + (RowHeight / 2))}\" class=\"edge\"/>");
        s.Add(Svg.Text((StartX + XBracket) / 2, loopY - 8, "starts the next skill", "ts", "middle"));
        foreach (double y0 in ys)
        {
            s.Add(Svg.Arrow(XBracket, y0 + (RowHeight / 2), XPicksUp - 1, y0 + (RowHeight / 2)));
        }

        return Svg.Document(Width, loopY + 50, Title, Description, string.Concat(s));
    }
}

internal static class Standing
{
    private const string Title = "Source standing";

    private const string Description =
        "Flow diagram: vet-source sorts a candidate source into citable, with two years of writing and all four criteria; the watch list, under two years or dormant; " +
        "or declined. A citable source is unmarked, marked Corroborate and never the only citation on a claim, or marked Discovery-only and never cited. " +
        "Citable and watch-listed sources go back through vet-source when they go dormant, drop in quality, or clear a blocker.";

    // Each marking a citable source can carry, and what it lets a citation rest on.
    private static readonly (string Marking, string Rule)[] Markings =
    [
        ("unmarked", "may carry a claim alone"),
        ("**Corroborate.**", "cited, never as the only source on a claim"),
        ("**Discovery-only.**", "never cited, only followed to its primary source"),
    ];

    // The candidate sits above vet-source, and each marking's rule sits under it, so the
    // diagram is narrow enough for its text to stay legible in a README. Boxes leave room
    // for DejaVu Sans, the widest fallback a reader without IBM Plex sees.
    private const double CandidateX = 20, CandidateY = 20, CandidateWidth = 232;
    private const double VetX = 136, VetY = 196, VetHalf = 38;
    private const double SpineX = 276;
    private const double OutX = 508, OutWidth = 416;
    private const double LoopX = 1066;
    private const double ChipHeight = 52;

    public static string Build()
    {
        List<string> s =
        [
            Svg.Node(CandidateX, CandidateY, CandidateWidth, 64, "Candidate source", "blog, docs, book, newsletter"),
            Svg.Arrow(VetX, CandidateY + 64, VetX, VetY - VetHalf - 2),
            Svg.Diamond(VetX, VetY, VetHalf, "decision"),
            Svg.Text(VetX + 12, CandidateY + 64 + 40, "vet-source", "tm"),
            Svg.Text(VetX, VetY + 4, "criteria", "ts", "middle"),
            $"<line x1=\"{Svg.N(VetX + VetHalf)}\" y1=\"{Svg.N(VetY)}\" x2=\"{Svg.N(SpineX)}\" y2=\"{Svg.N(VetY)}\" class=\"edge\"/>",
        ];

        // Citable, with its markings inside.
        const double CitableY = 20;
        double citableHeight = 64 + (Markings.Length * (ChipHeight + 8)) + 8;
        s.Add(Svg.Rect(OutX, CitableY, OutWidth, citableHeight, 8, "box"));
        s.Add($"<rect x=\"{Svg.N(OutX)}\" y=\"{Svg.N(CitableY)}\" width=\"4\" height=\"{Svg.N(citableHeight)}\" rx=\"2\" fill=\"var(--accent)\"/>");
        s.Add(Svg.Text(OutX + 16, CitableY + 27, "Citable", "tb"));
        s.Add(Svg.Text(OutX + 16, CitableY + 48, "quotable in opinions and templates, within its marking", "ts"));
        for (int i = 0; i < Markings.Length; i++)
        {
            double y0 = CitableY + 64 + (i * (ChipHeight + 8));
            s.Add(Svg.Rect(OutX + 16, y0, OutWidth - 32, ChipHeight, 6, "chip"));
            s.Add(Svg.Text(OutX + 28, y0 + 22, Markings[i].Marking, "tm"));
            s.Add(Svg.Text(OutX + 28, y0 + 42, Markings[i].Rule, "ts"));
        }

        double watchY = CitableY + citableHeight + 24;
        double declinedY = watchY + 84;
        double loopY = declinedY + 60 + 36;
        s.Add(Svg.Node(OutX, watchY, OutWidth, 60, "Watch list", "leads only, never cited", cls: "ext"));
        s.Add(Svg.Node(OutX, declinedY, OutWidth, 60, "Declined", "off the roster, the decision logged", cls: "ext"));

        foreach ((double y, string label) in new (double, string)[]
        {
            (CitableY + 32, "two years and all four criteria"),
            (watchY + 30, "under two years, or dormant"),
            (declinedY + 30, "otherwise"),
        })
        {
            s.Add(Svg.PathArrow($"M{Svg.N(SpineX)} {Svg.N(VetY)}V{Svg.N(y)}H{Svg.N(OutX - 2)}"));
            s.Add(Svg.Text(SpineX + 12, y - 8, label, "ts"));
        }

        // Standing is never permanent: citable and watch-listed sources share one path back.
        s.Add($"<path d=\"M{Svg.N(OutX + OutWidth)} {Svg.N(CitableY + 32)}H{Svg.N(LoopX)}V{Svg.N(loopY)}\" class=\"edge dash\"/>");
        s.Add($"<path d=\"M{Svg.N(OutX + OutWidth)} {Svg.N(watchY + 30)}H{Svg.N(LoopX)}\" class=\"edge dash\"/>");
        s.Add(Svg.PathArrow($"M{Svg.N(LoopX)} {Svg.N(loopY)}H{Svg.N(VetX)}V{Svg.N(VetY + VetHalf + 2)}", dash: true));
        s.Add(Svg.Text(OutX + OutWidth + 10, CitableY + 24, "dormant, weaker", "ts"));
        s.Add(Svg.Text(OutX + OutWidth + 10, watchY + 22, "blocker clears", "ts"));
        s.Add(Svg.Text((VetX + LoopX) / 2, loopY - 8, "re-vetted: standing is never permanent", "ts", "middle"));

        return Svg.Document(LoopX + 20, loopY + 28, Title, Description, string.Concat(s));
    }
}

internal static class Research
{
    private const string Title = "Research lifecycle";

    private const string Description =
        "Flow diagram: research-topic opens a pull request that saves research/<topic>.md; resolve-research then promotes it into the opinions and templates, " +
        "discards it, or promotes part of it, deleting the topic in the same pull request. A blocked remainder stays staged and goes back through resolve-research " +
        "once vet-source clears the source it waits on.";

    private static readonly (string Name, string Detail)[] Outcomes =
    [
        ("Promote", "opinions and templates updated, topic deleted"),
        ("Discard", "nothing woven, topic deleted"),
        ("Partial promotion", "the rest woven, the blocked remainder stays staged"),
    ];

    // research-topic sits above the topic it saves, so the diagram is narrow enough for its
    // text to stay legible in a README.
    private const double CenterY = 182;
    private const double TopicX = 20, TopicY = 20, TopicWidth = 232;
    private const double ResolveX = 340, ResolveHalf = 38;
    private const double SpineX = 424;
    private const double OutX = 448, OutWidth = 386, OutHeight = 60;
    private const double LoopY = 328;

    public static string Build()
    {
        List<string> s =
        [
            Svg.Node(TopicX, TopicY, TopicWidth, 64, "research-topic", "cited, on research/<topic>", titleCls: "tm"),
            Svg.Arrow(TopicX + (TopicWidth / 2), TopicY + 64, TopicX + (TopicWidth / 2), CenterY - 34),
            Svg.Text(TopicX + (TopicWidth / 2) + 12, CenterY - 60, "PR merges", "ts"),
            Svg.Node(TopicX, CenterY - 32, TopicWidth, 64, "research/<topic>.md", "on record, unresolved", cls: "store", titleCls: "tm"),
            Svg.Arrow(TopicX + TopicWidth, CenterY, ResolveX - ResolveHalf - 2, CenterY),
            Svg.Diamond(ResolveX, CenterY, ResolveHalf, "decision"),
            Svg.Text(ResolveX, CenterY - ResolveHalf - 14, "resolve-research", "tm", "middle"),
            Svg.Text(ResolveX, CenterY + 4, "own PR", "ts", "middle"),
            $"<line x1=\"{Svg.N(ResolveX + ResolveHalf)}\" y1=\"{Svg.N(CenterY)}\" x2=\"{Svg.N(SpineX)}\" y2=\"{Svg.N(CenterY)}\" class=\"edge\"/>",
        ];

        for (int i = 0; i < Outcomes.Length; i++)
        {
            double y0 = CenterY - OutHeight - 16 - (OutHeight / 2) + (i * (OutHeight + 16));
            double cy = y0 + (OutHeight / 2);
            s.Add(Svg.PathArrow($"M{Svg.N(SpineX)} {Svg.N(CenterY)}V{Svg.N(cy)}H{Svg.N(OutX - 2)}"));
            s.Add(Svg.Node(OutX, y0, OutWidth, OutHeight, Outcomes[i].Name, Outcomes[i].Detail, cls: i == 0 ? "box" : "ext", accent: i == 0 ? "var(--accent)" : null));
        }

        // The remainder waits for vet-source, then goes round again.
        double partialBottom = CenterY + OutHeight + 16 + (OutHeight / 2);
        s.Add(Svg.PathArrow($"M{Svg.N(OutX + (OutWidth / 2))} {Svg.N(partialBottom)}V{Svg.N(LoopY)}H{Svg.N(ResolveX)}V{Svg.N(CenterY + ResolveHalf + 2)}", dash: true));
        s.Add(Svg.Text((ResolveX + OutX + (OutWidth / 2)) / 2, LoopY - 8, "once vet-source clears its source", "ts", "middle"));

        return Svg.Document(OutX + OutWidth + 20, LoopY + 28, Title, Description, string.Concat(s));
    }
}

internal static class Svg
{
    // The design system's tokens: bg, surface, raised, line, ink, muted, brand, house,
    // and the three lane-* tokens.
    // Copy a changed value from https://claude.ai/artifact/QfYm5d57sbJxs3W3jMyGiy, as the
    // website repository's awesome-humans.css does. The .NET accent stays out: the design
    // system shows an ecosystem's accent only in four places on the site, and a diagram
    // isn't one. Brand marks the CI gate and the outcome the agents work towards, and house
    // what the owner decides. Each lane takes its lane-* token, chosen to stay apart from
    // those and from stale. lane-reports sits near the .NET accent, which no diagram shows.
    private const string TokensLight = """
        --bg: #ffffff; --surface: #f6f7f8; --band: #eef0f2; --fg: #0d1117; --muted: #5b6572; --line: #e3e6ea;
          --accent: #087d4e; --human: #9a6100; --auto: #087d4e;
          --sources: #017b80; --research: #6d7123; --opinions: #955080; --reports: #8253ac;
        """;

    private const string TokensDark = """
        --bg: #0d1117; --surface: #151a21; --band: #1c232c; --fg: #e8ecf0; --muted: #8b95a1; --line: #2a323c;
          --accent: #3dd392; --human: #f2c14e; --auto: #3dd392;
          --sources: #6ac9ce; --research: #b9be71; --opinions: #e79bce; --reports: #c9a5e9;
        """;

    // Font families are literal rather than custom properties, and set apart from size and
    // weight rather than through the font shorthand: a browser that rejects either form
    // drops the whole declaration and draws every label in its default font, which is wider
    // than the layout allows for. system-ui is left out because privacy-hardened browsers
    // restrict it. Each stack names the design system's IBM Plex first, then a common font
    // per platform before the generic family. An SVG shown as an image can't load a web
    // font, so most readers see a fallback.
    private const string Fonts = """
        .t, .tb, .ts, .gate-t, .check-t { font-family: "IBM Plex Sans", "Segoe UI", "Noto Sans", "DejaVu Sans", "Helvetica Neue", Arial, sans-serif; }
        .tm, .th { font-family: "IBM Plex Mono", "Cascadia Mono", Consolas, "Noto Sans Mono", "DejaVu Sans Mono", Menlo, monospace; }
        """;

    private const string Classes = """
        .bg { fill: var(--bg); }
        .box { fill: var(--surface); stroke: var(--line); stroke-width: 1.2; }
        .chip { fill: var(--band); stroke: none; }
        .ext { fill: var(--band); stroke: var(--muted); stroke-width: 1.2; }
        .store { fill: var(--band); stroke: var(--line); stroke-width: 1.2; }
        .t { fill: var(--fg); font-weight: 500; font-size: 16px; }
        .tb { fill: var(--fg); font-weight: 600; font-size: 16px; }
        .tm { fill: var(--fg); font-weight: 600; font-size: 15px; }
        .ts { fill: var(--muted); font-weight: 400; font-size: 14px; }
        .th { fill: var(--muted); font-weight: 600; font-size: 13px; letter-spacing: .08em; text-transform: uppercase; }
        .edge { stroke: var(--muted); stroke-width: 1.4; fill: none; }
        .edge.dash { stroke-dasharray: 4 4; }
        .arrowhead { fill: var(--muted); }
        .gate { fill: var(--surface); stroke: var(--human); stroke-width: 2; }
        .gate-t { fill: var(--human); font-weight: 600; font-size: 14px; }
        .check { fill: var(--surface); stroke: var(--auto); stroke-width: 2; }
        .check-t { fill: var(--auto); font-weight: 600; font-size: 14px; }
        .decision { fill: var(--surface); stroke: var(--accent); stroke-width: 2; }
        """;

    private const string Marker = "<defs><marker id=\"ah\" viewBox=\"0 0 10 10\" refX=\"9\" refY=\"5\" markerWidth=\"7\" markerHeight=\"7\" orient=\"auto-start-reverse\"><path class=\"arrowhead\" d=\"M0 0L10 5L0 10z\"/></marker></defs>";

    // Wraps the drawing in a standalone, accessible SVG document with its styles and background.
    public static string Document(double width, double height, string title, string description, string body)
    {
        string style = "\nsvg { " + TokensLight + " }\n"
            + "@media (prefers-color-scheme: dark) { svg { " + TokensDark + " } }\n\n"
            + Fonts + Classes + "\n\n";
        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + $"<svg role=\"img\" aria-labelledby=\"title desc\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {N(width)} {N(height)}\">"
            + $"<title id=\"title\">{Escape(title)}</title><desc id=\"desc\">{Escape(description)}</desc><style>{style}</style>{Marker}"
            + $"<rect width=\"{N(width)}\" height=\"{N(height)}\" rx=\"10\" class=\"bg\"/>"
            + body + "</svg>\n";
    }

    // A titled box with a subtitle and an optional coloured edge on its left.
    public static string Node(double x, double y, double width, double height, string title, string subtitle, string cls = "box", string titleCls = "tb", string? accent = null)
    {
        string edge = accent is null
            ? ""
            : $"<rect x=\"{N(x)}\" y=\"{N(y)}\" width=\"4\" height=\"{N(height)}\" rx=\"2\" fill=\"{accent}\"/>";
        double cy = y + (height / 2);
        return Rect(x, y, width, height, 8, cls) + edge + Text(x + 16, cy - 4, title, titleCls) + Text(x + 16, cy + 17, subtitle, "ts");
    }

    public static string Diamond(double cx, double cy, double half, string cls) =>
        $"<path d=\"M{N(cx)} {N(cy - half)}L{N(cx + half)} {N(cy)}L{N(cx)} {N(cy + half)}L{N(cx - half)} {N(cy)}z\" class=\"{cls}\"/>";

    public static string Text(double x, double y, string text, string cls = "t", string anchor = "start") =>
        $"<text x=\"{N(x)}\" y=\"{N(y)}\" class=\"{cls}\" text-anchor=\"{anchor}\">{Escape(text)}</text>";

    // Lines of text, vertically centred on y.
    public static string Lines(double x, double y, string[] items, string cls = "t")
    {
        const double Step = 19;
        double top = y - ((items.Length - 1) * Step / 2) + 5;
        return string.Concat(items.Select((item, i) => Text(x, top + (i * Step), item, cls)));
    }

    public static string Rect(double x, double y, double width, double height, double radius, string cls) =>
        $"<rect x=\"{N(x)}\" y=\"{N(y)}\" width=\"{N(width)}\" height=\"{N(height)}\" rx=\"{N(radius)}\" class=\"{cls}\"/>";

    public static string Arrow(double x1, double y1, double x2, double y2, bool dash = false) =>
        $"<line x1=\"{N(x1)}\" y1=\"{N(y1)}\" x2=\"{N(x2)}\" y2=\"{N(y2)}\" class=\"{EdgeClass(dash)}\" marker-end=\"url(#ah)\"/>";

    public static string PathArrow(string d, bool dash = false) =>
        $"<path d=\"{d}\" class=\"{EdgeClass(dash)}\" marker-end=\"url(#ah)\"/>";

    public static string N(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string EdgeClass(bool dash) => dash ? "edge dash" : "edge";

    private static string Escape(string text) => text
        .Replace("&", "&amp;", StringComparison.Ordinal)
        .Replace("<", "&lt;", StringComparison.Ordinal)
        .Replace(">", "&gt;", StringComparison.Ordinal)
        .Replace("\"", "&quot;", StringComparison.Ordinal);
}
