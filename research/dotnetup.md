---
targets: [net10.0]
last-reviewed: 2026-11-13
sources: [dotnet-blog, ms-learn]
---

# dotnetup: a user-level SDK and runtime manager

**In preview as of 2026-11-13, not yet an opinion.**
Add one "Coming next" note to [project-structure.md](../opinions/project-structure.md), and change nothing else.
Keep `actions/setup-dotnet` in CI, and keep `sdk.paths` with `dotnet-install` for preview SDKs.

At GA, the hypothetical opinion is: install the SDK on a development machine with `dotnetup sdk install` from the repository root, so the machine tracks the channel that `global.json` implies, and run `dotnetup update` after each Patch Tuesday.

## Maturity

Every dotnetup page on Microsoft Learn opens with the same note: "`dotnetup` is in public preview. Its features and behavior might change before general availability."
The overview adds that "the support lifecycle for the generally available release isn't final".
([Microsoft Learn: dotnetup overview](https://learn.microsoft.com/dotnet/core/tools/dotnetup/), reviewed 2026-10-13)

The public preview started on 2026-10-15. ([Husk: Introducing dotnetup: A toolchain manager for .NET](https://devblogs.microsoft.com/dotnet/introducing-dotnetup/))
The .NET 11 announcement of 2026-11-10 still says "It’s currently in public preview", so dotnetup didn't reach GA with .NET 11. ([.NET Blog: Announcing .NET 11](https://devblogs.microsoft.com/dotnet/announcing-dotnet-11/))
No roster source gives a GA date.

Microsoft Learn's own advice follows from that: "For production machines, use the .NET installers, a package manager, or Visual Studio." ([Microsoft Learn: When to use dotnetup](https://learn.microsoft.com/dotnet/core/tools/dotnetup/choose-dotnetup), reviewed 2026-09-30)

## What it changes: the patch gap on a development machine

A user-level SDK installed with `dotnet-install` stays on its first version. Microsoft Learn compares the three installation methods:

| Capability                   | dotnetup                    | Installers and package managers | `dotnet-install` scripts    |
| ---------------------------- | --------------------------- | ------------------------------- | --------------------------- |
| Scope                        | User                        | Machine                         | Any folder                  |
| Tracks installations         | Yes                         | Yes, through the OS             | No                          |
| Updates installations        | Yes, with `dotnetup update` | Depends on the OS and method    | "No. Run the script again." |
| Removes unused installations | Yes                         | Yes, through the OS             | No                          |
| Reads `global.json`          | Yes                         | No                              | Yes, with `--jsonfile`      |
| Support status               | Public preview              | Generally available             | Generally available         |

([Microsoft Learn: When to use dotnetup](https://learn.microsoft.com/dotnet/core/tools/dotnetup/choose-dotnetup), reviewed 2026-09-30)

The scripts were never meant for a development machine. Their reference page says: "To set up a development environment or to run apps, use the installers rather than these scripts." ([Microsoft Learn: dotnet-install scripts reference](https://learn.microsoft.com/dotnet/core/tools/dotnet-install-script), reviewed 2024-01-15)
So the GA answer today is an installer or a package manager, and the repository has no opinion that says so.

dotnetup stores what you asked for separately from what it installed. A channel such as `10.0` or `10.0.4xx` is a standing request, and `dotnetup update` resolves it again. An exact version such as `10.0.403` is pinned, and an update skips it. After a successful update, "garbage collection removes versions that no remaining specification needs". ([Microsoft Learn: dotnetup channels and versions](https://learn.microsoft.com/dotnet/core/tools/dotnetup/concepts/channels), reviewed 2026-11-10)

dotnetup also verifies what it downloads. Husk lists "signature and checksum validation of the installed SDKs and Runtimes" as something the scripts don't do. ([Husk: Introducing dotnetup: A toolchain manager for .NET](https://devblogs.microsoft.com/dotnet/introducing-dotnetup/), 2026-10-15)

It doesn't replace [security.md: Servicing](../opinions/security.md#servicing-take-the-monthly-patch). The same post says: "Updating your machine does not patch applications you have already shipped." The rebuild-and-redeploy rule stays as written.

## How it reads `global.json`

`dotnetup sdk install` with no channel reads the nearest `global.json`, and turns `sdk.version` and `rollForward` into a channel. For version `10.0.103`:

| `rollForward`                                   | Stored channel   |
| ----------------------------------------------- | ---------------- |
| Omitted or `latestPatch`                        | `10.0.1xx`       |
| `latestFeature`                                 | `10.0`           |
| `latestMinor`                                   | `10`             |
| `latestMajor`                                   | `latest`         |
| `disable`, `patch`, `feature`, `minor`, `major` | Exact `10.0.103` |

([Microsoft Learn: Install an SDK for a repository with dotnetup](https://learn.microsoft.com/dotnet/core/tools/dotnetup/usecases/install-with-global-json), reviewed 2026-08-07)

[templates/global.json](../templates/global.json) pins `10.0.400` with `latestFeature`, so it maps to the `10.0` channel: the newest .NET 10 SDK in any band. That matches what the host accepts under `latestFeature`.

The preview has four limits that affect this repository's opinions:

- **A `global.json` with comments fails.** "If the nearest file is malformed or contains comments, parsing fails." ([Microsoft Learn: Repository SDK requirements with dotnetup](https://learn.microsoft.com/dotnet/core/tools/dotnetup/concepts/repositories), reviewed 2026-08-07) The .NET host accepts them: "Comments in global.json files are supported using JavaScript or C# style comments." ([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json), reviewed 2026-03-05) The two pages disagree, and the dotnetup page describes a preview build, so treat it as a preview defect, not as a rule.
- **`$host$` isn't understood.** In `sdk.paths`, dotnetup "doesn't interpret `$host$` as a sentinel or skip empty entries". It installs into the first entry, so `[".dotnet", "$host$"]` works and `["$host$", ".dotnet"]` doesn't.
- **`allowPrerelease` is ignored** when dotnetup derives the channel.
- **An edit to `global.json` isn't picked up by `dotnetup update` alone.** Run `dotnetup sdk install` in the repository after you change `sdk.version` or `rollForward`.

([Microsoft Learn: Install an SDK for a repository with dotnetup](https://learn.microsoft.com/dotnet/core/tools/dotnetup/usecases/install-with-global-json), reviewed 2026-08-07)

**Don't use `--update-global-json`.** It rewrites `sdk.version` to the version dotnetup installed. project-structure.md says to treat the pinned version as a minimum and to raise it only for a feature you need, and this option raises it every month. The conclusion is this topic's own reading of the two, so it's **unsupported** until a roster source says the same.

## The five questions

### 1. Preview SDKs: keep `sdk.paths` with `dotnet-install`

`sdk.paths` and dotnetup don't overlap. `sdk.paths` tells the host where to look, and it's GA in .NET 10. dotnetup is one way to put an SDK in that folder, and it's in preview.

Microsoft Learn's page on testing prerelease SDKs still says: "Use the official dotnet-install scripts to download a prerelease SDK into a project-local directory." ([Microsoft Learn: Test prerelease .NET SDKs locally with global.json paths](https://learn.microsoft.com/dotnet/core/tools/test-prerelease-sdk-locally), reviewed 2026-04-08) That page doesn't mention dotnetup, so the bullet in project-structure.md matches the current documentation.

When dotnetup reaches GA, the hypothetical change is small: the bullet's install command and its `errorMessage` become `dotnetup sdk install`, which reads the version and the first `sdk.paths` entry from the same file. How dotnetup maps a prerelease `sdk.version` to a channel isn't documented, so check it then.

### 2. Development machines: a "Coming next" note, with the migration

The note can say that dotnetup tracks a channel and updates it, and that it's in preview. The migration from a `dotnet-install` SDK has these points:

- **The default root isn't `~/.dotnet`.** On Linux, it's `$XDG_DATA_HOME/dotnetup/dotnet`, or `~/.local/share/dotnetup/dotnet`. On macOS, it's `~/Library/Application Support/dotnetup/dotnet`, and on Windows, `%LOCALAPPDATA%\dotnetup\dotnet`. So the default install doesn't touch an existing `~/.dotnet` SDK.
- **A tracked install refuses a folder that holds another install.** "To prevent accidental mixing, a tracked install fails if the target contains an existing .NET installation that is not in the selected manifest." It matters only if you pass `--install-path ~/.dotnet`.

  ([Microsoft Learn: How dotnetup works](https://learn.microsoft.com/dotnet/core/tools/dotnetup/concepts/how-dotnetup-works), reviewed 2026-10-13)

- **`PATH` and `DOTNET_ROOT` have to point at the new root.** `dotnetup env set shell` writes both to the shell profile. Framework-dependent executables use `DOTNET_ROOT` to find the runtime. ([Microsoft Learn: dotnetup environment configuration](https://learn.microsoft.com/dotnet/core/tools/dotnetup/concepts/environment), reviewed 2026-08-11) Removing the old `~/.dotnet` lines from the profile is this topic's own step, and no page lists it: **unsupported**.
- **Global tools stay in `~/.dotnet/tools`.** The tool path depends on the operating system, not on the install root: `$HOME/.dotnet/tools` on Linux and macOS. ([Microsoft Learn: How to manage .NET tools](https://learn.microsoft.com/dotnet/core/tools/global-tools), reviewed 2025-10-22) No dotnetup page mentions tools. That they keep working after the move, because `DOTNET_ROOT` leads them to a runtime, is **unsupported**.
- **On Windows, choose the `shell` access mode, or migrate.** The default there is `everywhere`, which needs elevation. The page says it "Makes the dotnetup-managed installation take precedence over machine-wide .NET installations, such as the ones that Visual Studio installs". A project that needs an SDK only Visual Studio installed then fails to build. ([Microsoft Learn: Get started with dotnetup](https://learn.microsoft.com/dotnet/core/tools/dotnetup/getting-started), reviewed 2026-10-13) For a Visual Studio user, Microsoft Learn recommends against dotnetup altogether: "Visual Studio installs and services its own machine-wide copy of .NET."
- **Track the repository's channel, not `latest` alone.** `latest` "follows the latest stable SDK, including new major releases". ([Husk: Introducing dotnetup: A toolchain manager for .NET](https://devblogs.microsoft.com/dotnet/introducing-dotnetup/), 2026-10-15) `latestFeature` accepts only "the requested major and minor". ([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json), reviewed 2026-03-05) So a machine that tracks only `latest` moved to SDK 11.0.100 on 2026-11-10, and garbage collection can then remove the .NET 10 SDK that this repository's `global.json` needs. Each source states one half, and the combined failure is **unsupported**: nobody reproduced it here.

One observation, **unsupported** because it's a single machine: on 2026-11-13, dotnetup 0.3.0-preview.1 on Linux listed a `10.0` channel whose source was a `global.json` with `10.0.400` and `latestFeature`, resolved to SDK 10.0.404. That agrees with the mapping table.

### 3. CI: `actions/setup-dotnet` stays the opinion

The sources disagree. Husk answers "Can I use dotnetup in CI pipelines?" with "Absolutely". Microsoft Learn says to use a GA method on production machines. The preview status decides it, because [AGENTS.md](../AGENTS.md) doesn't let a preview become the opinion.

Two more reasons hold even at GA, until the workflow changes:

- **The documented workflow runs an unpinned script.** Its install step is `curl -fsSL https://aka.ms/dotnetup/get-dotnetup.sh | bash`. ([Microsoft Learn: Use dotnetup in automation](https://learn.microsoft.com/dotnet/core/tools/dotnetup/usecases/automation), reviewed 2026-09-30) [ci.md](../opinions/ci.md) pins every action to a commit SHA, and a script fetched through a redirect has no pin. The comparison is this topic's own: **unsupported**.
- **Later steps don't see the SDK.** "Other steps that run `dotnet` directly don't use the dotnetup-managed installation." Each step has to call `dotnetup dotnet`, or the job has to set `PATH` itself.

The plan is for the action to adopt dotnetup, not for workflows to call it. Husk wants to "integrate dotnetup into CI/CD systems like GitHub Actions and Azure DevOps – ideally through replacing parts of the implementations of those tools today that rely on the dotnet-install scripts". If that ships, a workflow that uses `setup-dotnet` with `global-json-file` changes nothing.

`actions/setup-dotnet` already resolves the `latest*` policies from `global.json`. ([actions/setup-dotnet: README](https://github.com/actions/setup-dotnet#using-the-global-json-file-input), a reference, read 2026-11-13)

### 4. Telemetry: one clause, not a rule

"Telemetry is on by default. To opt out, set the `DOTNET_CLI_TELEMETRY_OPTOUT` environment variable to `1` or `true`." It's the variable that turns off the SDK's own telemetry, so a machine that already opts out needs nothing new. The data includes the command, the operating system, whether dotnetup runs in CI, and "Whether `dotnetup` runs from an LLM agent, such as GitHub Copilot". ([Microsoft Learn: dotnetup telemetry](https://learn.microsoft.com/dotnet/core/tools/dotnetup/telemetry), reviewed 2026-09-30)

No opinion file mentions SDK telemetry today, so the note names the variable in a clause and stops there.

### 5. GA: what marks it ready, and the review date

Promote it when all of these hold:

1. The Microsoft Learn pages drop the public-preview note, and the "Support status" row for dotnetup reads "Generally available".
2. Microsoft Learn publishes the support lifecycle that the overview says "isn't final".
3. The download script installs a stable build by default. Today, "The download scripts install the latest `preview` build by default." ([Microsoft Learn: Get started with dotnetup](https://learn.microsoft.com/dotnet/core/tools/dotnetup/getting-started), reviewed 2026-10-13)
4. The .NET Blog announces GA.

Then re-check the four limits under [How it reads `global.json`](#how-it-reads-globaljson), because each is described as a property of the current preview build.

No source gives a date, so **Review by 2027-05-31** is this topic's choice, about six months out, and **unsupported**.

## A draft of the note

For a new "Coming next (preview, not yet the opinion)" section at the end of project-structure.md:

> **Review by 2027-05-31.** dotnetup is Microsoft's user-level SDK and runtime manager, in public preview since 2026-10-15. `dotnetup sdk install` reads the nearest `global.json` and tracks the channel it implies, which is `10.0` for `10.0.400` with `latestFeature`, and `dotnetup update` moves that channel to the newest SDK after each servicing release. A `dotnet-install` SDK never updates. Until dotnetup reaches GA, install the SDK on a development machine with an installer or a package manager, and keep `actions/setup-dotnet` in CI. dotnetup sends telemetry unless `DOTNET_CLI_TELEMETRY_OPTOUT` is `1`. Drop this note when dotnetup reaches GA and becomes the opinion.

Apart from the review date, every sentence in it has a citable source above. The migration points under question 2 stay out of the note until dotnetup is the opinion, because three of them are unsupported.

## What this reveals about the repository

1. **[project-structure.md](../opinions/project-structure.md) says how to pin the SDK, and never how to install it.** Microsoft Learn's GA answer for a development machine is an installer or a package manager, not `dotnet-install`. The note above states it, and the opinion could state it without dotnetup.
2. **The preview-SDK bullet is current.** It matches Microsoft Learn's page, and its `[".dotnet", "$host$"]` order is the one dotnetup's preview can read. [ui-frameworks.md](../opinions/ui-frameworks.md) points at the same bullet and needs no change.
3. **The bullet's example version is out of date for another reason.** It uses `11.0.100-preview.1.25000.1`, and .NET 11 reached GA on 2026-11-10. That belongs to `refresh-dotnet-versions`, not to this topic.
4. **A comment header on `templates/global.json` would break dotnetup's preview.** [global-json-roll-forward.md](global-json-roll-forward.md) notes that `global.json` allows comments, against the reason [AGENTS.md](../AGENTS.md) gives for exempting the file. Keep the exemption until dotnetup parses comments.

**Recommended next step:** `resolve-research`, to partly promote this topic: the "Coming next" note now, and the rest when dotnetup reaches GA.

**`vet-source` candidates:** None. The one page from outside the roster, [Start Debugging: dotnetup: .NET Finally Gets a rustup-Style SDK Version Manager](https://startdebugging.net/2026/06/dotnetup-official-dotnet-sdk-version-manager/) (**unvetted**, 2026-06-22), describes an internal preview that the public one has replaced, and no claim here rests on it.

**How this was checked:** Each quotation was matched as a string against the page's text, fetched with `curl` on 2026-11-13. The Microsoft Learn dates are each page's `ms.date`. The topic has no code to compile and recommends no package. Apart from `dotnetup list` and `dotnetup env show` for the observation under question 2, no dotnetup command here was run.
