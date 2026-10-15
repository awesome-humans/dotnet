---
name: vet-source
description: Evaluate a candidate source (blog, author, documentation site, newsletter) against the AWESOME-HUMANS.md admission criteria and admit, watch-list, decline, promote, or demote it. Use when proposing a new source, when a watch-list source may be ready for promotion, or when an admitted source has gone quiet or declined in quality.
license: See repository LICENSE
compatibility: Requires git and internet access
metadata:
  repo: awesome-humans/dotnet
  change-flow: branch-pr
---

# Vet a source

Apply the admission criteria in [AWESOME-HUMANS.md](../../AWESOME-HUMANS.md) to a candidate source, or re-evaluate an existing one. Every opinion cites only the roster, so when in doubt, decline.

## Orchestration

As [AGENTS.md: Orchestration and model economy](../../AGENTS.md#orchestration-and-model-economy) describes, delegate the evidence gathering in step 2, such as searching archives and sampling posts, to worker agents. Only the editor weighs the evidence, decides, and edits the roster.

## Steps

1. **Identify the candidate:** its URL, author, and focus area. If the roster already lists it, this is a re-evaluation for promotion or demotion, not an admission.
2. **Establish the track record** against each criterion:
   - **Longevity:** Find the earliest verifiable publication, from archives, post history, or the Wayback Machine. Admission needs two years of sustained output, and less means the watch list, whatever the quality. Record the exact start year in the roster's `Since` column.
   - **Depth:** Sample three to five representative posts. Do they offer original insight, such as internals, measurements, or worked reasoning, or do they paraphrase release notes?
   - **Accuracy:** Has the source published corrections? Has it left claims standing that were later shown wrong?
   - **Independence:** Does the content stand on its merit, or on marketing reach? Vendor blogs and personality-driven channels need extra scrutiny.
3. **Classify the source,** applying the two qualifying rules in [AWESOME-HUMANS.md: Admission criteria](../../AWESOME-HUMANS.md#admission-criteria) exactly as written there:

   | Evidence                                             | Outcome                                                                 |
   | ---------------------------------------------------- | ----------------------------------------------------------------------- |
   | All four criteria met, over two years or more        | **Admit** to the **Citable** table                                      |
   | A live independence concern                          | **Admit,** with a usage limit in the notes                              |
   | Publishing stopped over a year ago                   | **Watch list,** unless the writing continues elsewhere                  |
   | Strong on depth and accuracy, but short on longevity | **Watch list,** with the blocker recorded                               |
   | An aggregator, such as a link roundup or newsletter  | **Admit,** marked `**Discovery-only.**`. It never appears in `sources:` |
   | Thin on depth, but sound on the other three criteria | **Admit,** marked `**Corroborate.**`                                    |
   | Anything else                                        | **Decline,** with a one-line reason in the pull request only            |

   The table decides whether a source is citable. A marking or a usage limit says what a citation can rest on.

4. **For a re-evaluation,** promote a watch-list source whose blocker has cleared. Demote or annotate an admitted source that has published nothing in over a year, or whose quality has dropped.
   - A source that slips gets either a tighter usage limit in its notes, or the watch list.
   - A demoted source keeps its row, with a note. **Moving a source to the watch list revokes citation, back catalogue included,** and `scripts/validate-sources.cs` rejects a watch-list ID in any `sources:`.
   - So a demotion is finished only when no opinion cites the source. In the same pull request, re-source each affected claim from a citable source, or drop it. To keep the back catalogue citable, annotate the row instead of demoting it.
5. **Edit in a worktree** on the branch `vet/<source-id>`, or a slug for a multi-source pass, as [AGENTS.md](../../AGENTS.md) describes:
   1. Update the roster table. Assign a stable kebab-case `id`, and insert the row in ID order.
   2. Add the source's section under `## Source notes`, also in ID order, with the evidence and any marking, in the verdict-then-labelled-bullets shape the section's introduction describes.
   3. Add the decision to the decision log, in two sentences: the decision and the fact behind it. Put the evidence for each criterion in the pull request.
6. **Open a pull request** with the evidence for each criterion. A person approves it before the source can feed opinions.

## Edge cases

- **Institutional sources,** such as Microsoft or JetBrains: Longevity belongs to the publication, not individual authors. When you cite one author, still check their depth.
- **An author who moved platforms,** for example from a personal blog to an employer's engineering blog to a newsletter: The track record follows the person, so combine their writing across platforms. A move to video, talks, or a podcast leaves the scope ([AWESOME-HUMANS.md: Admission criteria](../../AWESOME-HUMANS.md#admission-criteria)).
- **A candidate found through one viral post:** Never admit a source on one post. At most, watch-list it.
- **Conflicts of interest,** where the candidate sells a product the opinions might recommend: You can admit it, but record the conflict in its `## Source notes` section, so opinions that cite it flag it.
