---
name: weave-house-opinion
description: Weave a house profile's opinion from HOUSE-OPINIONS.md (or given directly) into the opinion and template files, as a marked house block beside the sourced text it changes. Use when the owner adds an entry to the HOUSE-OPINIONS.md inbox, states a personal preference to record, or when a contributor PR proposes an experience-based opinion that the owner accepts.
license: See repository LICENSE
compatibility: Requires git
metadata:
  repo: awesome-humans/dotnet
  change-flow: branch-pr
---

# Weave a house opinion

House opinions are a house profile's preferences, such as the owner's `simon-mckenna`. They're always marked, and they sit beside the sourced text, never in it, so nobody mistakes them for community best practice. A reader or agent sees them only after choosing the profile.

## Steps

1. **Take the entry** from the HOUSE-OPINIONS.md inbox, with its profile. If the owner asks directly, add the entry to the inbox first, so the record starts there. The profile must be in HOUSE-OPINIONS.md's Profiles table and registered for this ecosystem in [awesome-humans/website: `houses.toml`](https://github.com/awesome-humans/website/blob/main/houses.toml). If it isn't, stop and ask the owner: a new profile needs their approval.
2. **Find where it belongs:** the matching `opinions/` file, and the matching `templates/` file if the opinion changes scaffolding, such as an `.editorconfig` rule or a build property. If it has no natural home, create an opinion file under the usual rules, including its `README.md` Scope bullet and Repository layout entry, which CI enforces.
3. **Write it in, marked, as [awesome-humans/website: House opinions](https://github.com/awesome-humans/website/blob/main/pages/conventions/house-opinions.md#how-this-works) defines:**
   - **In `opinions/`:** A block right after the sourced recommendation it changes, outside any list. It opens with `/// house | <profile>` and closes with `///`, each on its own line with a blank line inside. Inside, the opinion comes first, starting with **House:** in bold, and the rationale follows. Add `house:<profile>` to the file's `sources:`.
   - **In `templates/`:** Make the change between `house:<profile> begin` and `house:<profile> end` comment lines, in the file's comment syntax, and add `house:<profile>` to the `sources:` list in the comment header. Put a `**House:**` comment inside the region on the changed setting. The template must still build with the region removed.
   - **Never under a heading of its own.** A section that holds only a house block is empty for every other reader. Put the block in the nearest section with sourced text.
4. **Leave the sourced text whole.** When the house opinion conflicts with it, the block says so and gives the reason. A reader with the profile follows the block, and every other reader the sourced text, so never cut or reword the sourced text to fit.
5. **Move the inbox entry to the Woven table** in HOUSE-OPINIONS.md, with the date, the profile, and a link to where it went. Run `just validate-sources`, which checks that each `house:<profile>` ID and its blocks or regions appear together.
6. **Update `last-reviewed`** on every file you change. Work on a `house/<slug>` branch, as [AGENTS.md](../../AGENTS.md) describes, and open a pull request to the default branch with a title that shows the house origin, such as `docs: weave house opinion on <title>`. Fill in the [`house-opinion.md`](https://github.com/awesome-humans/.github/blob/main/PULL_REQUEST_TEMPLATE/house-opinion.md) template, as [AGENTS.md](../../AGENTS.md) describes.

## Rules

- **Never present a house opinion as sourced.** If a vetted source later publishes the same guidance, a harvest can add the citation inside the block, and the marking stays. Moving the guidance into the sourced text is a separate, sourced change.
- **House opinions need no source, but they need the same quality.** Code must compile against the declared `targets`, and the opinion must give one recommendation.
- **Only a profile owner's opinions are written in directly.** Contributors propose opinions in a pull request, as the organisation's [opinion pull request template](https://github.com/awesome-humans/.github/blob/main/PULL_REQUEST_TEMPLATE/opinion.md) describes. Once a profile's owner accepts one, it becomes a house opinion in their profile, and the Woven table's Provenance column credits the contributor and the pull request.
- **Harvest and refresh skills never remove or weaken a house block.** They update the sourced text around it.

## Edge cases

- **The owner changes their mind:** Write in the reversal the same way, and mark the old Woven row as superseded. The record is append-only.
- **A house opinion becomes obsolete,** for example because the framework now does it automatically: Mark it as superseded inside its block, name the release that made it obsolete, and cite the source.
- **Two profiles change the same sourced text:** Give each its own block, one after the other, sorted by profile.
