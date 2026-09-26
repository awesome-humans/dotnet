---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-14
last-used: 2026-09-25
sources:
  [milan-jovanovic, code-with-mukesh, mark-seemann, ms-learn, derek-comartin]
---

# Application architecture

Start with a modular monolith. Organise each module as vertical slices, and use layered (Clean) architecture only where domain rules justify the dependency boundaries.

<!-- Seeded 2026-08-14 by harvest-sources from newly admitted sources; see AWESOME-HUMANS.md decision log -->

## Opinions

- **Default to a modular monolith, not microservices.** Modules define the macro boundaries: business ownership, data ownership, communication, and public APIs. Make that decision early. A process boundary is a deployment choice you can make later, once a module has earned one. ([Jovanović: Modular monolith architecture in .NET](https://milanjovanovic.tech/blog/modular-monolith-architecture-dotnet))
- **Organise the inside of a module as vertical slices.** One use case owns its endpoint, request, validation, business logic, and data access. A feature can then change in one place instead of across a controller, service, and repository that exist only to separate technologies. ([Jovanović: Vertical slice architecture in .NET](https://milanjovanovic.tech/blog/vertical-slice-architecture-dotnet))
- **Vertical slices are not modules.** Slices organise behaviour within a boundary; modules define the boundary. Treating a slice as a module gives you a folder structure without the ownership guarantees of a module. ([Jovanović: Where vertical slices fit inside the modular monolith](https://milanjovanovic.tech/blog/where-vertical-slices-fit-inside-the-modular-monolith-architecture))
- **Apply Clean Architecture per slice, not per solution.** Layering controls dependency direction. Pay that cost where domain rules must stay independent of infrastructure; avoid it in a slice that reads a row and returns it. Forcing every slice through identical layers can turn a one-field change into edits across the schema, persistence type, domain type, view model, and their mappings. Seemann used that cost to argue that a CRUD-shaped application should not be layered at all. ([Jovanović: Vertical slice architecture in .NET](https://milanjovanovic.tech/blog/vertical-slice-architecture-dotnet), [Mukesh: Clean Architecture in .NET 10](https://codewithmukesh.com/blog/clean-architecture-dotnet/), [Seemann: Is Layering Worth the Mapping?](https://blog.ploeh.dk/2012/02/09/IsLayeringWorththeMapping/))
- **Make a module boundary something the compiler propagates.** A boundary that looks like an ordinary method call is easy to cross accidentally, which is how WCF invited round trips through code that looked local. Asynchrony makes the crossing explicit: `async` is contagious, so every caller up the stack has to acknowledge it. A helper method cannot hide the boundary again. ([Seemann: Boundaries are Explicit](https://blog.ploeh.dk/2024/03/11/boundaries-are-explicit/))
- **Publish integration events between modules; keep domain events inside the module that raised them.** A domain event is an implementation detail. Exposing it couples consumers to the internal model as tightly as publishing its schema. The distinction is about implementation: a domain event dispatches in-process within one model, while an integration event crosses the boundary asynchronously. Translate the internal sequence into one business fact, such as `OrderDispatched`, rather than publishing the `TruckReserved` and `CarrierAssigned` steps that produced it. Name events for the business fact: `TruckOrderNotUsed` and a generic cancellation can have different downstream meanings. ([Microsoft Learn: Domain events: design and implementation](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation), 2018 content last reviewed 2024-01-03; [Comartin: Domain Events Are NOT Your Public API](https://codeopinion.com/domain-events-are-not-your-public-api/), which carries an NServiceBus sponsorship in its body)
- **Let the folder structure name the feature, not the pattern.** `Orders/Cancel/` beats `Controllers/`, `Services/`, `Repositories/`. The repository layout opinions in [project-structure.md](project-structure.md) stop at the project boundary; inside a project, features are the top-level grouping.

## Source redundancy

The file opened on two sources admitted the same day under the lowered longevity bars: `milan-jovanovic`, with a conflict-of-interest note, and `code-with-mukesh`, marked **Corroborate.** Both sell templates and courses built on these patterns. `mark-seemann` and `ms-learn` now stand beside them, unmarked and with nothing to sell on this topic, so the central claims do not rest on commercially interested sources alone. `derek-comartin` is marked **Corroborate.** and takes NServiceBus sponsorship inside the article body, so the event bullet leads on Microsoft Learn and flags the sponsorship where a reader will see it.

Treat anything stronger than the above as unsourced: prescribed folder names, mediator libraries, and per-slice project counts.

<!-- TODO: extend with transactional boundaries across modules, and when a module has earned a process boundary -->
