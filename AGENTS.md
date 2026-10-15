# Agent instructions

This repository defines opinionated best practices for building with .NET. This file tells an AI agent how to _use_ the repository as a reference and how to _maintain_ it.

## What this repository is

- **The source of truth for "what good looks like" in .NET.** Each topic has one file under `opinions/`, and [README.md: Scope](README.md#scope) indexes them.
- **Copy-paste-ready template files** under `templates/` that encode those opinions.
- **Kept current.** Every resource records when it was last reviewed, and opinions and templates also record when they were last used. The skills under `skills/` keep the content current.

The repository is **LLM- and agent-agnostic**. Skills follow the [Agent Skills specification](https://agentskills.io/specification), and nothing here assumes a specific agent.

## Skills

Skills live at the specification's canonical path, `skills/<name>/SKILL.md`. Read and follow them from there. If your harness only discovers skills in a directory of its own, symlink that directory to `skills/` locally and keep the link out of git. Don't restructure the repository to suit a harness.

## Using this repository as a reference

When you generate or review .NET code for another project:

1. Read the relevant files under `opinions/`. Each one states the opinion first, then the rationale, then the sources.
2. Start from the `templates/` files for `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `.slnx`, `.slnf`, and project files. Copy them verbatim and trim, instead of writing your own.
3. Target the versions in the `targets:` frontmatter of the opinion files. Never silently downgrade to an older TFM or language version.
4. When you use an `opinions/` or `templates/` file as a reference, update its `last-used` date. For details, see [Metadata](#metadata). `research/` topics don't carry this date.

## Maintaining this repository

- **Trace every substantive change to a source in [AWESOME-HUMANS.md](AWESOME-HUMANS.md).** The one exception is a **house opinion**: the repository owner's own preference, recorded through [HOUSE-OPINIONS.md](HOUSE-OPINIONS.md) and the `weave-house-opinion` skill. It cites the reserved `house` source ID, and [HOUSE-OPINIONS.md: How this works](HOUSE-OPINIONS.md#how-this-works) is the canonical definition of its marking, its precedence over the roster, and contributor adoption. Work from that section, not a paraphrase.
- **Don't fold in guidance from unvetted sources.** Propose the source for admission with the `vet-source` skill instead. Linking a named package's own documentation as a **reference** doesn't count: a reference corroborates a claim from a roster source and never carries one. For the definition, see [AWESOME-HUMANS.md: Admission criteria](AWESOME-HUMANS.md#admission-criteria).
- **Target the latest released .NET, C#, and F# versions.** You can mention a preview in a clearly marked "coming next" note, but never make it the opinion.
- **Use a skill instead of an ad hoc edit when one covers the work,** such as a version bump (`refresh-dotnet-versions`), a content sweep (`harvest-sources`), or a staleness check (`audit-freshness`). [README.md: Maintenance via skills](README.md#maintenance-via-skills) lists them all.
- **Keep opinions opinionated.** Give one recommendation, not a menu. An alternative gets at most one line on why it lost.
- **List every new file under `opinions/` in `README.md`, in two places:**
  - A bullet under [Scope](README.md#scope), in the form `- **[Title](opinions/<file>.md):** one line on what it covers`. Sort the bullets by title, ignoring case, and keep the unlinked **Libraries** bullet last.
  - An entry in the [Repository layout](README.md#repository-layout) tree, sorted by file name.

  The two orders differ: `architecture.md` is titled "Application architecture". When you rename or remove a file, update both places in the same commit. A skill directory and its row in the [Maintenance via skills](README.md#maintenance-via-skills) table follow the same rule. `scripts/validate-readme-index.cs` fails the pull request if an entry is missing, out of order, or points at a file that doesn't exist.

- **Follow [Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/) for commit messages and pull request titles.** A pull request squash-merges under its title, so the title becomes the commit subject in `git log`. Use the form `<type>[(scope)]: <description>`, in lowercase and the imperative. The scope is optional, as in `chore(deps)` or `docs(verify-project)`.

  | Type       | Use for                                                  |
  | ---------- | -------------------------------------------------------- |
  | `docs`     | Opinion, template, and guidance changes                  |
  | `fix`      | Bug fixes in scripts and workflows                       |
  | `feat`     | New features in scripts and workflows                    |
  | `ci`       | Workflow plumbing                                        |
  | `chore`    | Dependencies and housekeeping                            |
  | `research` | Staging or discarding a research topic (repository-only) |

  A skill that sets its own title says so in its `SKILL.md`, as `research-topic` and `resolve-research` do.

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
- **Agent attribution in conversation.** End each comment an agent writes on an issue or pull request, including replies in review threads, with a line such as `Written by an agent: <harness>, <model>, running <skill>.` Leave out the skill when there isn't one. A reader, or another agent, then knows a person didn't write it.

## Metadata

Three kinds of resource carry this metadata, in the two syntaxes their file formats allow:

| Directory    | `targets` | `last-reviewed` | `last-used` | `sources` | Syntax             |
| ------------ | --------- | --------------- | ----------- | --------- | ------------------ |
| `opinions/`  | Yes       | Yes             | Yes         | Yes       | YAML frontmatter   |
| `templates/` | Yes       | Yes             | Yes         | Yes       | First-line comment |
| `research/`  | Yes       | Yes             | No          | Yes       | YAML frontmatter   |

**`opinions/` and `research/` carry YAML frontmatter:**

```yaml
---
targets: [net10.0, csharp-14, fsharp-10]
last-reviewed: 2026-08-12 # last time content was verified against sources
last-used: 2026-08-12 # last time an agent used this as a reference
sources: [andrew-lock, dotnet-blog] # ids from AWESOME-HUMANS.md, sorted
---
```

A `research/` topic has no `last-used` field and no status field:

- **No `last-used`:** The only way to consult a topic is to build on it, which re-verifies it. The two dates would always move together, so `last-reviewed` is enough.
- **No status:** Promoting or discarding a topic deletes it. A topic on disk is unresolved by definition, so a status could only ever read `open`.

For details, see [research-topic](skills/research-topic/SKILL.md) and [resolve-research](skills/resolve-research/SKILL.md).

**`templates/` carry a first-line comment header.** An XML, INI, or source file can't open with a `---` block and stay valid for the tools that read it. So the same fields go in a comment instead, separated by pipes after a fixed marker:

```xml
<!-- dotnet-awesome-humans template | targets: net10.0 | last-reviewed: 2026-08-12 | last-used: 2026-08-12 | sources: ms-learn -->
```

```ini
# dotnet-awesome-humans template | targets: net10.0 | last-reviewed: 2026-08-12 | last-used: 2026-08-12 | sources: ms-learn
```

```fsharp
// dotnet-awesome-humans template | targets: net10.0, fsharp-10 | last-reviewed: 2026-08-12 | last-used: 2026-08-12 | sources: scott-wlaschin
```

Rules:

- **Update `last-used` whenever you use an opinion or a template:** an opinion you read to decide something, or a template you diffed a project against. The date orders staleness triage: of ten drifted `last-reviewed` dates, review the most-read files first. It also marks candidates for pruning. Only reads _in this repository_ update it, so a person who copies a template or reads an opinion on the web leaves no trace, and the field undercounts. `audit-freshness` treats it as information, never as a finding.
- **Update `last-reviewed` only after you verify the content against its sources.**
- **Use `sources` IDs that exist in `AWESOME-HUMANS.md`,** either in the roster tables or as the reserved `house` ID.
  - CI reads the `**Discovery-only.**` and `**Corroborate.**` markings from a source's section under `## Source notes`, not from its table row.
  - In `opinions/` and `templates/`, the source must also be citable. A watch-list source or a `**Discovery-only.**` source is rejected.
  - A file whose citable sources are all marked `**Corroborate.**` is rejected too, because an unmarked source has to stand beside them.
  - `research/` is exempt: a topic can cite unvetted material if the text flags it. For details, see [research-topic](skills/research-topic/SKILL.md).
- **Sort `sources` by ID in ordinal order, with `house` sorted like any other ID.** Write `[andrew-lock, dotnet-blog, house, ms-learn]`. The rule applies in all three directories and to both syntaxes.
- **Write dates in ISO 8601 (`YYYY-MM-DD`).** Always use absolute dates, never relative ones.
- **Two template files are exempt from the header:** `templates/global.json` and `templates/example.slnf`, because JSON has no comment syntax. What they pin is audited against the latest releases instead. Smoke-testing the example projects leaves build output under `templates/` (`artifacts/`, `bin/`, and `obj/`, all gitignored). That output isn't a resource, and the check skips it.
- **`skills/` are deliberately exempt.** The [Agent Skills specification](https://agentskills.io/specification) defines their frontmatter, they're procedures and not reference material, and nothing reads a date from them. Use `git log` to see when a skill last changed.

CI runs `dotnet run scripts/validate-metadata.cs` against both syntaxes, and a missing field, a relative date, or an unsorted `sources` list fails the pull request.

## Writing style

These rules cover all prose in this repository: opinions, research topics, skills, template comments, and root documents such as this one. They also cover commit messages, pull request titles and descriptions, and the comments an agent posts on issues and pull requests. [Writing style for opinions](#writing-style-for-opinions) adds rules for opinions only.

The voice follows the [Google developer documentation style guide](https://developers.google.com/style): conversational but direct, plain, and easy to scan. Start with its [highlights](https://developers.google.com/style/highlights). The rules below adapt it to this repository. When a question comes up, use [Google's own order of precedence](https://developers.google.com/style#how-to-use):

1. The rules in this section and in [Writing style for opinions](#writing-style-for-opinions).
2. The Google developer documentation style guide.
3. For a technical term that neither settles, the [Microsoft Writing Style Guide](https://learn.microsoft.com/style-guide/welcome/), which Google names as its technical reference and which Microsoft Learn's .NET docs follow.

Apart from spelling, which [Spelling](#spelling) covers, this repository departs from Google's guide in two places:

- **Previews:** Google says not to pre-announce. An opinion can still carry a clearly marked "coming next" note about a .NET preview, as [Maintaining this repository](#maintaining-this-repository) allows.
- **Dates:** Write dates as ISO 8601 (`2026-10-13`), as [Metadata](#metadata) requires. Google accepts any unambiguous format.

### Voice

- **Write for a working .NET developer.** Assume they know C#, the BCL, and the SDK. Don't explain what dependency injection or a `Task` is.
- **Address the reader as "you", and use the imperative for instructions.** Write "Set `<Nullable>` to `enable`", not "The developer should set `<Nullable>`" or "One might enable nullable".
- **Use active voice and present tense.** Write "The analyzer reports `CA1307`", not "`CA1307` will be reported by the analyzer".
- **Use common contractions.** _It's_, _don't_, _isn't_, _can't_, and _you're_ read naturally. Prefer the negative contractions: a reader scanning the page can miss _not_, but rarely misreads _don't_. Avoid rare ones, such as _it'll_ or _should've_.
- **Use plain words.** Write _use_, not _utilize_; _about_, not _with regard to_; _for example_, not _e.g._; _that is_, not _i.e._ Use _can_ for ability and _might_ for possibility. Reserve _must_ for a hard requirement.
- **Drop filler from instructions.** Don't write _please_, _simply_, _easy_, _quickly_, or _let's_, and don't open with a placeholder such as _please note_ or _at this time_. Avoid exclamation marks.
- **Write for a global audience.** Avoid figurative language, idioms, and cultural references. They don't translate, and a reader whose first language isn't English can take them literally. [Stock phrases](#stock-phrases) lists the ones that got in.

### Content

- **Lead with what matters.** Put the recommendation or the action first in a section, a paragraph, and a sentence. Put the reason after it.
- **Put a condition before the instruction it limits.** Write "For details, see [testing.md](opinions/testing.md)", not "See [testing.md](opinions/testing.md) for details", and "To publish a package, run `dotnet pack`", not the reverse. A reader the condition excludes can then skip the instruction. The condition is the only thing that goes ahead of the action. The reason still follows it.
- **Make link text say where the link goes.** Use the target's title, as [Citations and labels](#citations-and-labels) describes. Never use _here_, _this page_, or a bare URL.
- **Make every sentence carry a fact the reader can act on or check.** Name the exact type, member, property, diagnostic ID, and version (`CA1307`, `<InvariantGlobalization>`, .NET 10) instead of describing it. A measured number beats an adjective: "5.5 ms to 0.826 ms", not "much faster".
- **Cut what doesn't add a fact.** Delete a sentence that restates the one before it. Delete one that comments on the text instead of the subject, such as "which is worth noticing" or "the part that matters".
- **Keep sentences short.** Aim for about 20 words. Split any sentence over 35 words at the clause that does separate work.
- **Keep paragraphs short.** Three or four sentences is plenty. Break up a longer one, or turn it into a list.
- **Make it scannable.** Use a numbered list for steps that happen in order, a bulleted list for items that don't, and a table when the same attributes repeat across items. Use sentence-style capitalisation for headings, and keep headings short and specific.
- **Use the serial comma.** Write "opinions, templates, and skills".
- **Write a range with _to_, not an en dash.** Write "22.2% to 22.3%" and "versions 8 to 10". Use a hyphen only where a compound modifier needs one, as in "a long-running service".

### Rhythm

Agents write most of the prose here, and it goes wrong in rhythm more than vocabulary. The well-known tells (_delve_, _leverage_, _seamless_, _robust_, _comprehensive_, _in today's_, a "here is" lead-in, an "in conclusion" wrap-up) don't appear here, so don't hunt for them. Four patterns are fine occasionally and read as machine-written in bulk:

- **Em dashes.** Use at most one per sentence, and only for a real turn. A colon, a comma, or a full stop usually works better.
- **"X, not Y" contrasts.** Keep one when the rejected option is the one a reader would otherwise choose: "Pin to commit SHAs, not mutable tags". Cut it when nobody would choose the alternative: "a build artefact, not an afterthought".
- **Parenthetical asides.** If an aside carries real content, make it a sentence. If it restates the sentence it's attached to, delete it.
- **Lists of three.** A real list can have any length. Don't pad a list to three items because three sounds complete.

### Stock phrases

A few agent idioms did get in. Replace each with the plain statement it stands for: the failure a check catches, the claim an opinion rests on, or the cost a feature repays.

| Avoid                                              | Write instead                                   |
| -------------------------------------------------- | ----------------------------------------------- |
| load-bearing                                       | what depends on it                              |
| belt and braces                                    | the second check, and what it catches           |
| earns its keep, earns its place                    | the benefit, measured                           |
| escape hatch                                       | the setting or API that opts out                |
| bites                                              | the failure, and when it happens                |
| for free                                           | without extra code, or what's actually included |
| reach for                                          | use                                             |
| worth noting                                       | (delete it, and state the fact)                 |
| _honest_, _genuinely_, _quietly_ (as intensifiers) | (delete them)                                   |

A literal use isn't an idiom. _Silently_ for a failure that raises no error stays, and so does _drift_ for two copies that diverge.

### Citations and labels

- **House:** Cite as `Source: Title`, never the reverse. The source comes first, whether it's a person or a publication: `[Toub: Performance Improvements in .NET 10]` and `[Microsoft Learn: Native AOT deployment]`, not `[Native AOT deployment: Microsoft Learn]`.
- **A description-list line uses the same colon:** `**Label:** definition`. Don't use an em dash as the separator.
- **Leave a source's own title as written.** A colon inside a title stays (`[Avalonia 12: Ready for What's Next]`), and an internal reference keeps its section (`[AGENTS.md: Metadata]`).

### Spelling

- **Use British spelling, except where .NET names the word.** Write _defence_, _modelling_, _catalogue_, _behaviour_, and _colour_. This overrides Google's American spelling and its dictionary, Merriam-Webster.
- **Use the platform's spelling for a .NET concept,** so prose and identifiers match and a search for one finds the other. `System.Globalization` and `IStringLocalizer` give _globalization_ and _localization_, down to the file name `opinions/globalization.md`. EF Core's materialization gives _materialized_.
- **Don't extend the exception to words that only share a stem with an API.** "A behaviour change in SQLite" keeps its British spelling, even though MAUI has a `Behavior` type. Never anglicise an API name.
- **`npm run spell` checks spelling.** For details, see [Formatting and linting](#formatting-and-linting). When it flags a correctly spelled name or term, add the word to `cspell.words.txt` in the same commit. Never add an American spelling there. In Markdown, it skips link text, inline code, and double-quoted text, so a cited title or a quotation keeps its spelling. For a title outside those, such as an italic book title, put `<!-- cspell:ignore <word> -->` above the paragraph.

### Scope of these rules

- **They're review guidance, not a build check.** Whether a contrast is justified can't be decided by counting. Read for these patterns in review, and don't add a check that fails a build over them. Spelling is the exception: a misspelled word is wrong whatever its context, so `npm run spell` fails the build over it.
- **They apply only to prose written here.** A third-party title and its URL keep the author's wording and spelling, so Andrew Lock's "Adding Localisation to an ASP.NET Core application" stays as written. Correcting a source misquotes it, and correcting a URL breaks it.
- **They don't apply to other people's code.** `verify-project` reviews a codebase against `opinions/`. Spelling and wording in someone else's identifiers, comments, or documentation is never a finding.

## Writing style for opinions

- **Put the opinion first.** State the recommendation in the first sentence. The rationale follows, and the sources come last.
- **Make it precise enough to apply mechanically.** An agent should be able to turn an opinion into an edit or a `verify-project` finding without interpreting it. A rule that names no setting, type, or threshold isn't finished.
- **Show code when it's clearer than prose.** Keep examples minimal and idiomatic for the declared `targets`. When an opinion replaces an old idiom, show a before-and-after pair. The same applies to `templates/`: example code files are welcome alongside configuration.
- **Make code samples compile against the declared `targets`.**
- **Use a table when the same attributes repeat across items.** Options, versions, or frameworks compared on the same few points read faster as rows than as parallel paragraphs, as in [datetime.md](opinions/datetime.md). Keep the reasoning in sentences: a cell holds a fact, not the argument for it.
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
- **Setup:** A fresh worktree has no gitignored local state. Run `npm install` in it before `npm run check` or `npm run verify`. If your harness needs the skills symlink, re-create it there. For details, see [Skills](#skills).
- **Finishing:** When a worktree's pull request merges, **offer to delete the worktree** with `git worktree remove <dir>` and then `git branch -d <branch>`. Ask first: the branch might still hold work that never reached the pull request.

### Formatting and linting

- Markdown follows GFM and markdownlint, configured in `.markdownlint-cli2.jsonc`. The configuration uses the defaults minus two rules that conflict with the prose style, and records why.
- Spelling follows cspell, configured in `cspell.jsonc` with the `en-GB` dictionary, the .NET dictionaries, and the repository's own word list in `cspell.words.txt`. The .NET dictionaries accept American spellings because API names use them, such as `Behavior` and `Color`, so the configuration flags them in Markdown prose. Editors with a cspell extension read the same configuration.
- Run `npm run format` to format, and `npm run check` to check formatting, lint, and spelling together. All three use the versions pinned in `package.json`. Don't run `npx prettier`, `npx markdownlint-cli2`, or `npx cspell` directly, because they resolve the latest version.
- Run `npm run verify` before you open a pull request. It runs `npm run check` and the scripts in `scripts/` that CI runs, and redraws the diagrams. See [README.md: Repository scripts](README.md#repository-scripts).
- Prettier formats Markdown and the JSON, JSONC, and YAML configuration, and reads `.editorconfig`, so the formatter and your editor agree. `.prettierignore` records two exceptions:
  - `package-lock.json` and `.github/state/dotnet-releases.json` are machine-written. Gating them on formatting would fail a build that nobody can fix by editing.
  - `lychee.toml` isn't formatted. Prettier has no built-in TOML parser, and one hand-written file doesn't justify a third-party plugin.

### Scripts

- CI checks live in `scripts/` as .NET file-based apps. Run them from the repository root with `dotnet run scripts/<name>.cs`. Code that two scripts share goes in a helper file that both `#:include`, not in a copy.
- Entry points use kebab-case names because you type them on a command line. Helper files take the name of the type they contain, following C# file-naming convention.
- Add a check as a script, not as an inline script in a workflow, and hold it to the repository's own opinions: `global.json` pins the SDK, `TreatWarningsAsErrors` is on, the language version is latest, and the `[scripts/*.cs]` block in `.editorconfig` turns the analyzer set into build failures.
- When you add or rename a script, update the [Repository scripts](README.md#repository-scripts) table and the layout tree.
- Renovate bumps `#:package` pins. It reads the directive but only scans `.cs` files under `scripts/`, because `renovate.json` opts that directory into the NuGet manager. A check placed anywhere else wouldn't get bumped.

### Site

- The awesome-humans.net site is generated. `scripts/build-site.cs` builds its `/dotnet/` pages from `README.md` (up to Repository layout), `opinions/`, `templates/`, `skills/`, `AWESOME-HUMANS.md`, and `HOUSE-OPINIONS.md`. Edit those files, never a staged page.
- With `-- --out <dir>`, `build-site.cs` stages only `<dir>/dotnet/`, as the [awesome-humans/website staging contract](https://github.com/awesome-humans/website#staging-contract) defines, with `nav.json`, `sources.json` exported from the roster, and this ecosystem's own `llms.txt`. The website repository adds the pages that every ecosystem shares, and builds and deploys the site.
- The opinion navigation follows [README.md: Scope](README.md#scope), and the skill navigation follows [README.md: Maintenance via skills](README.md#maintenance-via-skills), so a new opinion or skill needs no separate site entry.
- Two workflows hand the site to `awesome-humans/website`. `Site preview` calls its reusable `build.yml` and `deploy.yml` with the pull request's head commit, for a full-site preview at `pr-dotnet-<number>`. Its build fails a pull request on any link or anchor the site can't resolve. `Site dispatch` tells `website` to rebuild and deploy awesome-humans.net when `main` changes.

### Links

The `Markdown` workflow checks links with `lychee.toml`, and a broken link **blocks the pull request**, because a dead citation is a content defect. If a third-party host is down, re-run the job after it recovers. Never merge past a failed link check.
