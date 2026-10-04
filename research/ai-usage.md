---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-25
sources:
  [
    aaron-stannard,
    andrew-lock,
    dotnet-blog,
    jetbrains-dotnet,
    mark-seemann,
    jeremy-miller,
    ardalis,
    house,
  ]
---

# AI usage in .NET development

Treat a coding agent as a fast, untrusted contributor, and let the build you already have be what it answers to.
The gates this repository already recommends do most of the work: warnings as errors, nullable warnings as errors, analyzers in the build, pinned packages and a real test suite.
Three things belong on top of them.
Ground the agent in skills and live package data rather than its training data, and audit any skill pack against these opinions before installing it.
Never accept tests the agent wrote after the code as evidence the code works.
Run an agent given unattended permissions inside an isolation boundary.

Since `aaron-stannard` was admitted (#90, 2026-09-25), the roster can carry most of this.
Two unmarked, non-vendor, .NET-specific sources now anchor it: `aaron-stannard` on verifying agent-written code and `andrew-lock` on isolation.
Around them sit vendor writing about vendor tooling (`dotnet-blog`, `jetbrains-dotnet`), and sources whose notes point citations away from their 2026 AI writing (`mark-seemann`, `ardalis`) or mark them **Corroborate.** (`jeremy-miller`).

Scope: this topic covers using agents to write and maintain .NET code.
Building AI features into .NET applications is a separate question, noted at the end.

## What the repository already says

Nothing under `opinions/` is about AI, but three opinions already carry most of the guardrails:

- **[ci.md](../opinions/ci.md)** opens with "Treat supply-chain security as a requirement": SHA-pinned actions, a pinned SDK and pinned packages, and restricting which package assets may run at build time.
- **House:** warnings are errors everywhere, and every suppression carries an inline comment saying why ([ci.md](../opinions/ci.md)). This is local convention, not community consensus, but it's the rule most directly aimed at an agent that silences a warning to get a green build.
- **[csharp.md](../opinions/csharp.md)** makes nullable warnings errors and turns analyzers up to `latest-recommended` with `EnforceCodeStyleInBuild`, so the compiler rejects more of what an agent gets wrong before a reviewer has to catch it.
- **[testing.md](../opinions/testing.md)** holds tests to the same review bar as production code, which is exactly the bar agent-written tests tend to miss (below).

## Verification: the build is the reviewer, and agent-written tests aren't evidence

**Gate agent output on checks it can't talk its way past, and don't count tests the agent wrote after the code.**

- Aaron Stannard's greenfield case study is the most concrete .NET account on the roster. The agent "authored lots of anemic tests that checked the 'has test coverage' box without _really_ testing the functionality thoroughly". What held was `TreatWarningsAsErrors`, which he enabled "because this prevents all sorts of rot from setting into the code base", plus Verify snapshots whose approval files sit in source control. He also enabled the Aspire MCP server so the agent could "click test" the whole application through Playwright, with full OpenTelemetry traces. ([Stannard: Software 2.0: Planning and Verifying a Greenfield Project](https://aaronstannard.com/software-2.0-case-study-textforge/), 2026-03-13, `aaron-stannard`)
- He names the narrow case where he stops reading agent-written code, and fences it off. The mission must be narrow, the output verifiable exhaustively ("Verification tries to be exhaustive by design; testing is meant to be indicative"), and the feedback loop fast. His parser met all three, with a corpus of thousands of real commands regression-tested on every change. For everything else, "you should definitely be reading most LLM-authored code in order to understand what it does", and end-user applications, with "user data, secrets, connections to external systems", are named as the exception. ([Stannard: A Security-Critical Project Where I Don't Read the Code](https://aaronstannard.com/a-security-critical-project-where-i-dont-read-the-code/), 2026-08-10, `aaron-stannard`)
- Mark Seemann gives the reason retrofitted tests fail. LLM-generated tests for code that already exists are "tests as ceremony, rather than tests as an application of the scientific method", because a test's evidential value comes from having seen it fail. His remedies are to break the code, confirm the test fails, then revert. Better still, write the tests first and let the model implement against them. ([Seemann: AI-generated tests as ceremony](https://blog.ploeh.dk/2026/01/26/ai-generated-tests-as-ceremony/), 2026-01-26) In ["The hailo effect"](https://blog.ploeh.dk/2026/04/06/the-hailo-effect/) (2026-04-06) he adds that "LLM friendliness does not entail competency".
  **Limit:** `mark-seemann`'s notes say to cite his C#/.NET posts, and both of these are language-agnostic. Stannard's .NET report of the same failure is what lets the argument be cited.
- Jeremy Miller reports the productive side after two weeks of Claude Code across Marten and Wolverine: "great when you have very detailed compliance test frameworks that the AI tools can use to verify the completion of the work". ([Miller: 2 Weeks of Claude Code for Me](https://jeremydmiller.com/2026/02/14/2-weeks-of-claude-code-for-me/), 2026-02-14, `jeremy-miller`, **Corroborate.**)
  The three don't disagree on substance. All of them put the guardrail in checks the agent didn't write. Where they part is temperament: Miller and Stannard are enthusiastic about throughput, and Seemann distrusts the trust itself.
- Stannard's [dotnet-slopwatch](https://github.com/Aaronontheweb/dotnet-slopwatch) flags the shortcuts an agent takes to get to green: skipped tests, `#pragma warning disable`, `<NoWarn>`, empty `catch` blocks, `Task.Delay` in tests, and inline versions that bypass central package management. **Conflict:** it's his own tool, which his notes flag wherever it is named. It overlaps the **House:** suppression rule with one difference: the house allows a suppression that carries its reason, while slopwatch flags every one against a baseline.

**A direct disagreement with `csharp.md`.**
Stannard found that the `.editorconfig` checks "were largely worthless and burned millions of tokens over 1-2 months on trivial issues like whether or not there was an extra line ending in each `.cs` file. I removed that check entirely."
`csharp.md` recommends `EnforceCodeStyleInBuild`, and `ci.md` runs `dotnet format --verify-no-changes` as its own step.
The two positions can be reconciled without removing the rule: have the agent run `dotnet format`, which fixes whitespace and style mechanically, before it builds, so it never reasons about a formatting diagnostic.
Whether that answers the cost he measured is untested, and whether to adopt it is an owner call. It's the one place this topic would change an existing opinion rather than add to one.

## Skill packs: grounding that needs auditing

**Prefer skills from the platform or package owner, give agents live package data, and read any pack before trusting it.**
A model's .NET knowledge is months stale at best, which is why an unguided agent picks xUnit v2, `Newtonsoft.Json` or a package version that no longer exists.
Skills are the fix, but a skill pack is somebody's opinions in a form the agent will obey, and both packs examined here ship samples that contradict this repository.

- **[dotnet/skills](https://github.com/dotnet/skills)**, from the .NET team: about 100 skills across 16 plugins as of 2026-09-25. Each skill is scored against a no-skill baseline, explicitly not assuming "more context always yields better results" ([.NET Blog: Extend your coding agent with .NET Skills](https://devblogs.microsoft.com/dotnet/extend-your-coding-agent-with-dotnet-skills/), 2026-03-09, `dotnet-blog`). The results are published on a [skill value dashboard](https://dotnet.github.io/skills/) showing activation and not-passed rates per model.
  Three caveats, read from the repository itself. Its only test-authoring skill is `writing-mstest-tests`, and `dotnet-test-migration` converts xUnit to MSTest, which cuts against [testing.md](../opinions/testing.md)'s xUnit v3 choice. Two MSBuild skills still show xUnit `2.7.0` and `2.9.0` in their samples. And its `dotnet11` plugin covers .NET 11 APIs, which are **in preview as of 2026-09-25 and not an opinion**.
- **[Aaronontheweb/dotnet-skills](https://github.com/Aaronontheweb/dotnet-skills)**, cited as `aaron-stannard` for the skills he wrote, and only for their principles, never their samples (see [Vetting dotnet-skills](#vetting-dotnet-skills)). MIT licensed, created 2025-11-12, 1,187 stars, v1.6.0 on 2026-09-16. It has 37 skills and 6 subagents for Claude Code, Codex, Copilot and OpenCode, strongest on Akka.NET, Aspire and testing, which the Microsoft pack barely covers.
  - **Its evaluations are real but small.** [dotnet-skills-evals](https://github.com/Aaronontheweb/dotnet-skills-evals) (2026-02-20) covered the five Akka.NET skills only. With a skill loaded, Sonnet beat its no-skill baseline in 13 of 15 tasks, a mean gain of 1.60 on a 1–5 scale. The judge was also Sonnet, and 15 cases is a signal rather than a result.
  - Two findings from those evals generalise. A terse routing index of about 15 lines in the system prompt beat listing every skill's full description, on every model tested. Cutting skills over 500 lines down to 500 helped the general-knowledge skills and hurt the specialised ones, which argues for a short `SKILL.md` with reference files loaded on demand.
  - **Nothing compiles its samples.** Its own `AGENTS.md` says "There is no build system, tests, or compiled output." Its release notes record fixing "fabricated APIs, compile errors" in the OpenTelemetry skill (v1.4.1) and guidance that wrongly called adding optional parameters binary-compatible. Those corrections are to its credit, and they are also why the table below still finds live defects.
  - **The router snippet it recommends begins "Prefer retrieval-led reasoning over pretraining for any .NET work."** That's the grounding argument in one line, and it applies to this repository's opinions as much as to his skills.
- **Commercial packs are arriving.** JasperFx sells Critter Stack AI Skills for its own libraries ([Miller: Introducing the Critter Stack AI Skills](https://jeremydmiller.com/2026/05/20/introducing-the-critter-stack-ai-skills/), 2026-05-20, `jeremy-miller`, **Corroborate.**, conflict recorded). This shows a trend and recommends nothing.
- **Tool-backed skills measure best.** Across 80 runs, JetBrains' `dottrace-analyze` skill, which hands the agent profiler snapshots, lifted diagnosis accuracy from 4.71 to 8.15 out of 10. It also more than doubled perfect root-cause identifications, from 20 to 48, and in one scenario cut the cost of a run from $3.74 to $2.58 because the agent stopped exploring. ([JetBrains: Your AI Agent Keeps Missing The Real Bottleneck](https://blog.jetbrains.com/dotnet/2026/06/25/performance-profiling-agent-skill-in-rider/), 2026-06-25, `jetbrains-dotnet`) The `refactoring-code` skill applies the same logic, calling Rider's refactoring engine instead of editing text freehand ([JetBrains: Rider Hands AI Agents The Keys To Its Refactoring Engine](https://blog.jetbrains.com/dotnet/2026/08/19/rider-refactoring-code-skill/), 2026-08-19). Both are a vendor measuring its own paid tool once.
- **Live package data:** the NuGet MCP server gives agents current package data, private feeds included, and resolves vulnerable or conflicting dependencies to the lowest compatible safe version ([.NET Blog: Announcing the NuGet MCP Server Preview](https://devblogs.microsoft.com/dotnet/nuget-mcp-server-preview/), 2025-08-14, `dotnet-blog`). [Microsoft Learn](https://learn.microsoft.com/nuget/concepts/nuget-mcp-server) (updated 2026-01-20) documents it as built into Visual Studio 2026, run elsewhere through the .NET 10 SDK's `dnx`. Its coding-agent workflow still installs a preview SDK, and no GA announcement was found, so **its status is ambiguous as of 2026-09-25 and it isn't an opinion**.

### dotnet-skills against this repository's opinions

Its [`csharp-coding-standards`](https://github.com/Aaronontheweb/dotnet-skills/blob/master/skills/csharp-coding-standards/SKILL.md) skill agrees with this repository on direction: nullable enabled, `ArgumentNullException.ThrowIfNull` at boundaries, switch expressions and `TreatWarningsAsErrors`.
The specifics are another matter.
Each row was checked on 2026-09-25, and the three code rows were compiled or run against SDK 10.0.400:

| dotnet-skills says                                                                                                                                                                                   | This repository says                                                                                                                                                | Verified                                                                                                                      |
| ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| Records with `IReadOnlyList<T>` members (`ShoppingCart`, `Order`), and `record` for "domain entities"                                                                                                | [csharp.md: Records](../opinions/csharp.md#records): a collection member breaks value equality                                                                      | Two carts with identical contents compare unequal                                                                             |
| `record EmailAddress` validates in its constructor, but exposes `Value { get; init; }`                                                                                                               | [csharp.md: Records](../opinions/csharp.md#records): `with` doesn't re-run the constructor                                                                          | `new EmailAddress("a@b.c") with { Value = "junk" }` succeeds                                                                  |
| `readonly record struct OrderId(string Value)` plus a validating `OrderId(string value)` constructor                                                                                                 | Code samples must compile ([AGENTS.md](../AGENTS.md#writing-style-for-opinions))                                                                                    | Fails with CS0111: the constructor duplicates the primary one. And `default(OrderId)` skips any validation a struct can hold. |
| xUnit `2.9.x` and FluentAssertions `6.12.0`/`7.0.0` in [`package-management`](https://github.com/Aaronontheweb/dotnet-skills/blob/master/skills/package-management/SKILL.md) and `project-structure` | [testing.md](../opinions/testing.md): xUnit v3 on Microsoft.Testing.Platform. His own team removed FluentAssertions from Akka.NET on 2026-06-10                     | Read from the skill files                                                                                                     |
| `<PackageReference Include="xunit" Version="*" />` in `testcontainers` and `playwright-blazor`                                                                                                       | [ci.md](../opinions/ci.md): CI must not float versions the repository didn't choose. His own slopwatch flags inline versions as a central package management bypass | Read from the skill files                                                                                                     |

The pattern is a pack that states current principles and ships stale or broken examples.
An agent follows the example, not the principle, so a pack's samples are what to audit.
Stale test-package pins turn up in the Microsoft pack too, so this is a property of uncompiled samples rather than of one author.

### Vetting dotnet-skills

`aaron-stannard`'s admission (#90) excluded the repository in a note, without vetting it.
Authored documentation in a repository counts under the admission rules, so the exclusion needed evidence.
The criteria, applied to the repository as a source in its own right:

| Criterion        | Evidence                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Longevity**    | Created 2025-11-12, with 82 commits and v1.6.0 on 2026-09-16. It's active, but under a year old, well short of two.                                                                                                                                                                                                                                                                                                                             |
| **Depth**        | Mixed. `aot-trimming`, `csharp-nullable-reference-types` and the OpenTelemetry suite are reference-grade: the workers found no defects in them, and they cover ground no opinion here does. `csharp-coding-standards`, `package-management` and `project-structure` restate principles over stale examples. It's the only pack examined that publishes evaluations, and they are small.                                                         |
| **Accuracy**     | The blocker. Nothing compiles the samples, and its `AGENTS.md` says so. Verified on SDK 10.0.400: CS0246 in every `testcontainers` sample against the Testcontainers version its `Version="*"` restores, and CS0111 in a `csharp-coding-standards` value object, plus the record and package defects in the tables above. In its favour, its release notes record corrections, including fabricated APIs and a wrong binary-compatibility rule. |
| **Independence** | Stannard's Petabridge conflict carries over: six skills and a subagent cover Akka.NET, and `slopwatch` is his own tool. Authorship is split, though: 68 of 82 commits are his, but the strongest skills came largely from Daniel Marbach, Mauro Servienti and Tomasz Masternak of Particular Software. Admitting the repository would therefore admit bylines nobody has vetted, the blocker recorded for `duende-blog`.                        |

**Decision: folded into `aaron-stannard`**, after a first pass watch-listed it as `dotnet-skills` on the same day.
The track record follows the human, and he has sixteen years of it, so the repository's age was never the right blocker.
The two real concerns become usage limits in his notes rather than a separate id:
the skills Particular Software's engineers wrote aren't citable under his id, and a skill he wrote is cited for the principle it states, never for a sample nobody has compiled.

### Crossover with other opinions

Most of the 37 skills reach past `csharp.md`, so the pack was mapped against every opinion file on 2026-09-25.
Worker agents did the reading. Every conflict below was re-checked against the skill file, and code was compiled where a claim needed it.
Four worker claims failed that check and are left out:

- The Akka.NET `ITimeProvider` registration does compile: it is Akka's own interface, not a misspelt BCL type.
- The `Context.Child(...).GetOrElse(...)` lookup compiles and routes correctly, because `GetOrElse` treats `Nobody` as missing.
- A local tool manifest doesn't contradict `project-structure.md`, whose `dnx` advice is for one-shot tools.
- The Verify package complaint was right, but for a different reason (see the table).

**Across the pack:** six skills ship CI workflows pinned to `dotnet-version: 9.0.x` and none to 10. There are 22 `uses:` lines on mutable action tags and none on a commit SHA, which is exactly what [ci.md](../opinions/ci.md) forbids.

| Opinion                                                  | Skill                                                 | Conflict, verified                                                                                                                                                                                                                                                                                                         |
| -------------------------------------------------------- | ----------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [testing.md: Coverage](../opinions/testing.md#coverage)  | `crap-analysis`                                       | Recommends 80% line and 60% branch thresholds in a `coverage.props` "for CI enforcement". `testing.md`: "do not gate merges on an absolute percentage".                                                                                                                                                                    |
| [testing.md](../opinions/testing.md)                     | `testcontainers`                                      | Every sample uses `new TestcontainersBuilder<TestcontainersContainer>()`, which fails with CS0246 against Testcontainers 4.15.0, the version its own `Version="*"` reference restores. The typed builders replaced it.                                                                                                     |
| [testing.md](../opinions/testing.md)                     | `snapshot-testing`, `verify-email-snapshots`          | `dotnet add package Verify.Xunit`, whose current 31.12.5 depends on `xunit.extensibility.execution` 2.9.3, so it drags xUnit v2 in. xUnit v3 needs `Verify.XunitV3`.                                                                                                                                                       |
| [testing.md](../opinions/testing.md)                     | `aspire-integration-testing`, `akka-testing-patterns` | Reference `Microsoft.NET.Test.Sdk` with `xunit.runner.visualstudio`, the VSTest pairing `testing.md` rules out. The Aspire skill also sleeps with `Task.Delay(2000)`, which the pack's own slopwatch flags.                                                                                                                |
| [logging.md](../opinions/logging.md)                     | `aspire-service-defaults`                             | Presents `AddOpenTelemetry().UseOtlpExporter()` as the whole setup. `logging.md`: in-process OTLP export "batches in memory and has no on-disk queue", so run a collector on localhost.                                                                                                                                    |
| [project-structure.md](../opinions/project-structure.md) | `project-structure`                                   | Its complete `Directory.Build.props` omits `UseArtifactsOutput`, `EnforceCodeStyleInBuild` and nullable-as-errors. Its CPM setup omits `CentralPackageTransitivePinningEnabled`, and its `global.json` has no `test.runner`, all of which [the templates](../templates/) carry. It pins SDK 9.0.200 and `net8.0`/`net9.0`. |
| [architecture.md](../opinions/architecture.md)           | `database-performance`                                | Splits each feature into `IUserReadStore`/`IUserWriteStore` implementations. That's the pattern-named layering `architecture.md` argues against, although the files sit inside a feature folder.                                                                                                                           |
| [ci.md](../opinions/ci.md)                               | `local-tools`                                         | Gates formatting with CSharpier. `ci.md` uses `dotnet format --verify-no-changes`. A choice rather than an error, but an agent following the skill replaces the repository's gate.                                                                                                                                         |

**Agreement is broad where the pack stays on principle.** `aot-trimming` matches [runtime-performance.md: Native AOT](../opinions/runtime-performance.md) and the source-generated JSON advice in `testing.md`.
`csharp-type-design-performance` matches its `ValueTask` and `FrozenDictionary` guidance, and `csharp-concurrency-patterns` its rules on blocking and `ConfigureAwait`.
`efcore-patterns` matches `data-access.md` on `ExecuteUpdateAsync`, `r3-reactive-extensions` matches [datetime.md](../opinions/datetime.md) on `TimeProvider` and `FakeTimeProvider` exactly, and `slopwatch` matches the **House:** suppression rule.

**Gaps the pack covers and the opinions don't.**
These are harvest leads, not guidance. A skill Stannard wrote is citable as `aaron-stannard` for its principle, but several of these came from other authors, and none of the samples is compiled.
Where his blog already covers the ground, the lead is citable today:

- **Citable through his blog:** public API compatibility, which has no opinion file at all. `csharp-api-design` teaches extend-only design, the binary-breaking optional-parameter trap and API approval tests, and the same arguments are in ["Extend-Only Design"](https://aaronstannard.com/extend-only-design/) and ["OSS Compatibility Standards"](https://aaronstannard.com/oss-compatibility-standards/). His ["Stop Failing The `git clone && run` Test"](https://aaronstannard.com/git-clone-and-run/) (2025-10-17) backs the Testcontainers and Aspire first-run material, which no opinion covers.
- **Needs another roster source:**
  - [data-access.md](../opinions/data-access.md) already lists these as unsourced TODOs, and `efcore-patterns` and `database-performance` cover them: tracking defaults, the migrations workflow, `ExecutionStrategy` with transactions inside it, split queries against cartesian explosion, and `IDbContextFactory` for long-lived owners. `aspnet-core.md` applies `AsNoTracking()` per query, where the skill makes no-tracking the global default, so the owner will have to pick one posture.
  - The Options pattern (`ValidateOnStart`, `IValidateOptions<T>`, the `IOptions`/`IOptionsSnapshot`/`IOptionsMonitor` lifetimes) and DI registration conventions, including the scoped-in-singleton trap. No opinion covers either.
  - Metrics: `logging.md` covers logs and traces but never `Meter`, instrument naming or cardinality.
  - Trimming attributes and the IL2xxx/IL3xxx warning families, beyond `runtime-performance.md`'s one paragraph.
  - The escalation path from `async` to `Channel<T>` to Rx to actors, and serialization format choice beyond System.Text.Json.
  - Blazor end-to-end testing detail from `playwright-blazor`, such as waiting on DOM elements rather than network idle, for [ui-frameworks.md](../opinions/ui-frameworks.md).
  - `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false` for test suites that start many hosts, which applies to `WebApplicationFactory` suites as well.

## Isolation and supply chain

- **Run an agent with bypassed permission prompts only inside a microVM.** Andrew Lock recommends Docker Sandboxes because "unlike containers, which share the host kernel, each sandbox has its own kernel". Network traffic goes through a proxy that blocks host access and injects credentials, so the agent never holds them. He pairs each sandbox with its own git worktree, which matches how this repository already works. ([Andrew Lock: Running AI agents safely in a microVM using docker sandbox](https://andrewlock.net/running-ai-agents-safely-in-a-microvm-using-docker-sandbox/), 2026-04-07, `andrew-lock`) The post doesn't cover a .NET SDK or NuGet configuration inside the sandbox.
- **Hallucinated package names are an attack surface.** Across 576,000 samples from 16 models, at least 5.2% of the packages that commercial models recommended didn't exist, and at least 21.7% of those from open-source models, with 205,474 unique invented names. Attackers register the names models repeat ("slopsquatting"). ([USENIX Security 2025: We Have a Package for You!](https://www.usenix.org/conference/usenixsecurity25/presentation/spracklen), **unvetted**) The study covers two languages, neither of them C#, and nothing on the roster yet measures it for NuGet. The mitigations are already in `ci.md`: central package management, no floating versions, and a human choosing each new dependency.

## Cost

**Recorded for the decision, not as guidance: every source here is limited.**
Steve Smith argues that flat-rate agent subscriptions are an "open bar" that agentic usage breaks. He predicts prices 10 to 100 times their January 2026 level by the end of 2027, and advises treating AI tooling as a portfolio bet with the team's own skills kept sharp as a hedge. ([Smith: AI Benefits - But at What Cost?](https://ardalis.com/ai-benefits---but-at-what-cost/), 2026-03-19, `ardalis`, **Corroborate.**, whose notes point citations at its .NET posts)
Stannard makes the same market argument in ["Escaping the LLM Coding Rat Race"](https://aaronstannard.com/escaping-ai-coding-ratrace/) (2026-07-27): "There's only one winner in the AI coding rat race: your inference provider."
He also argues in ["There Has Never Been a Better Time to be a Junior Developer"](https://aaronstannard.com/jr-developer/) (2025-08-22) that subsidised pricing is a closing window.
Both are essays, which `aaron-stannard`'s notes put outside what a citation may rest on.

## Building AI into .NET applications (out of scope here)

Noted so a follow-up topic has somewhere to start, with nothing weighed:

- `Microsoft.Extensions.AI` (`IChatClient`) reached GA on 2025-05-21 ([.NET Blog: AI and Vector Data Extensions are now Generally Available](https://devblogs.microsoft.com/dotnet/ai-vector-data-dotnet-extensions-ga/), `dotnet-blog`).
- Microsoft Agent Framework reached 1.0 on 2026-04-03, bringing together Semantic Kernel and AutoGen ([Microsoft Agent Framework: Microsoft Agent Framework Version 1.0](https://devblogs.microsoft.com/agent-framework/microsoft-agent-framework-version-1-0/), the package's own release post, so a reference).
- The C# MCP SDK and a `dotnet new mcpserver` template are official ([Microsoft Learn: Create a minimal MCP server](https://learn.microsoft.com/dotnet/ai/quickstarts/build-mcp-server)).

## Closing the loop

- **Ready for `resolve-research`.** Stannard's admission closed the gap that made this topic thin. The likely shape is to fold into existing files rather than open a new one:
  - `testing.md`: agent-written tests aren't evidence, citing `aaron-stannard` with `mark-seemann` alongside.
  - `ci.md`: agent isolation (`andrew-lock`), and auditing skill packs as dependencies.
  - `csharp.md`: only if the owner settles the `.editorconfig` question below.
- **Open question for the owner:** keep `EnforceCodeStyleInBuild` and have agents run `dotnet format` first, or accept Stannard's measured cost and relax it for agent loops. This could be a **House:** call.
- **Harvest leads from the crossover map:** the API compatibility material is citable now through `aaron-stannard`'s blog and has no opinion file, so it may deserve a topic of its own. The `data-access.md` TODOs, Options and DI, and metrics each need a roster source first.
- **dotnet-skills is folded into `aaron-stannard`**, limited to the skills he wrote and to their principles; revisit the sample limit if the repository starts compiling its samples.
- **Possible `vet-source` candidate:** Daniel Marbach, who wrote much of the pack's strongest material (AOT and trimming, nullable reference types, OpenTelemetry). Vetting him as a person could reach that material without admitting the repository.
- **Other `vet-source` candidate, unvetted:** Rockford Lhotka ([blog.lhotka.net](https://blog.lhotka.net/)), whose 2026 series on building agents in .NET includes "Tools and Skills: Better Together".
- **Harvest lead:** Oskar Dudycz's "Vibing, Harness and OODA loop" (2026-04-26) wasn't read; `oskar-dudycz` is already citable.
