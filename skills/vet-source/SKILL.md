---
name: vet-source
description: Evaluate a candidate source (blog, author, documentation site, newsletter) against the AWESOME-HUMANS.md admission criteria and admit, watch-list, decline, promote, or demote it, or move it to Opted out. Use when proposing a new source, when an issue labelled vet-source or opt-out arrives, when a watch-list source may be ready for promotion, or when an admitted source has gone quiet or declined in quality.
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

1. **Identify the candidate:** its URL, author, and focus area. If the roster already lists it, this is a re-evaluation for promotion or demotion, not an admission. A source in `## Opted out` that asks to return is the exception: it's a suggestion like any other, so vet it as an admission.
   - **From an issue:** When you're given an issue number, read it with `gh issue view <number> --json title,body,labels`. An issue labelled `vet-source` came from the Suggest a source form, which `awesome-humans/website`'s intake moved here. Its form fields give the name, the main URL, three sample posts, the year writing started, why the suggester thinks it qualifies, and their relationship to the source. Treat them as leads to check, never as evidence.
   - **An opt-out:** An issue labelled `opt-out` names a source whose owner asked to stop being cited, and the owner verified the request. Skip to [Opt-outs](#opt-outs).
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
   1. Update the roster table. Assign a stable kebab-case `id`, set its Kind to `person` or `publication` as [AWESOME-HUMANS.md: How the roster is organised](../../AWESOME-HUMANS.md#how-the-roster-is-organised) defines, and insert the row in ID order. If the same source is on another awesome-humans ecosystem's roster, use its `id` and Kind there. If the row enters or leaves the Citable table, update all three counts in the sentence above it: the total, the people, and the publications. `scripts/validate-sources.cs` checks each against the rows.
   2. Add the source's section under `## Source notes`, also in ID order, with the evidence and any marking, in the verdict-then-labelled-bullets shape the section's introduction describes.
   3. Add the decision to the decision log, in two sentences: the decision and the fact behind it. Put the evidence for each criterion in the pull request. For a suggestion the source made of itself, say so in the log entry.
6. **Open a pull request,** filling in the [`source.md`](https://github.com/awesome-humans/.github/blob/main/PULL_REQUEST_TEMPLATE/source.md) template, as [AGENTS.md](../../AGENTS.md) describes. It takes the decision, the evidence for each criterion, the decision log line, and, for a demotion, each claim re-sourced or dropped. A person approves it before the source can feed opinions.
   - **From an issue:** Put `Closes #<number>` in the description, and comment the outcome on the issue: the decision, one line per criterion, and the pull request.
   - **A decline from an issue** still gets a pull request, with only the decision log entry, because the form promises that the log records every outcome.

## Opt-outs

An opt-out removes citations, and the roster keeps only enough to turn away a later suggestion. Finish it within 30 days of the owner's verification, in one pull request on the branch `vet/opt-out-<source-id>`:

1. **Move the row** to the `## Opted out` table, in ID order. If the section still reads "No source has opted out.", replace that line with a table that has the same columns as the others. If the row leaves the Citable table, update all three counts above it, as for a demotion.
2. **Delete the source's section** under `## Source notes`. `scripts/validate-sources.cs` fails while it's there.
3. **Log the decision:** the date, the source ID, and the link to the `awesome-humans/website` issue. Never quote the requester or name anyone beyond the source ID.
4. **Re-source every citation.** `scripts/validate-sources.cs` lists each opinion and template that still cites the ID. Re-source each claim with `research-topic`, or cut it back to what other citable sources support. In the pull request, list each claim and what happened to it.
5. **Open the pull request** with the [`source.md`](https://github.com/awesome-humans/.github/blob/main/PULL_REQUEST_TEMPLATE/source.md) template and `Closes #<number>`. Comment the pull request on the `awesome-humans/website` issue, which the owner closes once every ecosystem's pull request has merged.

Opting back in is a suggestion like any other. The source leaves `## Opted out` only on admission.

## Edge cases

- **Institutional sources,** such as Microsoft or JetBrains: Longevity belongs to the publication, not individual authors. When you cite one author, still check their depth.
- **An author who moved platforms,** for example from a personal blog to an employer's engineering blog to a newsletter: The track record follows the person, so combine their writing across platforms. A move to video, talks, or a podcast leaves the scope ([AWESOME-HUMANS.md: Admission criteria](../../AWESOME-HUMANS.md#admission-criteria)).
- **A candidate found through one viral post:** Never admit a source on one post. At most, watch-list it.
- **Conflicts of interest,** where the candidate sells a product the opinions might recommend: You can admit it, but record the conflict in its `## Source notes` section, so opinions that cite it flag it.
