---
name: harvest-sources
description: Sweep the vetted sources in AWESOME-HUMANS.md for new posts and updates, then fold notable guidance into the opinion files. Use on a periodic cadence, or when asked to check what the awesome humans have published lately, or after a major community post (such as a new "Performance Improvements in .NET" article) ships.
license: See repository LICENSE
compatibility: Requires git and internet access
metadata:
  repo: dotnet-awesome-humans
  change-flow: branch-pr
---

# Harvest sources

Collect what the vetted sources have published since the last sweep, and use it to refine the repository's opinions. The humans provide the insight, and this skill keeps the repository current with it.

## Orchestration

As [AGENTS.md: Orchestration and model economy](../../AGENTS.md#orchestration-and-model-economy) describes, fan the per-source sweeps in step 3 out to worker agents. Only the editor triages their findings and edits `opinions/` and `templates/`.

## Steps

1. **Load the roster** from [AWESOME-HUMANS.md](../../AWESOME-HUMANS.md). Only sources in the **Citable** table feed opinions. Use watch-list sources only for discovery and cross-checking: anything you find there needs a citable source to back it, or a `vet-source` admission, before it can shape an opinion.
2. **Set the sweep window.** Start from the most recent harvest entry in the decision log of `AWESOME-HUMANS.md`. If there isn't one, start from the newest `last-reviewed` date in `opinions/`. Sweep from that date to today.
3. **Sweep each source** for posts in the window. A source whose notes carry the `**Discovery-only.**` marking only leads to primary posts. Follow its links, and never cite the discovery source itself in `sources:`, because CI rejects it.
4. **Triage each notable post** into one of these groups:
   - **Changes an existing opinion:** The guidance replaces or refines something in `opinions/`. Queue an edit.
   - **Suggests a new opinion:** A recurring theme with no home yet. Create a stub opinion with metadata, the source link, and a `TODO`. In the same commit, add it to `README.md` as a linked Scope bullet and a Repository layout entry, or CI fails the pull request.
   - **Noise:** Release chatter, product marketing, or one-off tips that don't generalise. Skip it.
5. **Make the edits in a worktree,** on the branch `harvest/<YYYY-MM-DD>`, dated the day the sweep window ends, as [AGENTS.md](../../AGENTS.md) describes:
   - Keep opinions opinionated, with one recommendation. If a new post contradicts the current opinion, choose the position with the stronger sources or evidence, and note the change in one line.
   - **Never remove, weaken, or unmark house content** ([HOUSE-OPINIONS.md: How this works](../../HOUSE-OPINIONS.md#how-this-works)). When a source now agrees with a house opinion, add the citation beside the marking.
   - Add the post's source ID to the opinion's `sources:` metadata, and update `last-reviewed:`.
6. **Record the sweep** in the decision log of `AWESOME-HUMANS.md`: the date, the window covered, and what changed, at the same length as the existing entries. Per-source findings go in the pull request (step 7), not in the table cell.
7. **Open a pull request** to the default branch that summarises the findings per source and the changes per opinion. A person reviews it before it becomes the opinion.
   - If an open `Harvest due: <Month> <Year>` issue asked for this sweep, put `Closes #<number>` in the pull request body, so merging closes it.
   - The `Harvest reminder` workflow opens that issue on the 1st of each month, and skips the month if an open issue already has the title. So an issue left open blocks the next reminder.
   - If another pull request replaces this one, copy the keyword to the replacement, because the reference doesn't carry over by itself.

## Edge cases

- **Vetted sources disagree:** Present both positions in the pull request description, and choose one for the opinion, with the reason. Never leave a menu in the opinion file.
- **A source has gone quiet, or its quality has dropped:** Note it in the pull request, and propose demotion through `vet-source`, instead of editing the roster directly.
- **Preview-version content:** It can appear in a "coming next" note, but never becomes the opinion. For details, see the repository freshness policy.
- **Nothing notable found:** Still record the sweep in the decision log, on the default branch or in a small pull request, so window tracking stays accurate. Close the reminder issue either way. A sweep that found nothing is still a sweep, and an open issue costs the next month its reminder.
