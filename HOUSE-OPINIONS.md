# House opinions

House opinions are preferences from experience, not from a vetted source. Each belongs to a **house profile**, a named set of house opinions owned by a person or an organisation. They're **always visibly marked**, so a reader can tell community best practice from one owner's convention.

awesome-humans.net shows no house opinions by default, so every page is sourced community practice. A reader chooses a profile in the header to see its house opinions beside the sourced text, and an agent reads the profile's `llms-<profile>.txt` instead of `llms.txt`.

## Profiles

| Profile         | Owner                                 | Kind   |
| --------------- | ------------------------------------- | ------ |
| `simon-mckenna` | Simon McKenna, the repository's owner | person |

The site registers every profile in [awesome-humans/website: `houses.toml`](https://github.com/awesome-humans/website/blob/main/houses.toml), and its build fails on a house opinion whose profile isn't registered there for this ecosystem. A new profile needs the repository owner's approval, recorded there.

## How this works

1. **The profile's owner adds an entry to the [Inbox](#inbox),** naming the profile. A sentence or two is enough, as long as it gives the reasoning.
2. **The `weave-house-opinion` skill writes it in, beside the sourced text it changes.** It never rewrites the sourced text.
   - **In `opinions/`:** a block after the sourced recommendation it changes, which opens with `/// house | <profile>` and closes with `///`, each on its own line.
   - **In `templates/`:** a region between two comment lines, `house:<profile> begin` and `house:<profile> end`, in the file's comment syntax, such as `<!-- house:simon-mckenna begin -->`.
   - **In `sources:`:** `house:<profile>`, sorted like any other ID.

   The inbox entry then moves to [Woven](#woven), with a link.

3. **The marking is a fixed string.** Every house opinion starts with exactly `**House:**`, bold with the colon inside, so `grep -F '**House:**'` finds it. A `house:<profile>` ID and that profile's blocks or regions always appear together, `**House:**` appears only inside one, and `scripts/validate-sources.cs` fails the pull request otherwise.
4. **Keep both, and the profile decides which a reader sees.** This is the canonical statement, and other files link here. The sourced text stays whole, and the house block sits beside it. A reader or agent with the profile sees both, and the house opinion wins where they conflict. Everyone else sees only the sourced text. Harvest, refresh, and audit runs never remove, weaken, or unmark a house block, and never cut sourced text because a house block disagrees with it.
5. **Other contributors propose opinions in a pull request,** as the organisation's [opinion pull request template](https://github.com/awesome-humans/.github/blob/main/PULL_REQUEST_TEMPLATE/opinion.md) describes. When a profile's owner accepts an experience-based opinion, they adopt it into their profile, and the Woven table's Provenance column credits the contributor and the pull request.

The `house:` prefix is reserved and documented in [AWESOME-HUMANS.md](AWESOME-HUMANS.md). A profile skips the admission criteria because its authority is its owner's experience, which is why it's marked wherever it's used.

```markdown
/// house | simon-mckenna

**House:** Treat warnings as errors on every machine, not only in CI. …

///
```

## Entry format

```markdown
### <short title>

- **Profile:** <the house profile, such as simon-mckenna>
- **Opinion:** <what to do, one sentence>
- **Why:** <the experience or reasoning behind it>
- **Scope:** <which topic/file it belongs to, if known>
```

## Inbox

_Nothing waiting._

## Woven

These four were first woven by rewriting the sourced text, as rule 4 then required. On 2026-10-18, each became a house block or region beside the sourced text, as [How this works](#how-this-works) now describes.

| Date       | Profile         | Opinion                                                                                                                 | Woven into                                                                                                                                                           | Provenance |
| ---------- | --------------- | ----------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------- |
| 2026-08-12 | `simon-mckenna` | Name tests `UnitOfWork_Scenario_ExpectedBehaviour`, and mark Arrange, Act, and Assert with comments                     | [opinions/testing.md](opinions/testing.md), [templates/projects/Example.Library.Tests.csproj](templates/projects/Example.Library.Tests.csproj)                       | Owner      |
| 2026-08-12 | `simon-mckenna` | Treat warnings as errors everywhere, not only in CI. Every suppression has an inline comment that explains it           | [opinions/ci.md](opinions/ci.md), [opinions/project-structure.md](opinions/project-structure.md), [templates/Directory.Build.props](templates/Directory.Build.props) | Owner      |
| 2026-08-18 | `simon-mckenna` | Leave the dependency bot's dashboard issue open. It's state the bot manages, not a task, and closing it doesn't opt out | [opinions/ci.md](opinions/ci.md)                                                                                                                                     | Owner      |
| 2026-09-25 | `simon-mckenna` | Give the solution a `/build/` solution folder that lists the root build files                                           | [opinions/project-structure.md](opinions/project-structure.md), [templates/example.slnx](templates/example.slnx)                                                     | Owner      |
