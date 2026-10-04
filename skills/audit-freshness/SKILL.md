---
name: audit-freshness
description: Report every resource in this repository whose last-reviewed date has drifted past tolerance, whose targets lag the latest released .NET/C#/F# versions, whose template version pins (global.json SDK band, Directory.Packages.props package versions) lag the latest stable releases, or whose sources are no longer on the AWESOME-HUMANS.md roster. Use on a periodic cadence or when asked whether the repository is stale, current, or due for a refresh.
license: See repository LICENSE
compatibility: Requires git; internet access only needed to check latest release versions
metadata:
  repo: dotnet-awesome-humans
  change-flow: report-only
---

# Audit freshness

Make staleness visible. This skill only **reports**, and never edits content. Follow-up work goes to `refresh-dotnet-versions` for version drift, or `harvest-sources` for content drift.

## Tolerances

| Check                                                                             | Tolerance                                                                                                            |
| --------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| `last-reviewed` on any `opinions/` file                                           | 180 days                                                                                                             |
| `last-reviewed` after a new major .NET release ships                              | 60 days from the release                                                                                             |
| `targets:` against the latest released .NET, C#, and F#                           | Zero: any lag is a finding                                                                                           |
| `last-reviewed` on any commented `templates/` header                              | 180 days, the same as `opinions/`                                                                                    |
| `sdk.version` in `global.json` and `templates/global.json`                        | Not a band-lag check. For the findings, see [SDK pins](#sdk-pins)                                                    |
| `<PackageVersion>` pins in `templates/Directory.Packages.props` against nuget.org | One minor version, or any major version. Patch-only drift is informational                                           |
| The `Versions verified against nuget.org … on <date>` comment in the same file    | 90 days, because the pins change faster than the prose                                                               |
| `research/` topics                                                                | 90 days, then reported for promote-or-discard triage with `resolve-research`. For details, see [Research](#research) |
| `sources:` IDs                                                                    | For the rules, see [Sources](#sources)                                                                               |
| `**Review by YYYY-MM-DD.**` markings in `opinions/` and `templates/`              | Zero past the date. Report each one for drop-or-re-date triage                                                       |
| `last-used`                                                                       | Informational only, never a finding. Report never-used resources as candidates for pruning                           |
| `templates/` still implement what `opinions/` require                             | Zero. For details, see [Templates](#templates)                                                                       |
| `templates/` build and test without errors                                        | Zero. For details, see [Templates](#templates)                                                                       |
| Citation and roster URLs resolve                                                  | Zero: a dead link is a content finding. Read the most recent `Markdown` workflow run instead of checking each URL    |

### SDK pins

The SDK pin is a minimum, and `latestFeature` takes a newer band without an edit (`refresh-dotnet-versions`, step 4a). So don't report how far the pin is behind the latest band. Report these instead:

- A patch-level pin where a band minimum belongs, such as `10.0.412` instead of `10.0.400`.
- A `rollForward` that's missing, or narrower than `latestFeature`.
- A minimum whose major and minor version no longer match `targets:`.

### Research

Every topic on disk is unresolved, so all of them count. A topic still on disk after promotion is itself a finding, because promotion ends in deletion.

### Sources

- Every ID must resolve to a Citable row in `AWESOME-HUMANS.md`, or be the reserved `house` ID. `house` is never a finding.
- In an opinion or template, a watch-list ID or a `**Discovery-only.**` ID is a finding. So is a file whose citable sources are all marked `**Corroborate.**`. `scripts/validate-sources.cs` already fails the build on all three.
- In `research/` topics only, unvetted source names are allowed when the text flags them.

The `**Review by YYYY-MM-DD.**` marking heads transition guidance. For the rule, see [AGENTS.md: Writing style for opinions](../../AGENTS.md#writing-style-for-opinions).

### Templates

- **Templates match the opinions.** If an opinion names a property, package, or setting, and its nominated template leaves it out, that's a finding. People copy `templates/`, so a mismatch ships the advice without the substance.
- **Templates build and test.** Assemble the example projects into the layout that `example.slnx` describes, and run `dotnet build` and `dotnet test`. Analyzer and package compatibility failures only show up this way, never from reading the files.

## Steps

1. **Collect the metadata** from every file under `opinions/` and `research/`, and the comment headers of `templates/` files: `targets`, `last-reviewed`, and `sources`, plus `last-used` on `opinions/` and `templates/` only. `research/` topics have neither `last-used` nor a status field. AGENTS.md explains why.
2. **Find the latest released versions** of .NET, C#, and F#, using the same sources as `refresh-dotnet-versions` step 1. Also find the latest stable version of every package pinned in `templates/Directory.Packages.props`, using the same lookup as `refresh-dotnet-versions` step 4b.
   - You don't need to look up SDK bands: the `global.json` check reads the shape of the pin, not its distance from the newest release.
   - If you're offline, skip the checks you can't run, and say so in the report. Report an unchecked pin as "not verified", never as fresh.
3. **Evaluate each resource** against the tolerances, including any `**Review by YYYY-MM-DD.**` marking in its body. Missing or malformed metadata is itself a finding, with high severity, because the living reference depends on it.
4. **Check the roster.** Flag sources in `AWESOME-HUMANS.md` with no _published writing_ in over a year as candidates for `vet-source` re-evaluation. Repository activity doesn't count, because it isn't what an opinion cites.
5. **Write the report,** with the most stale resources first:
   - For each resource: the path, the findings, the days over tolerance, and which skill fixes it.
   - Summary counts of fresh, stale, and malformed resources, and the oldest `last-reviewed` date in the repository.
6. **Recommend next actions,** usually to run `refresh-dotnet-versions`, `harvest-sources`, or, for an open research topic past tolerance, `resolve-research`. Don't run them unless asked.

## Edge cases

- **A new repository with no opinions yet:** Report that there's nothing to check, instead of passing.
- **House content** (`house` in `sources:`, and `**House:**` markings; see HOUSE-OPINIONS.md): The reserved ID is never an unknown-source finding, and never suggest removing it. A `house` ID without `**House:**` content in the file, or the reverse, IS a finding: the marking and the ID always appear together.
- **Files that can't carry a comment header:** These are the repository-root `global.json`, and exactly two `templates/` files: `templates/global.json` and `templates/example.slnf`. They have no `last-reviewed`, so missing metadata isn't a finding for them. Audit the pinned value against the latest release instead.
  - `.slnx` is XML and does carry a header, so it isn't exempt.
  - `scripts/validate-metadata.cs` enforces exactly this two-file list in CI, so any other headerless JSON added under `templates/` fails there.
- **A package pin held back on purpose,** for a known-bad release or a major version whose migration is tracked elsewhere: Still report the drift, but as informational once a comment beside the pin records the reason. Silent staleness and deliberate staleness must look different in the report.
- **A resource that doesn't depend on a version,** such as naming conventions: It can declare `targets: [any]`. Version drift checks skip it, but review-age checks still apply.
- **Clock skew or future dates** in the metadata: Report them as malformed.
