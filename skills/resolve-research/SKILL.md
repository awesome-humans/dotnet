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

`research/` is a staging area, and this skill empties it. A research topic from `research-topic` is evidence gathered at one point in time. Promotion makes it the repository's position. Discard is an equally valid decision: the topic was only ever an answer to a question. Both end with the file deleted. The one exception is a partial promotion, which keeps the blocked part on disk. For details, see [Edge cases](#edge-cases).

## Orchestration

This is **editor-only work, so don't fan it out**. The broad sweeping happened during the research. What remains is the judgement about what an opinion says, which [AGENTS.md](../../AGENTS.md#orchestration-and-model-economy) reserves for the strongest available model. If a claim needs re-checking, run `research-topic` again instead of a worker sweep.

## Steps

1. **Read the whole topic** at `research/<topic-slug>.md` on the default branch.
   - The research pull request has already merged, so the topic is on record and was reviewed. If it's still open, merge it first, instead of resolving on top of it.
   - Resolution gets its own branch and pull request. It never continues the research branch.
   - The topic's closing section, on what the research reveals about the repository, is the promotion plan. It names the target files, and usually the sections. Treat it as a proposal to verify, not as instructions to follow without reading.
2. **Decide whether to promote or discard.** Ask only if the topic doesn't already settle it. **Discard is a normal outcome,** not a failure. A topic that found "the repository already answers this" has done its job, by confirming that the system works.
3. **Check every claim the recommendation rests on against the roster.** Go through the citations:
   - **Citable roster IDs:** You can promote these claims. Add their IDs to the target's `sources:` metadata.
   - **Watch-list, unvetted, or non-roster sources:** You can't promote these claims as written. Drop the claim from the promoted text, back it with a roster source, or hold it until `vet-source` admits the source. **Never launder an unvetted claim** by rewording it until its origin no longer shows. That breaks the trust boundary.
   - **References:** A named package's own documentation, release notes, issues, and pull requests, or the standard behind a named format. They promote as inline links that support a roster-sourced claim. They never go in `sources:`, and they're never the only support for a claim ([AWESOME-HUMANS.md: Admission criteria](../../AWESOME-HUMANS.md#admission-criteria)). They aren't blog-style sources, and don't belong on the roster.
4. **Choose the shape before you edit.** Either add to existing opinion files, or create a new one. The convention is one topic per file, under about 300 lines, so split a file instead of letting it grow. A new file under `opinions/` isn't finished until `README.md` lists it in both places, a linked Scope bullet and a Repository layout entry, in the same commit. Otherwise, CI fails the pull request.
5. **Weave it in.** Put the opinion first, the rationale after, and the sources last. Drop the surveying and hedging language. Give one recommendation, not a menu, and give an alternative at most one line on why it lost.
   - Never remove, weaken, or unmark house content.
   - If the research contradicts a house opinion, [HOUSE-OPINIONS.md: How this works](../../HOUSE-OPINIONS.md#how-this-works) applies. The house position stands, and the researched view stays as the cited one-line note.
6. **Carry the change into `templates/`** wherever the opinion changes scaffolding, such as a build property, an `.editorconfig` rule, or a package pin. If an opinion names a setting that its nominated template leaves out, `audit-freshness` reports it, so land both changes together. Add the source IDs to the template's comment header too.
7. **Fix what the research noticed along the way.** A sweep often finds collateral problems: a stale example, a path that now collides, or a rule enforced with no opinion behind it. Fix them in the same pull request.
8. **Update the metadata** on every file you change: the YAML frontmatter on an opinion, or the first-line comment header on a template. Add the promoted source IDs to `sources:`. Update `last-reviewed` only after you verify the content, as described in [Rules](#rules).
9. **Delete the research topic file,** and only that file. `research/.gitkeep` keeps the directory tracked after the last topic goes. Commit on the branch `resolve/<topic-slug>`, and open a pull request to the default branch. Title it `docs: promote <topic>` for a promotion, or `research: discard <topic>` for a discard.

## Rules

- **Only roster-sourced claims are promoted.** This one gate lets `research/` be permissive while `opinions/` stays trustworthy: research can cite anyone, and the opinions can't.
- **Promotion ends in deletion, in the same pull request as the weave.** A promoted topic left on disk is a second, unapproved copy of the opinion, and deletion is the only thing that marks it as promoted. `audit-freshness` reports a topic that remains. Git history is the record.
- **Promotion uses `docs:`, and discard uses `research:`.** A promotion edits opinions, so it takes the same type as other opinion edits. A discard only touches the staging area, so it keeps the research lifecycle's own type. Neither uses the `research: <topic>` form that `research-topic` uses when it saves a topic.
- **`last-reviewed` means verified against the sources, not copied over.** If the research is within the freshness tolerance, its verification still holds, and today's date is accurate. If it isn't, verify it again first, as described in [Edge cases](#edge-cases). Updating the date based on stale research records exactly the false verification that the metadata exists to prevent.
- **A research topic is evidence, not text to transplant.** It's written to survey and weigh. An opinion is written to be applied mechanically. Rewrite it; don't paste it.

## Edge cases

- **The topic is stale:** It's past the `audit-freshness` tolerance, or a major .NET release has shipped since. Verify the claims it rests on against their sources again before you promote it, and check whether `refresh-dotnet-versions` should run first. Research from before a release boundary is an input again, not a finished decision.
- **Only part of it can be promoted:** Promote the roster-sourced part, and reduce the file to the blocked part. Leave the file in place, with a note on what was promoted and what the rest is waiting for. Don't let one unvetted paragraph block a sound recommendation, and don't delete the blocked part silently.
- **Everything in it is unvetted:** It can't be promoted. Run `vet-source` on the strongest candidate source and come back to it, or discard the topic.
- **It covers a preview feature:** It can't become an opinion, because the freshness policy allows only a clearly marked "coming next" note. Promote that note if it's useful. Otherwise, leave the topic open until GA.
- **Two topics overlap,** in the same opinion file or the same underlying idea: Promote them in one pull request, or in a deliberate order, whichever avoids rewriting the same metadata twice.
- **A `harvest-sources` change already covered the recommendation:** Discard the topic, and say so in the pull request. Duplicated guidance is worse than none.
