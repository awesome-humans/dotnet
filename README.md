# dotnet-awesome-humans

Opinionated best practices for modern .NET.

This project distils the published guidance of awesome humans: people and publications with a proven, multi-year track record, listed in [AWESOME-HUMANS.md](AWESOME-HUMANS.md). It answers the question _"What does good look like in .NET right now?"_, and it keeps answering as .NET changes.

You can use it in three ways:

- **Point an agent at it.** Tell your agent: "Follow the conventions in <https://github.com/si618/dotnet-awesome-humans> when writing .NET code." The opinions cover conventions, project layout, language usage, and library choices.
- **Scaffold from it.** Copy files from [`templates/`](templates), or run [`verify-project`](skills/verify-project/SKILL.md) against an existing codebase.
- **Read it.** Browse it at [awesome-humans.net](https://awesome-humans.net/dotnet/) or here on GitHub. Each opinion gives the recommendation first, then the rationale, then the sources, with code examples where they help.

## Scope

The opinions cover the main areas of modern .NET. They live in [`opinions/`](opinions), one topic per file:

- **[Application architecture](opinions/architecture.md):** modular monoliths, vertical slices, and when layering pays for its cost
- **[ASP.NET Core](opinions/aspnet-core.md):** minimal APIs, hosting, authentication, OpenAPI, and performance
- **[C#](opinions/csharp.md):** the latest released language version and idiomatic use of its features
- **[CI & automation](opinions/ci.md):** pinned, reproducible builds and supply-chain security
- **[Data access](opinions/data-access.md):** EF Core defaults, set-based operations, and when to drop to SQL
- **[Dates, times & time zones](opinions/datetime.md):** choosing a type, UTC versus local storage, `TimeProvider`, and testing time
- **[F#](opinions/fsharp.md):** domain modelling, mixed C# and F# solutions, and testing
- **[Globalization & localization](opinions/globalization.md):** culture versus ordinal comparison, ICU and invariant mode, `IStringLocalizer`, and the data that containers drop
- **[Logging & tracing](opinions/logging.md):** structured logging, source-generated log messages, and OpenTelemetry over OTLP
- **[Project structure & SDK](opinions/project-structure.md):** project files, solution formats, central package management, analyzers, and source generators
- **[Runtime & BCL](opinions/runtime-performance.md):** performance idioms, `Span<T>` and memory, async code, and the GC
- **[Testing](opinions/testing.md):** choosing a framework, naming and structure, integration tests, and coverage
- **[UI frameworks](opinions/ui-frameworks.md):** Blazor and WebAssembly, .NET MAUI, and cross-platform desktop with Avalonia
- **Libraries:** what to use and what to avoid, covered in the files above

List a new opinion file in two places: here, in title order above **Libraries**, and in [Repository layout](#repository-layout), in file-name order. CI fails the pull request if either index is incomplete or out of order.

## Freshness policy

Opinions target the latest released versions of .NET, C#, and F#, never an older LTS release. Preview features appear only in "Coming next" notes. The [skills](#maintenance-via-skills) below bring in each new version.

Every resource records when it was last reviewed, so you can see when it's stale. Files in `opinions/` and `templates/` also record when they were last used as a reference. Opinions and research topics store these fields as YAML frontmatter. Templates store them in a first-line comment, because an XML or INI file can't open with a `---` block. [AGENTS.md: Metadata](AGENTS.md#metadata) defines the fields and their rules, and CI enforces them.

## Awesome humans

Every opinion traces back to a vetted source: a person, such as Stephen Toub or Andrew Lock, or a publication, such as the .NET Blog or Microsoft Learn. A source is admitted on its track record: at least two years of sustained writing, with depth, accuracy, and an independent view. Videos, talks, and podcasts are out of scope for now, because an opinion cites text that a reader can check. [AWESOME-HUMANS.md](AWESOME-HUMANS.md) has the roster and the full criteria.

The following diagram shows how a source gets in and what its standing lets it do. It's for orientation only: the admission criteria in AWESOME-HUMANS.md and the [`vet-source`](skills/vet-source/SKILL.md) skill are canonical.

![Source standing: vet-source sorts a candidate into citable, watch list or declined, and a citable source is unmarked, Corroborate or Discovery-only](assets/diagrams/source-standing.svg)

Standing isn't permanent. `vet-source` runs again when an admitted source goes dormant or drops in quality, and when the blocker on a watch-listed source clears.

There's one citable tier. Two markings in a source's notes narrow what a citation can rest on:

- **Corroborate:** A source marked `**Corroborate.**` is never the only citation for a claim.
- **Discovery-only:** A source marked `**Discovery-only.**` is never cited. Follow it to the primary source it points at instead.

Any other limit on a source, such as a concern about independence or an outdated back catalogue, goes in its notes as prose. It doesn't change the source's standing.

### House opinions

One person outranks the roster: the repository owner. The owner's preferences come in through [HOUSE-OPINIONS.md](HOUSE-OPINIONS.md) and the [`weave-house-opinion`](skills/weave-house-opinion/SKILL.md) skill. They're always marked where they appear, so you can tell community best practice from local convention. HOUSE-OPINIONS.md defines the marking and which side wins when the two conflict. Other contributors can propose opinions, sourced or based on experience, through the [pull request template](.github/PULL_REQUEST_TEMPLATE.md).

## Repository layout

```text
├── README.md                 ← you are here
├── AGENTS.md                 ← instructions for AI agents working in this repo
├── AWESOME-HUMANS.md         ← vetted sources and admission criteria
├── HOUSE-OPINIONS.md         ← the owner's own opinions: intake and audit trail
├── assets/diagrams/          ← generated SVG diagrams: rerun the script, never edit by hand
├── opinions/                 ← the opinions, one topic per file, code examples as needed
│   ├── architecture.md
│   ├── aspnet-core.md
│   ├── ci.md
│   ├── csharp.md
│   ├── data-access.md
│   ├── datetime.md
│   ├── fsharp.md
│   ├── globalization.md
│   ├── logging.md
│   ├── project-structure.md
│   ├── runtime-performance.md
│   ├── testing.md
│   └── ui-frameworks.md
├── research/                 ← saved research topics (staging: promote or discard)
├── site/                     ← the awesome-humans.net site: configuration and hand-written pages
├── scripts/                  ← this repository's own CI checks, as .NET file-based apps
│   ├── CommentHeader.cs      ← shared helper, pulled in with #:include
│   ├── Frontmatter.cs        ← shared helper, pulled in with #:include
│   ├── Opinions.cs           ← shared helper, pulled in with #:include
│   ├── build-site.cs
│   ├── export-diagrams.cs
│   ├── validate-metadata.cs
│   ├── validate-readme-index.cs
│   └── validate-sources.cs
├── templates/                ← copy-paste-ready example files
│   ├── .editorconfig
│   ├── Directory.Build.props
│   ├── Directory.Packages.props
│   ├── example.slnf
│   ├── example.slnx
│   ├── global.json
│   └── projects/             ← example .csproj / .fsproj files
└── skills/                   ← maintenance skills (see below)
```

## Maintenance via skills

The repository maintains itself through agent skills that follow the [Agent Skills specification](https://agentskills.io/specification), so any compliant agent can run them. Each skill is a directory under [`skills/`](skills) with a `SKILL.md` file.

The skills split work by cost. Cheaper worker agents fan out across web searches and source sweeps. The strongest available model acts as the editor, and it's the only one that writes to the opinions and templates.

Every skill except the two report-only ones works on its own branch. Its changes land through a pull request that passes CI and the owner's review. The following diagram is for orientation only; each `SKILL.md` is canonical.

![Maintenance pipeline: each skill from what starts it, through CI and the owner's review, to the roster, research/, or the opinions and templates; audit-freshness and verify-project only report](assets/diagrams/maintenance-pipeline.svg)

| Skill                                                                | Purpose                                                                                       |
| -------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| [`refresh-dotnet-versions`](skills/refresh-dotnet-versions/SKILL.md) | Detect new .NET, C#, and F# releases, and update all opinions and templates to target them    |
| [`harvest-sources`](skills/harvest-sources/SKILL.md)                 | Sweep the awesome-humans sources for new posts and fold notable guidance into the opinions    |
| [`vet-source`](skills/vet-source/SKILL.md)                           | Evaluate a candidate source against the track-record criteria and admit or decline            |
| [`audit-freshness`](skills/audit-freshness/SKILL.md)                 | Report resources whose `last-reviewed` date has drifted past tolerance                        |
| [`verify-project`](skills/verify-project/SKILL.md)                   | Check an external project against the template files and report deviations                    |
| [`weave-house-opinion`](skills/weave-house-opinion/SKILL.md)         | Weave a repository-owner opinion into the opinions and templates, visibly marked as House     |
| [`research-topic`](skills/research-topic/SKILL.md)                   | Research a .NET topic conversationally using the opinions and vetted sources, cited and saved |
| [`resolve-research`](skills/resolve-research/SKILL.md)               | Resolve a saved research topic by weaving it into the opinions and templates, or discard it   |

### Research lifecycle

Research is staged, never merged in place. `research-topic` saves the evidence, and `resolve-research` decides what becomes an opinion. Either way, the same pull request deletes the file. So a topic on disk is unresolved by definition, and deletion marks it as resolved. The following diagram is for orientation only; the two `SKILL.md` files are canonical.

![Research lifecycle: research-topic saves research/{topic}.md, and resolve-research promotes, discards or partly promotes it](assets/diagrams/research-lifecycle.svg)

If part of a topic is blocked on a source, it goes back through `resolve-research` after `vet-source` clears that source. Otherwise, it's discarded.

### Release watch automation

A scheduled GitHub Action, [`.github/workflows/dotnet-release-watch.yml`](.github/workflows/dotnet-release-watch.yml), checks the official [.NET releases index](https://github.com/dotnet/core/blob/main/release-notes/releases-index.json) daily against the snapshot in [`.github/state/dotnet-releases.json`](.github/state/dotnet-releases.json). When a new GA release appears, it opens a pull request that updates the snapshot. That pull request is the cue to run [`refresh-dotnet-versions`](skills/refresh-dotnet-versions/SKILL.md). It doesn't change any opinion or template itself.

## Repository scripts

The checks that gate a pull request use the same stack this repository has opinions about. [`scripts/`](scripts) holds them as .NET 10 file-based apps. They have no project file and no build step. Each one declares its dependencies inline with `#:package` and pulls in shared code with `#:include`.

| Script                                                         | Checks                                                                                                                                                                                                                   |
| -------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| [`validate-metadata.cs`](scripts/validate-metadata.cs)         | Every resource under `opinions/`, `research/`, and `templates/` has `targets`, `last-reviewed`, and `sources`, plus `last-used` outside `research/`, with ISO 8601 dates                                                 |
| [`validate-sources.cs`](scripts/validate-sources.cs)           | Every source ID in `opinions/` and `templates/` exists in the AWESOME-HUMANS.md roster and is citable. The roster tables and notes sections are sorted by ID, no ID appears twice, and every notes section matches a row |
| [`validate-readme-index.cs`](scripts/validate-readme-index.cs) | This README lists every opinion and skill, and lists nothing that doesn't exist. Scope is sorted by title, and the layout tree by file name                                                                              |
| [`export-diagrams.cs`](scripts/export-diagrams.cs)             | Draws the SVG diagrams in `assets/diagrams/`. It doesn't check anything, but CI reruns it and fails if the committed SVGs differ from its output                                                                         |
| [`build-site.cs`](scripts/build-site.cs)                       | Stages the awesome-humans.net site in `site/build/` and generates its `/dotnet/` pages from this repository's files. CI then builds the site with Zensical in strict mode, which fails on any broken link or anchor      |

Run them from the repository root, the same way CI does:

```sh
dotnet run scripts/validate-metadata.cs
```

Three helper files support the scripts:

- [`Opinions.cs`](scripts/Opinions.cs) lists the opinion files.
- [`Frontmatter.cs`](scripts/Frontmatter.cs) parses the YAML frontmatter in `opinions/` and `research/` files.
- [`CommentHeader.cs`](scripts/CommentHeader.cs) lists the `templates/` files that carry a header and parses their first-line comment.

The helpers don't run on their own. Each has no top-level statements and compiles into whichever script includes it with `#:include`.

The SDK version comes from [`global.json`](global.json). Its minimum is the first feature band that supports `#:include`. It's independent of [`templates/global.json`](templates/global.json), because each pin follows what its own consumers need. `rollForward: latestFeature` picks up newer feature bands without an edit. One check still runs in Python: the Agent Skills specification validator, which is published only to PyPI.

## License

See [LICENSE](LICENSE).
