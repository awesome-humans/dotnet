# Review instructions

These instructions tune automated pull request review in this repository. The agent reviewer (currently CodeRabbit) reads them, and so can any other agent asked to review a pull request here. They add to [AGENTS.md](AGENTS.md), which stays the source of every rule.

This repository is mostly prose. A finding matters when it would put a wrong or unsourced recommendation in front of a reader, or break a check or a skill that keeps the content current.

## What Important means here

Reserve Important for these. Everything else is Nit at most.

- **An unsourced claim.** A new or changed recommendation in `opinions/` or `templates/` that no ID in the file's `sources` carries, or that relies on a link used as a reference to carry it. Quote the claim and name the source IDs the file lists.
- **A misquoted source.** The text says a roster source recommends something that the cited page doesn't say. Quote the sentence from the source, with its link, or don't report it.
- **Guidance from an unvetted source,** folded in instead of proposed through `vet-source`.
- **A house opinion out of place.** A `**House:**` rule outside a `/// house | <profile>` block or a `house:<profile>` region, sourced text edited or deleted to make room for a house opinion, or a block whose profile isn't in `sources`.
- **A downgrade.** An example, template pin, or `targets:` value that moves to an older TFM, SDK, or language version without saying why, or a preview feature presented as the opinion instead of a "coming next" note.
- **An example that can't build.** A code block or template that wouldn't compile against the declared `targets`, with `TreatWarningsAsErrors` on. Cite the line and the diagnostic ID you expect, such as `CS0246`.
- **A broken skill.** A `SKILL.md` step that names a recipe, script, template, branch prefix, or file that doesn't exist, or that contradicts `AGENTS.md`.
- **A wrong check.** In `scripts/`, a check that passes on input it should reject, logic copied between scripts instead of shared through an `#:include` helper, or a script that no longer runs on its own with `dotnet run`. `build-site.cs` is the one the website runs that way.
- **An unsafe workflow.** A `pull_request_target` job that checks out or runs the pull request's code, a secret passed to a step that doesn't need it, or an action pinned to a tag instead of a commit SHA.

## Cap the nits

Report at most five Nits per review, the ones a reader would notice first. Writing-style findings, from `AGENTS.md: Writing style` and the shared page it links, are Nit at most.

## Do not report

- Anything CI already enforces: Prettier, markdownlint, cspell, analyzer warnings in `scripts/`, `just validate-metadata`, `just validate-sources`, `just validate-readme-index`, `just validate-skills`, redrawn diagrams, links, and the site's strict build.
- Generated files: `package-lock.json`, `packages.lock.json`, `.github/state/`, `assets/diagrams/`, and `artifacts/`.
- Wording, spelling, or titles inside a third-party quotation or link.
- An issue the pull request didn't introduce or make worse. Report one as pre-existing only when the changed lines depend on it.

## Always check

- A change to an opinion updates every template that encodes it in the same pull request, and the reverse.
- `last-reviewed` changes only where the content was checked against its sources in the same pull request.
- A transition note opens with `**Review by YYYY-MM-DD.**`, and the date is in the future.
- An opinion gives one recommendation. A second option gets at most one line on why it lost.
- A pull request that changes a skill or lifecycle also updates its diagram in `scripts/export-diagrams.cs`.

## Evidence

Cite `file:line` for every finding about this repository's behaviour, not an inference from a name. A finding about what a source says quotes the source.
