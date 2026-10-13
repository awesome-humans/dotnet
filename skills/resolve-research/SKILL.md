---
name: resolve-research
description: Resolve a saved research topic in research/ by weaving its recommendation into the opinion and template files or discarding it, then deleting the file once fully resolved. Use when a research topic is ready to become the opinion, when audit-freshness reports one open past tolerance, or when deciding that it has served its purpose and owes nothing further.
license: See repository LICENSE
compatibility: Requires git; internet access only needed to re-verify a stale research topic's claims
metadata:
  repo: dotnet-awesome-humans
  change-flow: branch-pr
---

# Resolve a research topic

`research/` is a staging area, and this skill empties it. A research topic is evidence gathered at one point in time. Promotion makes it the repository's position. Discard is just as valid: the topic answered a question, and that was its job. Both end with the file deleted, except a partial promotion, which keeps the blocked part on disk. For details, see [Edge cases](#edge-cases).

## Orchestration

This is **editor-only work, so don't fan it out**. The research did the sweeping. What remains is deciding what an opinion says, which [AGENTS.md](../../AGENTS.md#orchestration-and-model-economy) reserves for the strongest available model. To recheck a claim, run `research-topic` again instead of a worker sweep.

## Steps

1. **Read the whole topic** at `research/<topic-slug>.md` on the default branch.
   - The research pull request must have merged, so the topic is reviewed and on record. If it's still open, merge it first.
   - Resolve on a new branch and pull request, never on the research branch.
   - The topic's closing section is the promotion plan. It names the target files, and usually the sections. Verify it before you follow it.
2. **Decide whether to promote or discard.** Ask only if the topic doesn't settle it. **Discard is a normal outcome.** A topic that found "the repository already answers this" confirmed the opinions are complete.
3. **Check every claim the recommendation rests on against the roster:**
   - **Citable roster IDs:** You can promote these claims. Add the IDs to the target's `sources:`.
   - **Watch-list, unvetted, or non-roster sources:** You can't promote these claims as written. Drop the claim, back it with a roster source, or hold it until `vet-source` admits the source. **Never reword an unvetted claim to hide where it came from.**
   - **References:** A named package's own documentation, release notes, issues, and pull requests, or the standard behind a named format. They promote as inline links supporting a roster-sourced claim. They never go in `sources:`, never support a claim alone, and don't belong on the roster ([AWESOME-HUMANS.md: Admission criteria](../../AWESOME-HUMANS.md#admission-criteria)).
4. **Choose where it goes before you edit:** an existing opinion file, or a new one. Keep one topic per file, under about 300 lines, and split a file before it grows past that. A new file under `opinions/` needs a Scope bullet and a Repository layout entry in `README.md`, in the same commit, or CI fails the pull request.
5. **Write it in.** Put the opinion first, the rationale after, and the sources last. Cut the survey and hedging. Give one recommendation, and an alternative at most one line on why it lost.
   - Never remove, weaken, or unmark house content.
   - If the research contradicts a house opinion, [HOUSE-OPINIONS.md: How this works](../../HOUSE-OPINIONS.md#how-this-works) applies: the house position stands, and the researched view becomes the cited one-line note.
6. **Carry the change into `templates/`** wherever the opinion changes scaffolding, such as a build property, an `.editorconfig` rule, or a package pin. `audit-freshness` reports an opinion whose template leaves out a setting it names, so land both together. Add the source IDs to the template's comment header.
7. **Fix what the research found along the way,** such as a stale example, a colliding path, or a rule enforced with no opinion behind it, in the same pull request.
8. **Update the metadata** on every file you change: an opinion's frontmatter, or a template's first-line header. Add the promoted source IDs to `sources:`. Update `last-reviewed` only after you verify the content, as [Rules](#rules) describes.
9. **Delete the research topic file,** and only that file. `research/.gitkeep` keeps the directory after the last topic goes. Commit on the branch `resolve/<topic-slug>`, and open a pull request titled `docs: promote <topic>` or `research: discard <topic>`.

## Rules

- **Only roster-sourced claims are promoted.** This gate lets research cite anyone while opinions cite only the roster.
- **Promotion deletes the topic in the same pull request.** A promoted topic left on disk is a second, unapproved copy of the opinion, and the deletion is what marks it promoted. `audit-freshness` reports a topic that remains. Git history keeps the record.
- **Promotion uses `docs:`, and discard uses `research:`.** A promotion edits opinions, like any other opinion change. A discard touches only the staging area. Neither uses the `research: <topic>` form that `research-topic` uses to save a topic.
- **`last-reviewed` means verified against the sources.** If the research is within the freshness tolerance, its verification holds, and today's date is accurate. If not, verify it again first, as [Edge cases](#edge-cases) describes. Dating stale research records a verification that never happened.
- **Rewrite the topic; don't paste it.** A topic surveys and weighs. An opinion has to be applied mechanically.

## Edge cases

- **The topic is stale:** It's past the `audit-freshness` tolerance, or a major .NET release shipped since. Verify its claims against their sources again, and check whether `refresh-dotnet-versions` should run first.
- **Only part of it can be promoted:** Promote the roster-sourced part, and cut the file down to the blocked part, with a note on what was promoted and what the rest is waiting for. Don't let one unvetted paragraph block a sound recommendation, and don't delete the blocked part without saying so.
- **Everything in it is unvetted:** It can't be promoted. Run `vet-source` on the strongest candidate source and return to it, or discard the topic.
- **It covers a preview feature:** The freshness policy allows only a marked "coming next" note. Promote that note if it's useful. Otherwise, leave the topic open until GA.
- **Two topics overlap,** in one opinion file or one idea: Promote them in one pull request, or in an order that doesn't rewrite the same metadata twice.
- **A `harvest-sources` change already covered it:** Discard the topic, and say so in the pull request. Duplicated guidance is worse than none.
