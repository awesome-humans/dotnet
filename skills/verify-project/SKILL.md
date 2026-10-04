---
name: verify-project
description: Check an external .NET project or solution, given a local path or a repository URL, against this repository's opinions and templates, and report deviations with severity and suggested fixes. Use when asked to verify, audit, review, or grade a .NET codebase against dotnet-awesome-humans conventions, or to check whether a project "does what good looks like".
license: See repository LICENSE
compatibility: Requires read access to the target project; git and internet access for URL targets; .NET SDK helpful but optional
metadata:
  repo: dotnet-awesome-humans
  change-flow: report-only
---

# Verify project

Compare a target .NET project with this repository's opinions and `templates/` files, and report where it deviates. This skill **only reports by default**. It applies fixes only when the user explicitly asks.

## Steps

1. **Resolve the target,** from a local path or a repository URL:
   - **Local path:** Read it in place. Never write to it during the review.
   - **URL:** Read it remotely, with whatever repository browsing the host provides. Cloning is for _writing_: clone only when the user asks for a fix (step 9), or when the host can't browse the tree well enough to sample files. In that case, a shallow `git clone --depth 1` into a scratch location is a local read cache, not a write to the target.
   - **Record what you read.**
     - For a URL, record the branch and commit SHA. A review without a SHA can't be reproduced, and step 9 branches from it.
     - For a local path in a git worktree, record the SHA. If the tree has uncommitted changes, note that the findings describe the working tree, not the commit.
     - If the local path isn't a git checkout, say so, and let the file paths stand as the record.
2. **Inventory the target:** solution files (`.slnx`, `.sln`, or `.slnf`), project files, `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, test projects, and the language mix of C# and F#.
3. **Update `last-used`** on every opinion and template file you consult in this run: the YAML frontmatter on an opinion, or the first-line comment header on a template. Always use an ISO 8601 date, as described in [AGENTS.md: Metadata](../../AGENTS.md#metadata). This is how the repository knows it's being used.
4. **Compare structure and tooling** with `templates/`:
   - Files that the opinions require but are missing, such as no `.editorconfig`, or no Central Package Management.
   - Files that exist but differ. Diff each one against the template, and classify each difference as a _violation_ (it contradicts an opinion) or a _local choice_ (the opinions don't cover it).
5. **Compare versions:** TFMs, `LangVersion`, and the SDK version, against the `targets:` in the opinions.
   - An older LTS target is a finding under the freshness policy. Report it even if the project has reasons.
   - A `global.json` that's a feature band or two behind the newest isn't a finding. The pin is a minimum that `latestFeature` rolls forward from. Only report a patch-level pin, a narrower `rollForward`, or a minimum below the declared `targets:` ([project-structure.md](../../opinions/project-structure.md)).
6. **Compare code-level opinions** for the areas the target uses, such as ASP.NET Core, testing, or F#. Sample representative files instead of reading everything, and say what you sampled.
7. **Write the report:**
   - Group findings by severity:
     - **Violation:** Contradicts an opinion. Cite the opinion file.
     - **Drift:** Older versions.
     - **Gap:** Missing scaffolding.
     - **Observation:** Local choices worth a look.
   - For each finding, give the file or path in the target, what the opinion says, and a one-line fix, ideally "copy `templates/<file>` and trim".
   - If the violated opinion is marked `**House:**` (see HOUSE-OPINIONS.md), say so. The target might reasonably follow the community default instead of this repository's local convention, so grade those findings one level lower.
   - End with a short verdict paragraph that a person can read on its own.
8. **Ask whether to save the report, and where.** After you present the report, ask in one question. Never save silently, and never assume a location.
   - Offer a default of `verify-<target-name>-<YYYY-MM-DD>.md` in the directory where the user ran the skill, and accept any path they name instead. `<target-name>` is the target repository or solution name, so two reviews of different projects on the same day don't collide.
   - Write it as a single Markdown file with the same content as the report in the conversation. Start it with a heading that names the target, and the SHA or uncommitted-changes note from step 1, so the file makes sense on its own.
   - If the user declines, keep the report in the conversation only, and write nothing.

   **Never save the report inside this repository.** It describes someone else's code, and `opinions/`, `templates/`, and `research/` are all the wrong place for it: it's an output about a target, not a resource this repository maintains. You can save it into the _target_ if the user asks. But that's a write to a tree that step 1 promised not to touch, so it needs the same explicit approval as a fix.

9. **Only if the user asks for fixes:** Apply the changes in the _target_ project on a working branch, never its default branch. Follow the host environment's branch naming convention. Start with gaps and drift: mechanical fixes first, and opinionated rewrites only with explicit approval.
   - Clone a URL target that you reviewed remotely at this point, and start the branch from **the SHA recorded in step 1**.
   - A depth-1 clone has no earlier commit to branch from. Clone without `--depth 1`, or run `git fetch origin <full SHA>` to deepen the shallow cache. An abbreviated SHA is rejected there.
   - If the branch must start from a default branch that has moved since, re-check the files behind each finding, and say which ones changed. Fixes written against code that has moved are the one way this skill can produce a confidently wrong diff.

## Edge cases

- **The URL is unreachable, private without credentials, or not a git repository:** Say so and stop. Never verify from README text, a package listing, or what you remember about the project.
- **You can't push to the target** because it's someone else's repository: Make the branch in the clone, and hand back a diff or patch. Never push to a repository the user doesn't control.
- **The target pins an older .NET for a stated reason,** such as a deployment constraint documented in its README: Still report the drift, but mark it as acknowledged, not actionable.
- **F#-only or mixed solutions:** Verify against the F# opinions too. Don't report C#-specific conventions as violations in F# projects.
- **No opinion covers something the target does:** Report it as out of scope, and note it as a possible opinion gap for `harvest-sources`.
- **The target uses different spelling:** That's never a finding, either way. This repository's spelling rule (AGENTS.md) covers prose written here, not anyone else's code. British or American spelling in identifiers, comments, or documentation is the target's own business. Reporting it buries the real findings under noise that the author didn't ask for.
- **The templates and opinions disagree,** which is a bug in this repository: Report the inconsistency against _this_ repository, and verify the target against the opinion text, which takes precedence.
