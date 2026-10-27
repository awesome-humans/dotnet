# Agent instructions

This repository defines opinionated best practices for building with .NET. This file tells an AI agent how to _use_ the repository as a reference and how to _maintain_ it.

## What this repository is

- **The source of truth for "what good looks like" in .NET.** Each topic has one file under `opinions/`, and [README.md: Scope](README.md#scope) indexes them.
- **Copy-paste-ready template files** under `templates/` that encode those opinions.
- **Kept current.** Every resource records when it was last reviewed. The skills under `skills/` keep the content current.

The repository is **LLM- and agent-agnostic**. Skills follow the [Agent Skills specification](https://agentskills.io/specification), and nothing here assumes a specific agent.

## Skills

Skills live at the specification's canonical path, `skills/<name>/SKILL.md`. Read and follow them from there. If your harness only discovers skills in a directory of its own, symlink that directory to `skills/` locally and keep the link out of git. Don't restructure the repository to suit a harness.

## Using this repository as a reference

When you generate or review .NET code for another project:

1. Read the relevant files under `opinions/`. Each one states the opinion first, then the rationale, then the sources.
2. Start from the `templates/` files for `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `.slnx`, `.slnf`, and project files. Copy them verbatim and trim, instead of writing your own.
3. Target the versions in the `targets:` frontmatter of the opinion files. Never silently downgrade to an older TFM or language version.
4. Follow a house opinion only for the house profile the user chose. It's a block that opens with `/// house | <profile>` in an opinion, or a region between `house:<profile> begin` and `house:<profile> end` comments in a template. Without a chosen profile, skip every block, and copy a template without its regions. For details, see [HOUSE-OPINIONS.md](HOUSE-OPINIONS.md).

## Maintaining this repository

- **Trace every substantive change to a source in [AWESOME-HUMANS.md](AWESOME-HUMANS.md).** The one exception is a **house opinion**: a house profile's preference from experience, such as the repository owner's, recorded through [HOUSE-OPINIONS.md](HOUSE-OPINIONS.md) and the `weave-house-opinion` skill. It sits in a house block beside the sourced text and cites `house:<profile>`, and [awesome-humans/website: House opinions](https://github.com/awesome-humans/website/blob/main/pages/conventions/house-opinions.md#how-this-works) is the canonical definition of its format, its marking, how it relates to the sourced text, and contributor adoption. Work from that section, not a paraphrase.
- **Don't fold in guidance from unvetted sources.** Propose the source for admission with the `vet-source` skill instead. Linking a named package's own documentation as a **reference** doesn't count: a reference corroborates a claim from a roster source and never carries one. For the definition, see [awesome-humans/website: Admission criteria](https://github.com/awesome-humans/website/blob/main/pages/conventions/admission-criteria.md#reserved-ids-markings-and-references).
- **Target the latest released .NET, C#, and F# versions.** You can mention a preview in a clearly marked "coming next" note, but never make it the opinion.
- **Use a skill instead of an ad hoc edit when one covers the work,** such as a version bump (`refresh-dotnet-versions`), a content sweep (`harvest-sources`), or a staleness check (`audit-freshness`). [README.md: Maintenance via skills](README.md#maintenance-via-skills) lists them all.
- **Keep opinions opinionated.** Give one recommendation, not a menu. An alternative gets at most one line on why it lost.
- **List every new file under `opinions/` in `README.md`, in two places:**
  - A bullet under [Scope](README.md#scope), in the form `- **[Title](opinions/<file>.md):** one line on what it covers`. Sort the bullets by title, ignoring case, and keep the unlinked **Libraries** bullet last.
  - An entry in the [Repository layout](README.md#repository-layout) tree, sorted by file name.

  The two orders differ: `architecture.md` is titled "Application architecture". When you rename or remove a file, update both places in the same commit. The same goes for a skill directory and its row in the [Maintenance via skills](README.md#maintenance-via-skills) table, which is sorted by skill name. `scripts/validate-readme-index.cs` fails the pull request if an entry is missing, out of order, or points at a file that doesn't exist.

- **Follow [Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/) for commit messages and pull request titles.** A pull request squash-merges under its title, so the title becomes the commit subject in `git log`. Use the form `<type>[(scope)]: <description>`, in lowercase and the imperative. The scope is optional, as in `chore(deps)` or `docs(review-project)`.

  | Type       | Use for                                                  |
  | ---------- | -------------------------------------------------------- |
  | `docs`     | Opinion, template, and guidance changes                  |
  | `fix`      | Bug fixes in scripts and workflows                       |
  | `feat`     | New features in scripts and workflows                    |
  | `ci`       | Workflow plumbing                                        |
  | `chore`    | Dependencies and housekeeping                            |
  | `research` | Staging or discarding a research topic (repository-only) |

  A skill that sets its own title says so in its `SKILL.md`, as `research-topic` and `resolve-research` do.

- **Fill in the pull request template a skill names.** The templates are the organisation's, in [awesome-humans/.github: `PULL_REQUEST_TEMPLATE/`](https://github.com/awesome-humans/.github/tree/main/PULL_REQUEST_TEMPLATE), and each skill's "Open a pull request" step names its own. Fill in every section, and pass the result to `gh pr create --body-file`. A section that doesn't apply says "Doesn't apply" and why, instead of being deleted. The description becomes the squash commit's message, so write it as [Writing style: Commit messages and pull requests](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md#commit-messages-and-pull-requests) describes: delete the template's guidance comment, leave out tables, checkboxes, and inline links, and end with a `Closes: #<number>` or `Refs: #<number>` trailer for each related issue, written as `awesome-humans/<repository>#<number>` when the issue is in another repository. A person opening a pull request in the browser gets the default template, which links the others.

- **Update a diagram in the same commit as the skill or lifecycle it shows.** `README.md` has diagrams of the maintenance pipeline and of the research and source-admission lifecycles. They're for orientation only, and the prose here and in the `SKILL.md` files stays canonical. `scripts/export-diagrams.cs` draws them as SVG into `assets/diagrams/`. To change one, edit the data at the top of the diagram's class, rerun the script, and commit both. Never edit an SVG by hand: CI reruns the script and fails on any difference.

### Orchestration and model economy

Maintenance skills mix two kinds of work. Sweeping sources, fetching pages, and checking versions is wide and shallow. Deciding what an opinion says is narrow and deep. Split them:

- **Give the sweeps to lower-cost worker agents.** Web searches, feed checks, page fetches, and per-source summaries are cheap-model work. Run them in parallel where the host supports it. Workers return raw findings, such as links, dates, and extracted claims, never edits.
- **Use the strongest available model as the editor.** Only the editor, the orchestrating model, combines worker findings, resolves conflicts between sources, and updates `opinions/` and `templates/`. Model quality matters most here, so never delegate the final edit to a cheap model.
- **The split is a recommendation.** A single-model host can run everything itself, but it keeps gathering and deciding as separate steps.

### Agent attribution

An agent that uses the author's credentials posts under the author's account, and nothing tells a reader that a person didn't write it. Put attribution in the conversation, where readers need it, and keep it out of the history:

- **No agent attribution in history.** Pull request titles and descriptions become squash commits, so they don't carry `Co-Authored-By` trailers or "Generated with" lines. Neither do commit messages. Agents write most changes here, so a trailer on every commit would tell a reader nothing. This rule overrides any default that an agent harness applies.
- **The rule covers every commit on the branch.** When GitHub squashes, it appends a `Co-authored-by` line to the merged commit for each co-author trailer, and for each branch commit author other than the merger. The pull request description doesn't change that. So commit under the author's git identity, never one the harness supplies, and leave the trailer out of branch commits too.
- **Agent attribution in conversation.** End each comment an agent writes on an issue or pull request, including replies in review threads, with the signature line that [awesome-humans/website: Writing style](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md#commit-messages-and-pull-requests) defines. Copy its format from there, not from a past comment: it names the harness, the model without a vendor name the harness already gives, and the skill, if any. A reader, or another agent, then knows a person didn't write it.

### CodeRabbit reviews

[CodeRabbit](https://coderabbit.ai) reviews pull requests here, using the organisation's central configuration in [awesome-humans/coderabbit](https://github.com/awesome-humans/coderabbit/blob/main/.coderabbit.yaml). It reads this file as its guidelines.

- **Check that the review started.** CodeRabbit reviews a new pull request on its own, and each push after that. If its status comment says it skipped or paused the review, comment `@coderabbitai review`.
- **Its findings are advisory.** CodeRabbit never blocks a merge, and the owner's review stays the gate. Still, answer every finding: fix it, or reply with the reason you didn't. End the reply with the agent attribution line, and don't resolve a thread without doing one or the other.

## Metadata

Three kinds of resource carry this metadata, in the two syntaxes their file formats allow:

| Directory    | `targets` | `last-reviewed` | `sources` | Syntax             |
| ------------ | --------- | --------------- | --------- | ------------------ |
| `opinions/`  | Yes       | Yes             | Yes       | YAML frontmatter   |
| `templates/` | Yes       | Yes             | Yes       | First-line comment |
| `research/`  | Yes       | Yes             | Yes       | YAML frontmatter   |

**`opinions/` and `research/` carry YAML frontmatter:**

```yaml
---
targets: [net10.0, csharp-14, fsharp-10]
last-reviewed: 2026-08-12 # last time content was verified against sources
sources: [andrew-lock, dotnet-blog] # ids from AWESOME-HUMANS.md, sorted
---
```

A `research/` topic has no status field. Promoting or discarding a topic deletes it, so a topic on disk is unresolved by definition, and a status could only ever read `open`. For details, see [research-topic](skills/research-topic/SKILL.md) and [resolve-research](skills/resolve-research/SKILL.md).

**`templates/` carry a first-line comment header.** An XML, INI, or source file can't open with a `---` block and stay valid for the tools that read it. So the same fields go in a comment instead, separated by pipes after a fixed marker:

```xml
<!-- awesome-humans/dotnet template | targets: net10.0 | last-reviewed: 2026-08-12 | sources: ms-learn -->
```

```ini
# awesome-humans/dotnet template | targets: net10.0 | last-reviewed: 2026-08-12 | sources: ms-learn
```

```fsharp
// awesome-humans/dotnet template | targets: net10.0, fsharp-10 | last-reviewed: 2026-08-12 | sources: scott-wlaschin
```

Rules:

- **Don't record when a resource was used.** A `last-used` field is rejected. Writing a date on every read left uncommitted edits in every checkout an agent read from, and they conflicted with real changes to the same line. To find pruning candidates, `audit-freshness` counts what refers to each opinion and template instead.
- **Update `last-reviewed` only after you verify the content against its sources.**
- **Use `sources` IDs that exist in `AWESOME-HUMANS.md`,** either in the roster tables or as a house profile's reserved `house:<profile>` ID.
  - CI reads the `**Discovery-only.**` and `**Corroborate.**` markings from a source's section under `## Source notes`, not from its table row.
  - In `opinions/` and `templates/`, the source must also be citable. A watch-list source or a `**Discovery-only.**` source is rejected.
  - A file whose citable sources are all marked `**Corroborate.**` is rejected too, because an unmarked source has to stand beside them.
  - `research/` is exempt: a topic can cite unvetted material if the text flags it. For details, see [research-topic](skills/research-topic/SKILL.md).
- **Sort `sources` by ID in ordinal order, with `house:<profile>` sorted like any other ID.** Write `[andrew-lock, dotnet-blog, house:simon-mckenna, ms-learn]`. The rule applies in all three directories and to both syntaxes.
- **Write dates in ISO 8601 (`YYYY-MM-DD`), in UTC.** Always use absolute dates, never relative ones. To get today's date, run `date -u +%F`. CI, GitHub, and the site build run in UTC, as [awesome-humans/website: Writing style](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md) describes.
- **Two template files are exempt from the header:** `templates/global.json` and `templates/example.slnf`, because JSON has no comment syntax. What they pin is audited against the latest releases instead. Smoke-testing the example projects leaves build output under `templates/` (`artifacts/`, `bin/`, and `obj/`, all gitignored). That output isn't a resource, and the check skips it.
- **`skills/` are deliberately exempt.** The [Agent Skills specification](https://agentskills.io/specification) defines their frontmatter, they're procedures and not reference material, and nothing reads a date from them. Use `git log` to see when a skill last changed.

CI runs `just validate-metadata` against both syntaxes, and a missing field, a relative date, or an unsorted `sources` list fails the pull request.

## Writing style

Follow [awesome-humans/website: Writing style](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md), the shared rules for all prose in every awesome-humans repository, and read it before you write. It covers opinions, research topics, skills, template comments, root documents such as this one, commit messages, pull request titles and descriptions, and the comments an agent posts on issues and pull requests. This repository adds two rules, and [Writing style for opinions](#writing-style-for-opinions) adds more for opinions only:

- **Write for a working .NET developer.** Assume they know C#, the BCL, and the SDK. Don't explain what dependency injection or a `Task` is.
- **Use .NET's spelling for a .NET concept.** `System.Globalization` and `IStringLocalizer` give _globalization_ and _localization_, down to the file name `opinions/globalization.md`. EF Core's materialization gives _materialized_. A word that only shares a stem with an API keeps its British spelling: "a behaviour change in SQLite", even though MAUI has a `Behavior` type.

The shared page's exception for pre-release features covers the "coming next" note about a .NET preview that [Maintaining this repository](#maintaining-this-repository) allows.

## Writing style for opinions

- **Put the opinion first.** State the recommendation in the first sentence. The rationale follows, and the sources come last.
- **Make it precise enough to apply mechanically.** An agent should be able to turn an opinion into an edit or a `review-project` finding without interpreting it. A rule that names no setting, type, or threshold isn't finished.
- **Show code when it's clearer than prose.** Keep examples minimal and idiomatic for the declared `targets`. When an opinion replaces an old idiom, show a before-and-after pair. The same applies to `templates/`: example code files are welcome alongside configuration.
- **Make code samples compile against the declared `targets`.**
- **Use a table when the same attributes repeat across items.** Options, versions, or frameworks compared on the same few points read faster as rows than as parallel paragraphs, as in [datetime.md](opinions/datetime.md). Keep the reasoning in sentences: a cell holds a fact, not the argument for it.
- **Group sections by topic, and add a new one to its group.** Don't sort them alphabetically or append them to the end of the page. In `templates/`, sort `PackageVersion` and `PackageReference` entries by name, ignoring case, within their `ItemGroup`, and `just validate-template-order` checks it. For the rule, see [awesome-humans/website: Writing style: Order](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md#order).
- **Keep one topic per file under `opinions/`.** Keep each file under about 300 lines, and split it when it grows past that.
- **Give transition guidance an expiry date.** A note that matters only while the ecosystem changes, such as a migration off a retired tool, opens with `**Review by YYYY-MM-DD.**` and says what ends it. `audit-freshness` reports a marking whose date has passed. Either drop the note, or check that it still applies and re-date it. Never delete the marking alone.

## Workflow

### Worktrees

Work in a git worktree, with one branch per directory. The local clone is bare, and each branch is checked out beside it, so you move between pieces of work with `cd` instead of `git checkout`. An in-place branch switch changes the tree under anything that holds a path into it, such as a running `dotnet` build, a formatter, or another agent mid-edit. It also discards the gitignored state the old branch had built: `node_modules/`, `artifacts/`, `bin/`, and `obj/`.

- **Starting:** Start every new piece of work as a **new worktree, not just a new branch**, whether it's a feature, a fix, a research topic, or a version bump. Use `git worktree add -b <branch> <dir> main` as one step. Never run `git checkout -b` in a worktree that already holds other work.
- **Branch names:** Prefix a skill's branch with what the skill produces: `research/<topic-slug>`, `resolve/<topic-slug>`, `harvest/<YYYY-MM-DD>` (dated the day the sweep window ends), `vet/<source-id>` or a slug for a multi-source pass, `house/<slug>`, and `refresh/dotnet-<version>`.
  - Any other change uses its commit's [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) type (`docs/`, `fix/`, `feat/`, `ci/`, or `chore/`) and a kebab-case slug. `automation/` is reserved for workflows.
  - This convention outranks the host environment's own. Replace a generated name such as `agent/youthful-wozniak-gm4k7m` before the first push. If a host only pushes under its own prefix, put the convention after it: `agent/harvest/2026-09-14`.
  - Settle the name before you open the pull request. Renaming the head branch of an open pull request on GitHub closes it, and you can't reopen it under the new name.
- **Directory names:** Name the directory after the branch, with slashes replaced by dashes, so `resolve/globalization` becomes `resolve-globalization/`. Every worktree then sits one level beside the bare `.bare/` directory and tab-completes in one step. This also keeps a nested `research/` directory from appearing next to the repository's own `research/`.
- **Setup:** A fresh worktree has no gitignored local state. Run `just install` in it before `just check` or `just verify`. `just verify` also needs skills-ref: install it once with `uv tool install skills-ref --with-requirements .github/requirements.txt`, which keeps the pin in that file. Without uv, run `pip install -r .github/requirements.txt` in a virtual environment. If your harness needs the skills symlink, re-create it there. For details, see [Skills](#skills).
- **Finishing:** When a worktree's pull request merges, **offer to delete the worktree** with `git worktree remove <dir>` and then `git branch -d <branch>`. Ask first: the branch might still hold work that never reached the pull request.

### Formatting and linting

- Markdown follows GFM and markdownlint, configured in `.markdownlint-cli2.jsonc`. The configuration uses the defaults minus two rules that conflict with the prose style, and records why.
- Spelling follows cspell, configured in `cspell.jsonc` with the `en-GB` dictionary, the .NET dictionaries, and the repository's own word list in `cspell.words.txt`. The .NET dictionaries accept American spellings because API names use them, such as `Behavior` and `Color`, so the configuration flags them in Markdown prose. Editors with a cspell extension read the same configuration.
- Run `just format` to format, and `just check` to check formatting, lint, and spelling together. Each check also has its own recipe: `just format-check`, `just lint`, and `just spell`. The recipes run the scripts in `package.json`, which use the versions pinned there. Don't run `npx prettier`, `npx markdownlint-cli2`, or `npx cspell` directly, because they resolve the latest version.
- Run `just verify` before you open a pull request. It runs `just check` and every other offline check that CI runs, and fails until you commit any diagram it redraws. The link check and the full site build run only in CI. See [README.md: Repository scripts](README.md#repository-scripts).
- Prettier formats Markdown and the JSON, JSONC, and YAML configuration, and reads `.editorconfig`, so the formatter and your editor agree. `.prettierignore` records two exceptions:
  - `package-lock.json` and `.github/state/dotnet-releases.json` are machine-written. Gating them on formatting would fail a build that nobody can fix by editing.
  - `lychee.toml` isn't formatted. Prettier has no built-in TOML parser, and one hand-written file doesn't justify a third-party plugin.

### Scripts

- CI checks live in `scripts/` as .NET file-based apps. Run them from the repository root through their recipes, such as `just validate-metadata`. Code that two scripts share goes in a helper file that both `#:include`, not in a copy.
- The `justfile` is every local and CI entry point. Each check has a recipe named after its script, and CI calls the recipes, never `npm` or `dotnet` directly. `just` with no arguments lists them. The one exception is the website, which doesn't install `just` and runs `dotnet run scripts/build-site.cs` directly. So keep every script runnable on its own, with `dotnet run` or its shebang, even though nothing here calls it that way.
- Entry points use kebab-case names because you type them on a command line. Helper files take the name of the type they contain, following C# file-naming convention.
- Add a check as a script with a recipe, not as an inline script in a workflow, and hold it to the repository's own opinions: `global.json` pins the SDK, `TreatWarningsAsErrors` is on, the language version is latest, and the `[scripts/*.cs]` block in `.editorconfig` turns the analyzer set into build failures.
- When you add or rename a script, add or rename its recipe, and update the [Repository scripts](README.md#repository-scripts) table and the layout tree.
- Renovate bumps `#:package` pins. It reads the directive but only scans `.cs` files under `scripts/`, because `renovate.json` opts that directory into the NuGet manager. A check placed anywhere else wouldn't get bumped.

### Site

- The awesome-humans.net site is generated. `scripts/build-site.cs` builds its `/dotnet/` pages from `README.md` (up to Repository layout), `opinions/`, `templates/`, `skills/`, `AWESOME-HUMANS.md`, and `HOUSE-OPINIONS.md`. Edit those files, never a staged page.
- With `-- --out <dir>`, `build-site.cs` stages only `<dir>/dotnet/`, as the [awesome-humans/website staging contract](https://github.com/awesome-humans/website#staging-contract) defines, with `nav.json`, `sources.json` exported from the roster, and this ecosystem's own `llms.txt`. The website repository adds the pages that every ecosystem shares, and builds and deploys the site.
- House opinions stay in the staged pages as house blocks, and the site shows a profile's only to a reader who chooses it in the header. For agents, `build-site.cs` writes a copy of every file `llms.txt` links under `raw/`, so an agent reads them all from the one host. A file that holds house opinions gets one copy with none, linked from `llms.txt`, and one per profile, linked from that profile's `llms-<profile>.txt`. A skill's copy adds a note after its frontmatter that names this repository and says where a relative path such as `templates/` leads, because an agent that reads it there has no checkout.
- The opinion navigation follows [README.md: Scope](README.md#scope), and the skill navigation follows [README.md: Maintenance via skills](README.md#maintenance-via-skills), so a new opinion or skill needs no separate site entry.
- Two workflows hand the site to `awesome-humans/website`. `Site preview` calls its reusable `build.yml` and `deploy.yml` with the pull request's head commit, for a full-site preview at `pr-dotnet-<number>`. Its build fails a pull request on any link or anchor the site can't resolve. `Site dispatch` tells `website` to rebuild and deploy awesome-humans.net when `main` changes.

### Links

The `Markdown` workflow checks links with `lychee.toml`, and a broken link **blocks the pull request**, because a dead citation is a content defect. If a third-party host is down, re-run the job after it recovers. Never merge past a failed link check.
