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

- **Use `ExecuteUpdateAsync` and `ExecuteDeleteAsync` for updates and deletes that don't need the entities loaded.** Loading, modifying, and calling `SaveChanges` pulls every row into the change tracker, only to emit statements the database could generate itself. The set-based APIs issue one `UPDATE` or `DELETE`. Independent benchmarks put the difference at hundreds of times on 10,000-row operations. ([Mukesh: Bulk operations in EF Core 10](https://codewithmukesh.com/blog/bulk-operations-efcore/), [Jovanović: What you need to know about EF Core bulk updates](https://milanjovanovic.tech/blog/what-you-need-to-know-about-ef-core-bulk-updates))

  ```csharp
  // Set-based: one statement, no entities materialized
  await db.Orders
      .Where(o => o.Status == OrderStatus.Pending && o.Placed < cutoff)
      .ExecuteUpdateAsync(
          s => s.SetProperty(o => o.Status, OrderStatus.Expired),
          cancellationToken);
  ```

- **Know what the set-based APIs skip, and keep audited writes on `SaveChanges`.** They bypass the change tracker entirely. Interceptors don't fire, audit and outbox logic in `SaveChanges` doesn't run, and global query filters aren't applied to the predicate. So a soft-delete filter that you rely on everywhere else is silently missing. Write the filter out in the `Where` clause, or keep that write on `SaveChanges`. ([Mukesh: Bulk operations in EF Core 10](https://codewithmukesh.com/blog/bulk-operations-efcore/), [Jovanović: EF Core bulk updates](https://milanjovanovic.tech/blog/what-you-need-to-know-about-ef-core-bulk-updates))
- **Model dates and times with the types in [datetime.md](datetime.md), and let EF Core map them natively.** On EF Core 8 and later, `DateOnly` and `TimeOnly` map to the SQL date and time column types. Don't use the legacy pattern of a `DateTime` that holds only a date. For provider differences, and for the choice between UTC and local storage, see [datetime.md](datetime.md#persistence-split-by-kind-of-data-not-by-layer).
- **On PostgreSQL, store instants as `DateTime` with `Kind.Utc`, and let Npgsql map them to `timestamptz`.** Despite its name, `timestamptz` stores a UTC instant and no time zone. The offset on a `DateTimeOffset` has nowhere to go, so Npgsql:

- Rejects any `DateTimeOffset` with a non-zero offset.
- Maps `Kind.Utc` to `timestamptz`.
- Maps `Local` and `Unspecified` to plain `timestamp`.

The resulting "UTC everywhere" rule comes from the provider, not the domain. It describes what the column can store without loss, not what the domain can afford to forget. So it supports the split in [datetime.md](datetime.md#persistence-split-by-kind-of-data-not-by-layer) instead of contradicting it. The instant column is UTC either way, and a future or recurring event that a person schedules still needs its local time and IANA zone ID in their own columns beside it. ([Rojansky: PostgreSQL/.NET timestamp mapping](https://www.roji.org/postgresql-dotnet-timestamp-mapping))

- **Choose indexes from the queries the application runs, and then prove each one with `EXPLAIN ANALYZE`.** The access paths tell you what to index, not the schema. For a B-tree index, put the columns compared by equality first, and the column you filter by range or sort by last. Measure before and after, because the query plan is the only evidence that the database uses the index. In Jovanović's issue tracker demo over a million comments, a count drops from a 17 ms sequential scan to a 0.6 ms index-only scan, and a query on issues and users drops from 436 ms to about 0.5 ms. Those are his numbers on his data, so reproduce them on yours. ([Jovanović: SQL indexing explained: composite indexes and column order](https://www.milanjovanovic.tech/blog/how-to-design-the-right-sql-index))
- **Put row-level security under your global query filter. A query filter is a convenience, not tenant isolation.** EF Core adds the predicate only to the SQL it generates. `ExecuteSql`, an attached entity that you save, and anything behind `IgnoreQueryFilters()` bypass it. A PostgreSQL policy applies to every statement for every role that isn't exempt, so it still holds when someone forgets the filter. Three details decide whether it really holds:

- Connect as a role that owns nothing, because owners, superusers, and `BYPASSRLS` roles skip policies. Run migrations as a separate owner role.
- Add `FORCE ROW LEVEL SECURITY`, so the policy covers the owner too.
- Write the predicate so that an unset tenant matches no rows, not every row. ([Jovanović: PostgreSQL row-level security with EF Core and Npgsql](https://www.milanjovanovic.tech/blog/postgres-row-level-security-with-ef-core))
- **Set the tenant when the connection opens, not when the request starts.** EF Core opens a connection for each command and closes it afterward, and Npgsql resets pooled session state. So a `SET` at the start of a request runs on a connection that the next query never uses. Disabling the reset is worse: the pooled connection then arrives with the previous request's tenant. Override `ConnectionOpened` in a `DbConnectionInterceptor`. It runs on whichever physical connection the pool hands out, which is the only place where the setting is reliably in scope. ([Jovanović: PostgreSQL row-level security with EF Core and Npgsql](https://www.milanjovanovic.tech/blog/postgres-row-level-security-with-ef-core))
- **Insert with batched `SaveChanges` by default, and switch to bulk copy only above about 10,000 rows.** `AddRange` and one `SaveChanges` call keep interceptors and audit trails working, and they're fast enough for ordinary writes. Don't add entities one at a time in a loop: it's an order of magnitude slower than the batched call, for no benefit. ([Mukesh: Fastest way to bulk insert thousands of rows in EF Core](https://codewithmukesh.com/blog/ef-core-bulk-insert/))

## Source redundancy

`code-with-mukesh`, marked **Corroborate.**, and `milan-jovanovic` were both admitted on 2026-08-14 under the lowered longevity bars. The benchmark numbers behind the first opinion are theirs, and this repository hasn't reproduced them. Both sources support the general guidance: use set-based writes for set-based work, and accept that you lose change tracker behaviour. The specific ratios aren't this repository's claim. Check them against the EF Core documentation on Microsoft Learn before you quote them.

`shay-rojansky` has a recorded independence limit: he maintains Npgsql and works on Microsoft's EF Core team. So the mapping rules above cite him for how the mapping works, which is what his notes allow. The judgement on when to use those types comes from `jon-skeet` in [datetime.md](datetime.md), not from him.

<!-- TODO: extend with query-side opinions (AsNoTracking defaults, split queries, projection over Include), migrations workflow, and connection resiliency. None of it is sourced yet. -->
