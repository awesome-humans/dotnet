<!-- Thanks for contributing! For opinion or template changes, fill in the sections below.
     For mechanical fixes, such as typos, broken links, or CI, delete them and describe the fix. -->

## Proposed opinion

**Opinion (one sentence with one recommendation, not a menu):**

**Why (the reasoning, the trade-offs, and what it replaces):**

**Where it belongs (an `opinions/` or `templates/` file):**

## Provenance: pick one

- [ ] **Sourced:** It traces to a vetted source in [AWESOME-HUMANS.md](../AWESOME-HUMANS.md). Source IDs and links:
- [ ] **Candidate source:** It traces to a source that isn't on the roster yet. The source must pass [`vet-source`](../skills/vet-source/SKILL.md) before the opinion can merge. Source and evidence of its track record:
- [ ] **Experience-based:** It comes from my own practice, with no published source. If the owner accepts it, they **adopt** it as a house opinion ([HOUSE-OPINIONS.md](../HOUSE-OPINIONS.md)). The owner takes responsibility for it, and the Provenance column of the Woven table credits this pull request and its author.

## Checklist

- [ ] The opinion comes first: the recommendation in the first sentence, then the rationale, then the sources. For details, see [AGENTS.md](../AGENTS.md).
- [ ] Code examples compile against the declared `targets:`. State the SDK you built with.
- [ ] Every `opinions/`, `research/`, or `templates/` file you changed has complete metadata: `targets`, `last-reviewed`, and `sources`, plus `last-used` outside `research/`. CI validates these. [AGENTS.md](../AGENTS.md) lists the exemptions: the two JSON templates, and `skills/`, whose frontmatter the Agent Skills specification defines.
- [ ] Every new, renamed, or removed `opinions/` file is indexed in [README.md](../README.md), with a linked **Scope** bullet and a **Repository layout** entry, each in order, and no stale entries. CI validates this.
- [ ] `npm run check` passes. It checks Markdown, JSON, and YAML formatting, and runs markdownlint, at the versions pinned in `package.json`.
- [ ] No preview features are stated as opinions. Previews go in "Coming next" notes.
