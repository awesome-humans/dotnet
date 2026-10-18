---
name: audit-freshness
description: Report every resource in this repository whose last-reviewed date has drifted past tolerance, whose targets lag the latest released .NET/C#/F# versions, whose template version pins (global.json SDK band, Directory.Packages.props package versions) lag the latest stable releases, or whose sources are no longer on the AWESOME-HUMANS.md roster. Use on a periodic cadence or when asked whether the repository is stale, current, or due for a refresh.
license: See repository LICENSE
compatibility: Requires git; internet access only needed to check latest release versions
metadata:
  repo: awesome-humans/dotnet
  change-flow: report-only
---

# Audit freshness

Report what's stale. This skill never edits content. Version drift goes to `refresh-dotnet-versions`, and content drift goes to `harvest-sources`.

## Tolerances

| Check                                                                             | Tolerance                                                                                     |
| --------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `last-reviewed` on any `opinions/` file                                           | 180 days                                                                                      |
| `last-reviewed` after a new major .NET release ships                              | 60 days from the release                                                                      |
| `targets:` against the latest released .NET, C#, and F#                           | Zero: any lag is a finding                                                                    |
| `last-reviewed` on any commented `templates/` header                              | 180 days                                                                                      |
| `sdk.version` in `global.json` and `templates/global.json`                        | Checked for shape, not band lag. For details, see [SDK pins](#sdk-pins)                       |
| `<PackageVersion>` pins in `templates/Directory.Packages.props` against nuget.org | One minor version, or any major version. Patch-only drift is informational                    |
| The `Versions verified against nuget.org … on <date>` comment in the same file    | 90 days, because the pins change faster than the prose                                        |
| `research/` topics                                                                | 90 days, then reported for `resolve-research`. For details, see [Research](#research)         |
| `sources:` IDs                                                                    | For the rules, see [Sources](#sources)                                                        |
| `**Review by YYYY-MM-DD.**` markings in `opinions/` and `templates/`              | Zero past the date. Report each for drop-or-re-date triage                                    |
| `last-used`                                                                       | Informational, never a finding. Report never-used resources as pruning candidates             |
| `templates/` still implement what `opinions/` require                             | Zero. For details, see [Templates](#templates)                                                |
| `templates/` build and test without errors                                        | Zero. For details, see [Templates](#templates)                                                |
| Citation and roster URLs resolve                                                  | Zero: a dead link is a content finding. Read the latest `Markdown` workflow run, not each URL |

### SDK pins

The SDK pin is a minimum, and `latestFeature` picks up newer bands without an edit (`refresh-dotnet-versions`, step 4a). Don't report how far the pin trails the latest band. Report these instead:

- A patch-level pin where a band minimum belongs, such as `10.0.412` instead of `10.0.400`.
- A `rollForward` that's missing or narrower than `latestFeature`.
- A minimum whose major and minor version don't match `targets:`.

### Research

Every topic on disk is unresolved, so all of them count. A topic still on disk after promotion is a finding, because promotion ends in deletion.

### Sources

- Every ID must resolve to a Citable row in `AWESOME-HUMANS.md`, or be a reserved `house:<profile>` ID, which is never a finding.
- In an opinion or template, a watch-list ID or a `**Discovery-only.**` ID is a finding. So is a file whose citable sources are all marked `**Corroborate.**`. `scripts/validate-sources.cs` already fails the build on all three.
- `research/` topics can name unvetted sources if the text flags them.

### Templates

- **Templates match the opinions.** If an opinion names a property, package, or setting, and its template leaves it out, that's a finding. People copy `templates/`, so a gap ships the advice without the setting.
- **Templates build and test.** Assemble the example projects into the layout `example.slnx` describes, and run `dotnet build` and `dotnet test`. Analyzer and package compatibility failures show up only this way.

## Steps

1. **Collect the metadata** from every file under `opinions/` and `research/`, and from the comment headers in `templates/`: `targets`, `last-reviewed`, and `sources`, plus `last-used` on `opinions/` and `templates/`. AGENTS.md explains why `research/` has no `last-used`.
2. **Find the latest released versions** of .NET, C#, and F#, from the same sources as `refresh-dotnet-versions` step 1. Find the latest stable version of every package in `templates/Directory.Packages.props`, with the lookup in `refresh-dotnet-versions` step 4b.
   - Skip SDK band lookups: the `global.json` check reads the pin's shape, not its distance from the newest release.
   - If you're offline, skip the checks you can't run, and say so in the report. Report an unchecked pin as "not verified", never as fresh.
3. **Evaluate each resource** against the tolerances, including any `**Review by YYYY-MM-DD.**` marking in its body ([AGENTS.md: Writing style for opinions](../../AGENTS.md#writing-style-for-opinions)). Missing or malformed metadata is a high-severity finding.
4. **Check the roster.** Flag sources in `AWESOME-HUMANS.md` with no _published writing_ in over a year for `vet-source` re-evaluation. Repository activity doesn't count, because opinions don't cite it.
5. **Write the report,** most stale first:
   - For each resource: the path, the findings, the days over tolerance, and the skill that fixes it.
   - Counts of fresh, stale, and malformed resources, and the oldest `last-reviewed` date.
6. **Recommend next actions,** usually `refresh-dotnet-versions`, `harvest-sources`, or `resolve-research` for an overdue topic. Don't run them unless asked.

## Edge cases

- **A repository with no opinions yet:** Report that there's nothing to check. Don't pass.
- **House content** (`house:<profile>` in `sources:`, and that profile's house blocks or regions; see HOUSE-OPINIONS.md): The reserved ID is never an unknown-source finding, and never suggest removing it. `scripts/validate-sources.cs` fails a build where the ID and its blocks don't appear together, or `**House:**` sits outside a block. Review a house block's age with its file, and never count it as stale sourced text.
- **Files that can't carry a comment header:** The repository-root `global.json`, plus exactly two `templates/` files: `templates/global.json` and `templates/example.slnf`. Missing metadata isn't a finding for them. Audit the pinned value against the latest release instead.
  - `.slnx` is XML and carries a header, so it isn't exempt.
  - `scripts/validate-metadata.cs` enforces this two-file list in CI, so any other headerless JSON under `templates/` fails there.
- **A package pin held back on purpose,** for a known-bad release or a tracked major migration: Report the drift as informational once a comment beside the pin gives the reason. Deliberate staleness must look different from silent staleness.
- **A resource that doesn't depend on a version,** such as naming conventions: It can declare `targets: [any]`. Version checks skip it, and review-age checks still apply.
- **Clock skew or future dates** in the metadata: Report them as malformed.
