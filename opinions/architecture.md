---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-14
last-used: 2026-09-25
sources:
  [milan-jovanovic, code-with-mukesh, mark-seemann, ms-learn, derek-comartin]
---

# Application architecture

Start with a modular monolith. Build each module from vertical slices, and add layered (Clean) architecture only where domain rules need the dependency boundaries.

<!-- Seeded 2026-08-14 by harvest-sources from newly admitted sources; see AWESOME-HUMANS.md decision log -->

## Opinions

- **Default to a modular monolith, not microservices.** Modules set the large boundaries: business ownership, data ownership, communication, and public APIs. Decide those early. A process boundary is a deployment choice you can add later, when a module needs one. ([Jovanović: Modular monolith architecture in .NET](https://milanjovanovic.tech/blog/modular-monolith-architecture-dotnet))
- **Build each module from vertical slices.** One use case owns its endpoint, request, validation, business logic, and data access. A feature then changes in one place, not across a controller, a service, and a repository that exist only to separate technologies. ([Jovanović: Vertical slice architecture in .NET](https://milanjovanovic.tech/blog/vertical-slice-architecture-dotnet))
- **Don't treat a slice as a module.** A slice organises behaviour inside a boundary, and a module defines the boundary. A slice used as a module gives you folders without a module's ownership rules. ([Jovanović: Where vertical slices fit inside the modular monolith](https://milanjovanovic.tech/blog/where-vertical-slices-fit-inside-the-modular-monolith-architecture))
- **Apply Clean Architecture per slice, not per solution.** Layers control dependency direction. Pay for them where domain rules must stay independent of infrastructure, and skip them in a slice that reads a row and returns it. With the same layers in every slice, a one-field change touches the schema, the persistence type, the domain type, the view model, and the mappings between them. Seemann uses that cost to argue against layering a CRUD application at all. ([Jovanović: Vertical slice architecture in .NET](https://milanjovanovic.tech/blog/vertical-slice-architecture-dotnet), [Mukesh: Clean Architecture in .NET 10](https://codewithmukesh.com/blog/clean-architecture-dotnet/), [Seemann: Is Layering Worth the Mapping?](https://blog.ploeh.dk/2012/02/09/IsLayeringWorththeMapping/))
- **Make a module boundary visible to the compiler.** A call across a boundary that looks like a local method call is easy to make by accident. WCF invited round trips that way. Make the call `async`: every caller up the stack then has to handle the `Task`, and no helper method can hide the crossing. ([Seemann: Boundaries are Explicit](https://blog.ploeh.dk/2024/03/11/boundaries-are-explicit/))
- **Publish integration events between modules, and keep domain events inside the module that raised them.** A domain event dispatches in-process within one model. An integration event crosses the boundary asynchronously. Publishing a domain event couples consumers to the module's internal model as tightly as publishing its schema would.
  - Translate an internal sequence into one business fact. Publish `OrderDispatched`, not the `TruckReserved` and `CarrierAssigned` steps behind it.
  - Name an event for its business meaning. `TruckOrderNotUsed` and a generic cancellation can mean different things downstream.

  ([Microsoft Learn: Domain events: design and implementation](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation), 2018 content last reviewed 2024-01-03; [Comartin: Domain Events Are NOT Your Public API](https://codeopinion.com/domain-events-are-not-your-public-api/), sponsored by NServiceBus in its body)

- **Name folders after features, not patterns.** Use `Orders/Cancel/`, not `Controllers/`, `Services/`, and `Repositories/`. [project-structure.md](project-structure.md) covers layout down to the project. Inside a project, features are the top-level folders.

## Source redundancy

Two sources seeded this file on the same day, both admitted under the lowered longevity bars:

- `milan-jovanovic`, with a conflict-of-interest note.
- `code-with-mukesh`, marked **Corroborate.**

Both sell templates and courses built on these patterns. `mark-seemann` and `ms-learn` now support the same claims, unmarked and with nothing to sell on this topic. `derek-comartin` is marked **Corroborate.**, and NServiceBus sponsors the cited article. So the events bullet cites Microsoft Learn first and flags the sponsorship beside the link.

Treat anything more specific than these opinions as unsourced, such as prescribed folder names, mediator libraries, and project counts per slice.

<!-- TODO: extend with transactional boundaries across modules, and when a module needs its own process -->
