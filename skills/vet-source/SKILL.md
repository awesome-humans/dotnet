---
name: vet-source
description: Evaluate a candidate source (blog, author, documentation site, newsletter) against the AWESOME-HUMANS.md admission criteria and admit, watch-list, decline, promote, or demote it. Use when proposing a new source, when a watch-list source may be ready for promotion, or when an admitted source has gone quiet or declined in quality.
license: See repository LICENSE
compatibility: Requires git and internet access
metadata:
  repo: dotnet-awesome-humans
  change-flow: branch-pr
---

# Vet a source

Apply the admission criteria in [AWESOME-HUMANS.md](../../AWESOME-HUMANS.md) to a candidate source, or re-evaluate an existing one. The roster is the trust boundary for every opinion in this repository, so when in doubt, decline.

## Orchestration

As [AGENTS.md: Orchestration and model economy](../../AGENTS.md#orchestration-and-model-economy) describes, delegate the evidence gathering in step 2, digging through archives and sampling posts, to worker agents. Only the editor weighs the evidence, decides, and edits the roster.

## Steps

1. **Identify the candidate:** its URL, author, and focus area. Check whether the roster already lists it. If it does, this is a re-evaluation for promotion or demotion, not an admission.
2. **Establish the track record.** Gather evidence for each criterion:
   - **Longevity:** Find the earliest verifiable publication, from archives, post history, or the Wayback Machine. Admission needs two years of sustained output. Under two years means the watch list, whatever the quality. Record the start year exactly: it goes in the roster's `Since` column, which shows the length of a record now that there's one citable tier.
   - **Depth:** Sample three to five representative posts. Do they offer original insight, such as internals, measurements, or worked reasoning, or do they paraphrase release notes?
   - **Accuracy:** Has the source issued corrections? Has it made claims that were later shown wrong and left them standing?
   - **Independence:** Does the content stand on its merit, or on marketing reach and chasing an algorithm? Vendor blogs and personality-driven channels need extra scrutiny here.
3. **Classify the source,** applying the two qualifying rules in [AWESOME-HUMANS.md: Admission criteria](../../AWESOME-HUMANS.md#admission-criteria) exactly as written there:

   | Evidence                                             | Outcome                                                                           |
   | ---------------------------------------------------- | --------------------------------------------------------------------------------- |
   | All four criteria met, over two years or more        | **Admit** to the **Citable** table                                                |
   | A live independence concern                          | **Admit,** and record a usage limit in the notes. See below                       |
   | Publishing stopped over a year ago                   | **Watch list,** unless the writing continues elsewhere. See below                 |
   | Strong on depth and accuracy, but short on longevity | **Watch list,** with the blocker recorded                                         |
   | An aggregator, such as a link roundup or newsletter  | **Admit,** marked **discovery-only**. It never appears in an opinion's `sources:` |
   | Thin on depth, but sound on the other three criteria | **Admit,** marked **`**Corroborate.**`**                                          |
   | Anything else                                        | **Decline,** with a one-line reason in the pull request only, not the roster      |

   The two qualifying rules work like this:

   - **An independence concern is a usage limit, never a lower rank.** A concern is live when vendor developer relations or product-team employment produces adoption-focused content with little critical distance. Admit the source, and write in its notes what a citation can rest on, usually how something works, not whether to adopt it. Working for a vendor doesn't need a limit by itself: depth with critical distance stands on its own. A concern that no limit could contain means a decline or the watch list, not an admission with a weaker limit.
   - **Dormancy blocks admission.** The exception is a person whose **writing** clearly continues elsewhere, such as official documentation, another publication, or a book. Then the track record follows the person, and the notes record the dormant channel.
     - Repository activity isn't evidence. Opinions don't cite commits, releases, or issue threads, so a busy GitHub profile beside a silent blog is still a dormant source.
     - Documentation written in a repository does count. The test is what was published, not where the commits landed.
   - The table says that a source is citable. A marking says what a citation can rest on.

4. **For a re-evaluation,** promote a watch-list source whose blocker has cleared. Demote or annotate an admitted source that has gone dormant, with no posts in over a year, or whose quality has dropped.
   - With one citable tier, a source that slips has two possible outcomes: a tighter usage limit in its notes, or the watch list.
   - A demoted source keeps its row, with a note. But **moving a source to the watch list revokes citation, including its back catalogue,** and `scripts/validate-sources.cs` rejects a watch-list ID anywhere in `sources:`.
   - So a demotion isn't finished until no opinion cites the source. In the same pull request, re-source each affected claim from a citable source, or drop it. If the back catalogue is worth keeping citable, annotate the row instead of demoting it.
5. **Make the change in a worktree,** on the branch `vet/<source-id>`, or a slug that names a multi-source pass, as [AGENTS.md](../../AGENTS.md) describes:
   1. Update the roster table. Assign a stable kebab-case `id`, and insert the row in alphabetical order by ID.
   2. Add the source's section under `## Source notes`, also in alphabetical order, with the evidence and any marking. Use the verdict-then-labelled-bullets shape that the section's introduction describes.
   3. Add the decision to the decision log. **Keep the log entry to the decision and the fact behind it,** in two sentences. The evidence for each criterion goes in the pull request (step 6), not the table.
6. **Open a pull request** with the evidence for each criterion, so a person can approve the admission. A person reviews it before the source can feed opinions.

## Edge cases

- **Institutional sources,** such as Microsoft or JetBrains: Longevity belongs to the publication, not to individual authors. Depth still needs scrutiny per author when you cite one.
- **An author who moved platforms,** for example from a personal blog to an employer's engineering blog to a newsletter: The track record follows the person, not the URL, so combine their writing across platforms. A move to video, talks, or a podcast isn't a platform change: it leaves the scope ([AWESOME-HUMANS.md: Admission criteria](../../AWESOME-HUMANS.md#admission-criteria)).
- **A candidate found through a single viral post:** Never admit a source on one post. At most, add it to the watch list.
- **Conflicts of interest,** where the candidate sells a product the opinions might recommend: The source can be admitted, but record the conflict in its section under `## Source notes`, so opinions that cite it flag it.
