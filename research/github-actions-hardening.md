---
targets: [net10.0]
last-reviewed: 2026-10-17
sources: [dotnet-blog, meziantou]
---

# GitHub Actions hardening: credentials, secrets, and workflow linting

Set `persist-credentials: false` on every `actions/checkout` step unless a later step in the same job runs authenticated `git`.
Pass a reusable workflow only the secrets it uses, not `secrets: inherit`.
Lint the workflows in CI, so a regression fails the pull request instead of waiting for a review to spot it.

All three are sound practice, but the roster can't carry them yet.
No citable source covers credential persistence or secret inheritance, and the one that covers workflow checks recommends a different tool.
The evidence below is the actions' own documentation, which corroborates a claim and never carries one ([awesome-humans/website: Admission criteria](https://github.com/awesome-humans/website/blob/main/pages/conventions/admission-criteria.md#reserved-ids-markings-and-references)).
To promote any of it, either a roster source has to say it, or the owner has to adopt it as a house opinion.

## What `opinions/ci.md` already says

[ci.md](../opinions/ci.md) covers three workflow rules: pin actions to commit SHAs, never interpolate user input into a script, and set `permissions: contents: read` at the top of the workflow.
It says nothing about the token that `actions/checkout` leaves behind, about `secrets: inherit`, or about checking workflows automatically.
Its own sample workflow checks out without `persist-credentials: false`.

## Credential persistence after checkout

**By default, `actions/checkout` keeps the job's token on disk for the rest of the job.**
"The auth token is persisted in the local git config. This enables your scripts to run authenticated git commands. The token is removed during post-job cleanup. Set `persist-credentials: false` to opt-out."
Every major version, v7 included, still defaults the input to `true`.
([actions/checkout README](https://github.com/actions/checkout#readme))

**v6 moved the token, but didn't remove it.**
Since v6.0.0, released on 2025-11-20, "`persist-credentials` now stores credentials in a separate file under `$RUNNER_TEMP` instead of directly in `.git/config`".
([actions/checkout README](https://github.com/actions/checkout#readme), [actions/checkout: v6.0.0](https://github.com/actions/checkout/releases/tag/v6.0.0))

- **What v6 fixed:** Before v6, a step that uploaded the checkout directory as an artifact uploaded `.git/config`, and the token with it. With the token under `$RUNNER_TEMP`, the checkout directory no longer contains it.
- **What v6 didn't fix:** The token is still a file on the runner, and `.git/config` still points at it. Every later step in the job can read it, including third-party actions and build scripts that `dotnet restore` or `dotnet build` runs.
- **Why the default stays `true`:** GitHub "considered disabling `persist-credentials` by default" and rejected it as a breaking change. The v6 change wasn't backported, because that "would risk breaking customers who rely on the credential" being in the local git config. ([GitHub community: Towards a secure by default GitHub Actions](https://github.com/orgs/community/discussions/179107), opened 2025-11-07 by GitHub's Actions product and security teams)

zizmor's `artipacked` audit still reports a v6 or later checkout without the setting, at a lower severity: "even without this, persisting the credential on disk is non-ideal unless actually needed."
Its remediation is the rule above, and it adds one more: "If the persisted credential is needed, it should be made explicit with `persist-credentials: true`."
([zizmor: artipacked](https://docs.zizmor.sh/audits/#artipacked))

**A job that pushes might not need the persisted credential either.**
`peter-evans/create-pull-request` configures its own token from its `token` input, and removes it when it finishes.
It saves any persisted credential before it starts and restores it afterwards, so it works with `persist-credentials: false`.
([peter-evans/create-pull-request: git-config-helper.ts](https://github.com/peter-evans/create-pull-request/blob/main/src/git-config-helper.ts))
Check a pushing action's own documentation or source before you assume it needs the checkout's credential.

## Secrets in reusable workflows

**Forward each secret by name.**
`secrets: inherit` "makes it impossible to determine which exact secrets a reusable workflow was executed with".
Replace it "with a `secrets:` block that explicitly forwards each secret actually needed by the reusable workflow".
([zizmor: secrets-inherit](https://docs.zizmor.sh/audits/#secrets-inherit))

For a reusable workflow you own, declare each secret under `on.workflow_call.secrets`.
Mark a secret `required: false` when the workflow has to run without it, such as for a pull request from a fork, which gets no secrets.
Callers that still use `secrets: inherit` keep working.

**Coming next, in preview or planned as of 2026-10-17, not yet an opinion:** GitHub's 2026 roadmap plans scoped secrets that bind to trusted reusable workflows, so "callers would no longer pass credentials automatically".
It targeted a public preview within three to six months of 2026-03-26. No preview had shipped by 2026-10-17.
([GitHub Blog: What's coming to our GitHub Actions 2026 security roadmap](https://github.blog/news-insights/product-news/whats-coming-to-our-github-actions-2026-security-roadmap/))

## Checking workflows in CI

**The citable source recommends OpenSSF Scorecard.**
The .NET team's write-up on Scorecard for the NuGet ecosystem names read-only workflow tokens as one of the "low-hanging fruit" checks.
It ranks Dangerous-Workflow, which "Does the project avoid dangerous coding patterns in GitHub Action workflows?", as "the most critical security check".
For build and release workflows, it says to "pin dependencies by hash".
In over 250 of the .NET repositories it analysed, Dangerous-Workflow returned an error instead of a score.
([.NET Blog: OpenSSF Scorecard for .NET and the NuGet ecosystem](https://devblogs.microsoft.com/dotnet/openssf-scorecard-for-net-nuget/), 2024-11-04)

Scorecard scores a repository. It isn't a pull request gate for workflow mistakes.
Two tools are, and both run offline against the workflow files:

- **actionlint** checks syntax, expressions, and job wiring, and runs shellcheck on each `run:` script. ([actionlint](https://github.com/rhysd/actionlint))
- **zizmor** audits for security problems, including the two above, script injection, dangerous triggers, and unpinned actions. Given a token, its online audits also look up known-vulnerable action versions and commits that aren't on the action's repository. ([zizmor: audits](https://docs.zizmor.sh/audits/))

The two can disagree. zizmor 1.31.1's `self-repository` audit asks for GitHub's `$/` form of a same-repository `uses:`, which GitHub added in July 2026. actionlint 1.7.12, the latest release on 2026-10-17, rejects that form.
A repository that runs both has to turn one rule off until actionlint catches up.

Both are references for their own behaviour here, not for the practice.
Meziantou's posts already in ci.md cover two of zizmor's audits: SHA pinning, and passing input through environment variables to avoid script injection.
([Meziantou: Safely passing extra arguments in GitHub Actions workflows using PowerShell](https://www.meziantou.net/safely-passing-extra-arguments-in-github-actions-workflows-using-powershell.htm))
Neither he nor any other roster source recommends a workflow linter.

## Two changes GitHub has shipped since ci.md was reviewed

**`actions/checkout` v7 refuses fork code under `pull_request_target`.**
By default, v7 fails a checkout of a fork pull request's head or merge commit in a `pull_request_target` workflow, or a `workflow_run` workflow that a pull request triggered.
Those triggers run with the base repository's token and secrets, which is the "pwn request" pattern.
The `allow-unsafe-pr-checkout: true` input opts back in.
GitHub backported the block to every supported major version, enforced from 2026-07-20. A floating tag such as `@v4` picked it up, but a SHA pin didn't.
A `run:` step that fetches the fork's code with `git` or `gh` isn't blocked.
([GitHub Changelog: Safer pull_request_target defaults for GitHub Actions checkout](https://github.blog/changelog/2026-06-18-safer-pull_request_target-defaults-for-github-actions-checkout/), [actions/checkout: v7.0.0](https://github.com/actions/checkout/releases/tag/v7.0.0))

**Coming next, in preview as of 2026-10-14, not yet an opinion: workflow lockfiles.**
`.github/workflows/actions.lock` records commits for "actions and their transitive dependencies, as well as reusable workflow dependencies", so an onboarded workflow runs the locked commits even if a tag moves.
The `gh actions-lock` CLI extension writes it, and a "Require lockfile" policy can enforce it.
Dependabot regenerates the matching entry in its version update pull request. The announcement doesn't mention Renovate, which [ci.md](../opinions/ci.md) recommends.
([GitHub Changelog: GitHub Actions workflow lockfiles in public preview](https://github.blog/changelog/2026-10-14-github-actions-workflow-lockfiles-in-public-preview/))
SHA pinning covers only the `uses:` lines in your own files, so this would close the transitive gap that ci.md's pinning rule leaves open.
Revisit when it reaches GA, and check Renovate support first.

## What this reveals about the repository

This repository's own workflows break the first two rules.
actionlint 1.7.12 reports nothing. zizmor 1.31.1 reports:

| Workflow                   | Finding                                                           | Fix                                                                                                                                                       |
| -------------------------- | ----------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `dotnet-release-watch.yml` | Checkout keeps its credential                                     | `persist-credentials: false`. `create-pull-request` brings its own token                                                                                  |
| `markdown.yml`             | Both checkouts keep their credential                              | `persist-credentials: false`                                                                                                                              |
| `validate-skills.yml`      | Checkout keeps its credential                                     | `persist-credentials: false`                                                                                                                              |
| `site-preview.yml`         | `secrets: inherit` into website's `deploy.yml`                    | Forward `CLOUDFLARE_API_TOKEN` and `CLOUDFLARE_ACCOUNT_ID`. [website#33](https://github.com/awesome-humans/website/pull/33) declared both on `deploy.yml` |
| `site-preview.yml`         | Two `uses:` of website's reusable workflows at `@main`, not a SHA | Intended: a preview builds with website's current build. Suppress with the reason, rather than pin                                                        |

Four gaps to resolve:

1. **[ci.md](../opinions/ci.md) has no rule on credential persistence or secret inheritance,** and its sample workflow omits `persist-credentials: false`. No roster source carries either rule. Adopting them as house opinions, through `weave-house-opinion`, is the route that exists today. awesome-humans/website already follows both.
2. **ci.md doesn't recommend checking workflows automatically.** The citable source names Scorecard. actionlint and zizmor are the pull request gates, but no roster source names them. This is also a house opinion, or it waits for a source.
3. **The workflow fixes in the table are a `ci:` change to this repository,** not an opinion change. They can go ahead now, whatever happens to items 1 and 2, as website's did.
4. **Two later notes for ci.md:** the v7 `pull_request_target` block belongs beside the script-injection rule once a roster source covers it, and workflow lockfiles belong in a "coming next" note until they reach GA.

**Recommended next step:** `weave-house-opinion` for items 1 and 2, if the owner adopts them. Then `resolve-research` to promote the house opinions into ci.md, fix its sample workflow, and discard the rest.

**Source sweep note:** Searches of `meziantou`, `andrew-lock`, `dotnet-blog`, `damien-bowden`, `steve-gordon`, and the watch-list `barry-dorrans` found nothing on credential persistence, secret inheritance, or workflow linters.
Andrew Lock's Actions posts cover trusted publishing and SBOM attestations, which ci.md already uses.
The roster has no GitHub Actions security source, so no `vet-source` candidate came out of this sweep. The tools' own documentation and GitHub's announcements carried every claim.
