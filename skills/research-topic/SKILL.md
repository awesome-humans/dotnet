---
name: research-topic
description: Research a .NET topic conversationally using this repository's opinions and vetted sources, such as "research union types", "what's the story on hybrid caching", "should we adopt Native AOT". Produces a cited research topic where every claim carries its source and tier, respects the freshness policy's preview labeling, and closes by noticing what the research reveals about the repository (missing opinion, stale aside, GA'd feature). Use whenever the user wants to understand, evaluate, or decide on a .NET subject rather than change the repository.
license: See repository LICENSE
compatibility: Requires internet access for source reading; degrades to opinions-only offline
metadata:
  repo: dotnet-awesome-humans
  change-flow: branch-pr
---

# Research a topic

Answer "What should I know or do about X?" within this repository's trust boundary: its own opinions first, then the vetted roster. This skill writes only two things, both on a research branch: its research topic, and the `last-used` dates of the opinions it consulted. Changes to `opinions/` and `templates/` happen only through the skills it can recommend at the end.

## Orchestration

As [AGENTS.md: Orchestration and model economy](../../AGENTS.md#orchestration-and-model-economy) describes, fan the per-source reading out to worker agents. Only the editor weighs conflicting sources, assembles the research topic, and answers follow-up questions.

## Steps

1. **Clarify the intent with at most one question,** and only when the request could go more than one way:
   - _Learning_ it: an explanation and idioms.
   - _Deciding_ on it: trade-offs and maturity.
   - _Migrating_ to it: differences from the old way, and breaking changes.

   Research a clear request immediately.

2. **Start a worktree** on the branch `research/<topic-slug>`, such as `research/union-types`, following the worktree and branch name rules in [AGENTS.md](../../AGENTS.md). Create one per topic, before the first write, so the research topic and the `last-used` updates land together. Make the slug the topic in kebab case, as the repository already names it (`union-types`, not `union-type`), so the branch, research topic, and opinions share one search term.
3. **Start with the repository.** Read the matching `opinions/` files. The repository might already hold the answer, or a "Coming next" note. Present **House:** content as "local convention, not community consensus". Update the `last-used` metadata on every opinion you consult.
4. **Sweep the roster in order of precedence:**
   1. **Citable sources,** weighted by how well their Focus column matches. You can quote these.
   2. **Watch-list sources,** for discovery and cross-checking. Label them as watch-list.
   3. **Material from outside the roster.** It's welcome in research, because newer sources that aren't vetted yet often cover recent topics first.
      - Flag every such citation as **unvetted**, and keep unvetted claims visually separate from roster-sourced ones.
      - Record promising sources as `vet-source` candidates in the closing section.
      - A named package's own documentation, release notes, and issues are references, not unvetted sources, so they need no flag ([AWESOME-HUMANS.md: Admission criteria](../../AWESOME-HUMANS.md#admission-criteria)).

   Research is permissive. **Promotion is the strict gate,** as described in [Persistence and lifecycle](#persistence-and-lifecycle).

5. **Assemble the research topic:**
   - Put the answer first: the recommendation or current state in the opening sentences, and the detail after.
   - Cite a source ID for every claim. Say where the source stands when that limits the claim, such as `**Corroborate.**` or a limit in its notes. Date anything time-sensitive.
   - **Label preview features as the freshness policy requires:** "in preview as of `<date>`, not yet an opinion". Keep them clearly separate from GA guidance.
   - Where sources disagree, say so, and weigh them. Don't average them into a vague middle.
6. **Answer follow-up questions** from the material you've gathered. Sweep again only when a question goes beyond what you researched, in the same order of precedence.
7. **Close the loop.** End by noting what the research revealed about the repository, and recommend the matching skill. Never run it unprompted.

   | Finding                                                                      | Recommendation                                                                          |
   | ---------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- |
   | The topic has no opinion file or coverage                                    | `resolve-research`, which decides whether to add to an existing file or start a new one |
   | A note or opinion is stale, because a feature reached GA or guidance changed | `refresh-dotnet-versions` or `harvest-sources`                                          |
   | A strong source from outside the roster carried the research                 | `vet-source`                                                                            |
   | The repository already answers the question fully                            | Say so. The system is working, and `resolve-research` can discard the topic             |
   | A saved research topic on this subject exists in `research/`                 | Build on it, refresh its dates, and report its promote-or-discard status                |

## Persistence and lifecycle

**Save the research topic by default** to `research/<topic-slug>.md`, with the metadata a topic carries ([AGENTS.md: Metadata](../../AGENTS.md#metadata)). Skip saving only if the user says the question was a one-off.

Commit the topic and its `last-used` updates on the `research/<topic-slug>` branch, and open a pull request to the default branch. Merging puts the research on record. Only then does `resolve-research` pick it up, on its own branch and pull request. Never add the promote-or-discard decision to the open research pull request. If the user doesn't want it saved, delete the branch and answer in the conversation only.

**Use `research: <topic>` for the commit and pull request title,** such as `research: union types`, not `docs: research <topic>`. A separate type keeps staged research apart from settled opinion changes in `git log`, and lists every topic the repository has researched. `resolve-research` sets the type of the resolution commits.

Every saved topic must eventually be resolved, by promotion or discard. `resolve-research` handles both, and both end with the file deleted, so a topic on disk is unresolved by definition. `audit-freshness` reports a topic past its tolerance for promote-or-discard triage.

## Edge cases

- **Preview-only topics,** such as union types: Research them fully, because it's a legitimate request. Lead with the maturity status, and mark any guidance for "when it reaches GA" as hypothetical.
- **A topic outside .NET:** Say that the roster has no authority there, and stop. Don't present general knowledge as vetted research.
- **Offline:** Answer from `opinions/` alone, and say that you skipped the roster sweep. An answer from the opinions is still sourced, but it's only as current as their `last-reviewed` dates.
- **House and community positions conflict:** Present both, house first, and give the house reasoning. The reader might be working outside this repository's conventions.
