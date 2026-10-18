---
name: harvest-sources
description: Sweep the vetted sources in AWESOME-HUMANS.md for new posts and updates, then fold notable guidance into the opinion files. Use on a periodic cadence, or when asked to check what the awesome humans have published lately, or after a major community post (such as a new "Performance Improvements in .NET" article) ships.
license: See repository LICENSE
compatibility: Requires git and internet access
metadata:
  repo: awesome-humans/dotnet
  change-flow: branch-pr
---

# Harvest sources

Collect what the vetted sources have published since the last sweep, and update the opinions from it.

## Orchestration

As [AGENTS.md: Orchestration and model economy](../../AGENTS.md#orchestration-and-model-economy) describes, fan the per-source sweeps in step 3 out to worker agents. Only the editor triages their findings and edits `opinions/` and `templates/`.

## Steps

1. **Load the roster** from [AWESOME-HUMANS.md](../../AWESOME-HUMANS.md). Only sources in the **Citable** table feed opinions. Use watch-list sources for discovery and cross-checking only: what you find there needs a citable source, or a `vet-source` admission, before it can change an opinion.
2. **Set the sweep window,** from the most recent harvest entry in the `AWESOME-HUMANS.md` decision log to today. If there's no harvest entry, start from the newest `last-reviewed` date in `opinions/`.
3. **Sweep each source** for posts in the window. A source marked `**Discovery-only.**` only leads to primary posts: follow its links, and never cite it in `sources:`, because CI rejects it.
4. **Triage each notable post:**
   - **Changes an existing opinion:** It replaces or refines guidance in `opinions/`. Queue an edit.
   - **Suggests a new opinion:** It's a recurring theme with no home yet. Create a stub opinion with metadata, the source link, and a `TODO`. In the same commit, add it to `README.md` as a Scope bullet and a Repository layout entry, or CI fails the pull request.
   - **Noise:** Release chatter, marketing, or a one-off tip that doesn't generalise. Skip it.
5. **Edit in a worktree** on the branch `harvest/<YYYY-MM-DD>`, dated the day the sweep window ends, as [AGENTS.md](../../AGENTS.md) describes:
   - Keep one recommendation per opinion. If a post contradicts the current opinion, choose the position with the stronger sources or evidence, and note the change in one line.
   - **Never remove, weaken, or unmark a house block or region** ([HOUSE-OPINIONS.md: How this works](../../HOUSE-OPINIONS.md#how-this-works)). Update the sourced text beside it as usual. When a source now agrees with a house opinion, add the citation inside its block.
   - Add the post's source ID to the opinion's `sources:`, and update `last-reviewed:`.
6. **Record the sweep** in the `AWESOME-HUMANS.md` decision log: the date, the window, and what changed, as briefly as the existing entries. Per-source findings go in the pull request, not the table.
7. **Open a pull request** to the default branch, filling in the [`opinion.md`](https://github.com/awesome-humans/.github/blob/main/PULL_REQUEST_TEMPLATE/opinion.md) template, as [AGENTS.md](../../AGENTS.md) describes. Its **Skill run** section takes the findings per source, and the proposed opinion takes the changes per opinion. A person reviews it before it changes any opinion.
   - If an open `Harvest due: <Month> <Year>` issue asked for this sweep, put `Closes #<number>` in the pull request body.
   - The `Harvest reminder` workflow opens that issue on the 1st of each month, and skips the month while an issue with that title is open. An issue left open blocks the next reminder.
   - If another pull request replaces this one, copy the `Closes` line to it. The reference doesn't carry over.

## Edge cases

- **Vetted sources disagree:** Present both positions in the pull request, and choose one for the opinion, with the reason. Never leave a menu in the opinion.
- **A source has gone quiet, or its quality has dropped:** Note it in the pull request, and propose demotion through `vet-source`. Don't edit the roster directly.
- **Preview content:** It can go in a "coming next" note, never in the opinion itself.
- **Nothing notable found:** Record the sweep in the decision log anyway, on the default branch or in a small pull request, so the next window starts from the right date. Close the reminder issue either way, or next month's reminder doesn't open.
