---
name: research-topic
description: Research a .NET topic conversationally using this repository's opinions and vetted sources, such as "research union types", "what's the story on hybrid caching", "should we adopt Native AOT". Produces a cited research topic where every claim carries its source and that source's standing, labels preview features as the freshness policy requires, and closes with what the research reveals about the repository (a missing opinion, a stale aside, or a feature that reached GA). Use whenever the user wants to understand, evaluate, or decide on a .NET subject rather than change the repository.
license: See repository LICENSE
compatibility: Requires internet access for source reading; degrades to opinions-only offline
metadata:
  repo: awesome-humans/dotnet
  change-flow: branch-pr
---

# Research a topic

Answer "What should I know or do about X?" from this repository's opinions first, then its vetted roster. This skill writes two things, both on a research branch: the research topic, and the `last-used` dates of the opinions it read. It changes `opinions/` and `templates/` only through the skills it recommends at the end.

## Orchestration

As [AGENTS.md: Orchestration and model economy](../../AGENTS.md#orchestration-and-model-economy) describes, fan the per-source reading out to worker agents. Only the editor weighs conflicting sources, writes the research topic, and answers follow-up questions.

## Steps

1. **If the request could go more than one way, ask one question** to settle the intent:
   - _Learning_ it: an explanation and idioms.
   - _Deciding_ on it: trade-offs and maturity.
   - _Migrating_ to it: differences from the old way, and breaking changes.

   Research a clear request without asking.

2. **Start a worktree** on the branch `research/<topic-slug>`, such as `research/union-types`, following [AGENTS.md](../../AGENTS.md). Create it before the first write, so the topic and the `last-used` updates land together. Use the repository's existing kebab-case name for the topic (`union-types`, not `union-type`), so the branch, topic, and opinions share one search term.
3. **Start with the repository.** Read the matching `opinions/` files, which might already answer the question or carry a "Coming next" note. Skip house blocks unless the user named a house profile. Present that profile's blocks as "its owner's convention, not community consensus". Update `last-used` on every opinion you read.
4. **Sweep the roster in this order:**
   1. **Citable sources,** weighted by how well their Focus column matches. You can quote these.
   2. **Watch-list sources,** for discovery and cross-checking. Label them as watch-list.
   3. **Material from outside the roster.** Unvetted sources often cover new topics first, so use them.
      - Flag every such citation as **unvetted**, and keep unvetted claims visibly separate from roster-sourced ones.
      - List promising sources as `vet-source` candidates in the closing section.
      - A named package's own documentation, release notes, and issues are references, not unvetted sources, so they need no flag ([AWESOME-HUMANS.md: Admission criteria](../../AWESOME-HUMANS.md#admission-criteria)).

   Research accepts any source. **Promotion is the strict gate,** as [Persistence and lifecycle](#persistence-and-lifecycle) describes.

5. **Write the research topic:**
   - Put the recommendation or current state in the opening sentences, and the detail after.
   - Cite a source ID for every claim. Note a source's standing when it limits the claim, such as `**Corroborate.**` or a limit in its notes. Date anything time-sensitive.
   - **Label preview features as the freshness policy requires:** "in preview as of `<date>`, not yet an opinion". Keep them separate from GA guidance.
   - Where sources disagree, say so, and weigh them. Don't blur them into a vague middle.
6. **Answer follow-up questions** from what you've gathered. Sweep again, in the same order, only when a question goes beyond it.
7. **Close the loop.** End with what the research revealed about the repository, and recommend the matching skill. Never run it unprompted.

   | Finding                                                               | Recommendation                                                           |
   | --------------------------------------------------------------------- | ------------------------------------------------------------------------ |
   | The topic has no opinion file or coverage                             | `resolve-research`, which decides between an existing file and a new one |
   | A note or opinion is stale: a feature reached GA, or guidance changed | `refresh-dotnet-versions` or `harvest-sources`                           |
   | A strong source from outside the roster carried the research          | `vet-source`                                                             |
   | The repository already answers the question fully                     | Say so. `resolve-research` can discard the topic                         |
   | A saved research topic on this subject exists in `research/`          | Build on it, refresh its dates, and report its promote-or-discard status |

## Persistence and lifecycle

**Save the topic** to `research/<topic-slug>.md`, with the metadata a topic carries ([AGENTS.md: Metadata](../../AGENTS.md#metadata)), unless the user says the question was a one-off.

Commit the topic and its `last-used` updates on the `research/<topic-slug>` branch, and open a pull request to the default branch, filling in the [`research.md`](https://github.com/awesome-humans/.github/blob/main/PULL_REQUEST_TEMPLATE/research.md) template, as [AGENTS.md](../../AGENTS.md) describes. Its **For `resolve-research`** section proposes the outcome, but doesn't decide it. Once it merges, `resolve-research` picks it up on its own branch and pull request. Never put the promote-or-discard decision in the open research pull request. If the user doesn't want the topic saved, delete the branch and answer in the conversation.

**Title the commit and pull request `research: <topic>`,** such as `research: union types`, not `docs: research <topic>`. The separate type keeps staged research apart from settled opinion changes in `git log`, and lists every topic researched. `resolve-research` sets the type of the resolution commits.

Every saved topic is eventually promoted or discarded by `resolve-research`, and both end with the file deleted, so a topic on disk is unresolved. `audit-freshness` reports a topic past its tolerance.

## Edge cases

- **Preview-only topics,** such as union types: Research them fully. Lead with the maturity status, and mark guidance for "when it reaches GA" as hypothetical.
- **A topic outside .NET:** Say that the roster has no authority there, and stop. Don't present general knowledge as vetted research.
- **Offline:** Answer from `opinions/` alone, and say you skipped the roster sweep. The answer is sourced, but only as current as the opinions' `last-reviewed` dates.
- **House and community positions conflict, for the profile the user named:** Present both, house first, with the house reasoning. The reader might not follow the profile's conventions.
