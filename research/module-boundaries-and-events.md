---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-10
sources:
  [
    ms-learn,
    ardalis,
    mark-seemann,
    jeremy-miller,
    oskar-dudycz,
    derek-comartin,
    milan-jovanovic,
  ]
---

# Module boundaries and events

Research for the open TODO in [architecture.md](../opinions/architecture.md), which now reads "extend with transactional boundaries across modules, and when a module has earned a process boundary". Event sourcing and event-driven architecture sit in the same topic because both extensions depend on them.

**The TODO was longer when this topic opened, and two of its asks are already discharged.** It asked first for corroboration from an independent source, then for messaging between modules, and only then for the two extensions that remain. The harvest of 2026-09-14 discharged the first two: it added `mark-seemann`, `ms-learn` and `derek-comartin` to the file's `sources:`, added the boundary-explicitness and integration-event opinions, and narrowed the comment. So the corroboration question isn't open, and this file is no longer answering it. What it does instead is audit the discharge, and the audit doesn't come out clean, because the opinion the harvest left untouched is the one no source swept supports.

This is the third pass. The first, on 2026-08-24, had one independent source and reported that half the TODO was blocked. The second, on 2026-09-04, swept the four sources admitted on 2026-09-03 and found they split the opinions rather than confirming the file. This pass sweeps `ardalis`, admitted 2026-09-10, and it lands on the two opinions the second pass left least settled.

**The headline is that the disagreement over layering was smaller than it looked, because one side of it was a single author quoted twice.** The second pass weighed `ms-learn`, which prescribes layering solution-wide, against `mark-seemann` and `jeremy-miller`, who reject the application layer. The `ms-learn` page making that case is written by `ardalis`, and admitting him puts both citations under one name. He has also moved: the 2021 article gives an unqualified three-project prescription, and by 2024 he is calling Clean Architecture one option among several, denying that it delivers modularity at all, and shipping a second template that organises by vertical slice.

That reverses the second pass's verdict on the fourth opinion. It reported that no source swept held the position; the position now has partial support from the architect most identified with the pattern it names. The recommended rewrite is unchanged, and it's now positively corroborated rather than merely unopposed.

The pass also closes the commands-versus-events gap that the second pass left open for want of a third voice, and adds the first concrete, code-level enforcement rule for a module boundary that this topic has found.

## Sources swept

| id                | Standing                                                                                                                                                         | Used for                                                                                   |
| ----------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| `ms-learn`        | Citable, independent of the other sources but not of two of them by authorship: the architecture e-book pages are written by `ardalis` and by `james-montemagno` | Corroboration, transactional boundaries, process boundaries, idempotency, event sourcing   |
| `ardalis`         | Citable, **Corroborate.**                                                                                                                                        | Layering scope, boundary enforcement in EF Core, command and event ownership, Conway's Law |
| `mark-seemann`    | Citable, independent, unmarked                                                                                                                                   | Boundary explicitness, module contracts, coupling, layering                                |
| `jeremy-miller`   | Citable, **Corroborate.**                                                                                                                                        | Modular monolith criteria, vertical slices, outbox, event sourcing scope                   |
| `oskar-dudycz`    | Citable, **Corroborate.**                                                                                                                                        | Slices and modules, internal and external events, event sourcing scope, cutting services   |
| `derek-comartin`  | Citable, **Corroborate.**                                                                                                                                        | Boundary ownership, commands versus events, the cost of decoupling                         |
| `milan-jovanovic` | Citable, conflict of interest noted                                                                                                                              | Module communication patterns, event sourcing framing (carried from the first pass)        |
| `andrew-lock`     | Citable, independent                                                                                                                                             | Swept 2026-08-24, nothing on this ground (negative result, recorded below)                 |

`jimmy-bogard` and `kamil-grzybek` remain unvetted and no claim here rests on them. Their status is settled in item 4 at the end.

The `ardalis` id covers three properties and they aren't of equal weight. The blog is dated and citable. [DevIQ](https://deviq.com/) is reference-grade on taxonomy but shows no date to a reader at all, which the [Freshness](#freshness) section takes up. The Clean Architecture template documentation shows no date either and is the weakest of the three.

## 1. Corroboration, opinion by opinion

The first pass answered the TODO at the level of the file. Four sources later that's too coarse, because they split the opinions differently. Taken one at a time:

`architecture.md` now carries seven opinions. The five below are the ones this sweep bears on and the ones that predate it. The two the 2026-09-14 harvest added came from this topic's own sources and are already the repository's position, so they are treated as settled here and their material is developed in [section 2](#2-messaging-between-modules) rather than re-adjudicated. They are that a module boundary should be something the compiler propagates, and that integration events go out while domain events stay in.

### "Default to a modular monolith, not microservices"

**Corroborated, but more weakly than the first pass recorded, and every voice hedges.**

The first pass led on `ms-learn` "confirmed by a page reviewed as recently as 2026-07-08". That date is wrong. It's the page's `updated_at`, a docset build timestamp; the page's own review date is the `ms.date` field, and it reads 2021-12-12. The text descends from the 2018-era e-book and is authored by `ardalis`, who maintains the Clean Architecture solution template it links to. The [Freshness](#freshness) section below carries the full correction, which affects every `ms-learn` citation in this file.

The substance survives the date. `ms-learn` still says to start monolithic and treat separation as a later choice, and still names the cost: "If you can't deliver independent feature slices of the application, separating it only adds complexity." ([Microsoft Learn: Common web application architectures](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures), reviewed 2021-12-12)

What is new is that neither architect admitted for this gap will state the opinion as flatly as `architecture.md` does.

`mark-seemann` declines to prescribe a starting point at all. He reads the whole monolith-versus-services question as fashion: "One decade, service-oriented architecture (SOA) is cool; the next, consolidation sets in; then it's micro-services; and, as far as I can tell, monoliths are on the way in again, although I'm sure that we'll find something else to call them." ([Seemann: Pendulum swings](https://blog.ploeh.dk/2021/02/22/pendulum-swings/), 2021-02-22)

He goes further in the one place he does give advice, and it points away from the monolith. Describing his own book's sample code, he attributes its decoupling to years of F# and Haskell discipline rather than to any structure a reader could copy:

> I'm confident that the sample code is nicely decoupled, even though it's packaged as a monolith. But unless you have a similar experience, I recommend that you separate your code base into multiple packages.

([Seemann: Decomposing CTFiYH's sample code base](https://blog.ploeh.dk/2023/09/04/decomposing-ctfiyhs-sample-code-base/), 2023-09-04)

That's a recommendation to enforce module boundaries physically, at the package level, precisely because most teams won't hold them by convention. It's compatible with a modular monolith and it isn't the same opinion.

`jeremy-miller` is more sceptical still, and says so twice: "I'm still dubious that the modular monolith idea is going to be a panacea" ([Miller: Actually Talking about Modular Monoliths](https://jeremydmiller.com/2024/04/08/actually-talking-about-modular-monoliths/), 2024-04-08), and "I'm unfortunately dubious about the mechanics and whether or not this is just some 'modular' lipstick on the old 'monolith' pig" ([Miller: Thoughts on "Modular Monoliths"](https://jeremydmiller.com/2024/04/01/thoughts-on-modular-monoliths/), 2024-04-01). He remains "bullish on the long term usefulness of micro-services" for genuine bounded contexts.

His diagnosis of why old monoliths hurt is worth recording, because it doesn't blame the monolith: the pain came from "The usage of prescriptive architectures like Clean Architecture or Onion Architecture solution templates". That's an argument for the first opinion by way of contradicting the fourth.

`ardalis` is the fourth voice and the warmest, but he argues the migration rather than the default. His case is aimed at teams already in trouble: "What do you do when you find yourself in microservice hell?" The answer is to "Migrate to a modular monolith", keeping modularity while avoiding "the fallacies of distributed computing, such as network latency, bandwidth, and failure". ([Ardalis: From Microservices to Modular Monoliths](https://ardalis.com/from-microservices-to-modular-monoliths/), 2024-07-10). The post is three minutes long, carries no migration guidance despite its title, and closes on a course discount code, so it's the argument that's citable rather than the substance.

Where he does address greenfield choice he makes it conditional, listing a small-to-medium team, an early-stage project, a well-understood domain with uncertain future scale, or an escape from a big ball of mud. ([Ardalis: Introducing Modular Monoliths: The Goldilocks Architecture](https://ardalis.com/introducing-modular-monoliths-goldilocks-architecture/), 2024-02-21). The same post names the outcome worth avoiding: the distributed monolith "has all of the disadvantages of microservices (complexity, cost) as well as all the disadvantages of a monolith (many dependencies, difficulty making changes). It's the worst of all worlds and should be avoided if at all possible."

He also supplies the clearest counter-signal in the sweep, and it isn't about technology. Reading Conway's Law forward, he argues the decomposition should follow the organisation: microservices "map very well to bounded contexts", and "if these teams aren't aligned with the software modules you're building and shipping, you're going to have problems". ([Ardalis: Conway's Law, DDD, and Microservices](https://ardalis.com/conways-law-ddd-and-microservices/), 2020-08-26). On that reading a single team is the thing that makes a monolith right, and the opinion is a claim about team size wearing architectural clothing.

**What to do with it.** The opinion holds. The confidence should come down. The accurate form is that a modular monolith is the cheaper default because the process boundary is reversible and the module boundary isn't, rather than that the industry has settled on it. Four of the five sources state it conditionally, and the conditions they give are about team and organisation more than about code.

### "Organise the inside of a module as vertical slices"

**Corroborated by two sources, contradicted in framing by the one independent source.**

`jeremy-miller` states it structurally and recently: a slice "organizes code around features instead of technical layers. A slice owns its whole pathway: take the input, do the work, produce the output, all in one place." ([Miller: The Codebase Is the Prompt](https://jeremydmiller.com/2026/06/04/the-codebase-is-the-prompt-wolverine-vertical-slices-and-ai-assisted-development/), 2026-06-04). He calls himself "a proponent of 'Vertical Slice Architecture' code organization and a harsh critic of layered architecture approaches (Clean/Onion/Hexagonal) as they are commonly practiced", and attributes the term to Jimmy Bogard rather than claiming it.

`oskar-dudycz` agrees and sharpens the unit: "A vertical slice is one piece of functionality, cut through the whole application. For me, a slice is more a function than an entity." ([Dudycz: Vertical slices, their ownership and external dependencies](https://event-driven.io/en/vertical-slices-and-dependencies/), 2026-08-10)

`mark-seemann` uses the words and means something else by them. Vertical slicing appears in his writing as a way of working, not a way of arranging code:

> As I describe in Code That Fits in Your Head, I usually develop (vertical) feature slices one at a time, utilising an outside-in TDD process, during which I also figure out how to save or retrieve data from persistent storage.

([Seemann: Do ORMs reduce the need for mapping?](https://blog.ploeh.dk/2023/09/18/do-orms-reduce-the-need-for-mapping/), 2023-09-18)

Two sentences later in the same post he names the structure that code sits in, and it isn't slices: "the architecture is Ports and Adapters, or, if you will, Clean Architecture." Across 832 posts he never uses the capitalised term "Vertical Slice Architecture".

`ardalis` treats it as a named architecture style and gives it equal billing with the one he is known for. DevIQ's architecture index lists Clean Architecture and Vertical Slice Architecture as sibling styles, and he ships a template built on the second: the Minimal Clean Architecture template "maintains the core principles of Clean Architecture—separation of concerns, dependency inversion, and testability—while reducing complexity through a single-project Vertical Slice Architecture (VSA)", organised "by feature rather than technical layer". ([Ardalis: Minimal Clean Architecture](https://ardalis.github.io/CleanArchitecture/minimal-clean-architecture/), no date shown)

He is also the earliest voice on the term in this sweep, using it in 2012 for splitting user stories rather than arranging code. ([Ardalis: Stories Too Big — Vertical Slices](https://ardalis.com/stories-too-big--vertical-slices/), 2012-02-01). That is `mark-seemann`'s reading, not `jeremy-miller`'s, and the order matters: the delivery sense of the term is the older one and the code-organisation sense grew out of it.

**What to do with it.** The opinion holds, and it now has three marked sources plus a template rather than two marked sources. The file should still stop implying the framing is uncontested, because the one unmarked source in the sweep reads the same word as a delivery process, and because that reading came first.

### "Vertical slices aren't modules"

**Corroborated, and this is the cleanest result in the sweep.**

`oskar-dudycz` states the containment relation directly: "A module is a logical grouping of slices." He then draws the boundary rule that follows from it, and it's stricter than `architecture.md` currently says: "Everything outside the slice is external, whether it's the next folder or another system." On persistence he separates the two units explicitly: "Database schemas go per module... A slice is a feature, not a persistence boundary." (same post, 2026-08-10)

`jeremy-miller` confirms the distinction by complaining that the field ignores it: "there's a ton of disagreement about what the hell it is that 'vertical slice architecture' actually means and a lot of folks conflating that with bounded contexts or micro-services." ([Miller: We Don't Need No Stinkin' Repositories](https://jeremydmiller.com/2025/02/27/we-dont-need-no-stinkin-repositories-and-other-observations-on-dotnetrocks/), 2025-02-27)

`derek-comartin` supplies the ownership test that makes a module a module: "Logical boundaries are all about ownership. Who owns the data? Who owns the business rules? Who owns the invariants you need to enforce?" ([Comartin: Modular Monolith Boundaries Done Wrong](https://codeopinion.com/modular-monolith-boundaries/), 2026-06-02). His enforcement test is the query log rather than the namespace: "Your code might say Sales and Warehouse. But what do your queries say?" ([Comartin: Stop Joining Tables In Your "Modular" Monolith](https://codeopinion.com/stop-joining-tables-in-your-modular-monolith/), 2026-05-27)

**`ardalis` supplies the enforcement rule, and it's the first thing in this topic that a reviewer can check line by line.** Asked how to map an Entity Framework relationship to an entity in another module, his answer is that you don't: "don't use navigation properties for entities that live outside your module. Instead always just use keys". The reframing he offers is the useful part, because it generalises past EF Core:

> Imagine instead that the data owned by other modules is outside not just that module but outside your organization.

He states the rule for both units the opinion separates: "data that is outside of an aggregate or module should only be referenced using its key or ID, not as a navigation property". Where local access is needed, the escape is a Materialized View kept as "essentially a read-only cache", synchronised by events, and "If you need to make changes, send a command to the module that owns that data." ([Ardalis: Modeling Navigation Properties Between Aggregates or Modules](https://ardalis.com/navigation-properties-between-aggregates-modules/), 2024-06-19)

This is the same boundary `derek-comartin` polices from the query log, one level earlier: a navigation property is how the cross-module join gets written in the first place. It carries real code, which almost nothing else in this topic does, and the code is a mapping the reader either has or hasn't written.

**What to do with it.** Keep the opinion and add two rules under it. Schema per module, not per slice, is the concrete form of the boundary and it's testable. Reference other modules by key and never by navigation property is the form a reviewer can enforce in Entity Framework configuration, with the materialised view as the named exception and a command as the only write path back.

### "Apply Clean Architecture per slice, not per solution"

**Partly corroborated, after the second pass reported it contradicted from both directions with no source holding it. The change is `ardalis`, and it comes with a caveat about who was being counted.**

`ms-learn` contradicts it upward, prescribing the layering solution-wide and calling that arrangement "the most appropriate way to structure non-trivial monolithic applications". That page is written by `ardalis`, so it isn't a second voice beside him; it is his 2021 position under a Microsoft masthead.

The two new architecture sources contradict it downward, arguing the layering shouldn't be applied at either scale. `mark-seemann` is the more absolute, and this is his most recent statement on layering:

> I usually don't abstract application behaviour from frameworks. I don't create 'application layers', 'use-case classes', 'mediators', or similar. This is a deliberate architecture decision.

([Seemann: Ports and fat adapters](https://blog.ploeh.dk/2025/04/01/ports-and-fat-adapters/), 2025-04-01)

He also denies that the named architectures are distinct things to choose between, which removes the ground the opinion stands on. "If you apply the Dependency Inversion Principle to Layered Architecture, you end up with Ports and Adapters." ([Seemann: Layers, Onions, Ports, Adapters: it is all the same](https://blog.ploeh.dk/2013/12/03/layers-onions-ports-adapters-its-all-the-same/), 2013-12-03). Eleven years later he reports the same thing from consulting: "Today, most organizations that I consult with will tell me that they've decided on Ports and Adapters. Even so, if you do it right, it is the same architecture." ([Seemann: Three data architectures for the server](https://blog.ploeh.dk/2024/07/25/three-data-architectures-for-the-server/), 2024-07-25)

`jeremy-miller` attacks the specific benefit the layering is sold on: "I have almost never needed to reason about a system's entire data access layer in isolation even though that's held up as an advantage of Clean/Onion/Hexagonal layering approaches." He extends it to the repository abstraction, across tools: "I would generally recommend against using wrapping repository abstractions around low level persistence tooling like Marten, EF Core, or Dapper in systems in most cases", because such an interface "does pretty well nothing to add any value", pushes teams to a least-common-denominator API, and doesn't deliver the swappability it promises, which is "patently not true". (2025-02-27)

He also names the runtime cost, which is the strongest form of the argument because it's observable rather than aesthetic: "Big call stacks of a controller calling a mediator tool that calls one service that calls other services that call different repository abstractions that all make database queries is a common source of chattiness because it's hard to even see where all the chattiness is coming from by reading the code." ([Miller: Network Round Trips are Evil](https://jeremydmiller.com/2024/07/08/network-round-trips-are-evil/), 2024-07-08)

**`ardalis` is the source that moves this opinion, and he moves it by conceding most of the case against the pattern he is identified with.** His 2021 position is the unqualified one: "there are basically three projects: Core, Infrastructure, and Web", and "Some might argue that this is over-engineered but the end result is typically just 3 projects and I've never found that to be too many for any application of non-trivial complexity." ([Ardalis: Clean Architecture with ASP.NET Core](https://ardalis.com/clean-architecture-asp-net-core/), 2021-11-30). Vertical slices aren't mentioned on that page.

By 2024 he has narrowed the claim to almost exactly the conditional this repository's opinion body already contains:

> Clean Architecture, aka Ports-and-Adapters, has a primary goal of reducing tight coupling from the business rules of the system to infrastructure, and in particular, the database. That's it. It's not a panacea and it doesn't offer feature modularity - you need modular monoliths or microservices for that.

He then sets out three tiers by application, not one prescription: applications needing almost no architecture, applications that "benefit from minimal structure and just pipelines and handlers (often referred to as Vertical Slice Architecture)", and applications that "benefit from ports-and-adapters (aka hexagonal, onion, or clean architecture) style architecture, where a significant goal is to shield business logic from persistence and other infrastructure concerns". He closes: "There are no one-size-fits-all architectures". ([Ardalis: Clean Architecture Sucks](https://ardalis.com/clean-architecture-sucks/), 2024-05-23)

Two things follow. First, "aka Ports-and-Adapters" is `mark-seemann`'s identity claim in the mouth of the pattern's leading .NET advocate. The two sources most opposed on whether to apply the layering agree that the named architectures are one thing, which removes the last reason to put a pattern name in a headline. Second, the template line now forks on scale, with the minimal template organising by feature and the full one by layer, and the guidance "Not sure? Start with Minimal Clean and migrate to Full Clean Architecture if your application grows in complexity."

What he doesn't say is the repository's headline. The fork is one architecture per application chosen by complexity, never layers applied inside each slice of one application. The nearest he comes to the repository's instinct is a concession about services rather than layers: "Ok so if it's literally just CRUD, a service is perhaps overkill." ([Ardalis: Should Controllers Reference Repositories or Services?](https://ardalis.com/should-controllers-reference-repositories-services/), 2021-09-14, updated 2023-10-13)

**What to do with it.** The headline claim is still unsupported and should still go, and the recommendation from the second pass is unchanged. What has changed is its footing. It's no longer a rewrite forced by two critics over the objection of the documentation; it is the position the pattern's own advocate now argues, in his own words, against the documentation page he wrote five years ago. Lead with the conditional, drop the pattern name from the headline, and state the choice as one architecture per application selected by complexity, with the per-slice framing dropped rather than softened.

### "Let the folder structure name the feature, not the pattern"

**Corroborated, by two sources reaching it in different vocabulary.**

`jeremy-miller`: "Organize code around the 'verbs' of the system more than the 'nouns' (entities) of the system", with an explicit carve-out that matches the repository's own instinct: "if you're truly building a CRUD system, I think you can ignore everything I've said and just go bang out code." (2025-02-27)

`oskar-dudycz` reaches the same rule from the slice definition: "a slice is more a function than an entity" (2026-08-10).

Verbs over nouns and functions over entities are the same claim. Both sources are marked **Corroborate.**, so they can't carry the opinion between them: the roster rule wants something unmarked standing beside them, and the second pass recorded this the wrong way round. The problem is compounded by item 2 below, since these two are the pair that co-maintain Marten and so amount to one voice as well as two markings.

### Negative result worth keeping

`andrew-lock` was swept on 2026-08-24 as the other obvious independent citable candidate and carries nothing on module boundaries, messaging or the outbox. His background-work catalogue is Quartz.NET and `IHostedService` mechanics, which is hosting rather than architecture. Don't re-sweep him for this topic.

## 2. Messaging between modules

**The choice between an interface call and a message is a coupling decision, and the sweep now agrees on which coupling you are buying.**

- **Synchronous, via a public interface resolved through DI.** "Method calls are in-memory, so they are fast, easy to implement, and add no indirection." The calling module fails when the called module does. ([Jovanović: Modular monolith communication patterns](https://milanjovanovic.tech/blog/modular-monolith-communication-patterns), 2023-08-05)
- **Asynchronous, via messages.** "Messaging gives you loose coupling and high availability, since the receiving module does not need to be available when a message is sent." The cost is infrastructure. (same source)

**The rule that outranks the choice: don't build chains of synchronous calls across boundaries.** `ms-learn` names where that ends:

> If your internal microservices are communicating by creating chains of HTTP requests as described, it could be argued that you have a monolithic application, but one based on HTTP between processes instead of intra-process communication mechanisms.

([Microsoft Learn: Challenges and solutions for distributed data management](https://learn.microsoft.com/dotnet/architecture/microservices/architect-microservice-container-applications/distributed-data-management), reviewed 2018-09-20)

`derek-comartin` names the mechanism: "Blocking synchronous calls, such as HTTP, from service to service can provide issues with latency, fault tolerance, and availability, all because of temporal coupling." ([Comartin: Distributed Tracing to discover a Distributed BIG BALL of MUD](https://codeopinion.com/distributed-tracing-to-discover-a-distributed-big-ball-of-mud/), 2022-08-03)

This reads across to a modular monolith directly. A chain of in-process interface calls between modules has the coupling and the failure cascade without the latency, which makes it the cheapest version of the mistake to make and the hardest to see.

### A boundary you can hide isn't a boundary

The most useful thing `mark-seemann` contributes to this topic isn't a pattern but a failure mode, and it's the one a modular monolith is most exposed to. His four tenets of SOA series is his most recent sustained architecture writing and it is C# throughout.

He starts from the observation that tooling has always undone boundary explicitness: "So much for the principle that boundaries are explicit. They're not, and it bothered me twenty years ago, as it bothers me today." The consequence is the one every module-crossing helper method produces: "When the boundary is not explicit, you may inadvertently write client code that makes network calls, and you may not be aware of it."

He then works through the obvious fix and rejects it. Mandating that every cross-boundary operation be a command "is essentially an asynchronous messaging architecture", and it forces queries into Request-Reply with correlation identifiers. That works, and then somebody wraps it in a helper:

> We're back where we started. Boundaries are no longer explicit. Equivalent to how good names are only skin-deep, this attempt to make boundaries explicit can't resist programmers' natural tendency to make things easier for themselves.

His answer is to pick an abstraction that can't be wrapped away: "People often complain that async code is contagious. By that they mean that once a piece of code is asynchronous, the caller must also be asynchronous. This effect is transitive, and while this is often lamented as a problem, this is exactly what we need. Amplify the essential. Make boundaries explicit." ([Seemann: Boundaries are explicit](https://blog.ploeh.dk/2024/03/11/boundaries-are-explicit/), 2024-03-11)

For a modular monolith this is the sharpest available argument for messaging over interfaces, and it doesn't rest on availability or infrastructure. A message contract can't be collapsed into a property access; an injected interface can.

The companion tenet gives the contract rule: "The third SOA tenet emphasizes that only data travels over service boundaries. In order to communicate effectively, services must agree on the shape of data, and which operations are legal when. While they exchange data, however, they don't share address space, or even internal representation." ([Seemann: Services share schema and contract, not class](https://blog.ploeh.dk/2024/04/15/services-share-schema-and-contract-not-class/), 2024-04-15)

That converges with `oskar-dudycz` from a different direction: "Everything outside the slice is external, whether it's the next folder or another system." Two sources, one unmarked and one marked, agree that a module boundary should be treated with the discipline of a network boundary even when it's a folder.

### Domain events, integration events, and whether those are the right names

`ms-learn` separates them cleanly. A **domain event** is "something that happened in the domain that you want other parts of the same domain (in-process) to be aware of", dispatched in-process and possibly synchronously. An **integration event** propagates a committed change outward and "should occur only if the entity is successfully persisted, otherwise it is as if the entire operation never happened"; integration events "must be based on asynchronous communication". ([Microsoft Learn: Domain events, design and implementation](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation), reviewed 2018-10-08)

**`oskar-dudycz` rejects the vocabulary while keeping the distinction.**

> Why am I not using domain event and integration event terms? Because they're highly misleading.

His grounds are that the two names describe one kind of thing seen from two places, and that naming them separately hides it: "All of them should be domain events. They are just used in different contexts. Internal (or private) is understandable in the module context, and external (or public) is understandable in the whole system context."

The same post carries the only treatment of event granularity found in the sweep, and it's the answer to what a module should expose. Publishing internal events outward is what welds modules together: "If we expose our internal events, we must communicate and consult each change we make with other teams. We have to assume that the event is being used by someone else." His alternative is a deliberately separate event shaped for consumers, the Summary Event, and he distinguishes it from the pattern it is usually confused with:

> Summary Event can be easily mistaken with Event Carried State Transfer or Snapshot, yet they're not the same. Summary Event still gathers business information about the business fact that has happened. We can not only replicate the state (as with Event Carried State Transfer) but also trigger the workflow's next steps.

The framing that follows is the one worth carrying into an opinion: "Events should be treated as such. Yes, API." A module's external events are its public interface and change like one. ([Dudycz: Internal and external events, or how to design event-driven API](https://event-driven.io/en/internal_external_events/), 2023-10-15). The post uses C# and Marten, which is a direct conflict of interest and is flagged here rather than relied on.

`jeremy-miller` never uses "integration events" anywhere on his site. His vocabulary is domain events, cascading messages, or just events, with the in-process and forwarded cases distinguished by mechanism rather than by name.

**The practical consequence is unchanged by the naming argument.** An event dispatched inside a module and an event published across a module boundary are different things with different delivery guarantees, and conflating them is what produces a distributed monolith. Whether the repository writes that as domain versus integration or internal versus external is a wording decision for `resolve-research`. `ms-learn` is the more citable of the two on the distinction; `oskar-dudycz` is the better argument for why the usual names mislead, and the only source with anything to say about which events a module should publish.

### Commands or events across a boundary, where the sources differ

`derek-comartin`: "Generally, avoid crossing boundaries with commands." ([Comartin: Commands or Events: Which One for Workflow?](https://codeopinion.com/commands-or-events-which-one-for-workflow/), 2025-03-25)

`oskar-dudycz` gives the opposite-facing rule, as an anti-pattern: "If we'll always have a single consumer for an event that needs to run the specific logic and expect to get the particular event back, then it should be a command." ([Dudycz: Anti-patterns in event modelling - Passive-Aggressive Events](https://event-driven.io/en/passive_aggressive_events/), 2026-04-13)

These are reconcilable and the reconciliation is the useful part. Comartin is arguing against a module telling another module what to do. Dudycz is arguing against dressing a command up as an event to look decoupled while keeping a single known consumer and an expected reply. The shared rule underneath: the message type should match the actual coupling, and publishing an event to one known consumer that must answer is a command with extra steps. See [Where the sources disagree](#where-the-sources-disagree) for how to weigh them.

### Who owns a message, which settles the commands-versus-events split

The second pass recorded `derek-comartin` and `oskar-dudycz` disagreeing about commands across boundaries and noted that both are marked, so neither could carry an opinion. `ardalis` is the third voice, and he does better than break the tie: he supplies the structural rule the other two are each applying from one end.

> Command messages involve one handler, many potential senders.
> Event messages involve one sender, many potential recipients.

From that he derives ownership: "The owner of the contract is the end of the communication channel that has one, not the side that has many." So handlers own commands and senders own events, and the reason is that no one of many consumers can be allowed to set a format the others depend on. ([Ardalis: Commands, Events, Versions, and Owners](https://ardalis.com/commands-events-versions-and-owners/), 2022-05-04)

DevIQ states the consequence for the receiving end: "Unlike commands, events can have 0 to many handlers, and should not return a result." ([DevIQ: Domain Events Pattern](https://deviq.com/design-patterns/domain-events-pattern/), no date shown)

This adjudicates the split rather than averaging it. `oskar-dudycz`'s audit test, that an event with one known consumer expecting a reply was always a command, is the cardinality rule read backwards: one recipient plus a return value is a command's shape. `derek-comartin`'s rule, publish an event rather than direct another module, is the same rule read forwards. Both were right about their own direction, and the cardinality is what makes them one rule.

**One caveat on reach.** The post is written about distributed systems and explicitly makes each application "its own bounded context". Applying it to modules inside one process is a read across a boundary the source didn't write for, the same caution this file already applies to the `ms-learn` microservices e-book. The cardinality argument survives the move because it's about contracts rather than transport, but say so when citing it.

### The limit of the whole approach

`derek-comartin` supplies the caveat the repository should carry alongside any messaging opinion, because it's the one thing every vendor-adjacent source omits: "Event-driven architecture isn't some magical silver bullet that removes coupling; it doesn't." ([Comartin: Event-Driven Architecture lost its way](https://codeopinion.com/event-driven-architecture-lost-its-way/), 2024-03-07). And on the accounting: "You did not change the amount of complexity in the business process. You moved the complexity somewhere else." ([Comartin: Decoupling in Software Architecture Moves Complexity](https://codeopinion.com/decoupling-in-software-architecture-moves-complexity/), 2026-08-06)

Both posts carry the Particular Software sponsorship in the article body, which is noted in the roster row. The claims cut against the sponsor's product category rather than for it, which is the direction that matters.

## 3. Transactional boundaries

**The aggregate is the consistency boundary, and one transaction should normally cover one aggregate.** `ms-learn` quotes Evans, "Any rule that spans Aggregates will not be expected to be up-to-date at all times", and Vernon, "if executing a command on one aggregate instance requires that additional business rules execute on one or more aggregates, use eventual consistency". The reason given is lock contention at scale. (Domain events page, reviewed 2018-10-08)

**`ms-learn` doesn't follow that rule itself, and is explicit about the trade:**

> the initial deferred approach—raising the events before committing, so you use a single transaction—is the simplest approach when using EF Core and a relational database. It's easier to implement and valid in many business cases.

The mechanism is one line with a large consequence. Domain events accumulate on the entity and dispatch either side of `SaveChangesAsync`:

```csharp
public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
{
    // Before SaveChanges: handlers share the scoped DbContext, so their writes
    // join the same transaction and roll back with it.
    await _mediator.DispatchDomainEventsAsync(this);

    var result = await base.SaveChangesAsync(cancellationToken);
    // After SaveChanges instead: separate transactions, and you owe the system
    // eventual consistency plus compensating actions on failure.
    return result > 0;
}
```

The sample keeps the eShopOnContainers shape. `DispatchDomainEventsAsync` is a hand-rolled extension method in that repository rather than a MediatR API, and `architecture.md` still treats mediator libraries as unsourced, so the dispatch call is the mechanism to copy rather than the library. `ms-learn` names the cost of the other choice plainly: "if there's an issue and the event handlers cannot commit their side effects, you'll have inconsistencies between aggregates", recoverable only by storing the events and running a reconciliation batch.

**`ardalis` gives the two choices names, and the vocabulary is worth taking even though his default is the riskier one.** He distinguishes pre-persistence from post-persistence domain events, and DevIQ carries the same split as a section: pre-persistence events "are typically triggered and resolved immediately", while post-persistence events "are typically queued up on the triggering entity, and then once the entity has been saved, the events are dispatched. This ensures the events are only handled once the state change has been persisted." ([DevIQ: Domain Events Pattern](https://deviq.com/design-patterns/domain-events-pattern/), no date shown; [Ardalis: Immediate Domain Event Salvation with MediatR](https://ardalis.com/immediate-domain-event-salvation-with-mediatr/), 2020-07-08)

That's the `ms-learn` deferred-versus-after distinction with better names, and the names are the citable part. The implementations aren't. His Clean Architecture template dispatches post-persistence, which is the side that owes the system an outbox and doesn't have one, and the 2020 post's pre-persistence sample uses a static `[ThreadStatic]` mediator holder called from inside an entity with `.Wait()` on an async publish. Cite the pre-persistence and post-persistence terms; cite nothing from the sample.

**`jeremy-miller` supplies the objection to this exact pattern, and it's the one the repository needs to hear.** He describes the classic .NET domain-events implementation, an entity base class collecting events dispatched through a mediator on `SaveChangesAsync`, and then says: "I have always hated this Domain Events pattern and much prefer the full 'Critter Stack' approach with the Decider pattern and event sourcing." ([Miller: "Classic" .NET Domain Events with Wolverine and EF Core](https://jeremydmiller.com/2025/12/04/classic-net-domain-events-with-wolverine-and-ef-core/), 2025-12-04)

That preference is for his own product's pattern and isn't citable as a verdict. The tool-independent point inside it is: mediator-based dispatch carries no outbox, and a domain-event handler that touches anything outside the transaction is unsafe without one. He states the general version elsewhere: "a lot of shops I think are naively opting into MediatR as a core tool without realizing the important functionality it is completely missing in order to build a resilient system — like a transactional outbox." He names MassTransit alongside his own tool as a valid answer, which is worth recording because it's evidence he isn't arguing purely to his own catalogue. ([Miller: Build Resilient Systems with Wolverine's Transactional Outbox](https://jeremydmiller.com/2024/12/08/build-resilient-systems-with-wolverines-transactional-outbox/), 2024-12-08)

**The outbox is what makes a cross-boundary event reliable.** `ms-learn` describes it as "a transactional database table as a message queue", with the event row written "atomically with the original database operation, using a local transaction against the same database". `jeremy-miller` gives the same definition tool-independently: "The general idea of an 'outbox' is to obviate the lack of true 2 phase commits by ensuring that outgoing messages are held until the database transaction is successful, then somehow guaranteeing that the messages will be sent out afterward." ([Miller: Transactional Outbox/Inbox with Wolverine and why you care](https://jeremydmiller.com/2022/12/15/transactional-outbox-inbox-with-wolverine-and-why-you-care/), 2022-12-15)

He also states the delivery guarantee without overstating it. Nothing in his writing claims exactly-once; the guarantee is that a message "will be delivered at least once as long as the database transaction to save the new event succeeds".

### The consume side, which the first pass flagged as missing

At-least-once delivery makes idempotent consumption mandatory, and the first pass recorded that no page it had read covered the mechanics. The Azure Architecture Center page now in scope does.

> The durable solution isn't to eliminate duplicate delivery. It's to make the consumer tolerate it.

It names the inbox as "the consume-side companion to the Transactional Outbox pattern": record each processed message identifier in the same transaction as the work, so a redelivery finds the record and stops. Four points are worth carrying into an opinion:

- **Prefer natural idempotency first.** "Design for natural idempotency first." An operation that sets a value rather than incrementing one needs no bookkeeping.
- **Enforce with a unique constraint, not a check-then-set.** Reading for an existing record and then inserting is a race between concurrent consumers. "This approach makes the database the single arbiter of the race."
- **Retention must outlast redelivery.** The processed-message table has to be kept longer than the broker's redelivery window, including dead-letter resubmission, or a late duplicate finds nothing and runs twice.
- **Message identity has to be stable.** A producer-assigned identifier survives redelivery; one generated at consume time doesn't.

([Microsoft Learn: Idempotent Consumer pattern](https://learn.microsoft.com/azure/architecture/patterns/idempotent-consumer), reviewed 2026-08-13)

This page carries `ai-usage: ai-assisted` in its metadata. The Event Sourcing page doesn't. That isn't disqualifying, but record it on any citation of the page.

### A defensible opinion for this repository

Inside a module, dispatch domain events before `SaveChanges` and take the single transaction. A modular monolith has one database, and the lock-contention argument is a scale problem most modules never reach. Across modules, never share a transaction: publish through an outbox, consume idempotently, and accept eventual consistency. That puts the transactional boundary on exactly the line the repository already draws for ownership, which is the property worth having.

`ms-learn` supports the second half without qualification: "No microservice should ever include tables/storage owned by another microservice in its own transactions, not even in direct queries", and rules out two-phase commit as "against microservices principles". `derek-comartin` supplies the modular-monolith version of the same rule, which is the one that actually gets broken: stop joining tables across module boundaries, and check the queries rather than the namespaces.

Two existing files touch this and should be cross-linked rather than duplicated. [data-access.md](../opinions/data-access.md) warns that `ExecuteUpdateAsync` and `ExecuteDeleteAsync` bypass the change tracker, so "audit and outbox logic in `SaveChanges` doesn't run", which is precisely how an outbox stops working silently. [aspnet-core.md](../opinions/aspnet-core.md) already covers propagating OpenTelemetry context across outbox tables and queues.

## 4. When a module has earned a process boundary

Four sources give criteria and they compose into one list rather than competing.

**From `ms-learn`, three signals, all observations about an existing module rather than predictions.**

- **Scaling that cloning can't serve.** "Many applications, when they need to scale beyond a single instance, can do so through the relatively simple process of cloning that entire instance." Until one module's load profile diverges, the process boundary buys nothing.
- **Independent delivery.** "If you can't deliver independent feature slices of the application, separating it only adds complexity." A module that always ships alongside its neighbour hasn't earned anything.
- **The chattiness test, which runs both ways.** Validate that "there are no chatty calls between services, and if splitting functionality into two services causes them to be overly chatty, it might indicate those functions belong in the same service". `ms-learn` turns it into a merge signal too: a design that "involves constantly aggregating information from multiple microservices for complex queries" may be "a reason to merge microservices."

**From `jeremy-miller`, the same chattiness test with co-change added, which is the better version.** He states it twice in nearly the same words:

> if two or more services/modules are chatty between themselves and very frequently have to be modified at the same time, they're best described as a single bounded context and should probably be combined into a single service or module.

([Miller: Modular Monoliths and the "Critter Stack"](https://jeremydmiller.com/2024/04/15/modular-monoliths-and-the-critter-stack/), 2024-04-15; restated 2025-02-27)

Co-change frequency is the addition worth taking. It's measurable from git history, it needs no instrumentation, and it catches a bad boundary before any traffic exists.

He also gives the reason the decision is worth deferring, sourced to community experience rather than his own tools: "In every single experience report you'll ever find about a team trying to break up and modernize a large monolithic application the authors will invariably say that breaking apart the database was the single most challenging task." The shared-database failure he names the "Pond Scum Anti-Pattern": many modules over one murky schema.

His planning stance is Citadel and Outpost, which he credits to Glenn Henriksen rather than claiming: "I actually want to think a little bit upfront about having a path to easily break out modules into separate processes or services later." (2024-04-08)

**From `oskar-dudycz`, criteria that are about operations rather than code.** He lists differential traffic and workload, independent horizontal scaling, tenant isolation, and multi-region deployment as reasons to cut, and rejects two common ones: chasing total availability, and splitting because a module wants a different storage technology. ([Dudycz: How (not) to cut microservices](https://event-driven.io/en/how_to_cut_microservices/), 2021-01-13)

**From `derek-comartin`, the failure mode to watch for after cutting.** Temporal coupling through synchronous calls turns a set of services into a distributed big ball of mud, and distributed tracing is how you find out that's what you built.

**The cheapest predictor is already within reach.** A module that talks to its neighbours only through asynchronous messages can be extracted without rewriting its communication, which is `milan-jovanovic`'s stated reason for preferring messaging early: such modules are "much easier" to migrate. A module reached by direct interface calls hasn't been prepared, and extracting it is a rewrite rather than a move. `mark-seemann`'s async-contagion argument is the same point made structurally: if the boundary was never hideable, moving it costs nothing at the call sites.

**From `ardalis`, the criterion none of the other four give, and it isn't about the software.** Conway's Law read as a design constraint puts team structure ahead of module structure: "It would be unusual, and probably inefficient, to have a microservice that any number of different teams all share responsibility for maintaining and deploying", and "How you decompose and attach a large problem comes down to how you organize multiple teams of people, and if these teams aren't aligned with the software modules you're building and shipping, you're going to have problems." (2020-08-26)

The practical form is a question no metric answers: does a separate team own this module? A process boundary between two modules one team ships together buys deployment independence nobody will use and costs a network hop everybody pays. This is the criterion most likely to be missing from a repository opinion written from code alone, and it belongs beside the chattiness and co-change tests rather than under them.

`mark-seemann` also supplies the warning about what happens when the boundary is drawn without autonomy: "Many organizations inadvertently create distributed monoliths. I think that this often stems from a failure of heeding the tenet that services are autonomous." ([Seemann: Services are autonomous](https://blog.ploeh.dk/2024/03/25/services-are-autonomous/), 2024-03-25)

## 5. Event sourcing and event-driven architecture

**Event-driven between modules: yes, with the caveat from `derek-comartin` attached.** Integration events over an outbox with idempotent consumers is the shape every source describes. Nothing in the sweep argues against it for a modular monolith. What the sweep does add is that it moves complexity rather than removing it, and that it doesn't by itself remove coupling.

**Event sourcing as a persistence model: rarely, per module, and never by default.** This is the strongest result in the topic, because four sources reach it and two of them build event-sourcing tools for a living.

`ms-learn` leads the pattern page with the warning:

> Event sourcing is a complex pattern that introduces significant trade-offs. It changes how you store data, handle concurrency, evolve schemas, and query state. It's costly to migrate to or from an event sourcing solution, and after you adopt the pattern, it constrains future design decisions in the parts of the system that use it. [...] For most systems and most parts of a system, traditional data management is sufficient.

Five situations where it doesn't fit: straightforward CRUD with no audit requirement, prototypes and short-lived systems, anything needing real-time consistent views, mostly static reference data, and teams without event-driven experience, where "adopting it without the foundational knowledge increases the risk of antipatterns that are costly to reverse". ([Microsoft Learn: Event Sourcing pattern](https://learn.microsoft.com/azure/architecture/patterns/event-sourcing), reviewed 2026-03-27)

The costs it enumerates are concrete and mostly permanent: replay cost requiring snapshots, schema evolution needing upcasters or tolerant deserialization, no SQL-shaped querying, at-least-once delivery forcing idempotent consumers, and a collision between an immutable log and the right to erasure, mitigated only by keeping personal data outside the log or crypto-shredding by per-subject key.

**`oskar-dudycz` says the same thing against his own commercial interest**, which is the most weight any single quote in this topic carries:

> I'm always saying that a well-done CRUD is much better than a poorly done Event Sourcing.

and, decisively for this repository:

> Event Sourcing is not a system-wide architecture concept. It should be considered at the module level.

([Dudycz: When not to use Event Sourcing?](https://event-driven.io/en/when_not_to_use_event_sourcing/), 2021-06-23). He co-maintains Marten and sells Event Sourcing workshops, so a post titled this way is the opposite of a sales argument.

**`jeremy-miller` reaches the module-level rule independently of the page but not independently of the person**, which matters and is picked up in item 2 below:

> one of our current JasperFx Software clients has a large monolithic application where some workflow-centric modules would be a good fit for an event sourcing approach, while other modules are more CRUD centric or reporting-centric where a straight up RDBMS approach is probably much more appropriate.

(2024-04-15; the same point appears at 2024-04-08). He has no standalone essay arguing when not to use event sourcing; the position exists only as an aside inside the modular-monolith posts.

**`ms-learn`'s own closing advice is the opinion this repository would write:** "Event sourcing doesn't have to be an all-or-nothing decision for your entire system. Apply it selectively to the parts of your system that it benefits the most, such as a payment ledger or order-processing pipeline."

Two details are commonly got wrong and worth keeping:

- **An event store isn't a broker.** `ms-learn`: "Message brokers such as Apache Kafka typically lack per-entity stream queries and optimistic concurrency. They work well as a distribution layer [...] but they aren't a substitute for an event store." `oskar-dudycz` says it independently and more bluntly: "It's a common mistake to use tools like Kafka and Pulsar for event stores, but they are not." ([Dudycz: Event Streaming isn't Event Sourcing!](https://event-driven.io/en/event_streaming_is_not_event_sourcing/), 2021-12-01). `jeremy-miller` uses the same shape from the other side, framing Kafka as a downstream relay out of an event store rather than an alternative to one. This is corroboration across a real independence line, Microsoft and Dudycz, and it's the safest claim in the section.
- **Design events for intent, not for resulting state.** `ms-learn`: "an event that records _two seats were reserved_ is more valuable than an event that records _remaining seats changed to 42_." State-shaped events "reduce the event store to a change log that has no business meaning".

**`ardalis` adds a fifth voice to the caution by omission rather than by argument.** DevIQ carries no event sourcing entry. It's absent from the architecture index, the domain-driven-design index and the design-patterns list alike, and the whole of its coverage is one paragraph inside the CQRS entry describing it as an optional companion: "Event Sourcing is often combined with CQRS, though it's certainly not a prerequisite for using CQRS in your application." ([DevIQ: CQRS Pattern](https://deviq.com/design-patterns/cqrs-pattern/), no date shown)

An absence is weak evidence and it isn't a claim that he opposes the pattern. It counts for something because of what DevIQ is: a 255-page taxonomy of patterns, antipatterns and principles that does cover CQRS, domain events and event aggregators. A reference site of that shape omitting event sourcing entirely is consistent with the four sources that do argue it, and with none of the sources that treat it as a default. DevIQ also has no integration events entry and no microservices entry, which weakens it as the map of what to cover on precisely this topic.

**Negative results across the whole sweep, which decide what stays unsourced.** No source swept treats sagas as a design question of choreography versus orchestration. Event granularity is covered by `oskar-dudycz` alone, in the internal-and-external post above, and he is marked **Corroborate.** and writing about tooling he maintains, so that material can't carry an opinion until something corroborates it. Event-carried state transfer is no longer single-sourced: `ardalis` reaches the same mechanism from the data side, recommending a materialised view of another module's data "synchronized using events" as the escape from a cross-module join. That's a second voice on the mechanism, though both are marked and neither names the pattern, so it corroborates the technique without yet supplying the vocabulary. `mark-seemann` has no post on domain events, integration events, the outbox, idempotency or event sourcing at all: across 832 posts on the current blog, the only event-store entry is a library of his own from 2014, and the messaging posts date from 2011 to 2013. `jeremy-miller` has no idempotent-consumer essay; his outbox writing is producer-side. `derek-comartin` has no dedicated domain-versus-integration-events post and none on when not to use CQRS.

## Where the sources disagree

**Layering: solution-wide, per slice, or not at all. This is the entry the third pass rewrites, because one side of it turned out to be smaller than it was counted.**

The second pass set `ms-learn` against `mark-seemann` and `jeremy-miller`. With `ardalis` admitted, the `ms-learn` page and the `ardalis` blog are one author, and the disagreement is three people rather than four voices. Worse for the old weighing, the author has since moved: the 2021 version of him prescribes three projects for anything non-trivial, and the 2024 version says Clean Architecture's goal is decoupling from infrastructure, "That's it", denies it delivers modularity, and lists vertical slices as the appropriate style for a whole class of applications.

**Weigh towards `mark-seemann`, `jeremy-miller` and the later `ardalis`, which is now the same direction.** They are more recent, one is unmarked and independent, and on the identity of the named architectures the critic and the advocate agree word for word: Seemann's "Layers, Onions, Ports, Adapters: it is all the same" and Ardalis's "Clean Architecture, aka Ports-and-Adapters" are the same sentence. The `ms-learn` page holds the old position, and it is 2021 text descending from a 2018 e-book describing a monolith that hasn't been modularised, where layers are the only boundary available.

Where they still part company is scope. Seemann and Miller would remove the application layer at any scale. The later Ardalis keeps it and chooses per application by complexity. Neither side supports applying the layering inside each slice, which is what `architecture.md` currently says, so the repository's headline loses on all three readings.

**Commands across boundaries: avoid them, or insist on them.** `derek-comartin` says generally avoid crossing boundaries with commands. `oskar-dudycz` says an event with one known consumer that expects a reply should have been a command. Neither is wrong and the disagreement is about which failure is more common. Comartin is guarding against modules directing each other; Dudycz is guarding against theatre, where an event is used to look decoupled while the coupling is intact.

**Weigh them by direction of travel.** If you are choosing a message type for a new interaction, take Comartin's rule and publish an event. If you are auditing an existing interaction, take Dudycz's test: one consumer plus an expected response means you already have a command and should stop pretending. Both sources are marked **Corroborate.** and neither can carry this alone, so an opinion here needs a third voice.

**Event sourcing: cautionary or enthusiastic.** `ms-learn` leads with cost. `milan-jovanovic`'s introduction leads with benefit: reconstructing state at any point, historical data already present when a feature needs it, and named use cases in e-commerce, finance and IoT. ([Jovanović: Introduction to Event Sourcing for .NET Developers](https://www.milanjovanovic.tech/blog/introduction-to-event-sourcing-for-net-developers), 2024-08-31) **Weigh towards `ms-learn`:** it's independent, its page is recent at 2026-03-27, and it enumerates the exit costs. `oskar-dudycz` settles it, because he sells the workshops and still says a well-done CRUD beats a poorly done event store.

**Modular monolith: default, or fashion.** `architecture.md` and `milan-jovanovic` state it as a default. `mark-seemann` treats it as a swing of the pendulum and recommends physical package separation for teams without functional-programming discipline. `jeremy-miller` is openly dubious it is a panacea. `ardalis` endorses it warmly but conditionally, and elsewhere argues from Conway's Law that team structure decides the question. No source in the sweep contradicts the opinion, and three decline to endorse it as stated. That isn't grounds to drop it; it is grounds to state it as the cheaper reversible default rather than as consensus, and to name the team-shaped condition the sources keep returning to.

## Freshness

**Correction carried from the first pass, affecting every `ms-learn` citation.** The roster rule for `ms-learn` is to quote the page's own review date. The page's own review date is the `ms.date` metadata field, which the author sets. The first pass quoted `updated_at`, which is a docset build timestamp and is shared across every page in a bulk republish.

| Page                                                     | First pass claimed | Actual `ms.date` | `author` field     |
| -------------------------------------------------------- | ------------------ | ---------------- | ------------------ |
| Common web application architectures                     | 2026-07-08         | **2021-12-12**   | `ardalis`          |
| Challenges and solutions for distributed data management | 2023-10-12         | **2018-09-20**   | `jamesmontemagno`  |
| Domain events, design and implementation                 | 2024-01-03         | **2018-10-08**   | `jamesmontemagno`  |
| Applying simplified CQRS and DDD patterns                | 2023-05-27         | **2021-01-13**   | `jamesmontemagno`  |
| Event Sourcing pattern                                   | 2026-08-15         | **2026-03-27**   | `claytonsiemens77` |
| Idempotent Consumer pattern                              | not read           | **2026-08-13**   | `claytonsiemens77` |

The last column is the page's `author` metadata, a GitHub handle, not the `ms.author` alias beside it: `ardalis` is `wiwagn`, `jamesmontemagno` is `jamont`, `claytonsiemens77` is `pnp`. Two of the three handles are roster ids under a different spelling: `ardalis` and `james-montemagno`. That's picked up in [What this reveals](#what-this-reveals-about-the-repository).

The quoted field is demonstrably a build stamp, twice over. Event Sourcing and Idempotent Consumer share both `updated_at: 2026-08-15T05:02:00Z` and `git_commit_id: d73cd1632ffa489eff78a16bba4d101c510a1810`: one commit, two pages, one timestamp. And the stamp keeps moving without the text: re-checked on 2026-09-25, Event Sourcing reads `updated_at: 2026-09-25T05:03:00Z` while its `ms.date` still reads 2026-03-27. Six weeks of republishes, no review.

**What follows from the correction.** The microservices e-book material is 2018 and 2021 text, not 2023 and 2024. The first pass's argument that age isn't disqualifying still holds, because the reasoning is durable and the DDD citations it rests on are older still, but it now has to hold against 2018. Two consequences carry into any opinion: the e-book is framed for containerised microservices rather than modular monoliths, so every claim above has been read across a boundary the source didn't write for; and it predates the modular monolith becoming a named pattern, which is part of why it's silent on slices. Only the two Azure Architecture Center pattern pages are current.

**A second dating problem, on the source admitted this pass.** The `ms-learn` correction above turned on quoting the wrong metadata field. DevIQ has the sharper version of the same trouble: it shows a reader no date at all. Every page sampled carries its dates only in unrendered metadata, and on every one of them the published and modified values are identical, so nothing distinguishes an original from a revision. The domain-events entry stamps 2015-09-20 in all four of its date slots while its body teaches MediatR with `INotification` and `INotificationHandler`; the modular monolith entry stamps 2026-03-01 for both. Neither number can be quoted as a review date the way the roster's `ms-learn` rule now requires, because there's no evidence either was ever reviewed rather than published once and edited silently. Cite DevIQ for definition and taxonomy, and date the claim from a blog post or another source rather than from the entry.

**Authorship overlap, which is a freshness problem as well as an independence one.** The e-book pages carry individual authors, and two of them are roster ids. Common web application architectures and the `ardalis` blog are the same author, so the page's 2021 date isn't a second opinion holding steady across five years; it is one person's older position, still published, beside his newer one, and anywhere this file cites both it is citing him twice. The three microservices pages are `james-montemagno`, whose row carries a live independence concern and a pre-2024 citation cap. Their 2018 dates satisfy it, but a reader should know it before treating them as the independent leg of an argument.

**On the other sources.** `ardalis`'s blog material runs 2020 to 2024 and is C# throughout where it carries code, which is rarely; the two properties that matter most to the layering argument, the template documentation and DevIQ, carry no reader-visible date at all. `mark-seemann`'s layering position is 2025-04-01 and his boundary series is 2024, both within tolerance and both C#. His slice remark is 2023-09-18. `jeremy-miller`'s design essays run 2024-04 to 2026-06, all C#. `oskar-dudycz`'s slice post is 2026-08-10 but its examples are TypeScript, which the roster row requires flagging; his event-sourcing posts are 2021 and his internal-and-external-events post is 2023 and uses Marten. `derek-comartin`'s material is 2022 to 2026 and carries no code by design.

Nothing in this topic is preview-gated. All of it is GA guidance.

## What this reveals about the repository

1. **The remaining TODO is dischargeable, and the part already discharged needs revisiting.** Of the five older opinions, one, three and five gain corroboration and need only citations. Opinion two gains corroboration from three marked sources while the one unmarked source reads its central term differently, so it should stop implying the framing is uncontested. Opinion four is the one that has to be rewritten rather than cited: its headline is unsupported by every source swept, and that now includes the advocate of the pattern it names. Sections 3 and 4 answer the two extensions the comment still asks for.

   The 2026-09-14 harvest closed the corroboration ask and the messaging extension, and it closed them correctly as far as it went: the two opinions it added hold up against this sweep. What it didn't do is re-read the opinions already there against the sources it was adding, which is how opinion four kept a headline that `mark-seemann` and `jeremy-miller` both contradict and that `ardalis` has since walked away from. Route the whole thing through `resolve-research`, and treat this as evidence that a harvest adding sources to a file should re-check the claims already in it.

2. **The roster can say that a source has a conflict, but not that two sources share one.** This pass found the second instance and it's a different shape from the first, which is what makes it structural rather than incidental.
   - **One product, two people.** `jeremy-miller` created Marten; `oskar-dudycz` co-maintains it. Their agreement that event sourcing is a module-level decision is one voice.
   - **One person, two ids, twice over.** The `ms-learn` architecture e-book isn't one anonymous voice. The `author` field on the pages cited here names `ardalis` on Common web application architectures, and `james-montemagno` on all three microservices pages — distributed data management, domain events, and simplified CQRS. Both are Citable roster ids in their own right. Citing `ms-learn` beside `ardalis` on layering cites him twice; citing the microservices pages as independent corroboration cites a source whose own row carries a live independence concern and, since the re-vetting of 2026-08-23, a pre-2024 cap. The pages are 2018, which is inside that cap, so nothing here has to be withdrawn, but the independence those citations were counted for is thinner than the id suggests.

   The second shape is the more dangerous, because `scripts/validate-sources.cs` counts ids and nothing else. A file citing `[ms-learn, ardalis]` passes the **Corroborate.** check, since `ms-learn` is unmarked and stands beside the marked `ardalis`, while both citations rest on one author. The check can't catch this and shouldn't try, since authorship isn't in the roster's data. What the roster can do is carry the pairing in Notes, the way it already carries single-source conflicts. Worth raising with `vet-source` on four rows: `jeremy-miller` and `oskar-dudycz` as a pair, and `ms-learn` against both `ardalis` and `james-montemagno`.

3. **The `ms-learn` citation rule is fixed, and DevIQ now needs the same fix for a worse version of the problem.** The rule now names `ms.date` and says what `updated_at` is, which closed the error that put six dates in the previous pass wrong. DevIQ shows a reader no date at all, and its metadata dates are identical for published and modified on every page sampled, so there's no field to name. The workable rule is that DevIQ is citable for definition and taxonomy but can't date a claim, and anything time-sensitive taken from it needs its date from somewhere else. This is a `vet-source` amendment to the `ardalis` row, and the row's existing "reference-grade on definitions and taxonomy" wording is already most of the way there.

4. **The `vet-source` candidate list from the first pass is now resolved, and one item needs correcting.**
   - **Steve Smith (`ardalis`):** admitted 2026-09-10, swept here. He wasn't on the candidate list; he arrived through the roster's own vetting and turned out to bear on this topic more than any admission since the first pass.
   - **Mark Seemann, Jeremy D Miller, Derek Comartin:** admitted 2026-09-03. Swept here.
   - **Oskar Dudycz:** admitted 2026-08-24 and left unswept by the first pass. Swept here. One correction to that pass: his "Monolith-First - are you sure?" post is on `architecture-weekly.com`, a Substack, not on the `event-driven.io` domain the roster Source cell names. It's out of scope and no claim here rests on it.
   - **Jimmy Bogard:** activity is now checked, and the answer complicates the candidacy. He is active to 2026-09-01, but the recent output is AutoMapper and MediatR release posts plus promotion of his own commercial Vertical Slice Architecture webinar (2026-09-01 and 2026-07-23). He would therefore carry both the release-notes depth concern that `jeremy-miller`'s marking exists for and a direct commercial interest in precisely the opinion this topic is trying to corroborate. He originated the vertical-slice framing and the deferred domain-events pattern `ms-learn` quotes, so the archive matters; the case for admission is weaker than the first pass assumed.
   - **Kamil Grzybek:** unchanged. Watch list, not admission. Nothing published since 2023-12-05.
   - **Udi Dahan:** unchanged. Watch-listed 2026-08-24, dormant since 2016-02-19, discovery and cross-checking only.

5. **`derek-comartin`'s sponsorship is broader than his roster row describes.** The row says to "flag the sponsorship on anything touching message queues". In this sweep the Particular Software block appeared in the body of every post fetched, including the modular-monolith and coupling posts that have nothing to do with queues. The flag should be unconditional on that source rather than topic-scoped.

6. **The roster gap has shrunk again and is now down to one item.** Saga style, choreography versus orchestration, still returns nothing across five sources and the documentation set, and that's the gap. Commands versus events is closed: `ardalis` supplied the cardinality rule that adjudicates it, and the three voices on it are all marked, so an opinion needs one unmarked source beside them, which `ms-learn` supplies on the distributed side. Event-carried state transfer moved from one source to two. Event granularity remains `oskar-dudycz` alone.

7. **This topic has changed the roster four times in three passes.** The changes: `oskar-dudycz` admitted and the `ms-learn` scope widened after the first, the `ms.date` rule named after the second, and this pass raising the pairwise-independence gap and the DevIQ dating rule. A research topic can find defects in the trust boundary as well as consume it, so a topic taking many passes isn't by itself a sign it has stalled.

8. **Cross-links to add when these become opinions:** the outbox interaction in [data-access.md](../opinions/data-access.md) and the context-propagation note in [aspnet-core.md](../opinions/aspnet-core.md) both already exist and should be linked rather than restated. The navigation-property rule from `ardalis` belongs in [data-access.md](../opinions/data-access.md) as much as in `architecture.md`, since it's an Entity Framework configuration rule, and `resolve-research` should decide which file owns it rather than duplicating it into both.
