---
name: follow-opinions
description: Write or change .NET code in a project so that it follows this repository's opinions and templates. Reads the opinions that cover the change before writing, starts new project and build files from the templates, and says afterwards which opinions it applied and where the project's own decisions took precedence. Use when writing, scaffolding, or changing C# or F# code, project files, or CI in a project that follows the Awesome Humans of .NET opinions, or when asked to "follow the opinions" or to write something "the awesome-humans way".
license: See repository LICENSE
compatibility: Requires write access to the project; .NET SDK to build and test what it writes
metadata:
  repo: awesome-humans/dotnet
  change-flow: target-only
---

# Follow the opinions

Apply this repository's opinions and templates while you write or change code in a .NET project. The opinions are the reference, and the project is where you write. **Never write to this repository** from this skill.

This skill changes only what the task asks for. To find where a project's existing code deviates, run `review-project`.

By default, it follows sourced community practice only. If the user or the project names a house profile, such as `simon-mckenna`, it follows that profile's house opinions too. For the profiles, see [HOUSE-OPINIONS.md](../../HOUSE-OPINIONS.md).

## Steps

1. **Work out what the task touches,** before you read an opinion:
   - The languages: C#, F#, or both.
   - Project, solution, and build files, such as a `.csproj`, `Directory.Build.props`, or `global.json`.
   - Tests, and CI workflows.
   - The areas: ASP.NET Core, data access, dates and times, logging, globalization, a UI framework, or code that handles secrets or untrusted input.
2. **Read the project's own decisions,** in its `AGENTS.md`, `README.md`, and contributing guide:
   - **Record the house profile the user or the project names,** or that neither names one. Without one, skip every house block in `opinions/`, and leave the house regions out of anything you copy from `templates/`. With one, keep that profile's blocks and regions, and skip the others.
   - **Record each decision that departs from an opinion,** such as another test framework. A recorded decision takes precedence: follow it, and don't change it back.
3. **Read each opinion that covers the task, in full,** from `opinions/`:
   - The language opinion for each language you write: [csharp.md](../../opinions/csharp.md), [fsharp.md](../../opinions/fsharp.md).
   - [project-structure.md](../../opinions/project-structure.md), when you add or change a project, solution, or build file.
   - Every other opinion whose area step 1 found, such as [testing.md](../../opinions/testing.md) when you write a test.

   Read them each time. An opinion changes with each release, so don't write from what you remember of one. Don't read an opinion the task doesn't touch.

4. **Start a new file from its template.** When the task adds a file that `templates/` holds, such as a project file, `Directory.Packages.props`, or `.editorconfig`, copy the template and trim it. In a file that already exists, change only what the task needs. Don't replace it with the template.
5. **Write the code as the opinions recommend:**
   - **Keep the versions the project targets.** Don't raise a target framework, an SDK version, or a package version unless the task asks. A new project in an empty repository targets the opinions' `targets:`.
   - **Where an opinion gives a recommendation and an exception,** apply the exception only when its stated condition holds.
   - **Where the code around your change follows another convention,** write the new code as the opinion recommends, unless that leaves one file with two styles. Don't rewrite the code around it.
   - **Don't use a feature that an opinion marks as preview or "Coming next".**
   - **Where no opinion covers something,** follow the project's existing convention. Don't present your own preference as an opinion.
6. **Build and test what you wrote,** with the commands the project documents. Fix every warning your change adds: the templates treat warnings as errors.
7. **Report, after the work:**
   - The opinions you read in full, and the templates you copied.
   - Each place where a decision of the project's took precedence over an opinion.
   - Anything you noticed that deviates from an opinion and didn't change, in one line each. For a full comparison, suggest `review-project`.
   - Anything the task needed that no opinion covers, as a possible opinion gap. The user can raise it in an [issue](https://github.com/awesome-humans/dotnet/issues).

## Edge cases

- **The project doesn't follow the opinions yet:** Apply them to the code you write, and don't convert the rest as a side effect. Say so, and suggest `review-project`.
- **The user's instruction conflicts with an opinion:** Follow the user. Say once which opinion recommends otherwise, and why.
- **A house block conflicts with the sourced text beside it,** and the project follows that profile: Follow the house block.
- **F#-only or mixed solutions:** Follow the F# opinions in F# projects. Don't apply a C#-specific convention there.
- **The templates and opinions disagree:** That's a bug in this repository. Follow the opinion text, which takes precedence, and report it against _this_ repository.
- **The project spells differently:** Match the project's spelling in its identifiers, comments, and documentation. The spelling rule in AGENTS.md covers prose written here.
