# Checks: formatting, linting, scripts, links

## Formatting and linting

- Markdown follows GFM and markdownlint, configured in `.markdownlint-cli2.jsonc`. The configuration uses the defaults minus two rules that conflict with the prose style, and records why.
- Spelling follows cspell, configured in `cspell.jsonc` with the `en-GB` dictionary, the .NET dictionaries, and the repository's own word list in `cspell.words.txt`. The .NET dictionaries accept American spellings because API names use them, such as `Behavior` and `Color`, so the configuration flags them in Markdown prose. Editors with a cspell extension read the same configuration.
- Run `just format` to format, and `just check` to check formatting, lint, and spelling together. Each check also has its own recipe: `just format-check`, `just lint`, and `just spell`. The recipes run the scripts in `package.json`, which use the versions pinned there. Don't run `npx prettier`, `npx markdownlint-cli2`, or `npx cspell` directly, because they resolve the latest version.
- `just lint` also audits the workflows for security problems with zizmor, configured in `.github/zizmor.yml`. It runs through `uvx` at the version `.github/requirements.txt` pins, so it needs uv. Without a GitHub token, zizmor skips its online audits and says so. CI passes `github.token` as `GH_TOKEN`.
- Run `just verify` before you open a pull request. It runs `just check` and every other offline check that CI runs, and fails until you commit any diagram it redraws. The link check and the full site build run only in CI. After a change under `templates/`, also run `just build-templates`, which needs nuget.org and so isn't part of `just verify`. See [README.md: Repository scripts](../README.md#repository-scripts).
- Prettier formats Markdown and the JSON, JSONC, and YAML configuration, and reads `.editorconfig`, so the formatter and your editor agree. `.prettierignore` records two exceptions:
  - `package-lock.json` and `.github/state/dotnet-releases.json` are machine-written. Gating them on formatting would fail a build that nobody can fix by editing.
  - `lychee.toml` isn't formatted. Prettier has no built-in TOML parser, and one hand-written file doesn't justify a third-party plugin.

## Scripts

- CI checks live in `scripts/` as .NET file-based apps. Run them from the repository root through their recipes, such as `just validate-metadata`. Code that two scripts share goes in a helper file that both `#:include`, not in a copy.
- The `justfile` is every local and CI entry point. Each check has a recipe named after its script, and CI calls the recipes, never `npm` or `dotnet` directly. `just` with no arguments lists them. The one exception is the website, which doesn't install `just` and runs `dotnet run scripts/build-site.cs` directly. So keep every script runnable on its own, with `dotnet run` or its shebang, even though nothing here calls it that way.
- Entry points use kebab-case names because you type them on a command line. Helper files take the name of the type they contain, following C# file-naming convention.
- Add a check as a script with a recipe, not as an inline script in a workflow, and hold it to the repository's own opinions: `global.json` pins the SDK, `TreatWarningsAsErrors` is on, the language version is latest, and the `[scripts/*.cs]` block in `.editorconfig` turns the analyzer set into build failures.
- When you add or rename a script, add or rename its recipe, and update the [Repository scripts](../README.md#repository-scripts) table and the layout tree.
- Renovate bumps `#:package` pins. It reads the directive but only scans `.cs` files under `scripts/`, because `renovate.json` opts that directory into the NuGet manager. A check placed anywhere else wouldn't get bumped.

## Links

The `Markdown` workflow checks links with `lychee.toml`, and a broken link **blocks the pull request**, because a dead citation is a content defect. If a third-party host is down, re-run the job after it recovers. Never merge past a failed link check.
