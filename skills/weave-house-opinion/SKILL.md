---
name: weave-house-opinion
description: Weave a repository-owner opinion from HOUSE-OPINIONS.md (or given directly) into the opinion and template files, marked as House and kept distinguishable from source-derived opinions. Use when the owner adds an entry to the HOUSE-OPINIONS.md inbox, states a personal preference to record, or when a contributor PR proposes an experience-based opinion that the owner accepts.
license: See repository LICENSE
compatibility: Requires git
metadata:
  repo: awesome-humans/dotnet
  change-flow: branch-pr
---

# Weave a house opinion

House opinions are the owner's preferences. They carry full weight, and they're always marked, so nobody mistakes them for community best practice.

## Steps

1. **Take the entry** from the HOUSE-OPINIONS.md inbox. If the owner asks directly, add the entry to the inbox first, so the record starts there.
2. **Find where it belongs:** the matching `opinions/` file, and the matching `templates/` file if the opinion changes scaffolding, such as an `.editorconfig` rule or a build property. If it has no natural home, create an opinion file under the usual rules, including its `README.md` Scope bullet and Repository layout entry, which CI enforces.
3. **Write it in, marked:**
   - **In `opinions/`:** Opinion first, rationale after, starting with **House:** in bold. Add `house` to the file's `sources:`.
   - **In `templates/`:** Make the change, and add `house` to the `sources:` list in the comment header. Where the format allows, add an inline comment on the changed setting.
4. **Handle a conflict explicitly,** following [HOUSE-OPINIONS.md: How this works](../../HOUSE-OPINIONS.md#how-this-works). The house opinion wins, and the sourced position stays as the cited one-line note. Never delete the sourced view without saying so.
5. **Move the inbox entry to the Woven table** in HOUSE-OPINIONS.md, with the date and a link to where it went.
6. **Update `last-reviewed`** on every file you change. Work on a `house/<slug>` branch, as [AGENTS.md](../../AGENTS.md) describes, and open a pull request to the default branch with a title that shows the house origin, such as `docs: weave house opinion on <title>`.

## Rules

- **Never present a house opinion as sourced.** If a vetted source later publishes the same guidance, a harvest can add the citation beside the house marking, and the marking stays.
- **House opinions need no source, but they need the same quality.** Code must compile against the declared `targets`, and the opinion must give one recommendation.
- **Only the owner's opinions are written in directly.** Contributors propose opinions in a pull request, as the organisation's [pull request template](https://github.com/awesome-humans/.github/blob/main/PULL_REQUEST_TEMPLATE.md) describes. Once the owner accepts one, it becomes a house opinion, and the Woven table's Provenance column credits the contributor and the pull request.
- **Harvest and refresh skills never remove or weaken house content.** They update the sourced context around it.

## Edge cases

- **The owner changes their mind:** Write in the reversal the same way, and mark the old Woven row as superseded. The record is append-only.
- **A house opinion becomes obsolete,** for example because the framework now does it automatically: Mark it as superseded where it appears, name the release that made it obsolete, and cite the source.
