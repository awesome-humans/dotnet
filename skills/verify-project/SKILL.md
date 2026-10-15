---
name: verify-project
description: Check an external .NET project or solution, given a local path or a repository URL, against this repository's opinions and templates, and report deviations with severity and suggested fixes. Use when asked to verify, audit, review, or grade a .NET codebase against Awesome Humans of .NET conventions, or to check whether a project "does what good looks like".
license: See repository LICENSE
compatibility: Requires read access to the target project; git and internet access for URL targets; .NET SDK helpful but optional
metadata:
  repo: awesome-humans/dotnet
  change-flow: report-only
---

# Verify project

Compare a target .NET project with this repository's opinions and templates, and report where it deviates. This skill **only reports** unless the user asks for fixes.

## Steps

1. **Resolve the target,** from a local path or a repository URL:
   - **Local path:** Read it in place. Never write to it during the review.
   - **URL:** Read it remotely, with whatever repository browsing the host provides. Clone only when the user asks for a fix (step 9), or when the host can't browse the tree well enough to sample files. A shallow `git clone --depth 1` into a scratch location is a read cache, not a write to the target.
   - **Record what you read:**
     - For a URL, the branch and commit SHA. Without the SHA, nobody can reproduce the review, and step 9 branches from it.
     - For a local git worktree, the SHA. If the tree has uncommitted changes, note that the findings describe the working tree, not the commit.
     - For a local path outside git, say so, and let the file paths stand as the record.
2. **Inventory the target:** solution files (`.slnx`, `.sln`, or `.slnf`), project files, `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, test projects, and the mix of C# and F#.
3. **Update `last-used`** on every opinion and template you read in this run: an opinion's frontmatter, or a template's first-line header, with an ISO 8601 date ([AGENTS.md: Metadata](../../AGENTS.md#metadata)).
4. **Compare structure and tooling** with `templates/`:
   - Required files that are missing, such as `.editorconfig` or Central Package Management.
   - Files that differ. Diff each against its template, and classify each difference as a _violation_ (it contradicts an opinion) or a _local choice_ (no opinion covers it).
5. **Compare versions:** TFMs, `LangVersion`, and the SDK version against the opinions' `targets:`.
   - An older LTS target is a finding under the freshness policy, even if the project has reasons.
   - A `global.json` a band or two behind the newest isn't a finding, because `latestFeature` rolls forward from the minimum. Report only a patch-level pin, a narrower `rollForward`, or a minimum below `targets:` ([project-structure.md](../../opinions/project-structure.md)).
6. **Compare code-level opinions** for the areas the target uses, such as ASP.NET Core, testing, or F#. Sample representative files, and say what you sampled.
7. **Write the report:**
   - Group findings by severity:
     - **Violation:** Contradicts an opinion. Cite the opinion file.
     - **Drift:** Older versions.
     - **Gap:** Missing scaffolding.
     - **Observation:** A local choice worth a look.
   - For each finding, give the path in the target, what the opinion says, and a one-line fix, ideally "copy `templates/<file>` and trim".
   - If the violated opinion is marked `**House:**` (see HOUSE-OPINIONS.md), say so, and grade the finding one level lower. The target can reasonably follow the community default instead.
   - End with a short verdict that reads on its own.
8. **After you present the report, ask in one question whether to save it, and where.** Never save without asking, and never assume a location.
   - Offer `verify-<target-name>-<YYYY-MM-DD>.md` in the directory where the user ran the skill, or any path they name. `<target-name>` is the target repository or solution, so two same-day reviews don't collide.
   - Write one Markdown file with the report's content. Head it with the target's name and the SHA or uncommitted-changes note from step 1.
   - If the user declines, write nothing.

   **Never save the report inside this repository.** It describes someone else's code, not a resource this repository maintains. You can save it into the _target_ if the user asks, but that's a write step 1 ruled out, so it needs the same explicit approval as a fix.

9. **Only if the user asks for fixes:** Apply them in the _target_ on a working branch, never its default branch, named by the host's convention. Do gaps and drift first, because they're mechanical. Make opinionated rewrites only with explicit approval.
   - Clone a URL target now, and branch from **the SHA recorded in step 1**.
   - A depth-1 clone has no earlier commit to branch from. Clone without `--depth 1`, or run `git fetch origin <full SHA>`. An abbreviated SHA is rejected.
   - If you must branch from a default branch that has moved, recheck the files behind each finding, and say which changed. A fix written against moved code produces a confidently wrong diff.

## Edge cases

- **The URL is unreachable, private without credentials, or not a git repository:** Say so and stop. Never verify from README text, a package listing, or memory.
- **You can't push to the target:** Make the branch in the clone, and hand back a diff or patch. Never push to a repository the user doesn't control.
- **The target pins an older .NET for a stated reason,** such as a deployment constraint in its README: Report the drift as acknowledged, not actionable.
- **F#-only or mixed solutions:** Verify against the F# opinions too. Don't report C#-specific conventions as violations in F# projects.
- **The user narrows the review,** such as to testing, one opinion, or one project in a solution: Verify only that scope, and open the report with what you left out, so nobody reads it as a full review.
- **No opinion covers something the target does:** Report it as out of scope, and note it as a possible opinion gap for `harvest-sources`.
- **The target uses different spelling:** Never a finding. The spelling rule in AGENTS.md covers prose written here, not anyone else's identifiers, comments, or documentation.
- **The templates and opinions disagree:** That's a bug in this repository. Report it against _this_ repository, and verify the target against the opinion text, which takes precedence.
