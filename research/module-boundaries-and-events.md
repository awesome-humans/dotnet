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

Research for the open TODO in [architecture.md](../opinions/architecture.md), which now reads "extend with transactional boundaries across modules, and when a module needs its own process". Event sourcing and event-driven architecture are in scope because both extensions depend on them.

**Two of the TODO's original asks are already done.** It first asked for corroboration from an independent source, then for messaging between modules, and only then for the two extensions that remain. The 2026-09-14 harvest handled the first two: it added `mark-seemann`, `ms-learn`, and `derek-comartin` to the file's `sources:`, added the boundary and integration-event opinions, and shortened the comment. This file now audits that work, and the audit finds a problem: the one opinion the harvest left untouched is the one no source supports.

This is the third pass. The first, on 2026-08-24, had one independent source and found half the TODO blocked. The second, on 2026-09-04, swept the four sources admitted on 2026-09-03, and found they split the opinions instead of confirming them. This pass sweeps `ardalis`, admitted on 2026-09-10, and lands on the two opinions the second pass left least settled.

**The layering disagreement was smaller than it looked: one side was one author quoted twice.** The second pass weighed `ms-learn`, which prescribes layering solution-wide, against `mark-seemann` and `jeremy-miller`, who reject the application layer. `ardalis` wrote the `ms-learn` page, so admitting him puts both citations under one name. He has also changed his mind. His 2021 article prescribes three projects without qualification. By 2024, he calls Clean Architecture one option among several, denies that it delivers modularity, and ships a second template organised by vertical slice.

That reverses the second pass's verdict on the fourth opinion, which it found no source supported. The position now has partial support from the architect most identified with the pattern it names. The recommended rewrite is unchanged, but it's now positively corroborated, not just unopposed.

This pass also settles commands versus events, which the second pass left open because it lacked a third source, and adds the first concrete, code-level rule for enforcing a module boundary.

## Sources swept

| id                | Standing                                                                                                                              | Used for                                                                                     |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `ms-learn`        | Citable, independent of the other sources, except by authorship: `ardalis` and `james-montemagno` wrote the architecture e-book pages | Corroboration, transactional boundaries, process boundaries, idempotency, and event sourcing |
| `ardalis`         | Citable, **Corroborate.**                                                                                                             | Layering scope, boundary enforcement in EF Core, command and event ownership, Conway's Law   |
| `mark-seemann`    | Citable, independent, unmarked                                                                                                        | Boundary explicitness, module contracts, coupling, and layering                              |
| `jeremy-miller`   | Citable, **Corroborate.**                                                                                                             | Modular monolith criteria, vertical slices, the outbox, and event sourcing scope             |
| `oskar-dudycz`    | Citable, **Corroborate.**                                                                                                             | Slices and modules, internal and external events, event sourcing scope, and cutting services |
| `derek-comartin`  | Citable, **Corroborate.**                                                                                                             | Boundary ownership, commands versus events, and the cost of decoupling                       |
| `milan-jovanovic` | Citable, conflict of interest noted                                                                                                   | Module communication patterns and event sourcing framing (from the first pass)               |
| `andrew-lock`     | Citable, independent                                                                                                                  | Swept 2026-08-24: nothing on this ground (negative result, recorded below)                   |

`jimmy-bogard` and `kamil-grzybek` remain unvetted, and no claim here rests on them. Item 4 at the end settles their status.

The `ardalis` ID covers three properties of unequal weight. The blog is dated and citable. [DevIQ](https://deviq.com/) is reference-grade on taxonomy, but shows a reader no date, which [Freshness](#freshness) covers. The Clean Architecture template documentation shows no date either, and is the weakest of the three.

## 1. Corroboration, opinion by opinion

The first pass answered the TODO for the file as a whole. With four more sources, that's too coarse, because they split the opinions differently.

`architecture.md` now has seven opinions. The five below predate this sweep. The 2026-09-14 harvest added two from this topic's own sources: make a module boundary visible to the compiler, and publish integration events while keeping domain events inside the module. They're the repository's position, so [section 2](#2-messaging-between-modules) develops them instead of reopening them.

### "Default to a modular monolith, not microservices"

**Corroborated, but more weakly than the first pass recorded, and every source hedges.**

The first pass cited `ms-learn` as "confirmed by a page reviewed as recently as 2026-07-08". That date is wrong. It's the page's `updated_at`, a docset build timestamp. The page's own review date, its `ms.date` field, is 2021-12-12. The text descends from the 2018 e-book, and `ardalis` wrote it; he maintains the Clean Architecture template it links to. [Freshness](#freshness) carries the full correction, which affects every `ms-learn` citation here.

The substance survives the date. `ms-learn` still says to start monolithic and treat separation as a later choice, and names the cost: "If you can't deliver independent feature slices of the application, separating it only adds complexity." ([Microsoft Learn: Common web application architectures](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures), reviewed 2021-12-12)

What's new is that none of the architects admitted for this gap states the opinion as flatly as `architecture.md` does.

`mark-seemann` doesn't prescribe a starting point. He reads monolith versus services as fashion: "One decade, service-oriented architecture (SOA) is cool; the next, consolidation sets in; then it's micro-services; and, as far as I can tell, monoliths are on the way in again, although I'm sure that we'll find something else to call them." ([Seemann: Pendulum swings](https://blog.ploeh.dk/2021/02/22/pendulum-swings/), 2021-02-22)

The one time he gives advice, it points away from the monolith. He credits his book's decoupled sample code to years of F# and Haskell discipline, not to a structure a reader could copy:

> I'm confident that the sample code is nicely decoupled, even though it's packaged as a monolith. But unless you have a similar experience, I recommend that you separate your code base into multiple packages.

([Seemann: Decomposing CTFiYH's sample code base](https://blog.ploeh.dk/2023/09/04/decomposing-ctfiyhs-sample-code-base/), 2023-09-04)

That's a recommendation to enforce module boundaries physically, at the package level, because most teams won't hold them by convention. It's compatible with a modular monolith, but it isn't the same opinion.

`jeremy-miller` is more sceptical, twice: "I'm still dubious that the modular monolith idea is going to be a panacea" ([Miller: Actually Talking about Modular Monoliths](https://jeremydmiller.com/2024/04/08/actually-talking-about-modular-monoliths/), 2024-04-08), and "I'm unfortunately dubious about the mechanics and whether or not this is just some 'modular' lipstick on the old 'monolith' pig" ([Miller: Thoughts on "Modular Monoliths"](https://jeremydmiller.com/2024/04/01/thoughts-on-modular-monoliths/), 2024-04-01). He stays "bullish on the long term usefulness of micro-services" for real bounded contexts.

His diagnosis of why old monoliths hurt doesn't blame the monolith: the pain came from "The usage of prescriptive architectures like Clean Architecture or Onion Architecture solution templates". That supports the first opinion by contradicting the fourth.

`ardalis` is the warmest, but he argues for migration, not the default. He writes for teams already in trouble: "What do you do when you find yourself in microservice hell?" His answer is to "Migrate to a modular monolith", keeping modularity while avoiding "the fallacies of distributed computing, such as network latency, bandwidth, and failure". ([Ardalis: From Microservices to Modular Monoliths](https://ardalis.com/from-microservices-to-modular-monoliths/), 2024-07-10). The post is three minutes long, has no migration guidance despite its title, and ends on a course discount code, so cite the argument, not the substance.

For a new system, his recommendation is conditional: a small-to-medium team, an early-stage project, a well-understood domain with uncertain future scale, or an escape from a big ball of mud. ([Ardalis: Introducing Modular Monoliths: The Goldilocks Architecture](https://ardalis.com/introducing-modular-monoliths-goldilocks-architecture/), 2024-02-21). The same post names the outcome to avoid: the distributed monolith "has all of the disadvantages of microservices (complexity, cost) as well as all the disadvantages of a monolith (many dependencies, difficulty making changes). It's the worst of all worlds and should be avoided if at all possible."

He also gives the clearest counter-signal, and it isn't technical. Reading Conway's Law forward, decomposition should follow the organisation: microservices "map very well to bounded contexts", and "if these teams aren't aligned with the software modules you're building and shipping, you're going to have problems". ([Ardalis: Conway's Law, DDD, and Microservices](https://ardalis.com/conways-law-ddd-and-microservices/), 2020-08-26). On that reading, a single team is what makes a monolith right, and the opinion is really a claim about team size.

**Recommendation:** Keep the opinion, with less confidence. A modular monolith is the cheaper default because a process boundary can be reversed and a module boundary can't, not because the industry has settled on it. Four of the five sources state it conditionally, and their conditions are about team and organisation more than code.

### "Build each module from vertical slices"

**Corroborated by two sources, and framed differently by the one independent source.**

`jeremy-miller` states it structurally: a slice "organizes code around features instead of technical layers. A slice owns its whole pathway: take the input, do the work, produce the output, all in one place." ([Miller: The Codebase Is the Prompt](https://jeremydmiller.com/2026/06/04/the-codebase-is-the-prompt-wolverine-vertical-slices-and-ai-assisted-development/), 2026-06-04). He calls himself "a proponent of 'Vertical Slice Architecture' code organization and a harsh critic of layered architecture approaches (Clean/Onion/Hexagonal) as they are commonly practiced", and credits the term to Jimmy Bogard.

`oskar-dudycz` agrees, and sharpens the unit: "A vertical slice is one piece of functionality, cut through the whole application. For me, a slice is more a function than an entity." ([Dudycz: Vertical slices, their ownership and external dependencies](https://event-driven.io/en/vertical-slices-and-dependencies/), 2026-08-10)

`mark-seemann` uses the words for something else: a way of working, not a way of arranging code.

> As I describe in Code That Fits in Your Head, I usually develop (vertical) feature slices one at a time, utilising an outside-in TDD process, during which I also figure out how to save or retrieve data from persistent storage.

([Seemann: Do ORMs reduce the need for mapping?](https://blog.ploeh.dk/2023/09/18/do-orms-reduce-the-need-for-mapping/), 2023-09-18)

Two sentences later in the same post, he names the code's structure, and it isn't slices: "the architecture is Ports and Adapters, or, if you will, Clean Architecture." In 832 posts, he never uses the capitalised term "Vertical Slice Architecture".

`ardalis` treats it as a named architecture style, equal to the one he's known for. DevIQ's architecture index lists Clean Architecture and Vertical Slice Architecture side by side, and he ships a template built on the second: the Minimal Clean Architecture template "maintains the core principles of Clean Architecture—separation of concerns, dependency inversion, and testability—while reducing complexity through a single-project Vertical Slice Architecture (VSA)", organised "by feature rather than technical layer". ([Ardalis: Minimal Clean Architecture](https://ardalis.github.io/CleanArchitecture/minimal-clean-architecture/), no date shown)

He's also the earliest source on the term, using it in 2012 for splitting user stories, not arranging code. ([Ardalis: Stories Too Big — Vertical Slices](https://ardalis.com/stories-too-big--vertical-slices/), 2012-02-01). That's `mark-seemann`'s reading, not `jeremy-miller`'s, and the order matters: the delivery sense came first, and the code-organisation sense grew from it.

**Recommendation:** Keep the opinion, now with three marked sources and a template. Stop implying the framing is uncontested: the one unmarked source reads the term as a delivery process, and that reading came first.

### "Don't treat a slice as a module"

**Corroborated: the cleanest result in the sweep.**

`oskar-dudycz` states the containment directly: "A module is a logical grouping of slices." The boundary rule that follows is stricter than `architecture.md`'s: "Everything outside the slice is external, whether it's the next folder or another system." On persistence, he separates the two units: "Database schemas go per module... A slice is a feature, not a persistence boundary." (same post, 2026-08-10)

`jeremy-miller` confirms the distinction by complaining that people ignore it: "there's a ton of disagreement about what the hell it is that 'vertical slice architecture' actually means and a lot of folks conflating that with bounded contexts or micro-services." ([Miller: We Don't Need No Stinkin' Repositories](https://jeremydmiller.com/2025/02/27/we-dont-need-no-stinkin-repositories-and-other-observations-on-dotnetrocks/), 2025-02-27)

`derek-comartin` gives the ownership test that makes a module a module: "Logical boundaries are all about ownership. Who owns the data? Who owns the business rules? Who owns the invariants you need to enforce?" ([Comartin: Modular Monolith Boundaries Done Wrong](https://codeopinion.com/modular-monolith-boundaries/), 2026-06-02). He checks the query log, not the namespace: "Your code might say Sales and Warehouse. But what do your queries say?" ([Comartin: Stop Joining Tables In Your "Modular" Monolith](https://codeopinion.com/stop-joining-tables-in-your-modular-monolith/), 2026-05-27)

**`ardalis` gives the enforcement rule, the first thing in this topic a reviewer can check line by line.** Asked how to map an Entity Framework relationship to an entity in another module, he says don't: "don't use navigation properties for entities that live outside your module. Instead always just use keys". His reframing generalises past EF Core:

> Imagine instead that the data owned by other modules is outside not just that module but outside your organization.

He states the rule for both units the opinion separates: "data that is outside of an aggregate or module should only be referenced using its key or ID, not as a navigation property". For local access, he names a materialised view, "essentially a read-only cache", synchronised by events, and "If you need to make changes, send a command to the module that owns that data." ([Ardalis: Modeling Navigation Properties Between Aggregates or Modules](https://ardalis.com/navigation-properties-between-aggregates-modules/), 2024-06-19)

This is the boundary `derek-comartin` checks in the query log, one step earlier: a navigation property is how the cross-module join gets written. It comes with real code, which almost nothing else in this topic does, and a reader either has the mapping or doesn't.

**Recommendation:** Keep the opinion, and add two rules under it. A schema per module, not per slice, is the concrete, testable form of the boundary. Reference another module by key, never by navigation property, is the form a reviewer can enforce in EF Core configuration, with the materialised view as the named exception and a command as the only way to write back.

### "Apply Clean Architecture per slice, not per solution"

**Partly corroborated. The second pass found it contradicted from both directions, with no source holding it. `ardalis` changes that, with a caveat about who was being counted.**

`ms-learn` contradicts it from above, prescribing layering solution-wide as "the most appropriate way to structure non-trivial monolithic applications". `ardalis` wrote that page, so it isn't a second source beside him: it's his 2021 position under a Microsoft masthead.

The two new architecture sources contradict it from below: no layering at either scale. `mark-seemann` is the most absolute, in his most recent statement on layering:

> I usually don't abstract application behaviour from frameworks. I don't create 'application layers', 'use-case classes', 'mediators', or similar. This is a deliberate architecture decision.

([Seemann: Ports and fat adapters](https://blog.ploeh.dk/2025/04/01/ports-and-fat-adapters/), 2025-04-01)

He also denies that the named architectures are distinct choices, which removes the opinion's footing. "If you apply the Dependency Inversion Principle to Layered Architecture, you end up with Ports and Adapters." ([Seemann: Layers, Onions, Ports, Adapters: it is all the same](https://blog.ploeh.dk/2013/12/03/layers-onions-ports-adapters-its-all-the-same/), 2013-12-03). Eleven years later, from consulting: "Today, most organizations that I consult with will tell me that they've decided on Ports and Adapters. Even so, if you do it right, it is the same architecture." ([Seemann: Three data architectures for the server](https://blog.ploeh.dk/2024/07/25/three-data-architectures-for-the-server/), 2024-07-25)

`jeremy-miller` attacks the benefit layering is sold on: "I have almost never needed to reason about a system's entire data access layer in isolation even though that's held up as an advantage of Clean/Onion/Hexagonal layering approaches." He extends it to repository abstractions: "I would generally recommend against using wrapping repository abstractions around low level persistence tooling like Marten, EF Core, or Dapper in systems in most cases". Such an interface "does pretty well nothing to add any value", pushes teams to a lowest-common-denominator API, and doesn't deliver the promised swappability, a claim he calls "patently not true". (2025-02-27)

He also names the runtime cost, which is observable, not a matter of taste: "Big call stacks of a controller calling a mediator tool that calls one service that calls other services that call different repository abstractions that all make database queries is a common source of chattiness because it's hard to even see where all the chattiness is coming from by reading the code." ([Miller: Network Round Trips are Evil](https://jeremydmiller.com/2024/07/08/network-round-trips-are-evil/), 2024-07-08)

**`ardalis` moves this opinion by conceding most of the case against the pattern he's known for.** In 2021, he's unqualified: "there are basically three projects: Core, Infrastructure, and Web", and "Some might argue that this is over-engineered but the end result is typically just 3 projects and I've never found that to be too many for any application of non-trivial complexity." ([Ardalis: Clean Architecture with ASP.NET Core](https://ardalis.com/clean-architecture-asp-net-core/), 2021-11-30). The page doesn't mention vertical slices.

By 2024, he's narrowed the claim to almost exactly the condition in this repository's opinion body:

> Clean Architecture, aka Ports-and-Adapters, has a primary goal of reducing tight coupling from the business rules of the system to infrastructure, and in particular, the database. That's it. It's not a panacea and it doesn't offer feature modularity - you need modular monoliths or microservices for that.

He then sets out three tiers of application instead of one prescription:

- Applications that need almost no architecture.
- Applications that "benefit from minimal structure and just pipelines and handlers (often referred to as Vertical Slice Architecture)".
- Applications that "benefit from ports-and-adapters (aka hexagonal, onion, or clean architecture) style architecture, where a significant goal is to shield business logic from persistence and other infrastructure concerns".

He ends: "There are no one-size-fits-all architectures". ([Ardalis: Clean Architecture Sucks](https://ardalis.com/clean-architecture-sucks/), 2024-05-23)

Two consequences. First, "aka Ports-and-Adapters" is `mark-seemann`'s claim from the pattern's leading .NET advocate. The two sources most opposed on whether to layer agree the named architectures are one thing, so no pattern name belongs in the headline. Second, his templates now split by scale: the minimal one organises by feature and the full one by layer, with the advice "Not sure? Start with Minimal Clean and migrate to Full Clean Architecture if your application grows in complexity."

He doesn't support the repository's headline. His choice is one architecture per application, by complexity, never layers inside each slice of one application. The closest he comes is about services, not layers: "Ok so if it's literally just CRUD, a service is perhaps overkill." ([Ardalis: Should Controllers Reference Repositories or Services?](https://ardalis.com/should-controllers-reference-repositories-services/), 2021-09-14, updated 2023-10-13)

**Recommendation:** The headline is still unsupported, and should still go, as the second pass recommended. What's changed is the support for removing it: it's now the position of the pattern's own advocate, in his own words, against the documentation page he wrote five years ago. Lead with the condition, drop the pattern name from the headline, and state the choice as one architecture per application, chosen by complexity. Drop the per-slice framing; don't soften it.

### "Name folders after features, not patterns"

**Corroborated by two sources, in different words.**

`jeremy-miller`: "Organize code around the 'verbs' of the system more than the 'nouns' (entities) of the system", with an exception that matches the repository's: "if you're truly building a CRUD system, I think you can ignore everything I've said and just go bang out code." (2025-02-27)

`oskar-dudycz` reaches the same rule from his slice definition: "a slice is more a function than an entity" (2026-08-10).

Verbs over nouns and functions over entities are the same claim. Both sources are marked **Corroborate.**, so together they can't carry the opinion: the roster rule needs an unmarked source beside them, and the second pass recorded this the wrong way round. Item 2 below makes it worse: the two co-maintain Marten, so they're one voice as well as two markings.

### Negative result

`andrew-lock`, the other obvious independent citable candidate, was swept on 2026-08-24 and has nothing on module boundaries, messaging, or the outbox. His background-work catalogue covers Quartz.NET and `IHostedService` mechanics: hosting, not architecture. Don't sweep him again for this topic.

## 2. Messaging between modules

**Choosing between an interface call and a message is choosing a coupling, and the sources agree on which coupling each one buys.**

- **Synchronous, through a public interface resolved from DI.** "Method calls are in-memory, so they are fast, easy to implement, and add no indirection." The calling module fails when the called module does. ([Jovanović: Modular monolith communication patterns](https://milanjovanovic.tech/blog/modular-monolith-communication-patterns), 2023-08-05)
- **Asynchronous, through messages.** "Messaging gives you loose coupling and high availability, since the receiving module does not need to be available when a message is sent." The cost is infrastructure. (same source)

**Above that choice: don't build chains of synchronous calls across boundaries.** `ms-learn` says where they lead:

> If your internal microservices are communicating by creating chains of HTTP requests as described, it could be argued that you have a monolithic application, but one based on HTTP between processes instead of intra-process communication mechanisms.

([Microsoft Learn: Challenges and solutions for distributed data management](https://learn.microsoft.com/dotnet/architecture/microservices/architect-microservice-container-applications/distributed-data-management), reviewed 2018-09-20)

`derek-comartin` names the mechanism: "Blocking synchronous calls, such as HTTP, from service to service can provide issues with latency, fault tolerance, and availability, all because of temporal coupling." ([Comartin: Distributed Tracing to discover a Distributed BIG BALL of MUD](https://codeopinion.com/distributed-tracing-to-discover-a-distributed-big-ball-of-mud/), 2022-08-03)

This applies directly to a modular monolith. A chain of in-process interface calls between modules has the coupling and the cascading failure without the latency, so it's the cheapest version of the mistake and the hardest to see.

### A boundary you can hide isn't a boundary

`mark-seemann`'s main contribution is a failure mode, the one a modular monolith is most exposed to. His four tenets of SOA series is his most recent sustained architecture writing, in C# throughout.

He observes that tooling has always undone explicit boundaries: "So much for the principle that boundaries are explicit. They're not, and it bothered me twenty years ago, as it bothers me today." Every cross-module helper method produces the consequence: "When the boundary is not explicit, you may inadvertently write client code that makes network calls, and you may not be aware of it."

He then rejects the obvious fix. Requiring every cross-boundary operation to be a command "is essentially an asynchronous messaging architecture", and forces queries into Request-Reply with correlation IDs. That works until someone wraps it in a helper:

> We're back where we started. Boundaries are no longer explicit. Equivalent to how good names are only skin-deep, this attempt to make boundaries explicit can't resist programmers' natural tendency to make things easier for themselves.

His answer is an abstraction that can't be wrapped away: "People often complain that async code is contagious. By that they mean that once a piece of code is asynchronous, the caller must also be asynchronous. This effect is transitive, and while this is often lamented as a problem, this is exactly what we need. Amplify the essential. Make boundaries explicit." ([Seemann: Boundaries are explicit](https://blog.ploeh.dk/2024/03/11/boundaries-are-explicit/), 2024-03-11)

For a modular monolith, this is the strongest argument for messaging over interfaces, and it doesn't depend on availability or infrastructure. A message contract can't be collapsed into a property access, and an injected interface can.

The companion tenet gives the contract rule: "The third SOA tenet emphasizes that only data travels over service boundaries. In order to communicate effectively, services must agree on the shape of data, and which operations are legal when. While they exchange data, however, they don't share address space, or even internal representation." ([Seemann: Services share schema and contract, not class](https://blog.ploeh.dk/2024/04/15/services-share-schema-and-contract-not-class/), 2024-04-15)

`oskar-dudycz` reaches the same place from another direction: "Everything outside the slice is external, whether it's the next folder or another system." Two sources, one unmarked and one marked, agree: treat a module boundary like a network boundary, even when it's a folder.

### Domain events, integration events, and whether those are the right names

`ms-learn` separates them. A **domain event** is "something that happened in the domain that you want other parts of the same domain (in-process) to be aware of", dispatched in-process, possibly synchronously. An **integration event** propagates a committed change outward, "should occur only if the entity is successfully persisted, otherwise it is as if the entire operation never happened", and "must be based on asynchronous communication". ([Microsoft Learn: Domain events, design and implementation](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation), reviewed 2018-10-08)

**`oskar-dudycz` keeps the distinction and rejects the names.**

> Why am I not using domain event and integration event terms? Because they're highly misleading.

The two names, he says, describe one kind of thing seen from two places: "All of them should be domain events. They are just used in different contexts. Internal (or private) is understandable in the module context, and external (or public) is understandable in the whole system context."

The same post is the only treatment of event granularity in the sweep, and it answers what a module should expose. Publishing internal events outward welds modules together: "If we expose our internal events, we must communicate and consult each change we make with other teams. We have to assume that the event is being used by someone else." His alternative is a separate event shaped for consumers, the Summary Event, which he distinguishes from the pattern it's often confused with:

> Summary Event can be easily mistaken with Event Carried State Transfer or Snapshot, yet they're not the same. Summary Event still gathers business information about the business fact that has happened. We can not only replicate the state (as with Event Carried State Transfer) but also trigger the workflow's next steps.

The framing to carry into an opinion: "Events should be treated as such. Yes, API." A module's external events are its public interface, and change like one. ([Dudycz: Internal and external events, or how to design event-driven API](https://event-driven.io/en/internal_external_events/), 2023-10-15). The post uses C# and Marten, a direct conflict of interest, flagged here and not relied on.

`jeremy-miller` never uses "integration events" on his site. He says domain events, cascading messages, or just events, and distinguishes the in-process and forwarded cases by mechanism, not name.

**The naming doesn't change the practice.** An event dispatched inside a module and an event published across a boundary have different delivery guarantees, and conflating them produces a distributed monolith. Whether the repository says domain and integration or internal and external is a wording decision for `resolve-research`. `ms-learn` is the more citable source on the distinction. `oskar-dudycz` makes the better argument that the usual names mislead, and is the only source on which events a module should publish.

### Commands or events across a boundary, where the sources differ

`derek-comartin`: "Generally, avoid crossing boundaries with commands." ([Comartin: Commands or Events: Which One for Workflow?](https://codeopinion.com/commands-or-events-which-one-for-workflow/), 2025-03-25)

`oskar-dudycz` gives the opposite rule, as an anti-pattern: "If we'll always have a single consumer for an event that needs to run the specific logic and expect to get the particular event back, then it should be a command." ([Dudycz: Anti-patterns in event modelling - Passive-Aggressive Events](https://event-driven.io/en/passive_aggressive_events/), 2026-04-13)

They're reconcilable. Comartin argues against a module telling another module what to do. Dudycz argues against disguising a command as an event, with one known consumer and an expected reply, to look decoupled. The shared rule: the message type should match the real coupling, and an event to one known consumer that must answer is a command, whatever it's called. For how to weigh them, see [Where the sources disagree](#where-the-sources-disagree).

### Who owns a message, which settles commands versus events

The second pass recorded `derek-comartin` and `oskar-dudycz` disagreeing about commands across boundaries, and noted that both are marked, so neither could carry an opinion. `ardalis` is the third source, and he does more than break the tie: he gives the structural rule each of them applies from one end.

> Command messages involve one handler, many potential senders.
> Event messages involve one sender, many potential recipients.

Ownership follows: "The owner of the contract is the end of the communication channel that has one, not the side that has many." Handlers own commands, and senders own events, so no single consumer among many can set a format the others depend on. ([Ardalis: Commands, Events, Versions, and Owners](https://ardalis.com/commands-events-versions-and-owners/), 2022-05-04)

DevIQ states the consequence for the receiver: "Unlike commands, events can have 0 to many handlers, and should not return a result." ([DevIQ: Domain Events Pattern](https://deviq.com/design-patterns/domain-events-pattern/), no date shown)

This settles the split. `oskar-dudycz`'s test, that an event with one known consumer expecting a reply was always a command, is the cardinality rule read backwards: one recipient plus a return value is a command's shape. `derek-comartin`'s rule, publish an event instead of directing another module, is the same rule read forwards. Each was right about their own direction.

**One caveat.** The post is about distributed systems, and makes each application "its own bounded context". Applying it to modules in one process crosses a boundary the source didn't write for, the same caution this file applies to the `ms-learn` microservices e-book. The cardinality argument survives because it's about contracts, not transport, but say so when you cite it.

### The limit of the whole approach

`derek-comartin` gives the caveat to carry with any messaging opinion, which every vendor-adjacent source omits: "Event-driven architecture isn't some magical silver bullet that removes coupling; it doesn't." ([Comartin: Event-Driven Architecture lost its way](https://codeopinion.com/event-driven-architecture-lost-its-way/), 2024-03-07). And: "You did not change the amount of complexity in the business process. You moved the complexity somewhere else." ([Comartin: Decoupling in Software Architecture Moves Complexity](https://codeopinion.com/decoupling-in-software-architecture-moves-complexity/), 2026-08-06)

Both posts carry the Particular Software sponsorship in the body, as the roster notes record. Both claims cut against the sponsor's product category, which is the direction that matters.

## 3. Transactional boundaries

**The aggregate is the consistency boundary, and one transaction normally covers one aggregate.** `ms-learn` quotes Evans, "Any rule that spans Aggregates will not be expected to be up-to-date at all times", and Vernon, "if executing a command on one aggregate instance requires that additional business rules execute on one or more aggregates, use eventual consistency". The reason given is lock contention at scale. (Domain events page, reviewed 2018-10-08)

**`ms-learn` then breaks that rule itself, and says why:**

> the initial deferred approach—raising the events before committing, so you use a single transaction—is the simplest approach when using EF Core and a relational database. It's easier to implement and valid in many business cases.

The mechanism is one line with a large consequence. Domain events accumulate on the entity, and dispatch on one side of `SaveChangesAsync` or the other:

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

The sample keeps the eShopOnContainers shape. `DispatchDomainEventsAsync` is a hand-written extension method in that repository, not a MediatR API, and `architecture.md` treats mediator libraries as unsourced, so copy the dispatch point, not the library. `ms-learn` names the cost of dispatching after: "if there's an issue and the event handlers cannot commit their side effects, you'll have inconsistencies between aggregates", recoverable only by storing the events and running a reconciliation batch.

**`ardalis` names the two choices.** Take his vocabulary, even though his default is the riskier choice. He distinguishes pre-persistence and post-persistence domain events, and DevIQ has the same split: pre-persistence events "are typically triggered and resolved immediately", while post-persistence events "are typically queued up on the triggering entity, and then once the entity has been saved, the events are dispatched. This ensures the events are only handled once the state change has been persisted." ([DevIQ: Domain Events Pattern](https://deviq.com/design-patterns/domain-events-pattern/), no date shown; [Ardalis: Immediate Domain Event Salvation with MediatR](https://ardalis.com/immediate-domain-event-salvation-with-mediatr/), 2020-07-08)

That's `ms-learn`'s before-and-after distinction with better names, and the names are what to cite. The implementations aren't. His Clean Architecture template dispatches post-persistence, the side that needs an outbox, and has none. The 2020 post's pre-persistence sample calls a static `[ThreadStatic]` mediator holder from inside an entity, with `.Wait()` on an async publish. Cite the terms, nothing from the sample.

**`jeremy-miller` objects to this exact pattern.** He describes the classic .NET implementation, where an entity base class collects events and a mediator dispatches them on `SaveChangesAsync`, and says: "I have always hated this Domain Events pattern and much prefer the full 'Critter Stack' approach with the Decider pattern and event sourcing." ([Miller: "Classic" .NET Domain Events with Wolverine and EF Core](https://jeremydmiller.com/2025/12/04/classic-net-domain-events-with-wolverine-and-ef-core/), 2025-12-04)

That preference is for his own product's pattern, so it isn't citable as a verdict. The tool-independent point is: mediator dispatch has no outbox, and a domain-event handler that touches anything outside the transaction is unsafe without one. He says it generally elsewhere: "a lot of shops I think are naively opting into MediatR as a core tool without realizing the important functionality it is completely missing in order to build a resilient system — like a transactional outbox." He names MassTransit alongside his own tool as a valid answer, evidence he isn't arguing only for his catalogue. ([Miller: Build Resilient Systems with Wolverine's Transactional Outbox](https://jeremydmiller.com/2024/12/08/build-resilient-systems-with-wolverines-transactional-outbox/), 2024-12-08)

**The outbox makes a cross-boundary event reliable.** `ms-learn` describes it as "a transactional database table as a message queue", with the event row written "atomically with the original database operation, using a local transaction against the same database". `jeremy-miller`'s definition is tool-independent: "The general idea of an 'outbox' is to obviate the lack of true 2 phase commits by ensuring that outgoing messages are held until the database transaction is successful, then somehow guaranteeing that the messages will be sent out afterward." ([Miller: Transactional Outbox/Inbox with Wolverine and why you care](https://jeremydmiller.com/2022/12/15/transactional-outbox-inbox-with-wolverine-and-why-you-care/), 2022-12-15)

He doesn't overstate the guarantee. Nothing he writes claims exactly-once: a message "will be delivered at least once as long as the database transaction to save the new event succeeds".

### The consume side, which the first pass found missing

At-least-once delivery makes idempotent consumption mandatory, and the first pass found no page covering how. The Azure Architecture Center page now in scope does:

> The durable solution isn't to eliminate duplicate delivery. It's to make the consumer tolerate it.

It calls the inbox "the consume-side companion to the Transactional Outbox pattern": record each processed message ID in the same transaction as the work, so a redelivery finds the record and stops. Four points to carry into an opinion:

- **Prefer natural idempotency.** "Design for natural idempotency first." An operation that sets a value instead of incrementing one needs no bookkeeping.
- **Enforce with a unique constraint, not check-then-insert.** Reading for an existing record and then inserting races between concurrent consumers. "This approach makes the database the single arbiter of the race."
- **Keep records longer than redelivery can happen.** Keep the processed-message table longer than the broker's redelivery window, including dead-letter resubmission, or a late duplicate finds nothing and runs twice.
- **Use a stable message ID.** A producer-assigned ID survives redelivery, and one generated at consume time doesn't.

([Microsoft Learn: Idempotent Consumer pattern](https://learn.microsoft.com/azure/architecture/patterns/idempotent-consumer), reviewed 2026-08-13)

This page carries `ai-usage: ai-assisted` in its metadata, and the Event Sourcing page doesn't. That doesn't disqualify it, but note it on any citation.

### A defensible opinion for this repository

Inside a module, dispatch domain events before `SaveChanges`, in one transaction. A modular monolith has one database, and lock contention is a scale problem most modules never reach. Across modules, never share a transaction: publish through an outbox, consume idempotently, and accept eventual consistency. The transactional boundary then sits exactly on the ownership line the repository already draws.

`ms-learn` supports the second half without qualification: "No microservice should ever include tables/storage owned by another microservice in its own transactions, not even in direct queries", and it rules out two-phase commit as "against microservices principles". `derek-comartin` gives the modular-monolith version, the one that actually gets broken: stop joining tables across module boundaries, and check the queries, not the namespaces.

Link two existing files instead of repeating them. [data-access.md](../opinions/data-access.md) warns that `ExecuteUpdateAsync` and `ExecuteDeleteAsync` bypass the change tracker, so "audit and outbox logic in `SaveChanges` doesn't run", which is how an outbox stops working without an error. [aspnet-core.md](../opinions/aspnet-core.md) covers propagating OpenTelemetry context across outbox tables and queues.

## 4. When a module needs its own process

Four sources give criteria, and they combine into one list.

**`ms-learn` gives three signals, all observed in an existing module, not predicted.**

- **Scaling that cloning can't serve.** "Many applications, when they need to scale beyond a single instance, can do so through the relatively simple process of cloning that entire instance." Until one module's load profile diverges, a process boundary buys nothing.
- **Independent delivery.** "If you can't deliver independent feature slices of the application, separating it only adds complexity." A module that always ships with its neighbour doesn't qualify.
- **Chattiness, in both directions.** Check that "there are no chatty calls between services, and if splitting functionality into two services causes them to be overly chatty, it might indicate those functions belong in the same service". `ms-learn` also uses it as a merge signal: a design that "involves constantly aggregating information from multiple microservices for complex queries" might be "a reason to merge microservices."

**`jeremy-miller` adds co-change to the chattiness test, which improves it.** He says it twice, in nearly the same words:

> if two or more services/modules are chatty between themselves and very frequently have to be modified at the same time, they're best described as a single bounded context and should probably be combined into a single service or module.

([Miller: Modular Monoliths and the "Critter Stack"](https://jeremydmiller.com/2024/04/15/modular-monoliths-and-the-critter-stack/), 2024-04-15; restated 2025-02-27)

Take co-change frequency. It's measurable from git history, needs no instrumentation, and catches a bad boundary before any traffic exists.

He also gives the reason to defer the decision, from community experience, not his own tools: "In every single experience report you'll ever find about a team trying to break up and modernize a large monolithic application the authors will invariably say that breaking apart the database was the single most challenging task." He calls the shared-database failure the "Pond Scum Anti-Pattern": many modules over one murky schema.

His planning approach is Citadel and Outpost, which he credits to Glenn Henriksen: "I actually want to think a little bit upfront about having a path to easily break out modules into separate processes or services later." (2024-04-08)

**`oskar-dudycz` gives operational criteria.** He lists four reasons to split: different traffic and workload, independent horizontal scaling, tenant isolation, and multi-region deployment. He rejects two common ones: chasing total availability, and wanting a different storage technology. ([Dudycz: How (not) to cut microservices](https://event-driven.io/en/how_to_cut_microservices/), 2021-01-13)

**`derek-comartin` names the failure to watch for after splitting.** Temporal coupling through synchronous calls turns services into a distributed big ball of mud, and distributed tracing shows whether that's what you built.

**The cheapest predictor is already available.** A module that talks to its neighbours only through asynchronous messages can be extracted without rewriting its communication. That's `milan-jovanovic`'s reason for preferring messaging early: such modules are "much easier" to migrate. Extracting a module reached by direct interface calls is a rewrite, not a move. `mark-seemann`'s async argument makes the same point structurally: if the boundary was never hideable, moving it costs nothing at the call sites.

**`ardalis` gives the criterion none of the others do, and it isn't about software.** Conway's Law, read as a design constraint, puts team structure before module structure: "It would be unusual, and probably inefficient, to have a microservice that any number of different teams all share responsibility for maintaining and deploying", and "How you decompose and attach a large problem comes down to how you organize multiple teams of people, and if these teams aren't aligned with the software modules you're building and shipping, you're going to have problems." (2020-08-26)

In practice, ask a question no metric answers: does a separate team own this module? A process boundary between two modules one team ships together buys deployment independence nobody uses, and costs a network hop everybody pays. An opinion written from code alone is most likely to miss this, so put it beside the chattiness and co-change tests, not under them.

`mark-seemann` warns what happens when the boundary is drawn without autonomy: "Many organizations inadvertently create distributed monoliths. I think that this often stems from a failure of heeding the tenet that services are autonomous." ([Seemann: Services are autonomous](https://blog.ploeh.dk/2024/03/25/services-are-autonomous/), 2024-03-25)

## 5. Event sourcing and event-driven architecture

**Event-driven between modules: yes, with `derek-comartin`'s caveat.** Every source describes the same shape: integration events through an outbox, with idempotent consumers. Nothing in the sweep argues against it for a modular monolith. The sweep adds that it moves complexity instead of removing it, and doesn't remove coupling by itself.

**Event sourcing as a persistence model: rarely, per module, never by default.** This is the topic's strongest result: four sources reach it, and two of them build event sourcing tools for a living.

`ms-learn` opens the pattern page with the warning:

> Event sourcing is a complex pattern that introduces significant trade-offs. It changes how you store data, handle concurrency, evolve schemas, and query state. It's costly to migrate to or from an event sourcing solution, and after you adopt the pattern, it constrains future design decisions in the parts of the system that use it. [...] For most systems and most parts of a system, traditional data management is sufficient.

It lists five situations where it doesn't fit:

- Simple CRUD with no audit requirement.
- Prototypes and short-lived systems.
- Anything that needs real-time consistent views.
- Mostly static reference data.
- Teams without event-driven experience, where "adopting it without the foundational knowledge increases the risk of antipatterns that are costly to reverse". ([Microsoft Learn: Event Sourcing pattern](https://learn.microsoft.com/azure/architecture/patterns/event-sourcing), reviewed 2026-03-27)

The costs are concrete, and mostly permanent:

- Replay cost, which needs snapshots.
- Schema evolution, which needs upcasters or tolerant deserialization.
- No SQL-style querying.
- At-least-once delivery, which forces idempotent consumers.
- A conflict between an immutable log and the right to erasure. The only mitigations are keeping personal data out of the log, or crypto-shredding with a key per subject.

**`oskar-dudycz` says the same against his own commercial interest**, which gives the quote more weight than any other in this topic:

> I'm always saying that a well-done CRUD is much better than a poorly done Event Sourcing.

And, decisively for this repository:

> Event Sourcing is not a system-wide architecture concept. It should be considered at the module level.

([Dudycz: When not to use Event Sourcing?](https://event-driven.io/en/when_not_to_use_event_sourcing/), 2021-06-23). He co-maintains Marten and sells event sourcing workshops, so a post with this title is the opposite of a sales argument.

**`jeremy-miller` reaches the module-level rule independently of the page, but not of the person**, which item 2 below covers:

> one of our current JasperFx Software clients has a large monolithic application where some workflow-centric modules would be a good fit for an event sourcing approach, while other modules are more CRUD centric or reporting-centric where a straight up RDBMS approach is probably much more appropriate.

(2024-04-15; the same point appears on 2024-04-08). He has no essay on when not to use event sourcing. The position exists only as an aside in the modular-monolith posts.

**`ms-learn`'s closing advice is the opinion this repository would write:** "Event sourcing doesn't have to be an all-or-nothing decision for your entire system. Apply it selectively to the parts of your system that it benefits the most, such as a payment ledger or order-processing pipeline."

Two details people often get wrong:

- **An event store isn't a broker.** `ms-learn`: "Message brokers such as Apache Kafka typically lack per-entity stream queries and optimistic concurrency. They work well as a distribution layer [...] but they aren't a substitute for an event store." `oskar-dudycz` says it independently, more bluntly: "It's a common mistake to use tools like Kafka and Pulsar for event stores, but they are not." ([Dudycz: Event Streaming isn't Event Sourcing!](https://event-driven.io/en/event_streaming_is_not_event_sourcing/), 2021-12-01). `jeremy-miller` frames Kafka as a downstream relay out of an event store, not an alternative. Microsoft and Dudycz are independent of each other, so this is the safest claim in the section.
- **Design events for intent, not resulting state.** `ms-learn`: "an event that records _two seats were reserved_ is more valuable than an event that records _remaining seats changed to 42_." State-shaped events "reduce the event store to a change log that has no business meaning".

**`ardalis` adds a fifth cautious source, by omission.** DevIQ has no event sourcing entry: it's absent from the architecture index, the domain-driven design index, and the design patterns list. Its only coverage is one paragraph in the CQRS entry, which describes it as optional: "Event Sourcing is often combined with CQRS, though it's certainly not a prerequisite for using CQRS in your application." ([DevIQ: CQRS Pattern](https://deviq.com/design-patterns/cqrs-pattern/), no date shown)

An absence is weak evidence, and it doesn't mean he opposes the pattern. It counts for something because DevIQ is a 255-page taxonomy of patterns, antipatterns, and principles that covers CQRS, domain events, and event aggregators. Leaving out event sourcing is consistent with the four sources that argue for caution, and with none that treat it as a default. DevIQ also has no integration events or microservices entry, which makes it a weak map of what this topic should cover.

**Negative results, which decide what stays unsourced.**

- No source treats sagas as a design choice between choreography and orchestration.
- Event granularity has only `oskar-dudycz`, in the internal and external events post, and he's marked **Corroborate.** and writing about tooling he maintains, so it can't carry an opinion until something corroborates it.
- Event-carried state transfer now has two sources: `ardalis` reaches the mechanism from the data side, recommending a materialised view of another module's data "synchronized using events" instead of a cross-module join. Both are marked and neither names the pattern, so they corroborate the technique without the vocabulary.
- `mark-seemann` has no post on domain events, integration events, the outbox, idempotency, or event sourcing. In 832 posts, the only event store entry is a 2014 library of his own, and the messaging posts are from 2011 to 2013.
- `jeremy-miller` has no idempotent-consumer essay. His outbox writing is producer-side.
- `derek-comartin` has no dedicated domain-versus-integration-events post, and none on when not to use CQRS.

## Where the sources disagree

**Layering: solution-wide, per slice, or not at all. This pass rewrites this entry, because one side was smaller than it was counted.**

The second pass set `ms-learn` against `mark-seemann` and `jeremy-miller`. With `ardalis` admitted, the `ms-learn` page and the `ardalis` blog are one author, so the disagreement is three people, not four sources. And he's since changed position. In 2021, he prescribes three projects for anything non-trivial. In 2024, he says Clean Architecture's goal is decoupling from infrastructure, "That's it", denies that it delivers modularity, and recommends vertical slices for a whole class of applications.

**Weigh towards `mark-seemann`, `jeremy-miller`, and the later `ardalis`, who now point the same way.** They're more recent, and one is unmarked and independent. On what the named architectures are, the critic and the advocate agree: Seemann's "Layers, Onions, Ports, Adapters: it is all the same" and Ardalis's "Clean Architecture, aka Ports-and-Adapters" say the same thing. The `ms-learn` page holds the old position: 2021 text from a 2018 e-book that describes an unmodularised monolith, where layers are the only boundary available.

They still differ on scope. Seemann and Miller would remove the application layer at any scale. The later Ardalis keeps it, and chooses per application by complexity. None of them supports layering inside each slice, which is what `architecture.md` says, so its headline loses on all three readings.

**Commands across boundaries: avoid them, or insist on them.** `derek-comartin` says generally avoid crossing boundaries with commands. `oskar-dudycz` says an event with one known consumer that expects a reply should have been a command. Neither is wrong. They disagree about which failure is more common: Comartin guards against modules directing each other, and Dudycz against events used to look decoupled while the coupling stays.

**Weigh them by situation.** When you choose a message type for a new interaction, use Comartin's rule, and publish an event. When you audit an existing interaction, use Dudycz's test: one consumer plus an expected response means you already have a command. Both are marked **Corroborate.**, and neither can carry this alone, so an opinion here needs a third source.

**Event sourcing: cautious or enthusiastic.** `ms-learn` leads with cost. `milan-jovanovic`'s introduction leads with benefits: reconstructing state at any point, historical data already present when a feature needs it, and use cases in e-commerce, finance, and IoT. ([Jovanović: Introduction to Event Sourcing for .NET Developers](https://www.milanjovanovic.tech/blog/introduction-to-event-sourcing-for-net-developers), 2024-08-31) **Weigh towards `ms-learn`:** it's independent, its page is recent (2026-03-27), and it lists the exit costs. `oskar-dudycz` settles it: he sells the workshops, and still says a well-done CRUD beats a poorly done event store.

**Modular monolith: default, or fashion.** `architecture.md` and `milan-jovanovic` state it as a default. `mark-seemann` treats it as a swing of the pendulum, and recommends physical package separation for teams without functional programming discipline. `jeremy-miller` openly doubts it's a panacea. `ardalis` endorses it warmly but conditionally, and elsewhere argues from Conway's Law that team structure decides. No source contradicts the opinion, and three decline to endorse it as stated. That's not a reason to drop it, but it is a reason to state it as the cheaper, reversible default, not as consensus, and to name the team condition the sources keep returning to.

## Freshness

**A correction from the first pass, affecting every `ms-learn` citation.** The roster rule for `ms-learn` is to quote the page's own review date, which is the author-set `ms.date` field. The first pass quoted `updated_at`, a docset build timestamp shared by every page in a bulk republish.

| Page                                                     | First pass claimed | Actual `ms.date` | `author` field     |
| -------------------------------------------------------- | ------------------ | ---------------- | ------------------ |
| Common web application architectures                     | 2026-07-08         | **2021-12-12**   | `ardalis`          |
| Challenges and solutions for distributed data management | 2023-10-12         | **2018-09-20**   | `jamesmontemagno`  |
| Domain events, design and implementation                 | 2024-01-03         | **2018-10-08**   | `jamesmontemagno`  |
| Applying simplified CQRS and DDD patterns                | 2023-05-27         | **2021-01-13**   | `jamesmontemagno`  |
| Event Sourcing pattern                                   | 2026-08-15         | **2026-03-27**   | `claytonsiemens77` |
| Idempotent Consumer pattern                              | not read           | **2026-08-13**   | `claytonsiemens77` |

The last column is the page's `author` metadata, a GitHub handle, not the `ms.author` alias beside it: `ardalis` is `wiwagn`, `jamesmontemagno` is `jamont`, and `claytonsiemens77` is `pnp`. Two of the three handles are roster IDs spelled differently: `ardalis` and `james-montemagno`. [What this reveals](#what-this-reveals-about-the-repository) picks that up.

The quoted field is provably a build stamp, twice over. Event Sourcing and Idempotent Consumer share `updated_at: 2026-08-15T05:02:00Z` and `git_commit_id: d73cd1632ffa489eff78a16bba4d101c510a1810`: one commit, two pages, one timestamp. And the stamp moves without the text: rechecked on 2026-09-25, Event Sourcing reads `updated_at: 2026-09-25T05:03:00Z`, while its `ms.date` still reads 2026-03-27. Six weeks of republishes, no review.

**Consequences.** The microservices e-book material is 2018 and 2021 text, not 2023 and 2024. The first pass's argument that age doesn't disqualify it still holds, because the reasoning is durable and the DDD citations behind it are older still, but it now has to hold against 2018. Two consequences carry into any opinion:

- The e-book is written for containerised microservices, not modular monoliths, so every claim from it here crosses a boundary the source didn't write for.
- It predates the modular monolith as a named pattern, which is partly why it says nothing about slices.

Only the two Azure Architecture Center pattern pages are current.

**DevIQ has a worse dating problem.** The `ms-learn` correction was about quoting the wrong field. DevIQ shows a reader no date at all. Every sampled page keeps its dates in unrendered metadata, and on every one the published and modified values are identical, so nothing distinguishes an original from a revision. The domain-events entry stamps 2015-09-20 in all four date fields while teaching MediatR with `INotification` and `INotificationHandler`, and the modular monolith entry stamps 2026-03-01 in both. Neither can be quoted as a review date the way the roster's `ms-learn` rule requires, because there's no evidence either was ever reviewed. Cite DevIQ for definitions and taxonomy, and date the claim from a blog post or another source.

**Shared authorship is a freshness problem as well as an independence one.** The e-book pages have individual authors, and two are roster IDs. Common web application architectures and the `ardalis` blog have the same author, so the page's 2021 date isn't a second opinion holding for five years: it's one person's older position, still published, beside his newer one. Anywhere this file cites both, it cites him twice. The three microservices pages are by `james-montemagno`, whose roster notes record a live independence concern and limit citations to before 2024. Their 2018 dates meet that limit, but know it before treating them as the independent leg of an argument.

**The other sources.** `ardalis`'s blog material runs from 2020 to 2024, in C# where it has code, which is rarely. The two properties that matter most to the layering argument, the template documentation and DevIQ, show no date. `mark-seemann`'s layering position is from 2025-04-01 and his boundary series from 2024, both current and both C#. His slice remark is from 2023-09-18. `jeremy-miller`'s design essays run from 2024-04 to 2026-06, all C#. `oskar-dudycz`'s slice post is from 2026-08-10, with TypeScript examples, which his roster notes require flagging. His event sourcing posts are from 2021, and his internal and external events post is from 2023 and uses Marten. `derek-comartin`'s material runs from 2022 to 2026, with no code by design.

Nothing in this topic depends on a preview. All of it is GA guidance.

## What this reveals about the repository

1. **The remaining TODO can be closed, and the part already closed needs another look.** Of the five older opinions, the first, third, and fifth gain corroboration and only need citations. The second gains three marked sources, but the one unmarked source reads its central term differently, so it should stop implying the framing is uncontested. The fourth must be rewritten, not cited: no source swept supports its headline, now including the advocate of the pattern it names. Sections 3 and 4 answer the two extensions the comment still asks for.

   The 2026-09-14 harvest closed the corroboration and messaging asks correctly, and the two opinions it added hold up. But it didn't re-read the existing opinions against the sources it added, which is how the fourth kept a headline that `mark-seemann` and `jeremy-miller` contradict and `ardalis` has abandoned. Route the whole topic through `resolve-research`. And when a harvest adds sources to a file, it should recheck the claims already there.

2. **The roster can record that a source has a conflict, but not that two sources share one.** This pass found a second case, in a different shape, so the gap is structural.
   - **One product, two people.** `jeremy-miller` created Marten, and `oskar-dudycz` co-maintains it. Their agreement that event sourcing is a module-level decision is one voice.
   - **One person, two IDs, twice.** The `ms-learn` architecture e-book isn't anonymous. The `author` field names `ardalis` on Common web application architectures, and `james-montemagno` on all three microservices pages: distributed data management, domain events, and simplified CQRS. Both are citable roster IDs. Citing `ms-learn` beside `ardalis` on layering cites him twice. Citing the microservices pages as independent support cites a source whose notes record a live independence concern and, since the 2026-08-23 re-vetting, a pre-2024 limit. The pages are from 2018, inside that limit, so nothing has to be withdrawn, but the independence they were counted for is thinner than the ID suggests.

   The second shape is more dangerous, because `scripts/validate-sources.cs` counts IDs and nothing else. A file citing `[ms-learn, ardalis]` passes the **Corroborate.** check, because `ms-learn` is unmarked and stands beside the marked `ardalis`, while both rest on one author. The check can't catch this, and shouldn't try, because authorship isn't in the roster's data. The roster can record the pairing in the notes, as it already records single-source conflicts. Raise it with `vet-source` for four rows: `jeremy-miller` and `oskar-dudycz` as a pair, and `ms-learn` against both `ardalis` and `james-montemagno`.

3. **The `ms-learn` citation rule is fixed, and DevIQ needs a stricter fix.** The rule now names `ms.date` and explains `updated_at`, which ends the error that put six dates wrong in the previous pass. DevIQ shows no date, and its metadata dates are identical for published and modified on every sampled page, so there's no field to name. The workable rule: DevIQ is citable for definitions and taxonomy, but can't date a claim, so take the date for anything time-sensitive from another source. That's a `vet-source` change to the `ardalis` row, whose existing "reference-grade on definitions and taxonomy" wording already covers most of it.

4. **The first pass's `vet-source` candidates are resolved, with one correction.**
   - **Steve Smith (`ardalis`):** Admitted on 2026-09-10, swept here. He wasn't a candidate. He came through the roster's own vetting, and turned out to matter more to this topic than any admission since the first pass.
   - **Mark Seemann, Jeremy D Miller, and Derek Comartin:** Admitted on 2026-09-03, swept here.
   - **Oskar Dudycz:** Admitted on 2026-08-24, unswept by the first pass, swept here. A correction to that pass: his "Monolith-First - are you sure?" post is on `architecture-weekly.com`, a Substack, not the `event-driven.io` domain in his roster row. It's out of scope, and no claim here rests on it.
   - **Jimmy Bogard:** Active to 2026-09-01, but his recent output is AutoMapper and MediatR release posts, plus promotion of his own commercial Vertical Slice Architecture webinar (2026-09-01 and 2026-07-23). He'd carry the release-notes depth concern that `jeremy-miller`'s marking exists for, and a direct commercial interest in the opinion this topic is trying to corroborate. He originated the vertical-slice framing and the deferred domain-events pattern `ms-learn` quotes, so his archive matters, but the case for admission is weaker than the first pass assumed.
   - **Kamil Grzybek:** Unchanged. Watch list, not admission. Nothing published since 2023-12-05.
   - **Udi Dahan:** Unchanged. Watch-listed on 2026-08-24, dormant since 2016-02-19, for discovery and cross-checking only.

5. **`derek-comartin`'s sponsorship is broader than his roster notes say.** They say to "flag the sponsorship on anything about message queues". In this sweep, the Particular Software block appeared in the body of every post fetched, including the modular-monolith and coupling posts that have nothing to do with queues. Make the flag unconditional for this source.

6. **The roster gap is down to one item: saga style.** Choreography versus orchestration still returns nothing across five sources and the documentation. Commands versus events is settled: `ardalis` gave the cardinality rule, and since all three sources on it are marked, `ms-learn` supplies the unmarked one on the distributed side. Event-carried state transfer moved from one source to two. Event granularity still has only `oskar-dudycz`.

7. **This topic has changed the roster four times in three passes.** After the first pass, `oskar-dudycz` was admitted and the `ms-learn` scope widened. After the second, the `ms.date` rule was named. This pass raises the shared-authorship gap and the DevIQ dating rule. Research can find defects in the roster as well as use it, so a topic that takes many passes hasn't necessarily stalled.

8. **Cross-links for when these become opinions:** Link the outbox note in [data-access.md](../opinions/data-access.md) and the context-propagation note in [aspnet-core.md](../opinions/aspnet-core.md) instead of repeating them. `ardalis`'s navigation-property rule is an EF Core configuration rule, so it belongs in [data-access.md](../opinions/data-access.md) as much as `architecture.md`. `resolve-research` should choose one file, not duplicate it.
