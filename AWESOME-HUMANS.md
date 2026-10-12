# Awesome Humans

The vetted sources behind every opinion in this repository. Content only enters the opinions if it traces back to a source listed here, or carries the reserved `house` id (see below).

## Admission criteria

**Scope: published writing.** A source qualifies on text a reader can check by reading it: blog posts, documentation, books, newsletters. Video, conference talks and podcasts are out of scope at this stage, as citable material and as track-record evidence alike. Someone whose output moves to those formats stops qualifying, and someone who starts writing again is a candidate for `vet-source`.

A **source** is the published work of an awesome human: an individual or a publication. Admission requires an **established, proven track record**:

1. **Longevity:** sustained, consistent publishing, for at least two years. Under two years is the watch list, however good the writing is. Beyond the bar there is no second band: the `Since` column carries how far back a record goes, which is more than a bucket could say. Depth is handled by the `**Corroborate.**` marking instead, so a thin-but-long-lived source stays citable with a usage limit rather than being filed below a shorter-lived one.
2. **Depth:** original insight (internals, measurements, worked reasoning), not paraphrased release notes.
3. **Accuracy:** a history of being right; corrections issued when wrong.
4. **Independence of signal:** the content stands on its own merit, not on marketing reach or algorithm-chasing.

Two qualifying rules:

- **Independence is recorded as a usage limit, not a rank.** A source carrying a live independence concern (vendor DevRel or product-team employment producing adoption-focused content with little critical distance) is admitted, and its section under `## Source notes` says what a citation may rest on, typically mechanism rather than adoption, as `avalonia-blog`, `james-montemagno` and `shay-rojansky` each spell out. Vendor employment alone is not a concern: depth with critical distance (`stephen-toub`, `khalid`) needs no limit. A concern grave enough that no limit would contain it is a decline or a watch-listing, as `duende-blog` and `telerik-blog` were.
- **Dormancy blocks admission, with one exception.** A candidate whose publishing has stopped for over a year is watch-listed, not admitted, unless their **writing** demonstrably continues elsewhere (official documentation, another publication, a book), in which case the track record follows the human and the dormant channel is noted. Repository activity is not evidence: commits, releases and issue threads are not what an opinion cites, so a busy GitHub profile beside a silent blog is a dormant source rather than a live one. Documentation authored in a repository does count: what was published is the test, not where the commits landed. The same rule drives re-evaluation of admitted sources (see `vet-source`).

Candidates that don't yet qualify are tracked under [Watch list](#watch-list-track-record-still-forming-or-previously-strong-but-now-dormant) and re-evaluated by the `vet-source` skill. Admission and demotion decisions are recorded in the log at the bottom of this file.

Each source has a stable `id` used by the `sources:` frontmatter in `opinions/`.

Rows in every roster table below are sorted alphabetically by `id`, and so are the sections under [Source notes](#source-notes). Insert new sources in order rather than appending them.

The tables carry only what identifies a source: its id, where to read it, what it covers, and how far back it goes. The evidence behind a row lives in its notes section, in the labelled shape [Source notes](#source-notes) sets out, one sentence per line. That split is what keeps the roster reviewable, since a table row stays a short line and a reworded sentence shows up as that sentence rather than as a rewritten row. The decision log below is the one table exempt from Prettier's alignment, marked with `<!-- prettier-ignore -->`: its Detail column has no natural width, so realigning it rewrote every entry whenever one grew. Each entry is the decision and the fact behind it; the evidence is in the source's notes and the pull request.

**Reserved id: `house`.** The repository owner's own opinions carry the `house` source id; see [HOUSE-OPINIONS.md](HOUSE-OPINIONS.md). Its authority is repository ownership, not track record, so it is exempt from the admission criteria; in exchange, every house opinion must be visibly marked. The marking literal, conflict precedence, and contributor adoption are defined canonically in [HOUSE-OPINIONS.md: How this works](HOUSE-OPINIONS.md#how-this-works). The `house` id never appears in the roster tables below.

**Reserved marking: `**Discovery-only.**`.** Aggregators (link roundups, newsletters, curated lists) are admitted for discovery but never cited: they lead you to a primary source, and that primary source is what an opinion cites. A roster row whose Notes carry this literal is rejected by CI if its id appears in any `sources:` list under `opinions/` or `templates/`. The literal mirrors `**House:**` deliberately: a marking a check can match, not prose a reader has to interpret.

**Reserved marking: `**Corroborate.**`.** A source whose track record is real but whose depth is thin (SEO-shaped roundups, course funnels, release paraphrase with occasional substance) is admitted on that track record and marked. The marking is a usage constraint rather than a track-record claim: an opinion may cite it, but never alone, so every claim it carries also rests on an unmarked citable source. `scripts/validate-sources.cs` rejects any opinion or template whose every citable source is marked. Neither `house` nor a Discovery-only id counts as the unmarked one.

**References: a named package's own material.** When an opinion or template names a package, the project behind it may be linked inline as a reference: its documentation site and in-repository docs, release notes and changelogs, issues and pull requests, and its nuget.org page. The standard or specification behind a named format or protocol qualifies on the same terms, such as an RFC or the IANA time zone database. A reference corroborates and never carries. It can confirm how a thing is set up, what it supports and where it falls short, but the claim it sits beside still rests on a roster source, and a package's own material never decides whether to adopt that package. A reference needs no `vet-source` pass and never appears in `sources:`, so `scripts/validate-sources.cs` does not see it. Cite it in the same `Source: Title` shape as anything else, as in `[xUnit.net: Testing with Native AOT]`.

## Citable: quotable in opinions and templates

Every row here has cleared all four criteria. What separates them is recorded per source rather than by bucket: the `Since` column carries the length of the record, and any independence concern, conflict of interest or usage limit lives in that source's section under [Source notes](#source-notes).

| id                   | Source                                                                                                                                             | Focus                                                                                                           | Since                                       |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- | ------------------------------------------- |
| `aaron-stannard`     | [Aaron Stannard: Aaronontheweb](https://aaronstannard.com/) and his posts on the [Petabridge blog](https://petabridge.com/blog/)                   | OSS versioning and compatibility, property and performance testing, Akka.NET, checking agent code, agent skills | 2010                                        |
| `andrew-lock`        | [Andrew Lock: .NET Escapades](https://andrewlock.net/)                                                                                             | ASP.NET Core internals, "Exploring .NET" per-release series                                                     | 2016                                        |
| `ardalis`            | [Steve Smith: ardalis.com](https://ardalis.com/) and [DevIQ](https://deviq.com/)                                                                   | Application architecture, Clean Architecture and DDD, design patterns and principles                            | 2003                                        |
| `aspnet-blog`        | [ASP.NET Core category, .NET Blog](https://devblogs.microsoft.com/dotnet/category/aspnetcore/)                                                     | ASP.NET Core announcements & deep-dives                                                                         | ~2004                                       |
| `avalonia-blog`      | [Avalonia UI blog](https://avaloniaui.net/blog) and [docs](https://docs.avaloniaui.net/)                                                           | Cross-platform desktop UI, rendering                                                                            | ~2022                                       |
| `awesome-avalonia`   | [awesome-avalonia](https://github.com/AvaloniaCommunity/awesome-avalonia)                                                                          | Curated Avalonia library/resource list                                                                          | 2020                                        |
| `awesome-blazor`     | [awesome-blazor](https://github.com/AdrienTorris/awesome-blazor)                                                                                   | Curated Blazor library/resource list                                                                            | 2018                                        |
| `awesome-dotnet`     | [awesome-dotnet](https://github.com/quozd/awesome-dotnet)                                                                                          | Curated library/tool list                                                                                       | 2014                                        |
| `code-with-mukesh`   | [CodeWithMukesh](https://codewithmukesh.com/)                                                                                                      | Best-practice roundups, EF Core, benchmarks                                                                     | 2020                                        |
| `csharp-digest`      | [C# Digest](https://csharpdigest.net/)                                                                                                             | Weekly newsletter aggregation                                                                                   | ~2017                                       |
| `damien-bowden`      | [Damien Bowden: damienbod](https://damienbod.com/)                                                                                                 | ASP.NET Core security, OpenID Connect, BFF, passkeys                                                            | 2013                                        |
| `derek-comartin`     | [Derek Comartin: CodeOpinion](https://codeopinion.com/)                                                                                            | Event-driven architecture, messaging, CQRS, DDD, modular monoliths                                              | 2012                                        |
| `dotnet-blog`        | [.NET Blog](https://devblogs.microsoft.com/dotnet/)                                                                                                | Everything .NET; canonical announcements                                                                        | ~2004                                       |
| `fsharp-weekly`      | [Sergey Tihon: F# Weekly](https://sergeytihon.com/category/f-weekly/)                                                                              | Weekly F# link aggregation                                                                                      | 2011                                        |
| `gerald-versluis`    | [Gerald Versluis](https://blog.verslu.is/)                                                                                                         | .NET MAUI, Xamarin, Blazor Hybrid                                                                               | 2015                                        |
| `james-montemagno`   | [James Montemagno](https://montemagno.com/)                                                                                                        | Xamarin/.NET MAUI, cross-platform mobile                                                                        | ~2012                                       |
| `jeremy-miller`      | [Jeremy D. Miller: The Shade Tree Developer](https://jeremydmiller.com/)                                                                           | Messaging, event sourcing, persistence, dependency injection, testing long-lived codebases                      | 2007                                        |
| `jetbrains-dotnet`   | [JetBrains .NET Blog](https://blog.jetbrains.com/dotnet/)                                                                                          | C# language features, tooling                                                                                   | ~2010s                                      |
| `jon-skeet`          | [Jon Skeet's coding blog](https://codeblog.jonskeet.uk/)                                                                                           | C# language semantics (records, generics, equality), date/time correctness, API design                          | 2005                                        |
| `khalid`             | [Khalid Abuhakmeh](https://khalidabuhakmeh.com/) and his posts on the [Duende blog](https://duendesoftware.com/blog)                               | Practical .NET, tooling; identity and OpenID Connect since 2025                                                 | ~2012                                       |
| `mark-seemann`       | [Mark Seemann: ploeh blog](https://blog.ploeh.dk/)                                                                                                 | Unit testing and TDD, dependency injection, type-driven and functional design in C#, code quality               | 2006                                        |
| `meziantou`          | [Gérald Barré: Meziantou's blog](https://www.meziantou.net/)                                                                                       | C# language, analyzers, practical best practices                                                                | ~2015                                       |
| `milan-jovanovic`    | [Milan Jovanović](https://www.milanjovanovic.tech/blog)                                                                                            | Architecture, EF Core, CQRS, modular monoliths                                                                  | ~2020                                       |
| `ms-learn`           | [Microsoft Learn: .NET docs](https://learn.microsoft.com/dotnet/) and [Azure Architecture Center](https://learn.microsoft.com/azure/architecture/) | Official documentation, breaking changes, architecture e-books, cloud design patterns                           | ~2016 (docs.microsoft.com era; MSDN before) |
| `nicholas-blumhardt` | [Nicholas Blumhardt](https://nblumhardt.com/)                                                                                                      | Structured logging (Serilog), DI/IoC (Autofac), tracing and diagnostics                                         | 2007                                        |
| `oskar-dudycz`       | [Oskar Dudycz: Event-Driven.io](https://event-driven.io/)                                                                                          | Event sourcing, event-driven architecture, CQRS, vertical slices                                                | 2011                                        |
| `scott-wlaschin`     | [Scott Wlaschin: F# for Fun and Profit](https://fsharpforfunandprofit.com/)                                                                        | F#, functional design, domain modelling                                                                         | 2012                                        |
| `shay-rojansky`      | [Shay Rojansky](https://www.roji.org/)                                                                                                             | .NET ↔ PostgreSQL mapping (Npgsql), EF Core internals                                                           | 2014                                        |
| `stephen-toub`       | [Stephen Toub (author page)](https://devblogs.microsoft.com/dotnet/author/toub/)                                                                   | Runtime/BCL performance, async internals                                                                        | ~2005                                       |
| `steve-gordon`       | [Steve Gordon](https://www.stevejgordon.co.uk/)                                                                                                    | High-performance .NET, HttpClient, Span/ArrayPool                                                               | ~2016                                       |

## Watch list: track record still forming, or previously strong but now dormant

Never quotable as primary sources for opinions: `scripts/validate-sources.cs` rejects a watch-list id in any `sources:` list, whatever the age or quality of the source's back catalogue. Usable for discovery and cross-checking: follow a claim found here to a citable source and cite that one. Re-evaluate periodically.

| id                | Source                                                          | Focus                                             | Since |
| ----------------- | --------------------------------------------------------------- | ------------------------------------------------- | ----- |
| `barry-dorrans`   | [Barry Dorrans: idunno.org](https://idunno.org/)                | .NET security, CVE triage, supply chain           | 2025  |
| `chris-sainty`    | [Chris Sainty](https://chrissainty.com/)                        | Blazor components, routing, auth                  | 2018  |
| `duende-blog`     | [Duende Software blog](https://duendesoftware.com/blog)         | IdentityServer, BFF, OpenID Connect and SAML      | 2020  |
| `mikesdotnetting` | [Mike Brind: Mikesdotnetting](https://www.mikesdotnetting.com/) | ASP.NET web dev, Razor Pages                      | 2007  |
| `morning-brew`    | [The Morning Brew](https://blog.cwa.me.uk/)                     | Daily .NET link aggregation                       | 2008  |
| `nick-polyak`     | [Nick Polyak (Dev.to)](https://dev.to/npolyak)                  | Avalonia/WPF/XAML architecture, reactive patterns | 2008  |
| `scott-brady`     | [Scott Brady](https://www.scottbrady.io/)                       | OAuth, OpenID Connect, identity, cryptography     | —     |
| `syncfusion-blog` | [Syncfusion Blogs](https://www.syncfusion.com/blogs/)           | Release summaries, performance roundups           | —     |
| `telerik-blog`    | [Telerik Blogs: .NET](https://www.telerik.com/blogs)            | Release summaries                                 | —     |
| `udi-dahan`       | [Udi Dahan: The Software Simplist](https://udidahan.com/)       | SOA, messaging, service boundaries, domain events | ~2007 |

## Source notes

The evidence behind each row: what the track record is, what a citation may rest on, and any conflict worth declaring. One section per source, alphabetical by id, and a source with nothing to record has none.

A section opens with its verdict in a line or two, and for a watch-listed source that is what blocks it. A short note stops there. A longer one continues as a list, one `**Label:**` bullet per criterion or rule with something to record, in this order: **Scope**, **Longevity** or **Dormancy**, **Depth**, **Accuracy**, **Authorship**, **Independence**, **Conflict of interest**, then **Cite** for a citable source or **Use** for a watch-listed one, and **Re-evaluate** last. Leave out a label with nothing under it. Each sentence still takes its own line, with continuation lines indented under their bullet, so a reworded sentence diffs as that sentence.

Markings are read from the whole section, heading included, so `**Discovery-only.**` or `**Corroborate.**` in either place marks the source.

### `aaron-stannard`: Citable

Citable on the code-backed engineering posts and on the dotnet-skills he wrote himself, with a conflict on anything Petabridge sells.

- **Scope:** Takes in [dotnet-skills](https://github.com/Aaronontheweb/dotnet-skills), published as 37 skills and 6 subagents with v1.6.0 on 2026-09-16, which counts as authored documentation under the admission rules.
  It was watch-listed as a source of its own on 2026-09-25 and folded in the same day: the track record follows the human, so the repository's 2025-11-12 start does not hold it back.
- **Longevity:** Writing since [2010-05-14](https://aaronstannard.com/blog/page19/) on aaronstannard.com, and on the Petabridge blog since 2015; together the two show posts in every year from 2010 to 2026, and both are live, on 2026-08-21 and 2026-09-23.
  The personal blog alone thins to a handful of posts a year from 2016 to 2019, with its longest gap 347 days (2018-10-16 to 2019-09-28), and the Petabridge blog carries the volume across those years.
- **Depth:** ["Introducing NBench"](https://aaronstannard.com/introducing-nbench/) starts from a throughput regression unit tests could not catch, ["Property Testing in C# with FsCheck"](https://aaronstannard.com/fscheck-property-testing-csharp-part1/) works a shuffle through to a shrunk counter-example, and ["Extend-Only Design"](https://aaronstannard.com/extend-only-design/) and ["OSS Compatibility Standards"](https://aaronstannard.com/oss-compatibility-standards/) reason through schema and API evolution in code.
  His 2026 "Software 2.0" series is the most concrete .NET-specific writing found on verifying agent-written code, and it reports failures as well as wins: ["Planning and Verifying a Greenfield Project"](https://aaronstannard.com/software-2.0-case-study-textforge/) records the agent writing tests that only "checked the 'has test coverage' box".
  dotnet-skills publishes [evaluations](https://github.com/Aaronontheweb/dotnet-skills-evals), real but small: 15 Akka.NET tasks judged by the model that wrote the code.
- **Accuracy:** Sound, with in-post corrections on record: [".NET Core is Boiling the Ocean"](https://aaronstannard.com/dotnetcore-boil-ocean/) (2016) carries Miguel de Icaza's correction on WebAssembly and Mono, and ["The New Rules for Playing in Microsoft's Open Source Sandbox"](https://aaronstannard.com/new-rules-dotnet-oss/) (2020) carries an `EDIT` retracting a misreading of David Fowler.
  dotnet-skills is weaker, because nothing compiles its samples: its own `AGENTS.md` says "There is no build system, tests, or compiled output."
  Checked on 2026-09-25 against SDK 10.0.400, the `testcontainers` samples fail with CS0246 against Testcontainers 4.15.0, the version their own `Version="*"` references restore, and a `csharp-coding-standards` value object fails with CS0111.
  The same skill's records hold collections, which breaks value equality, and a validating record can be bypassed with `with`.
  The package examples pin xUnit v2, FluentAssertions 6 and 7 and `Verify.Xunit`, which pulls in xUnit v2, and six skills ship CI on `dotnet-version: 9.0.x` with 22 mutable action tags and no SHA pins.
  Its release notes do record corrections, including "fabricated APIs, compile errors" in the OpenTelemetry skill and guidance that wrongly called adding optional parameters binary-compatible.
- **Authorship:** He made 68 of 82 dotnet-skills commits, but the skills that held up best under checking (`aot-trimming`, `csharp-nullable-reference-types`, `opentelementry-dotnet-instrumentation`) came largely from Daniel Marbach, Mauro Servienti and Tomasz Masternak of Particular Software.
  Their work is not his writing and is not citable under this id; where one of them has a written track record, vet that person instead.
- **Conflict of interest:** He is CEO of Petabridge, which sells Akka.NET support plans, consulting and training and the Phobos monitoring product through Sdkbin, a marketplace he built, so never cite him alone on adopting Akka.NET, the actor model or Phobos.
  Six dotnet-skills skills and a subagent cover Akka.NET, so the same limit applies to them.
  His free AI tooling (dotnet-skills, dotnet-slopwatch, Netclaw) is promoted in the posts that describe his workflow, a milder conflict worth flagging where an opinion names one of them.
  He removed FluentAssertions from Akka.NET on licensing grounds on [2026-06-10](https://petabridge.com/blog/why-akkadotnet-remove-fluentassertions/), a decision that cost convenience, which is evidence of distance from convenient adoption rather than against it.
- **Cite:** The engineering posts: a large share of the catalogue is essays on the .NET ecosystem, open-source economics and running a business, which argue from experience rather than evidence.
  Petabridge posts for mechanism, not adoption: it is his company's blog and carries Akka.NET release announcements alongside the technical posts.
  A dotnet-skills skill only after its history shows it is his, and for the principle it states: never its package versions or CI pins, and never a code sample that has not been compiled against the citing file's `targets`.

### `andrew-lock`: Citable

The de facto reference for ASP.NET Core internals outside Microsoft; author of _ASP.NET Core in Action_.

### `ardalis`: Citable, **Corroborate.**

Marked because depth is mixed rather than absent, and conflicted on the architecture and data-access topics he would be cited for.

- **Scope:** The id also covers [DevIQ](https://deviq.com/), published by NimblePros with Sarah Dutkiewicz, and the authored documentation at [specification.ardalis.com](https://specification.ardalis.com/) and the [Clean Architecture design decisions](https://ardalis.github.io/CleanArchitecture/), which count as writing where the repositories' commits and releases do not.
- **Longevity:** Writing since [2003-02-04](https://ardalis.com/first-post/), carried across four earlier blogs onto the current domain, and live rather than dormant: latest post 2026-08-19, cadence settled to roughly monthly as effort moves to video and courses.
- **Depth:** ["Domain Modeling - Anemic Models"](https://ardalis.com/domain-modeling-anemic-models/) walks a concrete failure through to the principle, whereas ["From Microservices to Modular Monoliths"](https://ardalis.com/from-microservices-to-modular-monoliths/) carries no migration guidance and closes on a discount code, and the Specification v9 post is release paraphrase by his own account.
  DevIQ is reference-grade on definitions and taxonomy, strongest on [design patterns](https://deviq.com/design-patterns/), but its entries argue in prose and diagrams rather than code or measurements, so cite a measured source alongside it for any claim an opinion rests on.
- **Accuracy:** Sound: no claim was found reversed or left standing wrong.
  In ["Avoid Using C# Events"](https://ardalis.com/avoid-using-csharp-events-in-aspnetcore-apps/) the logged memory figures are non-monotonic with no control loop, so cite the argument and not the measurements.
- **Independence:** Co-founder of NimblePros and a Microsoft MVP rather than an employee, so no independence limit.
- **Conflict of interest:** He sells training, mentoring and courses and maintains Ardalis.Specification, GuardClauses, Result, SmartEnum and the Clean Architecture template, so the conflict is live on the architecture and data-access topics he would be cited for.
  The DevIQ entries closest to data access recommend Ardalis.Specification, whose `Include`/`AsNoTracking` surface is a query object rather than a Specification in Evans's sense, so never present that pattern as settled.
- **Cite:** The .NET posts: the 2026 run is mostly AI and agent tooling.
  Two currency traps: GuardClauses, Result and SmartEnum have shipped no release in about two years, and his Microsoft e-book [Architect modern web applications](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/) is still edition v8.0, so neither is current guidance for `net10.0`.

### `avalonia-blog`: Citable

A product-team blog, release-driven and marketing-adjacent, so cite it and the docs for how a thing works, not for whether to adopt it.

- **Scope:** Widened 2026-08-20 to the project's first-party documentation, which carries mechanism the blog does not, under the same limit.
- **Longevity:** About four years, which clears the bar.
- **Independence:** The concern holds whatever the longevity becomes.

### `awesome-avalonia`: Citable, **Discovery-only.**

A discovery channel, not an opinion source itself; org-backed and actively maintained.

### `awesome-blazor`: Citable, **Discovery-only.**

A discovery channel, not an opinion source itself.

### `awesome-dotnet`: Citable, **Discovery-only.**

Library discovery; verify recommendations independently.

### `barry-dorrans`: Watch list

Blocked on longevity, and nothing else.
Depth and accuracy are recorded here so a promotion is quick.

- **Longevity:** [idunno.org](https://idunno.org/) has been a blog only since 2025-11-18 and carries ten posts, the most recent 2026-08-03.
  The domain is old and Wayback captures reach back to 2003-06-04, but from at least 2015 until 2025-11-17 the site was a static conference and biography page, still advertising a September 2020 talk as upcoming in a December 2024 capture.
  What filled the decade between was talks, which the published-writing scope excludes, so the run that counts is under a year rather than the long record the domain suggests.
- **Depth:** ["The year in .NET Security"](https://idunno.org/the-year-in-net-security/) takes CVE-2025-55248, CVE-2025-55247 and CVE-2025-30399 from root cause to the API that replaced the vulnerable one.
  ["Protecting your .NET app from Server-Side Request Forgery"](https://idunno.org/protecting-your-net-app-from-server-side-request-forgery-ssrf-vulnerabilities/) reasons from the Capital One breach and the cloud metadata endpoint through to mitigation code, and the code-signing post runs about three thousand words of workflow configuration.
- **Accuracy:** He issues dated in-post updates when Microsoft renames a product.
- **Independence:** He is Microsoft's .NET security lead, and the writing is CVE mechanism from the inside rather than adoption copy, so no independence limit would attach to him once the longevity bar is met.
- **Re-evaluate:** From 2027-11, when the blog turns two.

### `chris-sainty`: Watch list

Dormancy, not longevity: no posts since 2023-08 and no verified continuing channel.
The 2018–2023 catalogue is strong and usable for cross-checking on the watch-list terms; citing it needs admission first.

### `code-with-mukesh`: Citable, **Corroborate.**

Blog since 2020, marked for depth: SEO- and roundup-shaped and course-funnelled, so the shape of the guidance is usable and the specifics are not.
The 2026 output carries real benchmarks.

### `csharp-digest`: Citable, **Discovery-only.**

A discovery channel, not an opinion source itself; archive verified back to 2017-12.

### `damien-bowden`: Citable

Independent depth on ASP.NET Core security, OpenID Connect and BFF, built in code rather than described.

- **Longevity:** Writing since [2013-01-31](https://damienbod.com/2013/01/31/welcome-to-my-blog/), technical from the first week, and still publishing on 2026-09-01.
  Every year sampled between those dates carries several pages of posts at a monthly-or-better cadence; 2014, 2017 and 2022 were not counted directly, and the years either side of each leave no room for a gap.
- **Depth:** [The BFF post](https://damienbod.com/2024/04/08/bff-secured-asp-net-core-application-using-downstream-api-and-an-oauth-client-credentials-jwt/) carries the OpenID Connect setup, a YARP transform and a token cache, and explains why the downstream API refuses the user's delegated token.
  [The passkey post](https://damienbod.com/2026/01/05/set-the-amr-claim-when-using-passkeys-authentication-in-asp-net-core/) diagnoses an ASP.NET Core defect, links the runtime issue, and gives the re-sign-in workaround against RFC 8176.
- **Accuracy:** Maintained in the open: posts carry dated History sections recording what each framework version changed.
  When Duende's co-founder objected in the comments that a 2023 post resembled Duende's own sample, he added the attribution and said so in the thread.
- **Independence:** A consultant in Switzerland with no vendor attachment, covering Duende, OpenIddict, Entra ID and ASP.NET Core Identity in turn rather than one of them, so no independence limit.
- **Conflict of interest:** He maintains `angular-auth-oidc-client` and writes tutorials that use it, and he sells application-security workshops through isolutions.
- **Cite:** Much of the catalogue pairs ASP.NET Core with an Angular front end, so check what a sample is actually configuring before citing it.

### `derek-comartin`: Citable, **Corroborate.**

Marked for two reasons that point the same way: no code or measurements, and sponsorship inside the article body.
Cite him for the shape of an argument, and flag the sponsorship on anything touching message queues.

- **Longevity:** Writing since 2012-12-11, at roughly 40–50 posts a year from 2016 through 2025, active 2026-08-19.
  Better known for the YouTube channel, but the blog clears the writing scope on its own: 2,000–2,500-word standalone articles that argue their case in prose, with the companion video embedded rather than standing in for the text.
- **Depth:** The arguments are conceptual, carrying no code and no measurements a reader could compile or reproduce.
- **Independence:** Particular Software's NServiceBus is a paid sponsor **inside the article body**, on a tracked link, in posts whose subject is the sponsor's own product category (verified 2026-09-03 on ["Decoupling in Software Architecture Moves Complexity"](https://codeopinion.com/decoupling-in-software-architecture-moves-complexity/), 2026-08-06).
  He is also a Particular "NServiceBus Champ" and a Microsoft MVP.

### `dotnet-blog`: Citable

Includes Stephen Toub's annual "Performance Improvements in .NET" posts and the "What's new" series.

### `duende-blog`: Watch list

Blocked on independence, not longevity, and on five bylines nobody has vetted.

- **Scope:** Dominick Baier's personal blog is a separate channel and is itself dormant, last posting 2024-10-01.
  It is named rather than linked because the host has stopped completing a TLS handshake, so a citation would fail the link check.
- **Longevity:** Publishing since [2020-10-01](https://duendesoftware.com/blog/20201001-helloduende) at one to four posts a month, so the six-year record would clear the bar several times over.
- **Authorship:** Multi-author, so independence and quality vary by byline, which is the `telerik-blog` and `syncfusion-blog` blocker rather than the `avalonia-blog` limit.
  Where one of the five has a track record, vet the human instead; that is done for `khalid`, whose posts here are citable under his own id with the limit recorded there.
- **Independence:** All four posts read in full for this pass end at the paid product the company sells, in the category the post is about, and none carried a caveat against it.
  The depth is what makes that a blocker rather than a footnote: a four-thousand-word piece with code and diagrams still arrives at Duende BFF as the recommendation.
- **Use:** Discovery and cross-checking on the usual terms.

### `fsharp-weekly`: Citable, **Discovery-only.**

A discovery channel for the F# ecosystem, not an opinion source itself.

### `gerald-versluis`: Citable

Five books, Pluralsight, active on blog and .NET Blog through 2026.
On the MAUI team, with the same independence caveat as `james-montemagno`.

### `james-montemagno`: Citable

Cite the pre-2024 Xamarin/MAUI catalogue only, and expect nothing new from harvests.

- **Longevity:** Blogging since ~2012 (motzcod.es → montemagno.com).
  Re-vetted 2026-08-23 on writing alone, the Merge Conflict podcast having left scope: the blog is still publishing (latest 2025-12-31), so he is not dormant, but the current output is Copilot and tooling and the last .NET post is 2024-01-19.
- **Independence:** A live concern: Microsoft DevRel, adoption-focused, with little critical distance, so weigh independence per piece.

### `jeremy-miller`: Citable, **Corroborate.**

Cite the design essays and never the release notes, and never cite him alone on whether to adopt Marten or Wolverine.

- **Scope:** The [JasperFx news feed](https://jasperfx.net/news/) is deliberately out of scope, being almost all release announcements.
- **Longevity:** Writing since at least 2007-10-18 at codebetter.com, which no longer resolves, so an earlier start is likely but not verifiable; continued on jeremydmiller.com near-daily and active 2026-09-02.
- **Conflict of interest:** He created StructureMap, Marten, Wolverine, Lamar and Alba, and founded JasperFx Software, which sells licences, support plans and CritterWatch around that same stack.
  His writing is almost entirely about it: eight of the nine posts on the front page as of 2026-09-03, five of them release announcements.
  Recorded rather than limited, per the `nicholas-blumhardt` and `oskar-dudycz` precedent.

### `jetbrains-dotnet`: Citable

Institutional; tool-flavoured, but the language explainers keep critical distance, so no independence limit.

### `jon-skeet`: Citable

Created Noda Time; author of _C# in Depth_; Stack Overflow's top contributor.

- **Longevity:** Low cadence (~4–5 posts a year), and much of the recent output is general data modelling from a personal election-data project.
- **Independence:** Formerly Google DevRel (Cloud .NET client libraries), between jobs as of 2026-07; the analysis criticises language design he doesn't own, so no independence limit.
- **Cite:** The C#/BCL posts.
  _C# in Depth_ 4th ed. (2019) pre-dates records and is not current guidance.

### `khalid`: Citable

The personal blog is citable in full, and his Duende posts for platform mechanism, never alone on whether to adopt a Duende product.

- **Scope:** Widened 2026-09-12 to his posts on the [Duende blog](https://duendesoftware.com/blog), which carry his byline and run at roughly two a month from 2025-03 through 2026-10-01.
  The publication is watch-listed separately, so this id reaches his byline and nobody else's.
  The documentation stays evidence rather than citable material, per `shay-rojansky`: 387 commits to the docs repository, the most recent 2026-09-03, are page authorship rather than typo fixes, but the published pages name no author for a citation to rest on.
- **Longevity:** [khalidabuhakmeh.com](https://khalidabuhakmeh.com/) went quiet from 2025-04-22 and resumed on 2026-10-05 with two posts, while the Duende byline carried the writing through the gap.
  The two new posts are introductory: one explains `int.TryParse`, `int.Parse` and `Convert.ToInt32`, and one maps Rails to ASP.NET Core.
  Neither has measurements, and no factual errors were found.
- **Independence:** Now at Duende Software, his JetBrains authorship having ended in 2024.
  The widening follows the `avalonia-blog` precedent and carries the same limit in a sharper form: each of the three Duende posts read in full for this pass closes on Duende BFF or IdentityServer.
  ["Understanding .NET 11 Automatic CSRF Protection"](https://duendesoftware.com/blog/20260901-understanding-dotnet-11-automatic-csrf-protection) (2026-09-01) is the shape that works: the subject is what the framework middleware does to OpenID Connect flows, and the product recommendation sits at the end where a reader can see it coming.
  ["Securing Minimal APIs with Scope-Based Authorization in .NET 10"](https://duendesoftware.com/blog/securing-minimal-apis-with-scope-based-authorization-in-dotnet-10) (2026-09-29) is citable for the `AddAuthorizationBuilder` and `RequireClaim("scope", …)` mechanism, but its sample assumes IdentityServer's one-claim-per-scope token shape, so check the issuer's format before quoting it.

### `mark-seemann`: Citable

Twenty years of writing, with depth verifiable in compiling code and real measurements.

- **Longevity:** Writing since 2006-01-05 on [MSDN blogs](https://learn.microsoft.com/archive/blogs/ploeh/) and on the current blog since 2009-01-28, with over 800 posts there alone, thinning in the mid-2010s, still going on 2026-08-13.
- **Independence:** Self-employed in Copenhagen with no vendor attachment, and the criticism lands on design he does not own, so no independence limit.
- **Conflict of interest:** He sells books and video courses and created AutoFixture, a mild conflict on testing and DI.
- **Cite:** The C#/.NET posts: a consistent third of the technical output is Haskell or F#, and the 2026 run turns from code to language-agnostic essays on AI and software philosophy after ["TDD as induction"](https://blog.ploeh.dk/2026/02/23/tdd-as-induction/) (2026-02-23).
  _Dependency Injection in .NET_ (2011) is superseded by _Dependency Injection Principles, Practices, and Patterns_ (2019).

### `meziantou`: Citable

Very high volume and consistency.

### `mikesdotnetting`: Watch list

Dormancy: dormant since 2023-04, GitHub quiet since 2024-08, no continuing channels.
The 2007–2023 archive and _Razor Pages in Action_ stay usable for cross-checking on the watch-list terms.

### `milan-jovanovic`: Citable

Promoted from the watch list when the longevity bars dropped.

- **Longevity:** About six years, which clears the two-year bar comfortably; the start year is a roster estimate.
- **Conflict of interest:** A commercial funnel (courses, templates, sponsored newsletter), recorded and not grave enough for a usage limit.

### `morning-brew`: Watch list

Dormancy: demoted from the citable roster on 2026-08-18.
Last issue #3995 (2024-08-02) announced a summer break and publication never resumed: the site is up, the RSS feed ends there.
The 2008–2024 archive remains usable for discovery.

### `ms-learn`: Citable

"What's new" and "Breaking changes" pages per release.

- **Scope:** Widened 2026-08-24 from the .NET docs to first-party technical documentation across `learn.microsoft.com`, taking in the .NET architecture e-books and the Azure Architecture Center pattern catalogue.
  Same publisher and editorial standard, so a clarification rather than an admission, following the `avalonia-blog` widening of 2026-08-20.
- **Cite:** For mechanism and pattern trade-offs, never product marketing or a service page arguing for adoption.
  Quote the page's own review date from the `ms.date` metadata field, never `updated_at`, which is a docset build timestamp shared across every page in a bulk republish and can run years ahead of the text.
  The field matters because these pages age unevenly: the microservices e-book is 2018 and 2021 content that has not been reviewed since, carrying `updated_at` values of 2023 and 2024 that are republishes rather than reviews, while the Azure Architecture Center pattern pages are revised on a rolling basis and carry recent `ms.date` values.

### `nicholas-blumhardt`: Citable

Created Autofac and Serilog; founder of Datalust, which sells Seq.

- **Dormancy:** Personal blog dormant since 2024-10, kept citable via the dormancy exception on published writing, not on repository activity.
  The engineering deep-dives on the [Datalust blog](https://datalust.co/blog/) continue, verified 2026-08-23 through a post dated 2026-07-20 and a January 2026 engineering update.
- **Conflict of interest:** Opinions citing him on logging, tracing or DI must flag that he authors and sells the tools in question.
- **Cite:** The personal blog and the engineering write-ups, never Seq release announcements.

### `nick-polyak`: Watch list

Dormancy: demoted from the citable roster on 2026-08-23.
Last post 2025-05-04 on Dev.to, the CodeProject archive (2008–2024) is read-only, and the 2025–26 GitHub output is code and version bumps with no documentation among it, so the exception does not reach him.
Back catalogue not citable while watch-listed; re-vet if the writing resumes.

### `oskar-dudycz`: Citable, **Corroborate.**

Event sourcing and event-driven design in worked, typed examples, from someone who builds tooling for the same patterns.

- **Longevity:** Writing since 2011-09-21 (Polish-language blog, migrated into the current site) with a sustained run from 2020, roughly 50 posts a year through 2023, active 2026-08-10.
- **Depth:** Verified on worked examples with traced scenarios and typed code.
- **Conflict of interest:** He co-maintains Marten and Emmett and sells Event Sourcing workshops.
  Recorded rather than limited, per the `milan-jovanovic` and `nicholas-blumhardt` precedent, and the marking carries the usage rule instead.
- **Cite:** The .NET material: he publishes across .NET and Node, so check the language of any example before citing it.

### `scott-brady`: Watch list

Dormancy, by his own statement on the site: "While I am no longer actively blogging since moving away from individual contributor roles, this website includes an archive of material for those looking to learn OAuth and web security."

- **Dormancy:** The last substantial article is "Understanding WS-Federation: A modern primer for an obsolete protocol" from 2024-04, and the 2026-03 entry above it in [the article index](https://www.scottbrady.io/articles) is a conference write-up.
  Nothing carries the dormancy exception: the Substack newsletter last issued 2023-03-23, and the most recent live Pluralsight course is from 2022-08.
  Start year not established, the blocker having settled the question before longevity was reached.
- **Use:** The OAuth and identity archive stays usable for cross-checking, and citing it needs admission first.

### `scott-wlaschin`: Citable

The definitive independent F# resource; author of _Domain Modeling Made Functional_.

### `shay-rojansky`: Citable

Lead maintainer of Npgsql and on Microsoft's EF Core team, with the deepest writing on .NET↔PostgreSQL type mapping, which no other roster source covers.

- **Longevity:** Nine years of writing, 2014–2023.
- **Dormancy:** Long-form writing dormant since 2023 on both roji.org and the .NET Blog.
  Held on the dormancy exception by the documentation he authors, re-verified 2026-08-23: the EF Core 11 what's-new and breaking-change pages in `dotnet/EntityFramework.Docs`, latest commit 2026-05-12, which publish to Microsoft Learn and so count as writing.
- **Independence:** A live concern: the citable body is first-party design rationale about products he maintains.
- **Cite:** For mechanism, meaning how Npgsql and EF Core behave and why they were designed that way, and corroborate adoption judgments elsewhere.
  The 2014–2023 blog catalogue is citable; expect nothing new from blog harvests.

### `stephen-toub`: Citable

Publishes via `dotnet-blog`; also the "Deep .NET" video series with Scott Hanselman.

### `steve-gordon`: Citable

Cadence variable (heavy 2024, quiet 2025, active again 2026); at Elastic.
Back catalogue authoritative.

### `syncfusion-blog`: Watch list

Independence and depth, not longevity: marketing-adjacent, quality varies by author.

### `telerik-blog`: Watch list

Independence and depth, not longevity: marketing-adjacent, quality varies by author.
The lowered longevity bar does not touch this blocker.

### `udi-dahan`: Watch list

Dormancy: last post 2016-02-19, a decade silent, and no continuing written channel: the talks and the Particular training courses are out of scope under the 2026-08-23 narrowing.

- **Longevity:** The start year is an estimate from a testimonial referring to the blog in 2007.
- **Independence:** Founder and CEO of Particular Software, which sells NServiceBus, so a concern would also apply.
- **Use:** The 2005–2016 archive stays usable for discovery and cross-checking, and `ms-learn` already cites him second-hand on domain events.

## Decision log

<!-- prettier-ignore -->
| Date | Decision | Detail |
| --- | --- | --- |
| 2026-08-12 | Initial roster | Seeded Tier 1, Tier 2 and the watch list from research verified against the .NET 10 era |
| 2026-08-12 | Admitted `scott-wlaschin`, `fsharp-weekly` (Tier 1) | Filled the F# gap; writing since 2012 and 2011 |
| 2026-08-12 | Admitted `steve-sanderson` (Tier 1) | Blog dormant since 2020-01; admitted on talks and repositories under the dormancy exception (removed 2026-08-23) |
| 2026-08-12 | Admitted `james-montemagno` (Tier 2) | Blogging since ~2012; capped for independence as Microsoft DevRel |
| 2026-08-12 | Admitted `gerald-versluis` (Tier 2) | Blogging since 2015-03; capped for independence as a MAUI team member |
| 2026-08-12 | Admitted `awesome-blazor` (Tier 2, discovery-only) | Curated list since 2018, actively maintained |
| 2026-08-12 | Watch-listed `chris-sainty` | Strong 2018–2023 Blazor catalogue, dormant since 2023-08 |
| 2026-08-12 | Watch-listed `avalonia-blog` | Blog only since ~2022: longevity blocker (admitted 2026-08-14) |
| 2026-08-12 | Admitted `nick-polyak` (Tier 1) | Publishing since 2008 on CodeProject, then Dev.to (demoted 2026-08-23) |
| 2026-08-12 | Watch-listed `awesome-avalonia` | Created 2020-02, short of the then 8-year discovery bar (admitted 2026-08-14) |
| 2026-08-12 | First harvest (window 2025-11-01 → 2026-08-12) | Seeded 8 opinion files; flagged `mikesdotnetting`, `khalid` and `scott-wlaschin` for `vet-source` |
| 2026-08-12 | Demoted `mikesdotnetting` (Tier 1 → watch list) | Dormant since 2023-04 with no continuing channel |
| 2026-08-12 | Annotated `khalid` (kept Tier 1) | Blog quiet since 2025-04; kept on GitHub activity, a basis withdrawn 2026-08-23 |
| 2026-08-12 | Corrected `scott-wlaschin` harvest flag | Low cadence, not dormant: the sweep missed his 2025-12 posts |
| 2026-08-12 | Updated `steve-gordon` note | Publishing again from 2026-01 |
| 2026-08-12 | Reserved `house` id | The owner's own opinions, exempt from admission and always visibly marked (PR #7) |
| 2026-08-14 | **Experiment:** lowered the longevity bars | Tier 1 `~decade → 6+ years`, Tier 2 `8+ → 3+ years`; the rows below follow from re-running `vet-source` and `harvest-sources` |
| 2026-08-14 | Promoted `andrew-lock`, `steve-gordon` (T2 → T1) | Both publishing since ~2016 |
| 2026-08-14 | Promoted `jetbrains-dotnet` (T2 → T1) | Publishing since the early 2010s; language explainers keep critical distance |
| 2026-08-14 | Promoted `awesome-blazor` (T2 → T1, discovery-only) | Curated since 2018 |
| 2026-08-14 | Admitted `milan-jovanovic` (Tier 1) | ~2020 start, weekly through 2026-08; conflict of interest recorded, not capped |
| 2026-08-14 | Admitted `csharp-digest` (Tier 1, discovery-only) | Archive verified back to 2017-12 |
| 2026-08-14 | Admitted `awesome-avalonia` (Tier 1, discovery-only) | Created 2020-02, which the new bar clears |
| 2026-08-14 | Admitted `nick-chapsas` (Tier 2) | Capped on depth: trend-driven video, with courses over the same ground (removed 2026-08-23) |
| 2026-08-14 | Admitted `code-with-mukesh` (Tier 2) | Blog since 2020; capped on depth as SEO-shaped and course-funnelled |
| 2026-08-14 | Admitted `avalonia-blog` (Tier 2) | ~2022 start clears the 3-year bar; capped for independence as a product-team blog |
| 2026-08-14 | Watch list retained: 4 of 10 rows | `chris-sainty` and `mikesdotnetting` on dormancy, `telerik-blog` and `syncfusion-blog` on independence and depth |
| 2026-08-14 | Harvest of newly admitted sources (2025-11-01 → 2026-08-14) | Folded in Avalonia 12 (`ui-frameworks.md`) and opened `architecture.md` and `data-access.md` |
| 2026-08-17 | **Experiment:** Tier 1 `6+ → 5+`, Tier 2 `3+ → 2–5 band` | No roster changes: every blocked row is blocked on something other than longevity |
| 2026-08-17 | Admitted `nicholas-blumhardt` (Tier 1) | Publishing since 2007-07; personal blog dormant since 2024-10, held on the Datalust engineering series; sells Seq, recorded as a conflict |
| 2026-08-18 | Harvest (2026-08-13 → 2026-08-18, plus first sweep of `nicholas-blumhardt` and `jon-skeet`) | Folded into `testing.md`, `runtime-performance.md` and `csharp.md`, and opened `logging.md`; flagged `james-montemagno`, `nick-polyak` and `khalid` |
| 2026-08-18 | Admitted `jon-skeet` (Tier 1) | Publishing since 2005-09; filled the records and immutable-collection gaps |
| 2026-08-18 | Demoted `morning-brew` (Tier 1 → watch list) | Last issue 2024-08-02; an aggregator has no second channel for the dormancy exception |
| 2026-08-18 | Re-evaluated `khalid`, `nick-polyak`, `james-montemagno` (no tier changes) | Notes narrowed for all three |
| 2026-08-20 | Widened `avalonia-blog` scope to the project docs (no tier change) | Gave the Avalonia localization mechanism a citable home; same publisher and cap |
| 2026-08-21 | Admitted `shay-rojansky` (Tier 2) | Filled the .NET↔PostgreSQL gap; capped for independence; dormant since 2023-05, admitted on the exception |
| 2026-08-23 | Clarified: the watch list never feeds `sources:` | Back catalogue included, as `validate-sources.cs` enforces; a demotion re-sources or drops its claims in the same PR |
| 2026-08-23 | Removed `nick-chapsas` (was Tier 2) | Never cited, and his notes already said to cite the primary source instead |
| 2026-08-23 | Scope narrowed to published writing | Video, talks and podcasts no longer count, as citations or as track record |
| 2026-08-23 | Removed `steve-sanderson` (was Tier 1) | Stood on talks and repositories, which the narrowed scope excludes; never cited |
| 2026-08-23 | Repository activity is not evidence of a live source | Commits and releases do not keep a dormant source admitted; documentation authored in a repository does |
| 2026-08-23 | Re-verified `nicholas-blumhardt` under the new rule (no tier change) | The Datalust blog alone holds the exception |
| 2026-08-23 | Depth handled by a `**Corroborate.**` marking, not a tier cap | `code-with-mukesh` moves to Tier 1 marked; CI rejects an opinion whose only citable source is marked |
| 2026-08-23 | Demoted `nick-polyak` to the watch list (was Tier 1) | No writing since 2025-05-04; his two `ui-frameworks.md` citations re-sourced to the Avalonia docs |
| 2026-08-23 | Re-vetted `james-montemagno` and `shay-rojansky` on writing (no tier changes) | Montemagno citable pre-2024 only; Rojansky held on EF Core documentation authorship |
| 2026-08-24 | Admitted `oskar-dudycz` (Tier 1, marked **Corroborate.**) | Sustained from 2020; filled the event-driven and vertical-slice gaps; conflict recorded |
| 2026-08-24 | Watch-listed `udi-dahan` | Dormant since 2016-02-19 with no continuing written channel |
| 2026-08-24 | Widened `ms-learn` scope across `learn.microsoft.com` (no tier change) | Took in the Azure Architecture Center; cite for mechanism and trade-offs, quoting each page's review date |
| 2026-09-03 | Admitted `mark-seemann` (Tier 1) | Twenty years of writing, depth verifiable in code and measurements; conflict recorded |
| 2026-09-03 | Admitted `jeremy-miller` (Tier 1, marked **Corroborate.**) | Writing since 2007; writes almost only about the stack his company sells |
| 2026-09-03 | Admitted `derek-comartin` (Tier 1, marked **Corroborate.**) | Blog clears the writing scope on its own; marked for no code or measurements and in-body sponsorship |
| 2026-09-03 | Admission pass aimed at `opinions/architecture.md` | Added `mark-seemann` as the unmarked design source the file lacked |
| 2026-09-04 | Named the date field in the `ms-learn` citation rule (no tier change) | The rule said to quote "the page's own review date" without naming it, and a research pass followed it into six wrong dates by quoting `updated_at`, a build timestamp; the rule now names `ms.date` |
| 2026-09-10 | Admitted `ardalis` (Tier 1, marked **Corroborate.**) | Writing since 2003, covering DevIQ and his documentation; marked for mixed depth; conflict recorded |
| 2026-09-12 | Admitted `damien-bowden` (Tier 1) | Writing since 2013-01-31; the BFF and browser-app authentication depth source the `security` research topic lacked |
| 2026-09-12 | Watch-listed `barry-dorrans` | Blog only since 2025-11-18; strong writing recorded in notes; re-evaluate from 2027-11 |
| 2026-09-12 | Widened `khalid` scope to his Duende writing (no tier change) | His bylined Duende posts come into scope; never cite him alone on adopting a Duende product |
| 2026-09-12 | Watch-listed `duende-blog` | Blocked on independence and five unvetted bylines; Khalid's posts citable under his own id |
| 2026-09-12 | Watch-listed `scott-brady` | Dormant by his own statement; last substantial article 2024-04 |
| 2026-09-12 | `vet-source` pass on the `security` research topic's candidates | Seven checked: `damien-bowden` admitted, `barry-dorrans` watch-listed, OWASP Cheat Sheet Series and Philippe De Ryck declined |
| 2026-09-14 | Harvest (2026-08-19 → 2026-09-14) | Folded into `testing.md`, `ci.md`, `runtime-performance.md`, `ui-frameworks.md`, `fsharp.md`, `aspnet-core.md` and `data-access.md`, and gave `architecture.md` its unmarked corroboration; flagged `scott-wlaschin`, `steve-gordon` and `nicholas-blumhardt` |
| 2026-09-15 | Defined references (no roster change) | A named package's own material may corroborate a roster-sourced claim inline, outside `sources:` (raised in review of #74) |
| 2026-09-20 | Collapsed Tier 1 and Tier 2 into one **Citable** table | No skill or check read the tiers differently; longevity is now two years, carried by `Since`, and the independence cap became a usage limit in the notes. No source's citability changed |
| 2026-09-25 | Admitted `aaron-stannard` | Sixteen years of writing, with depth in code-backed engineering posts; gave the `ai-usage` research topic a .NET practitioner source; Petabridge recorded as a conflict |
| 2026-09-25 | Watch-listed `dotnet-skills` | Under a year old, samples verified not to compile against current packages, and multi-author; re-evaluate from 2027-11 |
| 2026-09-25 | Folded `dotnet-skills` into `aaron-stannard` | Same human, so his track record carries the repository; uncompiled samples and co-authored skills became usage limits in his notes |
| 2026-10-09 | Harvest (2026-09-15 → 2026-10-09) | Folded into `runtime-performance.md` (ILLink substitutions, a .NET 11 runtime async aside) and `ci.md` (signer allow-list upkeep); flagged `khalid`, `james-montemagno` and `nicholas-blumhardt` |
| 2026-10-09 | Re-evaluated `khalid` (no tier change) | His personal blog resumed on 2026-10-05 after a gap from 2025-04-22, which the Duende byline bridged. The notes now record the restart and a token-shape limit on citing his scope-authorization post |
