# dotnet-awesome-humans

Opinionated best practices for modern .NET.

Distils the published guidance of awesome humans: people and publications with a proven, multi-year track record, catalogued in [AWESOME-HUMANS.md](AWESOME-HUMANS.md). It answers the question _"what does good look like in .NET right now?"_ and keeps answering it as .NET improves.

How to use this project:

- **Point an agent at it.** "Follow the conventions in <https://github.com/si618/dotnet-awesome-humans> when writing .NET code." The opinions cover conventions, project layout, language usage, and library choices.
- **Scaffold from it.** Copy files out of [`templates/`](templates), or run [`verify-project`](skills/verify-project/SKILL.md) against a codebase you already have.
- **Read it.** At [awesome-humans.net](https://awesome-humans.net/dotnet/), or here on GitHub. Every opinion starts with the recommendation, then the rationale, then the sources, with code examples where they help.

## Scope

The main areas of modern .NET. The opinions live in [`opinions/`](opinions), one topic per file:

- **[Application architecture](opinions/architecture.md):** modular monolith, vertical slices, and when layering is worth its cost
- **[ASP.NET Core](opinions/aspnet-core.md):** minimal APIs, hosting, auth, OpenAPI, performance
- **[C#](opinions/csharp.md):** the latest released language version, and idiomatic use of what it added
- **[CI & automation](opinions/ci.md):** pinned, reproducible builds and supply-chain hygiene
- **[Data access](opinions/data-access.md):** EF Core defaults, set-based work, when to drop to SQL
- **[Dates, times & time zones](opinions/datetime.md):** type choice, UTC vs local storage, `TimeProvider`, testing time
- **[F#](opinions/fsharp.md):** domain modelling, mixed C#/F# solutions, testing
- **[Globalization & localization](opinions/globalization.md):** culture vs ordinal, ICU and invariant mode, `IStringLocalizer`, and the data containers drop
- **[Logging & tracing](opinions/logging.md):** structured logging, source-generated log messages, OpenTelemetry over OTLP
- **[Project structure & SDK](opinions/project-structure.md):** project files, solution formats, central package management, analyzers, source generators
- **[Runtime & BCL](opinions/runtime-performance.md):** performance idioms, `Span<T>`/memory, async, GC awareness
- **[Testing](opinions/testing.md):** framework choice, naming and structure, integration tests, coverage
- **[UI frameworks](opinions/ui-frameworks.md):** Blazor/WebAssembly, .NET MAUI, and cross-platform desktop (Avalonia)
- **Libraries:** what to use and what to avoid, spread across the files above

A new opinion file must appear both here, in title order above **Libraries**, and in [Repository layout](#repository-layout), in file-name order. CI fails the pull request when either index is incomplete or out of order.

## Freshness policy

Opinions target the latest released versions of .NET, C#, and F#, never an older LTS, with preview features confined to "Coming next" asides. New versions are folded in by the [skills](#maintenance-via-skills) below.

Every resource records when it was last reviewed, so staleness is visible, and `opinions/` and `templates/` also record when each was last used as a reference. Opinions and research topics carry the fields as YAML frontmatter; templates carry them in a first-line comment header, because an XML or INI file cannot open with a `---` block. [AGENTS.md: Metadata](AGENTS.md#metadata) defines the fields and their rules, and CI enforces them.

## Awesome humans

Opinions have to be earned. Each one traces back to a vetted source: an individual (Stephen Toub, Andrew Lock) or a publication (the .NET Blog, Microsoft Learn). Admission is based on track record: two years of sustained writing at minimum, plus depth, accuracy, and independence of signal. Video, talks and podcasts are out of scope at this stage, because an opinion cites text a reader can check. The roster and the full criteria are in [AWESOME-HUMANS.md](AWESOME-HUMANS.md).

How a source gets in, and what its standing permits it to do (orientation only: the admission criteria in AWESOME-HUMANS.md and the [`vet-source`](skills/vet-source/SKILL.md) skill are canonical):

![Source standing: vet-source sorts a candidate into citable, watch list or declined, and a citable source is unmarked, Corroborate or Discovery-only](assets/diagrams/source-standing.svg)

Standing is never permanent. `vet-source` runs again when an admitted source goes dormant or drops in quality, and when a watch-listed source's blocker clears. There is one citable tier, and two markings in a source's notes section narrow what a citation may rest on: a `**Corroborate.**` source is never the only citation on a claim, and a `**Discovery-only.**` source is never cited at all, only followed to the primary source it points at. Anything else that limits a source, such as an independence concern or a back catalogue that has aged out, is written into its notes as prose rather than encoded in its standing.

### House opinions

One human outranks the roster: the repository owner. Their preferences enter through [HOUSE-OPINIONS.md](HOUSE-OPINIONS.md) and the [`weave-house-opinion`](skills/weave-house-opinion/SKILL.md) skill, and are always marked in place, so a reader can tell community best practice from local convention. HOUSE-OPINIONS.md defines the marking literal and what wins when the two conflict. Other contributors propose opinions, sourced or experience-based, through the [pull request template](.github/PULL_REQUEST_TEMPLATE.md).

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

The repository maintains itself through agent skills following the [Agent Skills specification](https://agentskills.io/specification), so any compliant agent can run them. Each is a directory under [`skills/`](skills) with a `SKILL.md`.

They are built for a cost-aware split: cheaper worker agents fan out across web searches and source sweeps, and the strongest available model acts as editor, the only one that writes to the opinions and templates.

Every skill but the two report-only ones works on its own branch and lands through a pull request that passes CI and the owner's review (orientation only; each `SKILL.md` is canonical):

![Maintenance pipeline: each skill from what starts it, through CI and the owner's review, to the roster, research/, or the opinions and templates; audit-freshness and verify-project only report](assets/diagrams/maintenance-pipeline.svg)

| Skill                                                                | Purpose                                                                                       |
| -------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`refresh-dotnet-versions`](skills/refresh-dotnet-versions/SKILL.md) | Detect new .NET / C# / F# releases and update all opinions and templates to target them       |
| [`harvest-sources`](skills/harvest-sources/SKILL.md)                 | Sweep the awesome-humans sources for new posts and fold notable guidance into the opinions    |
| [`vet-source`](skills/vet-source/SKILL.md)                           | Evaluate a candidate source against the track-record criteria and admit or decline            |
| [`audit-freshness`](skills/audit-freshness/SKILL.md)                 | Report resources whose `last-reviewed` date has drifted past tolerance                        |
| [`verify-project`](skills/verify-project/SKILL.md)                   | Check an external project against the template files and report deviations                    |
| [`weave-house-opinion`](skills/weave-house-opinion/SKILL.md)         | Weave a repository-owner opinion into the opinions and templates, visibly marked as House     |
| [`research-topic`](skills/research-topic/SKILL.md)                   | Research a .NET topic conversationally using the opinions and vetted sources, cited and saved |
| [`resolve-research`](skills/resolve-research/SKILL.md)               | Resolve a saved research topic by weaving it into the opinions and templates, or discard it   |

### Research lifecycle

Research is staged, never merged in place: `research-topic` saves the evidence, `resolve-research` decides what becomes the opinion. Both outcomes end with the file deleted in the same pull request. A topic on disk is unresolved by definition, and deletion is the promotion marker (orientation only; the two `SKILL.md` files are canonical):

![Research lifecycle: research-topic saves research/{topic}.md, and resolve-research promotes, discards or partly promotes it](assets/diagrams/research-lifecycle.svg)

A blocked remainder goes back through `resolve-research` once `vet-source` clears the source it was waiting on, or is discarded.

### Release watch automation

A scheduled GitHub Action ([`.github/workflows/dotnet-release-watch.yml`](.github/workflows/dotnet-release-watch.yml)) polls the official [.NET releases index](https://github.com/dotnet/core/blob/main/release-notes/releases-index.json) daily against the checked-in snapshot at [`.github/state/dotnet-releases.json`](.github/state/dotnet-releases.json). A new GA release opens a pull request updating that snapshot. The PR is a trigger for running [`refresh-dotnet-versions`](skills/refresh-dotnet-versions/SKILL.md); it changes no opinion or template itself.

## Repository scripts

The checks that gate a pull request are written in the stack this repository has opinions about. [`scripts/`](scripts) holds them as .NET 10 file-based apps: no project file, no build step, dependencies declared inline with `#:package` and shared code pulled in with `#:include`.

| Script                                                         | Checks                                                                                                                                                                                                                                      |
| -------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`validate-metadata.cs`](scripts/validate-metadata.cs)         | Every resource under `opinions/`, `research/` and `templates/` carries `targets`, `last-reviewed` and `sources` (plus `last-used` outside `research/`) with ISO 8601 dates                                                                  |
| [`validate-sources.cs`](scripts/validate-sources.cs)           | Every source id in `opinions/` and `templates/` resolves to the roster in AWESOME-HUMANS.md and is allowed to cite; the roster tables and the notes sections are sorted by id, with no id used twice, and every notes section matches a row |
| [`validate-readme-index.cs`](scripts/validate-readme-index.cs) | This README indexes every opinion and skill, in both directions, with Scope sorted by title and the layout tree by file name                                                                                                                |
| [`export-diagrams.cs`](scripts/export-diagrams.cs)             | Draws the SVG diagrams in `assets/diagrams/` rather than checking anything. CI reruns it and fails when the committed SVGs differ from what it draws                                                                                        |
| [`build-site.cs`](scripts/build-site.cs)                       | Stages the awesome-humans.net site into `site/build/`, generating its `/dotnet/` pages from this repository's files; CI then builds it with Zensical in strict mode, which fails on any broken link or anchor                               |

Run them from the repository root, exactly as CI does:

```sh
dotnet run scripts/validate-metadata.cs
```

[`Opinions.cs`](scripts/Opinions.cs) lists the opinion files, [`Frontmatter.cs`](scripts/Frontmatter.cs) parses the YAML frontmatter on `opinions/` and `research/` files, and [`CommentHeader.cs`](scripts/CommentHeader.cs) lists the header-carrying `templates/` files and parses their first-line comment header. None of the three helpers runs alone: each declares no top-level statements and compiles into whichever script `#:include`s it.

The SDK comes from [`global.json`](global.json). Its floor is the feature band that understands `#:include`. It is not tied to [`templates/global.json`](templates/global.json): each pin follows what its consumers need, and `rollForward: latestFeature` picks up newer bands without an edit. One check is still Python: the Agent Skills spec validator, published only to PyPI.

## License

See [LICENSE](LICENSE).
