# House opinions

These are the repository owner's opinions: preferences from experience, not from a vetted source. They carry the same weight as sourced opinions, and they're **always visibly marked**, so a reader, person or agent, can tell community best practice from this repository's convention.

## How this works

1. **The owner adds an entry to the [Inbox](#inbox).** A sentence or two is enough, as long as it gives the reasoning.
2. **The `weave-house-opinion` skill writes it in.** The opinion goes into the right `opinions/` file, and into `templates/` where it applies, with `house` in the file's `sources:`. The inbox entry then moves to [Woven](#woven), with a link.
3. **The marking is a fixed string.** Every house opinion starts with exactly `**House:**`, bold with the colon inside, so `grep -F '**House:**'` finds it. The `house` ID and the marking always appear together, and one without the other is an audit finding.
4. **A house opinion wins a conflict with the roster.** This is the canonical statement, and other files link here. Keep the house opinion, and cut the sourced position to a one-line note that cites the source: "The community default is X (source); we do Y because Z." Harvest, refresh, and audit runs never remove, weaken, or unmark house content.
5. **Other contributors propose opinions in a pull request,** as the [PR template](.github/PULL_REQUEST_TEMPLATE.md) describes. When the owner accepts an experience-based opinion, the owner adopts it as a house opinion, and the Woven table's Provenance column credits the contributor and the pull request.

The `house` source ID is reserved and documented in [AWESOME-HUMANS.md](AWESOME-HUMANS.md). Its authority comes from owning the repository, so it skips the admission criteria, and that's why it stays marked wherever it's used.

## Entry format

```markdown
### <short title>

- **Opinion:** <what to do, one sentence>
- **Why:** <the experience or reasoning behind it>
- **Scope:** <which topic/file it belongs to, if known>
```

## Inbox

_Nothing waiting._

## Woven

| Date       | Opinion                                                                                                                 | Woven into                                                                                                                                                           | Provenance |
| ---------- | ----------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------- |
| 2026-08-12 | Name tests `UnitOfWork_Scenario_ExpectedBehaviour`, and mark Arrange, Act, and Assert with comments                     | [opinions/testing.md](opinions/testing.md) (added retroactively; predates this file)                                                                                 | Owner      |
| 2026-08-12 | Treat warnings as errors everywhere, not only in CI. Every suppression has an inline comment that explains it           | [opinions/ci.md](opinions/ci.md), [opinions/project-structure.md](opinions/project-structure.md), [templates/Directory.Build.props](templates/Directory.Build.props) | Owner      |
| 2026-08-18 | Leave the dependency bot's dashboard issue open. It's state the bot manages, not a task, and closing it doesn't opt out | [opinions/ci.md](opinions/ci.md)                                                                                                                                     | Owner      |
| 2026-09-25 | Give the solution a `/build/` solution folder that lists the root build files                                           | [opinions/project-structure.md](opinions/project-structure.md), [templates/example.slnx](templates/example.slnx)                                                     | Owner      |
