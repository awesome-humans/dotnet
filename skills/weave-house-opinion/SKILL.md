---
name: weave-house-opinion
description: Weave a repository-owner opinion from HOUSE-OPINIONS.md (or given directly) into the opinion and template files, marked as House and kept distinguishable from source-derived opinions. Use when the owner adds an entry to the HOUSE-OPINIONS.md inbox, states a personal preference to record, or when a contributor PR proposes an experience-based opinion that the owner accepts.
license: See repository LICENSE
compatibility: Requires git
metadata:
  repo: dotnet-awesome-humans
  change-flow: branch-pr
---

# Weave a house opinion

House opinions are the owner's preferences. They carry full weight, but they never pass as community best practice. This skill adds them to the living reference, and keeps the two visibly separate.

## Steps

1. **Take the entry** from the HOUSE-OPINIONS.md inbox. If the owner asks directly instead, add the entry to the inbox first, so the audit trail starts there.
2. **Find where it belongs:** the matching `opinions/` file, and, if the opinion changes scaffolding such as an `.editorconfig` rule or a build property, the matching `templates/` file. If a house opinion has no natural home yet, create a new opinion file under the usual rules. That includes adding it to `README.md`, as a linked Scope bullet and a Repository layout entry, which CI enforces.
3. **Weave it in, marked:**
   - **In `opinions/`:** Write it in the house style, with the opinion first and the rationale after, and start it with **House:** in bold. Add `house` to the file's `sources:` metadata.
   - **In `templates/`:** Make the change, and add `house` to the `sources:` list in the comment header that `audit-freshness` reads. Where the format allows, add an inline comment on the changed setting.
4. **Handle conflicts explicitly,** following the canonical precedence rule in [HOUSE-OPINIONS.md: How this works](../../HOUSE-OPINIONS.md#how-this-works). The house opinion wins, and the sourced position stays as the cited one-line note. Never silently delete the sourced view.
5. **Move the inbox entry to the Woven table** in HOUSE-OPINIONS.md, with the date and a link to where it went.
6. **Update `last-reviewed`** on every file you change. Work on a `house/<slug>` branch, as described in the branch name rules in [AGENTS.md](../../AGENTS.md), and open a pull request to the default branch. Give it a title that makes the house origin obvious, such as `docs: weave house opinion on <title>`.

## Rules

- **Never launder a house opinion into a sourced one.** If a vetted source later publishes the same guidance, a harvest can add the citation next to the house marking, and the marking stays.
- **House opinions don't need source tracing, but they do need quality.** Code examples must still compile against the declared `targets`, and the opinion must still be one recommendation, not a menu.
- **Only the repository owner's opinions are woven in directly.** Contributors propose opinions in a pull request, as described in `.github/PULL_REQUEST_TEMPLATE.md`. Their opinions are woven in only after the owner accepts them. They then become house opinions, credited in the pull request history.
- **Harvest and refresh skills must not remove or weaken house content.** They update the sourced context around it.

## Edge cases

- **The owner changes their mind:** Weave in the reversal the same way, and update the old Woven row's note to show that it's superseded. Treat the audit trail as append-only, and don't rewrite history.
- **A house opinion becomes obsolete,** for example because the framework now does it automatically: Mark it as superseded where it appears, name the release that made it obsolete, and cite the source.
