# Awesome humans

These are the vetted sources behind every opinion in this repository. An opinion can use content only if it traces to a source listed here, or carries the reserved `house` ID described below.

To propose a source, [suggest it for vetting](https://github.com/awesome-humans/website/issues/new?template=suggest-source.yml&ecosystem=.NET). If you're a listed source and don't want your work cited, [ask to opt out](https://github.com/awesome-humans/website/issues/new?template=opt-out.yml). For how both work, see [Want in or out?](https://awesome-humans.net/want-in-or-out/)

## Admission criteria

**Scope: published writing.** A source qualifies on text a reader can check: blog posts, documentation, books, and newsletters. Videos, conference talks, and podcasts are out of scope for now, both as citable material and as evidence of a track record. A source that moves to those formats stops qualifying, and becomes a `vet-source` candidate again if the writing resumes.

A **source** is the published work of an awesome human: a person or a publication. Admission needs an **established track record**:

1. **Longevity:** At least two years of sustained publishing. A source with less goes on the watch list, however good the writing. The `Since` column shows how far back each record goes.
2. **Depth:** Original insight, such as internals, measurements, or worked reasoning. Paraphrased release notes don't count. A long-lived source with thin depth is admitted with the `**Corroborate.**` marking, which limits how it's cited.
3. **Accuracy:** A history of being right, and of publishing corrections when wrong.
4. **Independence:** The content stands on its merit, not on marketing reach.

Two rules qualify these criteria:

- **An independence concern is a usage limit, not a lower rank.** A concern is live when vendor developer relations or product-team work produces adoption-focused content with little critical distance. The source is still admitted, and its section under `## Source notes` says what a citation can rest on, usually how something works, not whether to adopt it. `avalonia-blog`, `james-montemagno`, and `shay-rojansky` each record one.
  - Working for a vendor isn't a concern by itself. Depth with critical distance, as from `stephen-toub` or `khalid`, needs no limit.
  - A concern that no limit could contain leads to a decline or the watch list, as with `duende-blog` and `telerik-blog`.
- **Dormancy blocks admission, with one exception.** A candidate who hasn't published for over a year goes on the watch list, unless the person's **writing** clearly continues elsewhere, such as official documentation, another publication, or a book. Then the track record follows the person, and the notes record the dormant channel.
  - Repository activity isn't evidence. Opinions don't cite commits, releases, or issue threads, so a busy GitHub profile beside a silent blog is still dormant.
  - Documentation written in a repository counts. The test is what was published, not where the commits landed.
  - The same rule triggers re-evaluation of admitted sources. For details, see `vet-source`.

The [Watch list](#watch-list-track-record-still-forming-or-previously-strong-but-now-dormant) tracks candidates that don't qualify yet, and `vet-source` re-evaluates them. The decision log at the end records every admission and demotion.

### How the roster is organised

- **IDs:** Each source has a stable `id`, which `sources:` metadata uses.
- **Order:** Rows in every roster table, and the sections under [Source notes](#source-notes), are sorted by `id`. Insert a new source in order.
- **Tables:** A row holds only what identifies a source: its ID, its kind, where to read it, what it covers, and how far back it goes.
- **Kind:** `person` or `publication`. A newsletter or link blog that one person writes is still a `publication`. A person's Source link text starts with their name, as in `Andrew Lock: .NET Escapades`, because the site takes the name from it.
- **Notes:** The evidence behind a row goes in its notes section, in the shape [Source notes](#source-notes) describes, one sentence per line. Rows stay short, and a reworded sentence shows in a diff as that one sentence.
- **Decision log:** Each entry gives the decision and the fact behind it. The evidence is in the source's notes and the pull request. It's the only table Prettier doesn't align: `<!-- prettier-ignore -->` stops one long Detail cell from realigning every row.

### Reserved IDs, markings, and references

**Reserved ID: `house`.** The repository owner's opinions use the `house` source ID, described in [HOUSE-OPINIONS.md](HOUSE-OPINIONS.md). Its authority is ownership of the repository, not a track record, so it skips the admission criteria, and every house opinion is visibly marked instead. [HOUSE-OPINIONS.md: How this works](HOUSE-OPINIONS.md#how-this-works) is the canonical definition of the marking, of precedence in a conflict, and of contributor adoption. The `house` ID never appears in the roster tables.

**Reserved marking: `**Discovery-only.**`.** Aggregators, such as link roundups, newsletters, and curated lists, are admitted for discovery and never cited. They lead to a primary source, and the opinion cites that. CI rejects the ID of a source whose notes carry this marking in any `sources:` list under `opinions/` or `templates/`. Like `**House:**`, it's a fixed string, so a check can match it exactly.

**Reserved marking: `**Corroborate.**`.** Some sources have a long track record but thin depth: SEO-shaped roundups, course funnels, or release paraphrase with occasional substance. They're admitted and marked. The marking limits how you cite the source, not its standing. An opinion can cite it, but never alone: every claim it supports also needs an unmarked citable source. `scripts/validate-sources.cs` rejects an opinion or template whose citable sources are all marked. Neither `house` nor a Discovery-only ID counts as the unmarked source.

**References: a named package's own material.** When an opinion or template names a package, you can link the project's own material inline as a reference: its documentation site or in-repository docs, release notes and changelogs, issues and pull requests, or nuget.org page. The standard behind a named format or protocol qualifies on the same terms, such as an RFC or the IANA time zone database.

- A reference corroborates a claim and never carries it. It can confirm how something is set up, what it supports, and where it falls short, but the claim still rests on a roster source. A package's own material never decides whether to adopt that package.
- A reference needs no `vet-source` pass and never appears in `sources:`, so `scripts/validate-sources.cs` doesn't see it.
- Cite it in the same `Source: Title` form as anything else, as in `[xUnit.net: Testing with Native AOT]`.

## Citable: quotable in opinions and templates

Every source here meets all four criteria. The `Since` column shows the length of each record, and any independence concern, conflict of interest, or usage limit is in the source's section under [Source notes](#source-notes).

The table lists 31 sources: 21 people and 10 publications.

| id                   | Kind        | Source                                                                                                                                             | Focus                                                                                                           | Since                                       |
| -------------------- | ----------- | -------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- | ------------------------------------------- |
| `aaron-stannard`     | person      | [Aaron Stannard: Aaronontheweb](https://aaronstannard.com/) and his posts on the [Petabridge blog](https://petabridge.com/blog/)                   | OSS versioning and compatibility, property and performance testing, Akka.NET, checking agent code, agent skills | 2010                                        |
| `andrew-lock`        | person      | [Andrew Lock: .NET Escapades](https://andrewlock.net/)                                                                                             | ASP.NET Core internals, "Exploring .NET" per-release series                                                     | 2016                                        |
| `ardalis`            | person      | [Steve Smith: ardalis.com](https://ardalis.com/) and [DevIQ](https://deviq.com/)                                                                   | Application architecture, Clean Architecture and DDD, design patterns and principles                            | 2003                                        |
| `aspnet-blog`        | publication | [ASP.NET Core category, .NET Blog](https://devblogs.microsoft.com/dotnet/category/aspnetcore/)                                                     | ASP.NET Core announcements & deep-dives                                                                         | ~2004                                       |
| `avalonia-blog`      | publication | [Avalonia UI blog](https://avaloniaui.net/blog) and [docs](https://docs.avaloniaui.net/)                                                           | Cross-platform desktop UI, rendering                                                                            | ~2022                                       |
| `awesome-avalonia`   | publication | [awesome-avalonia](https://github.com/AvaloniaCommunity/awesome-avalonia)                                                                          | Curated Avalonia library/resource list                                                                          | 2020                                        |
| `awesome-blazor`     | publication | [awesome-blazor](https://github.com/AdrienTorris/awesome-blazor)                                                                                   | Curated Blazor library/resource list                                                                            | 2018                                        |
| `awesome-dotnet`     | publication | [awesome-dotnet](https://github.com/quozd/awesome-dotnet)                                                                                          | Curated library/tool list                                                                                       | 2014                                        |
| `code-with-mukesh`   | person      | [Mukesh Murugan: CodeWithMukesh](https://codewithmukesh.com/)                                                                                      | Best-practice roundups, EF Core, benchmarks                                                                     | 2020                                        |
| `csharp-digest`      | publication | [C# Digest](https://csharpdigest.net/)                                                                                                             | Weekly newsletter aggregation                                                                                   | ~2017                                       |
| `damien-bowden`      | person      | [Damien Bowden: damienbod](https://damienbod.com/)                                                                                                 | ASP.NET Core security, OpenID Connect, BFF, passkeys                                                            | 2013                                        |
| `derek-comartin`     | person      | [Derek Comartin: CodeOpinion](https://codeopinion.com/)                                                                                            | Event-driven architecture, messaging, CQRS, DDD, modular monoliths                                              | 2012                                        |
| `dotnet-blog`        | publication | [.NET Blog](https://devblogs.microsoft.com/dotnet/)                                                                                                | Everything .NET; canonical announcements                                                                        | ~2004                                       |
| `fsharp-weekly`      | publication | [Sergey Tihon: F# Weekly](https://sergeytihon.com/category/f-weekly/)                                                                              | Weekly F# link aggregation                                                                                      | 2011                                        |
| `gerald-versluis`    | person      | [Gerald Versluis](https://blog.verslu.is/)                                                                                                         | .NET MAUI, Xamarin, Blazor Hybrid                                                                               | 2015                                        |
| `james-montemagno`   | person      | [James Montemagno](https://montemagno.com/)                                                                                                        | Xamarin/.NET MAUI, cross-platform mobile                                                                        | ~2012                                       |
| `jeremy-miller`      | person      | [Jeremy D. Miller: The Shade Tree Developer](https://jeremydmiller.com/)                                                                           | Messaging, event sourcing, persistence, dependency injection, testing long-lived codebases                      | 2007                                        |
| `jetbrains-dotnet`   | publication | [JetBrains .NET Blog](https://blog.jetbrains.com/dotnet/)                                                                                          | C# language features, tooling                                                                                   | ~2010s                                      |
| `jon-skeet`          | person      | [Jon Skeet's coding blog](https://codeblog.jonskeet.uk/)                                                                                           | C# language semantics (records, generics, equality), date/time correctness, API design                          | 2005                                        |
| `khalid`             | person      | [Khalid Abuhakmeh](https://khalidabuhakmeh.com/) and his posts on the [Duende blog](https://duendesoftware.com/blog)                               | Practical .NET, tooling; identity and OpenID Connect since 2025                                                 | ~2012                                       |
| `mark-seemann`       | person      | [Mark Seemann: ploeh blog](https://blog.ploeh.dk/)                                                                                                 | Unit testing and TDD, dependency injection, type-driven and functional design in C#, code quality               | 2006                                        |
| `meziantou`          | person      | [Gérald Barré: Meziantou's blog](https://www.meziantou.net/)                                                                                       | C# language, analyzers, practical best practices                                                                | ~2015                                       |
| `milan-jovanovic`    | person      | [Milan Jovanović](https://www.milanjovanovic.tech/blog)                                                                                            | Architecture, EF Core, CQRS, modular monoliths                                                                  | ~2020                                       |
| `ms-learn`           | publication | [Microsoft Learn: .NET docs](https://learn.microsoft.com/dotnet/) and [Azure Architecture Center](https://learn.microsoft.com/azure/architecture/) | Official documentation, breaking changes, architecture e-books, cloud design patterns                           | ~2016 (docs.microsoft.com era; MSDN before) |
| `nicholas-blumhardt` | person      | [Nicholas Blumhardt](https://nblumhardt.com/)                                                                                                      | Structured logging (Serilog), DI/IoC (Autofac), tracing and diagnostics                                         | 2007                                        |
| `oskar-dudycz`       | person      | [Oskar Dudycz: Event-Driven.io](https://event-driven.io/)                                                                                          | Event sourcing, event-driven architecture, CQRS, vertical slices                                                | 2011                                        |
| `scott-wlaschin`     | person      | [Scott Wlaschin: F# for Fun and Profit](https://fsharpforfunandprofit.com/)                                                                        | F#, functional design, domain modelling                                                                         | 2012                                        |
| `shay-rojansky`      | person      | [Shay Rojansky](https://www.roji.org/)                                                                                                             | .NET ↔ PostgreSQL mapping (Npgsql), EF Core internals                                                           | 2014                                        |
| `stephen-cleary`     | person      | [Stephen Cleary](https://blog.stephencleary.com/)                                                                                                  | Async and concurrency, `SynchronizationContext` and `ConfigureAwait`, cancellation                              | 2008                                        |
| `stephen-toub`       | person      | [Stephen Toub (author page)](https://devblogs.microsoft.com/dotnet/author/toub/)                                                                   | Runtime/BCL performance, async internals                                                                        | ~2005                                       |
| `steve-gordon`       | person      | [Steve Gordon](https://www.stevejgordon.co.uk/)                                                                                                    | High-performance .NET, HttpClient, Span/ArrayPool                                                               | ~2016                                       |

## Watch list: track record still forming, or previously strong but now dormant

A watch-list source is never cited. `scripts/validate-sources.cs` rejects a watch-list ID in any `sources:` list, however good the back catalogue. Use these sources for discovery and cross-checking: follow a claim you find here to a citable source, and cite that. `vet-source` re-evaluates them when a blocker might have cleared.

| id                | Kind        | Source                                                          | Focus                                             | Since |
| ----------------- | ----------- | --------------------------------------------------------------- | ------------------------------------------------- | ----- |
| `barry-dorrans`   | person      | [Barry Dorrans: idunno.org](https://idunno.org/)                | .NET security, CVE triage, supply chain           | 2025  |
| `chris-sainty`    | person      | [Chris Sainty](https://chrissainty.com/)                        | Blazor components, routing, auth                  | 2018  |
| `duende-blog`     | publication | [Duende Software blog](https://duendesoftware.com/blog)         | IdentityServer, BFF, OpenID Connect and SAML      | 2020  |
| `mikesdotnetting` | person      | [Mike Brind: Mikesdotnetting](https://www.mikesdotnetting.com/) | ASP.NET web dev, Razor Pages                      | 2007  |
| `morning-brew`    | publication | [The Morning Brew](https://blog.cwa.me.uk/)                     | Daily .NET link aggregation                       | 2008  |
| `nick-polyak`     | person      | [Nick Polyak (Dev.to)](https://dev.to/npolyak)                  | Avalonia/WPF/XAML architecture, reactive patterns | 2008  |
| `scott-brady`     | person      | [Scott Brady](https://www.scottbrady.io/)                       | OAuth, OpenID Connect, identity, cryptography     | —     |
| `syncfusion-blog` | publication | [Syncfusion Blogs](https://www.syncfusion.com/blogs/)           | Release summaries, performance roundups           | —     |
| `telerik-blog`    | publication | [Telerik Blogs: .NET](https://www.telerik.com/blogs)            | Release summaries                                 | —     |
| `udi-dahan`       | person      | [Udi Dahan: The Software Simplist](https://udidahan.com/)       | SOA, messaging, service boundaries, domain events | ~2007 |

## Opted out

A source moves here when its owner asks to stop being cited. Its row keeps the same five columns, and its section under [Source notes](#source-notes) goes. The ID stays reserved, so a later suggestion of the source is turned away, and `scripts/validate-sources.cs` rejects it in any `sources:` list. Opting back in is a suggestion like any other, and the source leaves this section only on admission. For the policy, see [Want in or out?](https://awesome-humans.net/want-in-or-out/)

No source has opted out.

## Source notes

Each section holds the evidence behind a row: the track record, what a citation can rest on, and any conflict to declare. Each source has at most one section, sorted by ID. A source with nothing to record has none.

Write each section in this shape:

1. **Verdict:** Open with the verdict in a line or two. For a watch-listed source, say what blocks it. A short note stops here.
2. **Labelled bullets:** A longer note continues with one `**Label:**` bullet per criterion or rule that has something to record, in this order, leaving out empty labels:
   1. **Scope**
   2. **Longevity** or **Dormancy**
   3. **Depth**
   4. **Accuracy**
   5. **Authorship**
   6. **Independence**
   7. **Conflict of interest**
   8. **Cite** for a citable source, or **Use** for a watch-listed one
   9. **Re-evaluate**
3. **One sentence per line:** Put each sentence on its own line, indented under its bullet, so a reworded sentence shows in a diff as that one sentence.

The check reads markings from the whole section, heading included, so `**Discovery-only.**` or `**Corroborate.**` in either place marks the source.

### `aaron-stannard`: Citable

Citable on his code-backed engineering posts and on the dotnet-skills he wrote himself, with a conflict on anything Petabridge sells.

- **Scope:** Includes [dotnet-skills](https://github.com/Aaronontheweb/dotnet-skills), at v1.6.0 on 2026-09-16 with 37 skills and 6 subagents, which counts as authored documentation.
  It was watch-listed as a separate source on 2026-09-25 and merged into this ID the same day: the track record follows the person, so the repository's 2025-11-12 start doesn't hold it back.
- **Longevity:** Writing on aaronstannard.com since [2010-05-14](https://aaronstannard.com/blog/page19/), and on the Petabridge blog since 2015.
  Together they show posts in every year from 2010 to 2026, and both are live, with posts on 2026-08-21 and 2026-09-23.
  The personal blog thins to a few posts a year from 2016 to 2019, with a longest gap of 347 days (2018-10-16 to 2019-09-28), and the Petabridge blog carries those years.
- **Depth:** ["Introducing NBench"](https://aaronstannard.com/introducing-nbench/) starts from a throughput regression that unit tests couldn't catch.
  ["Property Testing in C# with FsCheck"](https://aaronstannard.com/fscheck-property-testing-csharp-part1/) works a shuffle through to a shrunk counter-example.
  ["Extend-Only Design"](https://aaronstannard.com/extend-only-design/) and ["OSS Compatibility Standards"](https://aaronstannard.com/oss-compatibility-standards/) reason through schema and API evolution in code.
  His 2026 "Software 2.0" series is the most concrete .NET-specific writing found on verifying agent-written code.
  It reports failures as well as wins: ["Planning and Verifying a Greenfield Project"](https://aaronstannard.com/software-2.0-case-study-textforge/) records the agent writing tests that only "checked the 'has test coverage' box".
  dotnet-skills publishes [evaluations](https://github.com/Aaronontheweb/dotnet-skills-evals), but they're small: 15 Akka.NET tasks, judged by the model that wrote the code.
- **Accuracy:** Sound, with in-post corrections on record.
  [".NET Core is Boiling the Ocean"](https://aaronstannard.com/dotnetcore-boil-ocean/) (2016) carries Miguel de Icaza's correction on WebAssembly and Mono.
  ["The New Rules for Playing in Microsoft's Open Source Sandbox"](https://aaronstannard.com/new-rules-dotnet-oss/) (2020) carries an `EDIT` that retracts a misreading of David Fowler.
  dotnet-skills is weaker, because nothing compiles its samples: its own `AGENTS.md` says "There is no build system, tests, or compiled output."
  On 2026-09-25, against SDK 10.0.400, the `testcontainers` samples failed with CS0246 against Testcontainers 4.15.0, which their own `Version="*"` references restore, and a `csharp-coding-standards` value object failed with CS0111.
  The same skill's records hold collections, which breaks value equality, and `with` bypasses a validating record.
  The package examples pin xUnit v2, FluentAssertions 6 and 7, and `Verify.Xunit`, which pulls in xUnit v2.
  Six skills ship CI on `dotnet-version: 9.0.x`, with 22 mutable action tags and no SHA pins.
  Its release notes do record corrections, including "fabricated APIs, compile errors" in the OpenTelemetry skill, and guidance that wrongly called adding optional parameters binary-compatible.
- **Authorship:** He made 68 of 82 dotnet-skills commits, but the skills that held up best under checking (`aot-trimming`, `csharp-nullable-reference-types`, and `opentelementry-dotnet-instrumentation` (sic)) came largely from Daniel Marbach, Mauro Servienti, and Tomasz Masternak of Particular Software.
  Their work isn't citable under this ID. Where one of them has a written track record, vet that person.
- **Conflict of interest:** He's CEO of Petabridge, which sells Akka.NET support plans, consulting, and training, and sells the Phobos monitoring product through Sdkbin, a marketplace he built.
  Never cite him alone on adopting Akka.NET, the actor model, or Phobos.
  The same limit covers the six dotnet-skills skills and one subagent on Akka.NET.
  The posts on his workflow promote his free AI tooling (dotnet-skills, dotnet-slopwatch, and Netclaw), a milder conflict to flag where an opinion names one of them.
  On [2026-06-10](https://petabridge.com/blog/why-akkadotnet-remove-fluentassertions/), he removed FluentAssertions from Akka.NET on licensing grounds at a cost in convenience, which is evidence of critical distance.
- **Cite:** The engineering posts.
  Much of the catalogue is essays on the .NET ecosystem, open-source economics, and running a business, which argue from experience, not evidence.
  Cite Petabridge posts for mechanism, not adoption: it's his company's blog, with Akka.NET release announcements beside the technical posts.
  Cite a dotnet-skills skill only after its history shows he wrote it, and only for the principle it states: never its package versions or CI pins, and never a code sample you haven't compiled against the citing file's `targets`.

### `andrew-lock`: Citable

The standard reference for ASP.NET Core internals outside Microsoft, and author of _ASP.NET Core in Action_.

### `ardalis`: Citable, **Corroborate.**

Marked for mixed depth, and conflicted on the architecture and data-access topics he'd be cited for.

- **Scope:** The ID also covers [DevIQ](https://deviq.com/), published by NimblePros with Sarah Dutkiewicz, and the authored documentation at [specification.ardalis.com](https://specification.ardalis.com/) and the [Clean Architecture design decisions](https://ardalis.github.io/CleanArchitecture/).
  The documentation counts as writing; the repositories' commits and releases don't.
- **Longevity:** Writing since [2003-02-04](https://ardalis.com/first-post/), across four earlier blogs and the current domain.
  He's live: the latest post is from 2026-08-19, at about one a month as his effort moves to video and courses.
- **Depth:** ["Domain Modeling - Anemic Models"](https://ardalis.com/domain-modeling-anemic-models/) works a concrete failure through to the principle.
  ["From Microservices to Modular Monoliths"](https://ardalis.com/from-microservices-to-modular-monoliths/) has no migration guidance and ends on a discount code, and the Specification v9 post is release paraphrase by his own account.
  DevIQ is reference-grade on definitions and taxonomy, strongest on [design patterns](https://deviq.com/design-patterns/).
  Its entries argue in prose and diagrams, not code or measurements, so cite a measured source beside it for any claim an opinion rests on.
- **Accuracy:** Sound: no claim found reversed or left standing wrong.
  In ["Avoid Using C# Events"](https://ardalis.com/avoid-using-csharp-events-in-aspnetcore-apps/), the logged memory figures don't increase steadily and there's no control loop, so cite the argument, not the measurements.
- **Independence:** Co-founder of NimblePros and a Microsoft MVP, not an employee, so no independence limit.
- **Conflict of interest:** He sells training, mentoring, and courses, and maintains Ardalis.Specification, GuardClauses, Result, SmartEnum, and the Clean Architecture template.
  The DevIQ entries nearest data access recommend Ardalis.Specification, whose `Include` and `AsNoTracking` surface is a query object, not a Specification in Evans's sense, so never present that pattern as settled.
- **Cite:** The .NET posts. Most of the 2026 output is AI and agent tooling.
  GuardClauses, Result, and SmartEnum have had no release in about two years, and his Microsoft e-book [Architect modern web applications](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/) is still edition v8.0, so neither is current guidance for `net10.0`.

### `aspnet-blog`: Citable

The ASP.NET Core category of `dotnet-blog`: the same publisher and editorial standard, narrowed to ASP.NET Core posts.

### `avalonia-blog`: Citable

A product-team blog, release-driven and close to marketing: cite it and the docs for how something works, not whether to adopt it.

- **Scope:** Widened on 2026-08-20 to the project's own documentation, which explains mechanism the blog doesn't, under the same limit.
- **Longevity:** About four years, which clears the bar.
- **Independence:** The concern stands however long the record grows.

### `awesome-avalonia`: Citable, **Discovery-only.**

A discovery channel, not an opinion source. Backed by the Avalonia community organisation and actively maintained.

### `awesome-blazor`: Citable, **Discovery-only.**

A discovery channel, not an opinion source.

### `awesome-dotnet`: Citable, **Discovery-only.**

Library discovery. Verify its recommendations independently.

### `barry-dorrans`: Watch list

Blocked on longevity alone.
Depth and accuracy are recorded here, so a promotion is quick.

- **Longevity:** [idunno.org](https://idunno.org/) has been a blog only since 2025-11-18, with ten posts, the latest on 2026-08-03.
  The domain is older, and Wayback captures go back to 2003-06-04, but from at least 2015 to 2025-11-17 it was a static conference and biography page. A December 2024 capture still advertised a September 2020 talk as upcoming.
  Talks filled the decade between, and they're out of scope, so the record that counts is under a year.
- **Depth:** ["The year in .NET Security"](https://idunno.org/the-year-in-net-security/) takes CVE-2025-55248, CVE-2025-55247, and CVE-2025-30399 from root cause to the API that replaced the vulnerable one.
  ["Protecting your .NET app from Server-Side Request Forgery"](https://idunno.org/protecting-your-net-app-from-server-side-request-forgery-ssrf-vulnerabilities/) reasons from the Capital One breach and the cloud metadata endpoint to mitigation code.
  The code-signing post has about three thousand words of workflow configuration.
- **Accuracy:** He adds dated in-post updates when Microsoft renames a product.
- **Independence:** He's Microsoft's .NET security lead, and he writes CVE mechanism from the inside, not adoption copy, so no independence limit would apply once he clears the longevity bar.
- **Re-evaluate:** From 2027-11, when the blog turns two.

### `chris-sainty`: Watch list

Blocked on dormancy: no posts since 2023-08, and no verified continuing channel.
The 2018 to 2023 catalogue is strong, and usable for cross-checking on watch-list terms. Citing it needs admission first.

### `code-with-mukesh`: Citable, **Corroborate.**

A blog since 2020, marked for depth: SEO-shaped roundups that lead to courses, so use the shape of the guidance, not its specifics.
The 2026 output includes real benchmarks.

### `csharp-digest`: Citable, **Discovery-only.**

A discovery channel, not an opinion source. Archive verified back to 2017-12.

### `damien-bowden`: Citable

Independent depth on ASP.NET Core security, OpenID Connect, and BFF, shown in working code.

- **Longevity:** Writing since [2013-01-31](https://damienbod.com/2013/01/31/welcome-to-my-blog/), technical from the first week, and still publishing on 2026-09-01.
  Every sampled year between carries several pages of posts, at least monthly. 2014, 2017, and 2022 weren't counted, and the years either side leave no room for a gap.
- **Depth:** [The BFF post](https://damienbod.com/2024/04/08/bff-secured-asp-net-core-application-using-downstream-api-and-an-oauth-client-credentials-jwt/) carries the OpenID Connect setup, a YARP transform, and a token cache, and explains why the downstream API refuses the user's delegated token.
  [The passkey post](https://damienbod.com/2026/01/05/set-the-amr-claim-when-using-passkeys-authentication-in-asp-net-core/) diagnoses an ASP.NET Core defect, links the runtime issue, and gives a re-sign-in workaround against RFC 8176.
- **Accuracy:** Maintained in the open: posts carry dated History sections that record what each framework version changed.
  When Duende's co-founder commented that a 2023 post resembled Duende's own sample, he added the attribution and said so in the thread.
- **Independence:** A consultant in Switzerland with no vendor attachment, who covers Duende, OpenIddict, Entra ID, and ASP.NET Core Identity in turn, so no independence limit.
- **Conflict of interest:** He maintains `angular-auth-oidc-client` and writes tutorials that use it, and he sells application-security workshops through isolutions.
- **Cite:** Much of the catalogue pairs ASP.NET Core with an Angular front end, so check what a sample configures before you cite it.

### `derek-comartin`: Citable, **Corroborate.**

Marked for two reasons: no code or measurements, and sponsorship in the article body.
Cite him for the shape of an argument, and flag the sponsorship on anything about message queues.

- **Longevity:** Writing since 2012-12-11, at about 40 to 50 posts a year from 2016 to 2025, and active on 2026-08-19.
  He's better known for his YouTube channel, but the blog qualifies by itself: 2,000- to 2,500-word articles that argue in prose, with the companion video embedded, not replacing the text.
- **Depth:** The arguments are conceptual, with no code a reader could compile and no measurements they could reproduce.
- **Independence:** Particular Software's NServiceBus is a paid sponsor **inside the article body**, on a tracked link, in posts about the sponsor's own product category. Verified on 2026-09-03 on ["Decoupling in Software Architecture Moves Complexity"](https://codeopinion.com/decoupling-in-software-architecture-moves-complexity/), from 2026-08-06.
  He's also a Particular "NServiceBus Champ" and a Microsoft MVP.

### `dotnet-blog`: Citable

Includes Stephen Toub's annual "Performance Improvements in .NET" posts and the "What's new" series.

### `duende-blog`: Watch list

Blocked on independence, not longevity, and on five unvetted bylines.

- **Scope:** Dominick Baier's personal blog is a separate channel, dormant since 2024-10-01.
  It's named, not linked, because the host no longer completes a TLS handshake, so a link would fail the link check.
- **Longevity:** Publishing since [2020-10-01](https://duendesoftware.com/blog/20201001-helloduende) at one to four posts a month, so six years would clear the bar.
- **Authorship:** Several authors, so independence and quality vary by byline, the same blocker as `telerik-blog` and `syncfusion-blog`, not the `avalonia-blog` limit.
  Where one of the five has a track record, vet that person. `khalid` is vetted, and his posts here are citable under his own ID, with the limit recorded there.
- **Independence:** All four posts read in full for this pass end at the paid product in the post's own category, with no caveat against it.
  The depth makes that a blocker: a four-thousand-word piece with code and diagrams still ends by recommending Duende BFF.
- **Use:** Discovery and cross-checking, on the usual terms.

### `fsharp-weekly`: Citable, **Discovery-only.**

A discovery channel for the F# ecosystem, not an opinion source.

### `gerald-versluis`: Citable

Five books, Pluralsight courses, and active on his blog and the .NET Blog through 2026.
He's on the MAUI team, with the same independence limit as `james-montemagno`.

### `james-montemagno`: Citable

Cite only the pre-2024 Xamarin and MAUI catalogue, and expect nothing new from harvests.

- **Longevity:** Blogging since about 2012, first at motzcod.es, then montemagno.com.
  Re-vetted on 2026-08-23 on his writing alone, after the Merge Conflict podcast left scope. The blog still publishes, most recently on 2025-12-31, so he isn't dormant, but it now covers Copilot and tooling, and the last .NET post is from 2024-01-19.
- **Independence:** A live concern: Microsoft developer relations, adoption-focused, with little critical distance. Weigh independence per piece.

### `jeremy-miller`: Citable, **Corroborate.**

Cite the design essays, never the release notes, and never cite him alone on whether to adopt Marten or Wolverine.

- **Scope:** The [JasperFx news feed](https://jasperfx.net/news/) is out of scope, because it's almost all release announcements.
- **Longevity:** Writing since at least 2007-10-18 at codebetter.com, which no longer resolves, so an earlier start is likely but unverifiable. He continues on jeremydmiller.com almost daily, and was active on 2026-09-02.
- **Conflict of interest:** He created StructureMap, Marten, Wolverine, Lamar, and Alba, and founded JasperFx Software, which sells licences, support plans, and CritterWatch for that stack.
  He writes almost only about it: on 2026-09-03, eight of the nine posts on the front page were, and five of those were release announcements.
  The conflict is recorded, not limited, following `nicholas-blumhardt` and `oskar-dudycz`.

### `jetbrains-dotnet`: Citable

An institutional blog. It's about tools, but the language explainers keep critical distance, so no independence limit.

### `jon-skeet`: Citable

Created Noda Time, wrote _C# in Depth_, and is Stack Overflow's top contributor.

- **Longevity:** About four or five posts a year. Much of the recent output is general data modelling from a personal election-data project.
- **Independence:** Formerly Google developer relations, on the Cloud .NET client libraries, and between jobs as of 2026-07. He criticises language design he doesn't own, so no independence limit.
- **Cite:** The C# and BCL posts.
  _C# in Depth_ 4th edition (2019) predates records and isn't current guidance.

### `khalid`: Citable

Cite the personal blog in full. Cite his Duende posts for platform mechanism, and never alone on whether to adopt a Duende product.

- **Scope:** Widened on 2026-09-12 to his bylined posts on the [Duende blog](https://duendesoftware.com/blog), about two a month from 2025-03 to 2026-10-01.
  The publication is watch-listed separately, so this ID covers his byline and nobody else's.
  The documentation is evidence, not citable material, as with `shay-rojansky`: his 387 commits to the docs repository, the latest on 2026-09-03, author whole pages, but the published pages name no author to cite.
- **Longevity:** [khalidabuhakmeh.com](https://khalidabuhakmeh.com/) went quiet from 2025-04-22 and resumed on 2026-10-05 with two posts. The Duende byline carried his writing through the gap.
  The two new posts are introductory: one explains `int.TryParse`, `int.Parse`, and `Convert.ToInt32`, and one maps Rails to ASP.NET Core.
  Neither has measurements, and no factual errors were found.
- **Independence:** Now at Duende Software. His JetBrains authorship ended in 2024.
  The widening follows `avalonia-blog`, with a stricter version of the same limit: each of the three Duende posts read in full for this pass ends on Duende BFF or IdentityServer.
  ["Understanding .NET 11 Automatic CSRF Protection"](https://duendesoftware.com/blog/20260901-understanding-dotnet-11-automatic-csrf-protection) (2026-09-01) shows the citable kind: it explains what the framework middleware does to OpenID Connect flows, and the product recommendation sits at the end, where a reader expects it.
  ["Securing Minimal APIs with Scope-Based Authorization in .NET 10"](https://duendesoftware.com/blog/securing-minimal-apis-with-scope-based-authorization-in-dotnet-10) (2026-09-29) is citable for the `AddAuthorizationBuilder` and `RequireClaim("scope", …)` mechanism. Its sample assumes IdentityServer's one-claim-per-scope token shape, so check the issuer's format before you quote it.

### `mark-seemann`: Citable

Twenty years of writing, with depth you can verify in compiling code and real measurements.

- **Longevity:** Writing since 2006-01-05 on [MSDN blogs](https://learn.microsoft.com/archive/blogs/ploeh/), and on the current blog since 2009-01-28, with over 800 posts there. He wrote less in the mid-2010s, and was still publishing on 2026-08-13.
- **Independence:** Self-employed in Copenhagen with no vendor attachment, and he criticises design he doesn't own, so no independence limit.
- **Conflict of interest:** He sells books and video courses, and created AutoFixture, a mild conflict on testing and DI.
- **Cite:** The C# and .NET posts. About a third of the technical output is Haskell or F#, and from ["TDD as induction"](https://blog.ploeh.dk/2026/02/23/tdd-as-induction/) (2026-02-23), the 2026 posts turn to language-agnostic essays on AI and software philosophy.
  _Dependency Injection Principles, Practices, and Patterns_ (2019) supersedes _Dependency Injection in .NET_ (2011).

### `meziantou`: Citable

Very high volume and consistency.

### `mikesdotnetting`: Watch list

Blocked on dormancy: no posts since 2023-04, GitHub quiet since 2024-08, and no continuing channel.
The 2007 to 2023 archive and _Razor Pages in Action_ stay usable for cross-checking on watch-list terms.

### `milan-jovanovic`: Citable

Promoted from the watch list when the longevity bar dropped.

- **Longevity:** About six years, well over the two-year bar. The start year is an estimate.
- **Conflict of interest:** He sells courses and templates, and runs a sponsored newsletter. The conflict is recorded, and isn't serious enough for a usage limit.

### `morning-brew`: Watch list

Blocked on dormancy: demoted from citable on 2026-08-18.
The last issue, #3995 on 2024-08-02, announced a summer break, and publication never resumed. The site is up, and the RSS feed ends there.
The 2008 to 2024 archive stays usable for discovery.

### `ms-learn`: Citable

The "What's new" and "Breaking changes" pages for each release.

- **Scope:** Widened on 2026-08-24 from the .NET docs to first-party technical documentation across `learn.microsoft.com`, including the .NET architecture e-books and the Azure Architecture Center pattern catalogue.
  Same publisher and editorial standard, so a clarification, not an admission, following the `avalonia-blog` widening of 2026-08-20.
- **Cite:** For mechanism and pattern trade-offs, never product marketing or a service page that argues for adoption.
  Quote the page's review date from its `ms.date` metadata field, never `updated_at`. `updated_at` is a docset build timestamp shared by every page in a bulk republish, and it can run years ahead of the text.
  The difference matters because the pages age unevenly.
  The microservices e-book is 2018 and 2021 content, unreviewed since, and its 2023 and 2024 `updated_at` values are republishes.
  The Azure Architecture Center pattern pages are revised continually, and carry recent `ms.date` values.

### `nicholas-blumhardt`: Citable

Created Autofac and Serilog, and founded Datalust, which sells Seq.

- **Dormancy:** The personal blog has been dormant since 2024-10. He stays citable under the dormancy exception, on published writing, not repository activity.
  The engineering posts on the [Datalust blog](https://datalust.co/blog/) continue, verified on 2026-08-23 through a post dated 2026-07-20 and a January 2026 engineering update.
- **Conflict of interest:** An opinion that cites him on logging, tracing, or DI must flag that he writes and sells the tools in question.
- **Cite:** The personal blog and the engineering posts, never Seq release announcements.

### `nick-polyak`: Watch list

Blocked on dormancy: demoted from citable on 2026-08-23.
His last post was on 2025-05-04 on Dev.to, the CodeProject archive (2008 to 2024) is read-only, and his 2025 and 2026 GitHub output is code and version bumps with no documentation, so the exception doesn't apply.
The back catalogue isn't citable while he's watch-listed. Re-vet him if the writing resumes.

### `oskar-dudycz`: Citable, **Corroborate.**

Event sourcing and event-driven design in worked, typed examples, from someone who builds tooling for those patterns.

- **Longevity:** Writing since 2011-09-21, on a Polish-language blog later moved into the current site, and steadily from 2020, at about 50 posts a year through 2023. Active on 2026-08-10.
- **Depth:** Verified on worked examples, with traced scenarios and typed code.
- **Conflict of interest:** He co-maintains Marten and Emmett, and sells event sourcing workshops.
  The conflict is recorded, not limited, following `milan-jovanovic` and `nicholas-blumhardt`. The marking carries the usage rule.
- **Cite:** The .NET material. He also writes about Node, so check an example's language before you cite it.

### `scott-brady`: Watch list

Blocked on dormancy, in his own words on the site: "While I am no longer actively blogging since moving away from individual contributor roles, this website includes an archive of material for those looking to learn OAuth and web security."

- **Dormancy:** The last substantial article is "Understanding WS-Federation: A modern primer for an obsolete protocol", from 2024-04. The 2026-03 entry above it in [the article index](https://www.scottbrady.io/articles) is a conference write-up.
  Nothing qualifies for the dormancy exception: the Substack newsletter's last issue was on 2023-03-23, and his newest live Pluralsight course is from 2022-08.
  The start year wasn't established, because dormancy settled the question first.
- **Use:** The OAuth and identity archive stays usable for cross-checking. Citing it needs admission first.

### `scott-wlaschin`: Citable

<!-- cspell:ignore Modeling -->

The standard independent F# resource, and author of _Domain Modeling Made Functional_.

### `shay-rojansky`: Citable

Lead maintainer of Npgsql and on Microsoft's EF Core team, with the deepest writing on .NET to PostgreSQL type mapping, which no other roster source covers.

- **Longevity:** Nine years of writing, 2014 to 2023.
- **Dormancy:** No long-form writing since 2023, on roji.org or the .NET Blog.
  He's held on the dormancy exception by the documentation he writes, re-verified on 2026-08-23: the EF Core 11 what's-new and breaking-change pages in `dotnet/EntityFramework.Docs`, latest commit 2026-05-12. They publish to Microsoft Learn, so they count as writing.
- **Independence:** A live concern: the citable material is first-party design rationale for products he maintains.
- **Cite:** For mechanism, meaning how Npgsql and EF Core behave and why. Corroborate adoption judgements elsewhere.
  The 2014 to 2023 blog catalogue is citable. Expect nothing new from blog harvests.

### `stephen-cleary`: Citable

Independent depth on async and concurrency in .NET, and author of _Concurrency in C# Cookbook_.
The cadence is thin, so watch it for dormancy.

- **Longevity:** Writing since [2008-06-20](https://blog.stephencleary.com/2008/06/interop-on-64-bit-platforms.html), at 30 to 67 posts a year from 2009 to 2013, and fewer than 20 a year after.
  The blog has no posts in 2019, between 2018-06 and 2020-03. The second edition of the book (O'Reilly, 2019-08) covers that year.
  He wrote eight MSDN Magazine articles from 2011 to 2015, including ["Async/Await: Best Practices in Asynchronous Programming"](https://learn.microsoft.com/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming) (2013-03).
- **Dormancy:** Not dormant, but close: the latest post is ["Debug Dumps in Visual Studio"](https://blog.stephencleary.com/2025/12/debug-dumps-in-visual-studio.html) (2025-12-04), the only post in 2025, after five in 2024.
- **Depth:** ["There Is No Thread"](https://blog.stephencleary.com/2013/11/there-is-no-thread.html) traces an async write from the library through the driver, the interrupt, and the I/O completion port back to the awaiting context.
  ["Don't Block on Async Code"](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html) walks a `SynchronizationContext` deadlock step by step, and argues for async all the way over `ConfigureAwait(false)` as the fix.
  ["ConfigureAwait in .NET 8"](https://blog.stephencleary.com/2023/11/configureawait-in-net-8.html) covers each `ConfigureAwaitOptions` value, including that `SuppressThrowing` throws on `Task<T>`, which `CA2261` reports.
  ["Cancellation, Part 6: Linking"](https://blog.stephencleary.com/2024/10/cancellation-6-linking.html) explains why comparing `OperationCanceledException.CancellationToken` fails once a linked source sits between, and checks `IsCancellationRequested` instead.
  The 2025 post is a Visual Studio settings recipe, thinner than the async catalogue.
- **Accuracy:** Corrections are in the posts.
  The .NET 8 post says an earlier version wrongly claimed EF Core avoids `ConfigureAwait(false)`, and that EF Core added it in 5.0.0.
  The 2012 deadlock post carries a 2014-12-01 update pointing to newer material.
  ["ASP.NET Core SynchronizationContext"](https://blog.stephencleary.com/2017/03/aspnetcore-synchronization-context.html) (2017) revisits the deadlock guidance for a host with no context.
- **Independence:** An independent consultant and Microsoft MVP with no vendor employer, so no independence limit.
- **Conflict of interest:** He sells the book and contract work, and maintains the free Nito.AsyncEx library, which some posts link. Flag it where an opinion names Nito.AsyncEx.
- **Cite:** The async, concurrency, and cancellation posts.
  The 2012 and 2013 posts predate ASP.NET Core, so check any claim about a `SynchronizationContext` against the host the citing opinion targets.
- **Re-evaluate:** From 2026-12-04, if no post or other writing has appeared since 2025-12-04.

### `stephen-toub`: Citable

Publishes through `dotnet-blog`.

### `steve-gordon`: Citable

Variable cadence: heavy in 2024, quiet in 2025, and active again in 2026. Works at Elastic.
The back catalogue is authoritative.

### `syncfusion-blog`: Watch list

Blocked on independence and depth, not longevity: close to marketing, with quality that varies by author.

### `telerik-blog`: Watch list

Blocked on independence and depth, not longevity: close to marketing, with quality that varies by author.
The lower longevity bar doesn't change this.

### `udi-dahan`: Watch list

Blocked on dormancy: no posts since 2016-02-19, and no continuing written channel. His talks and the Particular training courses are out of scope since the 2026-08-23 narrowing.

- **Longevity:** The start year is estimated from a testimonial that mentions the blog in 2007.
- **Independence:** Founder and CEO of Particular Software, which sells NServiceBus, so a concern would also apply.
- **Use:** The 2005 to 2016 archive stays usable for discovery and cross-checking. `ms-learn` already cites him second-hand on domain events.

## Decision log

<!-- prettier-ignore -->
| Date | Decision | Detail |
| --- | --- | --- |
| 2026-08-12 | Initial roster | Seeded Tier 1, Tier 2, and the watch list from research verified against .NET 10 |
| 2026-08-12 | Admitted `scott-wlaschin` and `fsharp-weekly` (Tier 1) | Filled the F# gap. Writing since 2012 and 2011 |
| 2026-08-12 | Admitted `steve-sanderson` (Tier 1) | Blog dormant since 2020-01. Admitted on talks and repositories under the dormancy exception (removed 2026-08-23) |
| 2026-08-12 | Admitted `james-montemagno` (Tier 2) | Blogging since about 2012. Capped for independence as Microsoft developer relations |
| 2026-08-12 | Admitted `gerald-versluis` (Tier 2) | Blogging since 2015-03. Capped for independence as a MAUI team member |
| 2026-08-12 | Admitted `awesome-blazor` (Tier 2, discovery-only) | Curated list since 2018, actively maintained |
| 2026-08-12 | Watch-listed `chris-sainty` | Strong 2018 to 2023 Blazor catalogue, dormant since 2023-08 |
| 2026-08-12 | Watch-listed `avalonia-blog` | Blog only since about 2022, short on longevity (admitted 2026-08-14) |
| 2026-08-12 | Admitted `nick-polyak` (Tier 1) | Publishing since 2008 on CodeProject, then Dev.to (demoted 2026-08-23) |
| 2026-08-12 | Watch-listed `awesome-avalonia` | Created 2020-02, short of the eight-year discovery bar then in force (admitted 2026-08-14) |
| 2026-08-12 | First harvest (2025-11-01 to 2026-08-12) | Seeded 8 opinion files. Flagged `mikesdotnetting`, `khalid`, and `scott-wlaschin` for `vet-source` |
| 2026-08-12 | Demoted `mikesdotnetting` (Tier 1 to watch list) | Dormant since 2023-04, with no continuing channel |
| 2026-08-12 | Annotated `khalid` (kept Tier 1) | Blog quiet since 2025-04. Kept on GitHub activity, a basis withdrawn on 2026-08-23 |
| 2026-08-12 | Corrected the `scott-wlaschin` harvest flag | Low cadence, not dormant: the sweep missed his 2025-12 posts |
| 2026-08-12 | Updated the `steve-gordon` note | Publishing again from 2026-01 |
| 2026-08-12 | Reserved the `house` ID | The owner's own opinions, exempt from admission and always visibly marked (#7) |
| 2026-08-14 | **Experiment:** lowered the longevity bars | Tier 1 from about a decade to 6 years, and Tier 2 from 8 years to 3. The rows below follow from re-running `vet-source` and `harvest-sources` |
| 2026-08-14 | Promoted `andrew-lock` and `steve-gordon` (Tier 2 to Tier 1) | Both publishing since about 2016 |
| 2026-08-14 | Promoted `jetbrains-dotnet` (Tier 2 to Tier 1) | Publishing since the early 2010s. The language explainers keep critical distance |
| 2026-08-14 | Promoted `awesome-blazor` (Tier 2 to Tier 1, discovery-only) | Curated since 2018 |
| 2026-08-14 | Admitted `milan-jovanovic` (Tier 1) | Started about 2020, weekly through 2026-08. Conflict of interest recorded, not capped |
| 2026-08-14 | Admitted `csharp-digest` (Tier 1, discovery-only) | Archive verified back to 2017-12 |
| 2026-08-14 | Admitted `awesome-avalonia` (Tier 1, discovery-only) | Created 2020-02, which the new bar clears |
| 2026-08-14 | Admitted `nick-chapsas` (Tier 2) | Capped on depth: trend-driven video, with courses over the same ground (removed 2026-08-23) |
| 2026-08-14 | Admitted `code-with-mukesh` (Tier 2) | Blog since 2020. Capped on depth as SEO-shaped and course-funnelled |
| 2026-08-14 | Admitted `avalonia-blog` (Tier 2) | Its 2022 start clears the 3-year bar. Capped for independence as a product-team blog |
| 2026-08-14 | Kept 4 of 10 watch-list rows | `chris-sainty` and `mikesdotnetting` on dormancy, and `telerik-blog` and `syncfusion-blog` on independence and depth |
| 2026-08-14 | Harvest of newly admitted sources (2025-11-01 to 2026-08-14) | Added Avalonia 12 to `ui-frameworks.md`, and opened `architecture.md` and `data-access.md` |
| 2026-08-17 | **Experiment:** Tier 1 from 6 years to 5, and Tier 2 from 3 years to a 2-to-5-year band | No roster changes: every blocked row is blocked on something other than longevity |
| 2026-08-17 | Admitted `nicholas-blumhardt` (Tier 1) | Publishing since 2007-07. Personal blog dormant since 2024-10, held on the Datalust engineering series. Sells Seq, recorded as a conflict |
| 2026-08-18 | Harvest (2026-08-13 to 2026-08-18), plus a first sweep of `nicholas-blumhardt` and `jon-skeet` | Updated `testing.md`, `runtime-performance.md`, and `csharp.md`, and opened `logging.md`. Flagged `james-montemagno`, `nick-polyak`, and `khalid` |
| 2026-08-18 | Admitted `jon-skeet` (Tier 1) | Publishing since 2005-09. Filled the records and immutable-collection gaps |
| 2026-08-18 | Demoted `morning-brew` (Tier 1 to watch list) | Last issue 2024-08-02. An aggregator has no second channel for the dormancy exception |
| 2026-08-18 | Re-evaluated `khalid`, `nick-polyak`, and `james-montemagno` (no tier changes) | Narrowed the notes for all three |
| 2026-08-20 | Widened `avalonia-blog` to the project docs (no tier change) | Gave the Avalonia localization mechanism a citable home. Same publisher, same cap |
| 2026-08-21 | Admitted `shay-rojansky` (Tier 2) | Filled the .NET to PostgreSQL gap. Capped for independence. Dormant since 2023-05, admitted on the exception |
| 2026-08-23 | Clarified that the watch list never feeds `sources:` | Back catalogue included, as `validate-sources.cs` enforces. A demotion re-sources or drops its claims in the same pull request |
| 2026-08-23 | Removed `nick-chapsas` (was Tier 2) | Never cited, and his notes already said to cite the primary source instead |
| 2026-08-23 | Narrowed scope to published writing | Video, talks, and podcasts no longer count, as citations or as track record |
| 2026-08-23 | Removed `steve-sanderson` (was Tier 1) | Stood on talks and repositories, which the narrowed scope excludes. Never cited |
| 2026-08-23 | Ruled that repository activity isn't evidence of a live source | Commits and releases don't keep a dormant source admitted. Documentation written in a repository does |
| 2026-08-23 | Re-verified `nicholas-blumhardt` under the new rule (no tier change) | The Datalust blog alone holds the exception |
| 2026-08-23 | Replaced the depth cap with the `**Corroborate.**` marking | `code-with-mukesh` moves to Tier 1, marked. CI rejects an opinion whose only citable source is marked |
| 2026-08-23 | Demoted `nick-polyak` to the watch list (was Tier 1) | No writing since 2025-05-04. His two `ui-frameworks.md` citations moved to the Avalonia docs |
| 2026-08-23 | Re-vetted `james-montemagno` and `shay-rojansky` on writing alone (no tier changes) | Montemagno is citable before 2024 only. Rojansky is held on his EF Core documentation |
| 2026-08-24 | Admitted `oskar-dudycz` (Tier 1, marked **Corroborate.**) | Sustained output from 2020. Filled the event-driven and vertical-slice gaps. Conflict recorded |
| 2026-08-24 | Watch-listed `udi-dahan` | Dormant since 2016-02-19, with no continuing written channel |
| 2026-08-24 | Widened `ms-learn` across `learn.microsoft.com` (no tier change) | Took in the Azure Architecture Center. Cite it for mechanism and trade-offs, with each page's review date |
| 2026-09-03 | Admitted `mark-seemann` (Tier 1) | Twenty years of writing, with depth you can verify in code and measurements. Conflict recorded |
| 2026-09-03 | Admitted `jeremy-miller` (Tier 1, marked **Corroborate.**) | Writing since 2007, almost only about the stack his company sells |
| 2026-09-03 | Admitted `derek-comartin` (Tier 1, marked **Corroborate.**) | The blog clears the writing scope by itself. Marked for having no code or measurements, and for sponsorship in the article body |
| 2026-09-03 | Ran an admission pass for `opinions/architecture.md` | Added `mark-seemann` as the unmarked design source the file lacked |
| 2026-09-04 | Named the date field in the `ms-learn` citation rule (no tier change) | A research pass quoted `updated_at`, a build timestamp, and got six dates wrong. The rule now names `ms.date` |
| 2026-09-10 | Admitted `ardalis` (Tier 1, marked **Corroborate.**) | Writing since 2003, including DevIQ and his documentation. Marked for mixed depth. Conflict recorded |
| 2026-09-12 | Admitted `damien-bowden` (Tier 1) | Writing since 2013-01-31. The BFF and browser-authentication depth source the `security` research topic lacked |
| 2026-09-12 | Watch-listed `barry-dorrans` | Blog only since 2025-11-18. Strong writing recorded in the notes. Re-evaluate from 2027-11 |
| 2026-09-12 | Widened `khalid` to his Duende writing (no tier change) | His bylined Duende posts come into scope. Never cite him alone on adopting a Duende product |
| 2026-09-12 | Watch-listed `duende-blog` | Blocked on independence and five unvetted bylines. Khalid's posts are citable under his own ID |
| 2026-09-12 | Watch-listed `scott-brady` | Dormant by his own statement. Last substantial article 2024-04 |
| 2026-09-12 | Ran `vet-source` on the `security` research topic's candidates | Seven checked: `damien-bowden` admitted, `barry-dorrans` watch-listed, and the OWASP Cheat Sheet Series and Philippe De Ryck declined |
| 2026-09-14 | Harvest (2026-08-19 to 2026-09-14) | Updated `testing.md`, `ci.md`, `runtime-performance.md`, `ui-frameworks.md`, `fsharp.md`, `aspnet-core.md`, and `data-access.md`, and gave `architecture.md` its unmarked corroboration. Flagged `scott-wlaschin`, `steve-gordon`, and `nicholas-blumhardt` |
| 2026-09-15 | Defined references (no roster change) | A named package's own material can corroborate a roster-sourced claim inline, outside `sources:` (raised in review of #74) |
| 2026-09-20 | Merged Tier 1 and Tier 2 into one **Citable** table | No skill or check treated the tiers differently. Longevity is now two years, shown by `Since`, and the independence cap became a usage limit in the notes. No source's citability changed |
| 2026-09-25 | Admitted `aaron-stannard` | Sixteen years of writing, with depth in code-backed engineering posts. Gave the `ai-usage` research topic a .NET practitioner source. Petabridge recorded as a conflict |
| 2026-09-25 | Watch-listed `dotnet-skills` | Under a year old, with samples that don't compile against current packages, and several authors. Re-evaluate from 2027-11 |
| 2026-09-25 | Merged `dotnet-skills` into `aaron-stannard` | The same person, so his track record covers the repository. Uncompiled samples and co-authored skills became usage limits in his notes |
| 2026-10-09 | Harvest (2026-09-15 to 2026-10-09) | Updated `runtime-performance.md` (ILLink substitutions, and a .NET 11 runtime async note) and `ci.md` (signer allow-list upkeep). Flagged `khalid`, `james-montemagno`, and `nicholas-blumhardt` |
| 2026-10-09 | Re-evaluated `khalid` (no tier change) | His personal blog resumed on 2026-10-05 after a gap from 2025-04-22, which the Duende byline covered. The notes now record the restart, and a token-shape limit on citing his scope-authorization post |
| 2026-10-17 | Admitted `stephen-cleary` | Writing on async and concurrency since 2008-06-20, with in-post corrections. Latest post 2025-12-04, so re-evaluate from 2026-12-04 if nothing follows (#167) |
