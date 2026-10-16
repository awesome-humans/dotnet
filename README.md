# Awesome Humans of .NET

An agentic take on curated awesome-\* lists. Instead of links, it distils what proven .NET writers recommend into opinions your agent can follow, each one citing its sources.

The writers are listed in [AWESOME-HUMANS.md](AWESOME-HUMANS.md): people and publications with at least two years of accurate, independent writing on .NET. It answers _"What does good look like in .NET right now?"_, and updates the answer as .NET changes.

## Usage

- **Point an agent at it.** Tell your agent: "Follow the conventions in <https://awesome-humans.net/llms.txt> when you write .NET code." The opinions cover conventions, project layout, language usage, and library choices.
- **Scaffold from it.** Copy files from [`templates/`](templates), or check an existing codebase with [`verify-project`](skills/verify-project/SKILL.md).
- **Read it.** Browse it at [awesome-humans.net](https://awesome-humans.net/dotnet/) or on GitHub. Each opinion gives the recommendation, then the rationale, then the sources, with code where it helps.

## Scope

[`opinions/`](opinions) holds one file per topic:

- **[Application architecture](opinions/architecture.md):** modular monoliths, vertical slices, and when layering is worth its cost
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
- **[Security](opinions/security.md):** secrets, deserialization, cryptography, and servicing cadence
- **[Testing](opinions/testing.md):** choosing a framework, naming and structure, integration tests, and coverage
- **[UI frameworks](opinions/ui-frameworks.md):** Blazor and WebAssembly, .NET MAUI, and cross-platform desktop with Avalonia
- **Libraries:** what to use and what to avoid, covered in the files above

## Freshness policy

Opinions target the latest released .NET, C#, and F#, never an older LTS release. Preview features appear only in "Coming next" notes. The [skills](#maintenance-via-skills) move the repository to each new version.

Every resource records when it was last reviewed against its sources, and opinions and templates also record when an agent last used them. [AGENTS.md: Metadata](AGENTS.md#metadata) defines the fields, and CI enforces them.

## Awesome humans

Every opinion cites a vetted source: a person, such as Stephen Toub or Andrew Lock, or a publication, such as the .NET Blog or Microsoft Learn. A source is admitted on its track record: at least two years of sustained writing, with depth, accuracy, and an independent view. Videos, talks, and podcasts are out of scope for now, because an opinion cites text a reader can check. [AWESOME-HUMANS.md](AWESOME-HUMANS.md) has the roster and the criteria.

This diagram shows how a source is admitted and what its standing allows. The admission criteria in AWESOME-HUMANS.md and the [`vet-source`](skills/vet-source/SKILL.md) skill are canonical.

![Source standing: vet-source sorts a candidate into citable, watch list or declined, and a citable source is unmarked, Corroborate or Discovery-only](assets/diagrams/source-standing.svg)

Standing can change. `vet-source` runs again when an admitted source goes dormant or drops in quality, and when a watch-listed source's blocker clears.

There's one citable tier. Two markings in a source's notes limit how it's cited:

- **Corroborate:** A source marked `**Corroborate.**` is never the only citation for a claim.
- **Discovery-only:** A source marked `**Discovery-only.**` is never cited. Cite the primary source it links to instead.

Any other limit, such as a concern about independence or an outdated back catalogue, goes in the source's notes as prose, and doesn't change its standing.

### House opinions

The repository owner outranks the roster. The owner's preferences come in through [HOUSE-OPINIONS.md](HOUSE-OPINIONS.md) and the [`weave-house-opinion`](skills/weave-house-opinion/SKILL.md) skill, and they're marked wherever they appear, so you can tell community best practice from local convention. HOUSE-OPINIONS.md defines the marking and which side wins a conflict. Other contributors can propose sourced or experience-based opinions through the organisation's [pull request template](https://github.com/awesome-humans/.github/blob/main/PULL_REQUEST_TEMPLATE.md).

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
│   ├── security.md
│   ├── testing.md
│   └── ui-frameworks.md
├── research/                 ← saved research topics (staging: promote or discard)
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

Agent skills maintain the repository. They follow the [Agent Skills specification](https://agentskills.io/specification), so any compliant agent can run them. Each skill is a directory under [`skills/`](skills) with a `SKILL.md` file.

The skills split work by cost. Cheaper worker agents run the web searches and source sweeps in parallel. The strongest available model is the editor, and only the editor writes to the opinions and templates.

Every skill except the two report-only ones works on its own branch, and its changes land through a pull request that passes CI and the owner's review. Each `SKILL.md` is canonical over this diagram.

![Maintenance pipeline: each skill from what starts it, through CI and the owner's review, to the roster, research/, or the opinions and templates; audit-freshness and verify-project only report](assets/diagrams/maintenance-pipeline.svg)

| Skill                                                                | Purpose                                                                                     |
| -------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| [`refresh-dotnet-versions`](skills/refresh-dotnet-versions/SKILL.md) | Detect new .NET, C#, and F# releases, and update all opinions and templates to target them  |
| [`harvest-sources`](skills/harvest-sources/SKILL.md)                 | Sweep the awesome-humans sources for new posts, and add notable guidance to the opinions    |
| [`vet-source`](skills/vet-source/SKILL.md)                           | Evaluate a candidate source against the track-record criteria and admit or decline          |
| [`audit-freshness`](skills/audit-freshness/SKILL.md)                 | Report resources whose `last-reviewed` date has drifted past tolerance                      |
| [`verify-project`](skills/verify-project/SKILL.md)                   | Check an external project against the opinions and templates, and report deviations         |
| [`weave-house-opinion`](skills/weave-house-opinion/SKILL.md)         | Weave a repository-owner opinion into the opinions and templates, visibly marked as House   |
| [`research-topic`](skills/research-topic/SKILL.md)                   | Research a .NET topic from the opinions and vetted sources, and save it with citations      |
| [`resolve-research`](skills/resolve-research/SKILL.md)               | Resolve a saved research topic by weaving it into the opinions and templates, or discard it |

### Research lifecycle

Research is staged, never merged in place. `research-topic` saves the evidence, and `resolve-research` decides what becomes an opinion and deletes the file in the same pull request. A topic on disk is unresolved. The two `SKILL.md` files are canonical over this diagram.

![Research lifecycle: research-topic saves a topic under research/, and resolve-research promotes, discards or partly promotes it](assets/diagrams/research-lifecycle.svg)

A part that waits on an unvetted source stays on disk. It goes back through `resolve-research` if `vet-source` admits the source, and is discarded if not.

### Release watch automation

A scheduled GitHub Action, [`.github/workflows/dotnet-release-watch.yml`](.github/workflows/dotnet-release-watch.yml), checks the official [.NET releases index](https://github.com/dotnet/core/blob/main/release-notes/releases-index.json) daily against the snapshot in [`.github/state/dotnet-releases.json`](.github/state/dotnet-releases.json). When a new GA release appears, it opens a pull request that updates only the snapshot. That pull request is the cue to run [`refresh-dotnet-versions`](skills/refresh-dotnet-versions/SKILL.md).

## Repository scripts

[`scripts/`](scripts) holds the pull request checks as .NET 10 file-based apps, with no project file and no build step. Each declares its dependencies with `#:package` and pulls in shared code with `#:include`.

| Script                                                         | Checks                                                                                                                                                                                                                                                                                                                                    |
| -------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`validate-metadata.cs`](scripts/validate-metadata.cs)         | Every resource under `opinions/`, `research/`, and `templates/` has `targets`, `last-reviewed`, and `sources`, plus `last-used` outside `research/`, with ISO 8601 dates and a sorted `sources` list                                                                                                                                      |
| [`validate-sources.cs`](scripts/validate-sources.cs)           | Every source ID in `opinions/` and `templates/` exists in the AWESOME-HUMANS.md roster and is citable. The roster tables and notes sections are sorted by ID, no ID appears twice, and every notes section matches a row                                                                                                                  |
| [`validate-readme-index.cs`](scripts/validate-readme-index.cs) | This README lists every opinion and skill, and lists nothing that doesn't exist. Scope is sorted by title, and the layout tree by file name                                                                                                                                                                                               |
| [`export-diagrams.cs`](scripts/export-diagrams.cs)             | Draws the SVG diagrams in `assets/diagrams/`. It doesn't check anything, but CI reruns it and fails if the committed SVGs differ from its output                                                                                                                                                                                          |
| [`build-site.cs`](scripts/build-site.cs)                       | Stages the `/dotnet/` pages of awesome-humans.net in `<dir>/dotnet/`, given `-- --out <dir>`, to the [awesome-humans/website staging contract](https://github.com/awesome-humans/website#staging-contract). The `Site preview` workflow then builds the whole site with Zensical in strict mode, which fails on any broken link or anchor |

Run them from the repository root, as CI does:

```sh
dotnet run scripts/validate-metadata.cs
```

`npm run verify` runs everything CI checks locally, in one command: `npm run check`, the three validators, `export-diagrams.cs` and `build-site.cs`, which stages into `artifacts/site/`. It redraws the diagrams in place, so commit any SVG it changes. The Zensical build is left to CI, because it needs Python and the website repository.

Three helper files support the scripts:

- [`Opinions.cs`](scripts/Opinions.cs) lists the opinion files.
- [`Frontmatter.cs`](scripts/Frontmatter.cs) parses the YAML frontmatter in `opinions/` and `research/` files.
- [`CommentHeader.cs`](scripts/CommentHeader.cs) lists the `templates/` files that carry a header and parses their first-line comment.

The helpers have no top-level statements, so they don't run alone. Each compiles into the scripts that `#:include` it.

[`global.json`](global.json) sets the SDK minimum to the first feature band that supports `#:include`, and `rollForward: latestFeature` picks up newer bands. It's independent of [`templates/global.json`](templates/global.json), because each pin follows what its own consumers need.

One check runs in Python: the Agent Skills specification validator, which is published only to PyPI.

## Acknowledgement

Agents build and maintain this project, but it wouldn't exist without the people whose writing it draws on. Every opinion cites their work. Thank you to all of these awesome humans, and to everyone who supports them.

## License

See [LICENSE](LICENSE).
