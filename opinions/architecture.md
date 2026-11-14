---
targets: [net11.0, csharp-15]
last-reviewed: 2026-11-14
sources:
  [code-with-mukesh, derek-comartin, mark-seemann, milan-jovanovic, ms-learn]
---

# Application architecture

Start with a modular monolith. Build each module from vertical slices, and add layered (Clean) architecture only where domain rules need the dependency boundaries.

<!-- Seeded 2026-08-14 by harvest-sources from newly admitted sources; see AWESOME-HUMANS.md decision log -->

## Opinions

- **Default to a modular monolith, not microservices.** Modules set the large boundaries: business ownership, data ownership, communication, and public APIs. Getting them right is the most important design decision. Move a module into its own process only when you've measured a need for one of these, and it justifies the cost of a distributed system:
  - Independent scaling or availability
  - A separate team and release cadence
  - A security or compliance boundary

  Being easy to extract isn't a reason. ([Jovanović: Modular monolith architecture in .NET](https://milanjovanovic.tech/blog/modular-monolith-architecture-dotnet))

- **Build each module from vertical slices.** Put everything one use case needs in its slice: the endpoint, request, validation, business logic, and data access. A feature then changes in one place, not across a controller, a service, and a repository that exist only to separate technologies. ([Jovanović: Vertical slice architecture in .NET](https://milanjovanovic.tech/blog/vertical-slice-architecture-dotnet))
- **Don't treat a slice as a module.** A module defines a boundary: it owns its data and exposes a public API. A slice organises the code for one feature inside that boundary. ([Jovanović: Where vertical slices fit inside the modular monolith](https://milanjovanovic.tech/blog/where-vertical-slices-fit-inside-the-modular-monolith-architecture))
- **Apply Clean Architecture per slice, not per solution.** Layers control dependency direction. Use them in a slice whose domain rules must stay independent of infrastructure, and skip them in a slice that only reads a database row and returns it. In a fully layered app, a one-field change touches the schema, the persistence type, the domain type, the view model, and the mappings between them. Seemann counts those changes and goes further than this opinion: he argues against layering a CRUD application at all. ([Jovanović: Vertical slice architecture in .NET](https://milanjovanovic.tech/blog/vertical-slice-architecture-dotnet), [Mukesh: Clean Architecture in .NET 10](https://codewithmukesh.com/blog/clean-architecture-dotnet/), [Seemann: Is Layering Worth the Mapping?](https://blog.ploeh.dk/2012/02/09/IsLayeringWorththeMapping/))
- **Make any network or other I/O call `async`, so a remote call can't pass for a local one.** A network call that looks like any other method call is easy to make without noticing, even inside a loop. An `async` call can't hide, because its `Task` spreads to every caller up the stack. Keep the domain model and business rules pure and synchronous, and make the I/O around them `async` in the slice's handler: "This doesn't mean that your entire code base has to be asynchronous." ([Seemann: Boundaries are Explicit](https://blog.ploeh.dk/2024/03/11/boundaries-are-explicit/))
- **Publish integration events between modules, and keep domain events inside the module that raised them.** A domain event dispatches in-process within one model. An integration event crosses the boundary asynchronously. Publishing a domain event couples consumers to the module's internal model as tightly as publishing its schema would.
  - Translate an internal sequence into one business fact. Publish `OrderDispatched`, not the `TruckReserved` and `CarrierAssigned` steps behind it.
  - Name an event for its business meaning. `TruckOrderNotUsed` says the truck arrived, which billing and carrier settlement treat differently from a cancellation before dispatch.

  ([Microsoft Learn: Domain events: design and implementation](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation), 2018 content last reviewed 2024-01-03; [Comartin: Domain Events Are NOT Your Public API](https://codeopinion.com/domain-events-are-not-your-public-api/), sponsored by NServiceBus)

- **Name folders after features, not patterns.** Use a feature folder such as `Orders/Cancel/`, not `Controllers/`, `Services/`, and `Repositories/`. Inside a project, features are the top-level folders. For layout down to the project, see [project-structure.md](project-structure.md). ([Jovanović: Vertical slice architecture in .NET](https://milanjovanovic.tech/blog/vertical-slice-architecture-dotnet))

## Source redundancy

`milan-jovanovic`, with a recorded conflict of interest, and `code-with-mukesh`, marked **Corroborate.**, seeded this file. Both sell templates and courses built on these patterns. `mark-seemann` and `ms-learn` support the same claims, unmarked and with nothing to sell on this topic. `derek-comartin` is marked **Corroborate.**, and NServiceBus sponsors the cited article, so the events bullet cites Microsoft Learn first and flags the sponsorship beside the link.

Treat anything more specific than these opinions as unsourced, such as prescribed folder names, mediator libraries, and project counts per slice.

<!-- TODO: extend with transactional boundaries across modules -->
