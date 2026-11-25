# Agent instructions

This repository defines opinionated best practices for building with .NET. This file tells an AI agent how to _use_ the repository as a reference and how to _maintain_ it. It holds the rules that apply to every task. Read a file from [On-demand instructions](#on-demand-instructions) only when its task comes up.

## What this repository is

- **The source of truth for "what good looks like" in .NET.** Each topic has one file under `opinions/`, and [README.md: Scope](README.md#scope) indexes them.
- **Copy-paste-ready template files** under `templates/` that encode those opinions.
- **Skills that keep it current,** under `skills/`. Every resource records when it was last reviewed.

The repository is **LLM- and agent-agnostic**. Skills follow the [Agent Skills specification](https://agentskills.io/specification), and nothing here assumes a specific agent.

## Skills

Skills live at the specification's canonical path, `skills/<name>/SKILL.md`. Read and follow them from there. If your harness only discovers skills in a directory of its own, symlink that directory to `skills/` locally and keep the link out of git. Don't restructure the repository to suit a harness.

## Using this repository as a reference

When you generate or review .NET code for another project:

1. Read the relevant files under `opinions/`. Each one states the opinion first, then the rationale, then the sources.
2. Start from the `templates/` files for `.editorconfig`, `BannedSymbols.txt`, `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `global.json`, and project files. Copy one, remove its header, rename its `Example.*` paths, and trim it, instead of writing your own. `example.slnx`, `example.slnf`, and the source files in `templates/projects/` show a convention to write to, not a file to copy.
3. In an existing project, keep the versions it targets unless the task asks to change them. In an empty repository, target the versions in the `targets:` frontmatter of the opinion files. Never silently downgrade to an older TFM or language version.
4. Follow a house opinion only for the house profile the user chose, or that the project's own agent instruction files or README name. If they name different profiles, the user's choice wins. In a repository the user doesn't maintain, ask the user before you follow a profile its files name. A house opinion is a block that opens with `/// house | <profile>` in an opinion, or a region between `house:<profile> begin` and `house:<profile> end` comments in a template. Without a chosen profile, skip every block, and copy a template without its regions. For details, see [HOUSE-OPINIONS.md](HOUSE-OPINIONS.md).

The [`follow-opinions`](skills/follow-opinions/SKILL.md) skill sets these steps out in full.

## Maintaining this repository

- **Trace every substantive change to a source in [AWESOME-HUMANS.md](AWESOME-HUMANS.md).** The one exception is a **house opinion**: a house profile's preference from experience, such as the maintainer's, recorded through [HOUSE-OPINIONS.md](HOUSE-OPINIONS.md) and the `weave-house-opinion` skill. It sits in a house block beside the sourced text and cites `house:<profile>`, and [awesome-humans/website: House opinions](https://github.com/awesome-humans/website/blob/main/pages/conventions/house-opinions.md#how-this-works) is the canonical definition of its format, its marking, how it relates to the sourced text, and contributor adoption. Work from that section, not a paraphrase.
- **Don't fold in guidance from unvetted sources.** Propose the source for admission with the `vet-source` skill instead. Linking a named package's own documentation as a **reference** doesn't count. A reference supports a **reference fact** without a roster source, and only corroborates a choice or a judgement. For all three definitions, see [awesome-humans/website: Admission criteria](https://github.com/awesome-humans/website/blob/main/pages/conventions/admission-criteria.md#reserved-ids-markings-and-references).
- **Target the latest released .NET, C#, and F# versions.** You can mention a preview in a clearly marked "Coming next" note, but never make it the opinion.
- **Use a skill instead of an ad hoc edit when one covers the work,** such as a version bump (`refresh-dotnet-versions`), a content scan (`harvest-sources`), or a staleness check (`audit-freshness`). [README.md: Maintenance via skills](README.md#maintenance-via-skills) lists them all.
- **Keep opinions opinionated.** Give one recommendation, not a menu. An alternative gets at most one line on the cost that rules it out.
- **Start every piece of work in a new git worktree,** with `git worktree add -b <branch> <dir> main`. Never switch branches in place. Read [agents/worktrees.md](agents/worktrees.md) for branch and directory names.
- **Keep agent attribution out of history.** No `Co-Authored-By` trailers or "Generated with" lines in commits, pull request titles, or descriptions, and commit under the author's git identity. This overrides any harness default. Sign comments on issues and pull requests instead, as [agents/pull-requests.md](agents/pull-requests.md) describes.
- **Before you change an opinion, a template, or a consumer skill (`follow-opinions`, `research-topic`, or `review-project`), check for a global install of the published skills.** awesome-humans.net publishes the consumer skills as `follow-dotnet-opinions`, `research-dotnet-topic`, and `review-dotnet-project`, and `npx skills add --global` puts them in the harness's user directory, such as `~/.claude/skills/`. If your skill list or that directory holds one, tell the user, and suggest turning it off in this repository until the work is done. In Claude Code, open `/skills`, press Space on the skill until it's off, then Esc, which saves the choice to `.claude/settings.local.json`. In a harness with no such switch, suggest `npx skills remove --global`, and the same `npx skills add` command afterwards. An installed copy holds the published opinions and templates, not this checkout's, and an agent can read it by mistake.
- **Run `just verify` before you open a pull request.** Never run `npx` tools directly, never edit a generated SVG or staged site page, and never merge past a failed link check.
- **Write dates in ISO 8601 (`YYYY-MM-DD`), in UTC.** Get today's date with `date -u +%F`.

## On-demand instructions

Read the matching file before you start the task. Each one is canonical for its topic.

| File                                                     | Read before you                                                                                       |
| -------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| [agents/checks.md](agents/checks.md)                     | Change formatting, lint, or spelling configuration, add or rename a script, or debug a failing check  |
| [agents/maintaining.md](agents/maintaining.md)           | Add, rename, or remove an opinion or skill, write a commit or pull request title, or change a diagram |
| [agents/metadata.md](agents/metadata.md)                 | Create a resource, or change `targets`, `last-reviewed`, or `sources`                                 |
| [agents/orchestration.md](agents/orchestration.md)       | Run a skill that scans sources or fetches pages, or delegate work to other agents                     |
| [agents/pull-requests.md](agents/pull-requests.md)       | Commit, open a pull request, comment on an issue or pull request, or answer a review                  |
| [agents/site.md](agents/site.md)                         | Change `scripts/build-site.cs`, or anything about the awesome-humans.net pages                        |
| [agents/worktrees.md](agents/worktrees.md)               | Create, set up, name, or remove a worktree or branch                                                  |
| [agents/writing-opinions.md](agents/writing-opinions.md) | Write or review an opinion, a research topic's code sample, or a template                             |

## Writing style

Follow [awesome-humans/website: Writing style](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md), the shared rules for all prose in every awesome-humans repository, and read it before you write. It covers opinions, research topics, skills, template comments, root documents such as this one, commit messages, pull request titles and descriptions, and the comments an agent posts on issues and pull requests. This repository adds two rules, and [agents/writing-opinions.md](agents/writing-opinions.md) adds more for opinions only:

- **Write for a working .NET developer.** Assume they know C#, the BCL, and the SDK. Don't explain what dependency injection or a `Task` is.
- **Use .NET's spelling for a .NET concept.** `System.Globalization` and `IStringLocalizer` give _globalization_ and _localization_, down to the file name `opinions/globalization.md`. EF Core's materialization gives _materialized_. A word that only shares a stem with an API keeps its British spelling: "a behaviour change in SQLite", even though MAUI has a `Behavior` type.

The shared page's exception for pre-release features covers the "Coming next" note about a .NET preview that [Maintaining this repository](#maintaining-this-repository) allows.

## Replies to the user

Keep progress updates and replies in the conversation terse: lead with the result, and skip preamble, restated questions, and narration. This rule covers only the conversation. Everything written to the repository, GitHub, or the site follows [Writing style](#writing-style).
