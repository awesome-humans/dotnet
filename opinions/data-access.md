---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-14
last-used: 2026-09-25
sources: [code-with-mukesh, milan-jovanovic, shay-rojansky]
---

# Data access

Use EF Core as the default ORM. Write set-based work as set-based SQL, and keep the change tracker for the writes that need it.

<!-- Seeded 2026-08-14 by harvest-sources from newly admitted sources; see AWESOME-HUMANS.md decision log -->

## Opinions

- **Use `ExecuteUpdateAsync` and `ExecuteDeleteAsync` for updates and deletes that don't need the entities loaded.** Load-modify-`SaveChanges` pulls every row into the change tracker to emit statements the database could run by itself. The set-based APIs issue one `UPDATE` or `DELETE`. Mukesh and Jovanović measure them hundreds of times faster on 10,000-row operations. ([Mukesh: Bulk operations in EF Core 10](https://codewithmukesh.com/blog/bulk-operations-efcore/), [Jovanović: What you need to know about EF Core bulk updates](https://milanjovanovic.tech/blog/what-you-need-to-know-about-ef-core-bulk-updates))

  ```csharp
  // Set-based: one statement, no entities materialized
  await db.Orders
      .Where(o => o.Status == OrderStatus.Pending && o.Placed < cutoff)
      .ExecuteUpdateAsync(
          s => s.SetProperty(o => o.Status, OrderStatus.Expired),
          cancellationToken);
  ```

- **Keep audited writes on `SaveChanges`.** The set-based APIs bypass the change tracker. Interceptors don't fire, audit and outbox logic in `SaveChanges` doesn't run, and global query filters don't apply to the predicate, so a soft-delete filter is silently missing. Write the filter into the `Where` clause, or keep the write on `SaveChanges`. ([Mukesh: Bulk operations in EF Core 10](https://codewithmukesh.com/blog/bulk-operations-efcore/), [Jovanović: EF Core bulk updates](https://milanjovanovic.tech/blog/what-you-need-to-know-about-ef-core-bulk-updates))
- **Model dates and times with the types in [datetime.md](datetime.md), and let EF Core map them.** On EF Core 8 and later, `DateOnly` and `TimeOnly` map to SQL `date` and `time` columns. Don't store a date in a `DateTime`. For provider differences, and for UTC versus local storage, see [datetime.md](datetime.md#persistence-split-by-kind-of-data-not-by-layer).
- **On PostgreSQL, store instants as `DateTime` with `Kind.Utc`, which Npgsql maps to `timestamptz`.** Despite its name, `timestamptz` stores a UTC instant and no time zone, so a `DateTimeOffset`'s offset has nowhere to go. Npgsql:
  - Rejects a `DateTimeOffset` with a non-zero offset.
  - Maps `Kind.Utc` to `timestamptz`.
  - Maps `Local` and `Unspecified` to `timestamp`.

  This "UTC everywhere" rule describes what the column can store without loss. It doesn't contradict the split in [datetime.md](datetime.md#persistence-split-by-kind-of-data-not-by-layer). The instant column is UTC either way, and a future or recurring event that a person schedules still needs its local time and IANA zone ID in columns beside it. ([Rojansky: PostgreSQL/.NET timestamp mapping](https://www.roji.org/postgresql-dotnet-timestamp-mapping))

- **Choose indexes from the queries the application runs, and prove each one with `EXPLAIN ANALYZE`.** In a B-tree index, put equality columns first and the range or sort column last. Compare the plan before and after: it's the only evidence that the database uses the index. In Jovanović's demo with a million comments, a count drops from a 17 ms sequential scan to a 0.6 ms index-only scan, and a query on issues and users drops from 436 ms to about 0.5 ms. Reproduce numbers like these on your own data. ([Jovanović: SQL indexing explained: composite indexes and column order](https://www.milanjovanovic.tech/blog/how-to-design-the-right-sql-index))
- **Enforce tenant isolation with PostgreSQL row-level security under your global query filter.** EF Core adds the filter only to the SQL it generates. `ExecuteSql`, an attached entity you save, and anything behind `IgnoreQueryFilters()` skip it. A row-level security policy applies to every statement from every non-exempt role, so it still holds when someone forgets the filter. To make it hold:
  - Connect as a role that owns nothing, because owners, superusers, and `BYPASSRLS` roles skip policies. Run migrations as a separate owner role.
  - Add `FORCE ROW LEVEL SECURITY`, so the policy covers the owner too.
  - Write the predicate so that an unset tenant matches no rows. ([Jovanović: PostgreSQL row-level security with EF Core and Npgsql](https://www.milanjovanovic.tech/blog/postgres-row-level-security-with-ef-core))
- **Set the tenant when the connection opens, not when the request starts.** EF Core opens and closes a connection for each command, and Npgsql resets pooled session state. A `SET` at the start of a request runs on a connection the next query doesn't use. Disabling the reset is worse: the next request then gets the previous tenant. Override `ConnectionOpened` in a `DbConnectionInterceptor`, which runs on whichever physical connection the pool hands out. ([Jovanović: PostgreSQL row-level security with EF Core and Npgsql](https://www.milanjovanovic.tech/blog/postgres-row-level-security-with-ef-core))
- **Insert with `AddRange` and one `SaveChanges` call, and switch to bulk copy only above about 10,000 rows.** The batched call keeps interceptors and audit trails working, and it's fast enough for ordinary writes. Never add and save entities one at a time in a loop: it's about ten times slower. ([Mukesh: Fastest way to bulk insert thousands of rows in EF Core](https://codewithmukesh.com/blog/ef-core-bulk-insert/))

## Source redundancy

The benchmark numbers in the first opinion come from `code-with-mukesh`, marked **Corroborate.**, and `milan-jovanovic`, and this repository hasn't reproduced them. Both support the general rule: use set-based writes for set-based work, and accept losing change tracker behaviour. Before you quote the ratios, check them against the EF Core documentation on Microsoft Learn.

`shay-rojansky` has a recorded independence limit: he maintains Npgsql and works on Microsoft's EF Core team. This file cites him only for how the mapping works, as his notes allow. When to use those types comes from `jon-skeet`, in [datetime.md](datetime.md).

<!-- TODO: extend with query-side opinions (AsNoTracking defaults, split queries, projection over Include), migrations workflow, and connection resiliency. None of it is sourced yet. -->
