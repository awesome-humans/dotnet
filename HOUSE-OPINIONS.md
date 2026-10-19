# House opinions

House opinions are preferences from experience, not from a vetted source. Each belongs to a **house profile**, a named set of house opinions owned by a person or an organisation. They're **always visibly marked**, so a reader can tell community best practice from one owner's convention.

awesome-humans.net shows no house opinions by default, so every page is sourced community practice. A reader chooses a profile in the header to see its house opinions beside the sourced text, and an agent reads the profile's `llms-<profile>.txt` instead of `llms.txt`.

## Profiles

| Profile         | Owner                                 | Kind   |
| --------------- | ------------------------------------- | ------ |
| `simon-mckenna` | Simon McKenna, the repository's owner | person |

The site registers every profile in [awesome-humans/website: `houses.toml`](https://github.com/awesome-humans/website/blob/main/houses.toml), and its build fails on a house opinion whose profile isn't registered there for this ecosystem. A new profile needs the repository owner's approval, recorded there.

## How this works

Every awesome-humans ecosystem weaves house opinions the same way, as [awesome-humans/website: House opinions](https://github.com/awesome-humans/website/blob/main/pages/conventions/house-opinions.md) defines. It gives the [entry format](https://github.com/awesome-humans/website/blob/main/pages/conventions/house-opinions.md#entry-format) for the [Inbox](#inbox), how `weave-house-opinion` writes an entry in beside the sourced text, the fixed `**House:**` marking, and how a contributor's opinion is adopted. It's the canonical statement, so read it before you add or weave an entry, and work from it, not a paraphrase.

## Inbox

_Nothing waiting._

## Woven

These four were first woven by rewriting the sourced text, as rule 4 then required. On 2026-10-18, each became a house block or region beside the sourced text, as [awesome-humans/website: House opinions](https://github.com/awesome-humans/website/blob/main/pages/conventions/house-opinions.md#how-this-works) now describes.

| Date       | Profile         | Opinion                                                                                                                 | Woven into                                                                                                                                                           | Provenance |
| ---------- | --------------- | ----------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------- |
| 2026-08-12 | `simon-mckenna` | Name tests `UnitOfWork_Scenario_ExpectedBehaviour`, and mark Arrange, Act, and Assert with comments                     | [opinions/testing.md](opinions/testing.md), [templates/projects/Example.Library.Tests.csproj](templates/projects/Example.Library.Tests.csproj)                       | Owner      |
| 2026-08-12 | `simon-mckenna` | Treat warnings as errors everywhere, not only in CI. Every suppression has an inline comment that explains it           | [opinions/ci.md](opinions/ci.md), [opinions/project-structure.md](opinions/project-structure.md), [templates/Directory.Build.props](templates/Directory.Build.props) | Owner      |
| 2026-08-18 | `simon-mckenna` | Leave the dependency bot's dashboard issue open. It's state the bot manages, not a task, and closing it doesn't opt out | [opinions/ci.md](opinions/ci.md)                                                                                                                                     | Owner      |
| 2026-09-25 | `simon-mckenna` | Give the solution a `/build/` solution folder that lists the root build files                                           | [opinions/project-structure.md](opinions/project-structure.md), [templates/example.slnx](templates/example.slnx)                                                     | Owner      |
