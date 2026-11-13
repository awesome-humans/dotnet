---
targets: [net10.0, csharp-14]
last-reviewed: 2026-11-13
sources: [code-with-mukesh, milan-jovanovic, ms-learn, shay-rojansky]
---

# Data access

Use EF Core as the default ORM. Send a change to many rows as one statement, and load entities only for the writes that need the change tracker.

<!-- Seeded 2026-08-14 by harvest-sources from newly admitted sources; see AWESOME-HUMANS.md decision log -->

## Writes

- **Use `ExecuteUpdateAsync` and `ExecuteDeleteAsync` for updates and deletes that don't need the entities loaded.** Loading the entities, changing them, and calling `SaveChanges` pulls every row into the change tracker, then sends a statement for each one. The set-based APIs issue one `UPDATE` or `DELETE`. ([Mukesh: Bulk operations in EF Core 10](https://codewithmukesh.com/blog/bulk-operations-efcore/), [Jovanović: What you need to know about EF Core bulk updates](https://milanjovanovic.tech/blog/what-you-need-to-know-about-ef-core-bulk-updates))

  ```csharp
  // Set-based: one statement, no entities materialized
  await db.Orders
      .Where(o => o.Status == OrderStatus.Pending && o.Placed < cutoff)
      .ExecuteUpdateAsync(
          s => s.SetProperty(o => o.Status, OrderStatus.Expired),
          cancellationToken);
  ```

- **Keep audited writes on `SaveChanges`.** The set-based APIs bypass the change tracker. `SaveChanges` interceptors don't fire, and audit and outbox logic in `SaveChanges` doesn't run. A `DbCommandInterceptor` still sees each statement. The set-based APIs also can't apply concurrency control automatically, because no tracked entity holds the original concurrency token. To check it yourself, put the token in the `Where` clause, and treat zero affected rows as a conflict. ([Mukesh: Bulk operations in EF Core 10](https://codewithmukesh.com/blog/bulk-operations-efcore/), [Jovanović: EF Core bulk updates](https://milanjovanovic.tech/blog/what-you-need-to-know-about-ef-core-bulk-updates), [Microsoft Learn: ExecuteUpdate and ExecuteDelete](https://learn.microsoft.com/ef/core/saving/execute-insert-update-delete), [dotnet/efcore: RelationalShapedQueryCompilingExpressionVisitor.cs](https://github.com/dotnet/efcore/blob/38b28f589137695449b65df598ba4119eedfe20b/src/EFCore.Relational/Query/RelationalShapedQueryCompilingExpressionVisitor.cs#L148), [dotnet/efcore: RelationalCommand.cs](https://github.com/dotnet/efcore/blob/38b28f589137695449b65df598ba4119eedfe20b/src/EFCore.Relational/Storage/RelationalCommand.cs#L97))
- **Insert with `AddRange` and one `SaveChanges` call, and switch to bulk copy only above about 10,000 rows.** The batched call keeps interceptors and audit trails working, and it's fast enough for ordinary writes: Jovanović measured about 200 ms for 10,000 rows, against about 70 ms for `SqlBulkCopy`. Bulk copy skips `SaveChanges` too, so audited inserts stay on the batched call, however many rows there are. Never add and save entities one at a time in a loop: the same 10,000 rows took him almost 9 seconds. ([Mukesh: Fastest way to bulk insert thousands of rows in EF Core](https://codewithmukesh.com/blog/ef-core-bulk-insert/), [Jovanović: Fast SQL bulk inserts with C# and EF Core](https://www.milanjovanovic.tech/blog/fast-sql-bulk-inserts-with-csharp-and-ef-core))

## Dates and times

- **Model dates and times with the types in [datetime.md](datetime.md), and let EF Core map them.** On EF Core 8 and later, `DateOnly` and `TimeOnly` map to SQL `date` and `time` columns. Don't store a date in a `DateTime`. For provider differences, and for UTC versus local storage, see [datetime.md](datetime.md#persistence-split-by-kind-of-data-not-by-layer).
- **On PostgreSQL, store instants as `DateTime` with `Kind.Utc`, which Npgsql maps to `timestamptz`.** Despite its name, `timestamptz` stores a UTC instant and no time zone, so a `DateTimeOffset`'s offset has nowhere to go. Npgsql:
  - Rejects a `DateTimeOffset` with a non-zero offset.
  - Maps `Kind.Utc` to `timestamptz`.
  - Maps `Local` and `Unspecified` to `timestamp`.

  This "UTC everywhere" rule describes what the column can store without loss. It doesn't contradict the split in [datetime.md](datetime.md#persistence-split-by-kind-of-data-not-by-layer). The instant column is UTC either way, and a future or recurring event that a person schedules still needs its local time and IANA zone ID in columns beside it. ([Rojansky: PostgreSQL/.NET timestamp mapping](https://www.roji.org/postgresql-dotnet-timestamp-mapping))

## Indexes

- **Choose indexes from the queries the application runs, and prove each one with `EXPLAIN ANALYZE`.** In a B-tree index, put equality columns first and the range or sort column last. Compare the plan before and after: it's the only evidence that the database uses the index. In Jovanović's demo with a million comments, a count drops from a 17 ms sequential scan to a 0.6 ms index-only scan, and a query for the newest open issues, each with its latest comment, drops from 436 ms to about 0.5 ms. Reproduce numbers like these on your own data. ([Jovanović: SQL indexing explained: composite indexes and column order](https://www.milanjovanovic.tech/blog/how-to-design-the-right-sql-index))

## Raw SQL

- **Write raw SQL with the interpolated methods, and never pass input to a `*Raw` method.** "The FromSql and FromSqlInterpolated methods are safe against SQL injection, and always integrate parameter data as a separate SQL parameter. However, the FromSqlRaw method can be vulnerable to SQL injection attacks, if improperly used". The same split applies to `ExecuteSql` and `ExecuteSqlRaw`, and to `SqlQuery` and `SqlQueryRaw`. ([Microsoft Learn: SQL Queries](https://learn.microsoft.com/ef/core/querying/sql-queries)) Use a `*Raw` method only for SQL your own code builds, such as a table name from a fixed list, and never for a value from a request.

  ```csharp
  // Interpolated: the value becomes a parameter, not part of the statement.
  var orders = await db.Orders
      .FromSql($"SELECT * FROM Orders WHERE Status = {status}")
      .ToListAsync(cancellationToken);
  ```

## Tenant isolation

- **Enforce tenant isolation in the database with row-level security, under your global query filter.** EF Core adds the filter only to the SQL it generates. `ExecuteSql`, an attached entity you save, and anything behind `IgnoreQueryFilters()` skip it. A row-level security policy sits in the database, so it still holds when someone forgets the filter: "The database system applies the access restrictions every time that data access is attempted from any tier". ([Jovanović: PostgreSQL row-level security with EF Core and Npgsql](https://www.milanjovanovic.tech/blog/postgres-row-level-security-with-ef-core), [Microsoft Learn: Row-level security](https://learn.microsoft.com/sql/relational-databases/security/row-level-security))
- **On PostgreSQL, make the policy apply to every role, and match no rows when no tenant is set:**
  - Connect the application as a role that doesn't own the tables, isn't a superuser, and doesn't have `BYPASSRLS`, because a role with any of those skips policies. Run migrations as a separate role that owns the tables.
  - Set `FORCE ROW LEVEL SECURITY` on each tenant table as well as `ENABLE ROW LEVEL SECURITY`. The policy then covers the owner too, so a migration script can't change every tenant's rows by accident.
  - Compare the tenant column with `NULLIF(current_setting('app.tenant_id', true), '')` in the policy. A missing or empty setting then becomes `NULL`, which matches no rows.
  - Insert into a tenant table in batches with `AddRange`, however many rows there are. Bulk copy fails there: "`COPY FROM` is not supported for tables with row-level security".

  ([Jovanović: PostgreSQL row-level security with EF Core and Npgsql](https://www.milanjovanovic.tech/blog/postgres-row-level-security-with-ef-core), [PostgreSQL: COPY](https://www.postgresql.org/docs/current/sql-copy.html))

- **On SQL Server, add a block predicate to the security policy beside the filter predicate.** A filter predicate hides other tenants' rows from `SELECT`, `UPDATE`, and `DELETE`, but "The application can `INSERT` rows, even if they'll be filtered during any other operation".
  - Add an `AFTER INSERT` block predicate that uses the same function.
  - Add an `AFTER UPDATE` block predicate too, or deny `UPDATE` on the tenant column.
  - Read the tenant from `SESSION_CONTEXT` in the predicate function.

  The policy needs no equivalent of `FORCE`: it already applies to `dbo`, to members of `db_owner`, and to the table owner. ([Microsoft Learn: Row-level security](https://learn.microsoft.com/sql/relational-databases/security/row-level-security))

  **Review by 2027-05-31.** On SQL Server 2019 CU 14 and later, enable trace flag 11042, which makes `SESSION_CONTEXT` run serially. Without it, "You might encounter AV exceptions or wrong results when the `SESSION_CONTEXT` function runs within a parallel execution plan when the session is reset for reuse", and a pooled connection is reset each time it's reused. Microsoft lists the issue as open, with this workaround. Drop this note once Microsoft resolves it. ([Microsoft Learn: SESSION_CONTEXT (Transact-SQL)](https://learn.microsoft.com/sql/t-sql/functions/session-context-transact-sql#known-issues))

- **Set the tenant when the connection opens, not when the request starts.** EF Core opens and closes a connection for each command, and Npgsql resets pooled session state. A `SET` at the start of a request runs on a connection the next query doesn't use. Disabling the reset is worse: the next request then gets the previous tenant. Override both `ConnectionOpened` and `ConnectionOpenedAsync` in a `DbConnectionInterceptor`, which runs on whichever physical connection the pool hands out. EF Core calls the async method after `OpenAsync`, so an interceptor with only the sync one leaves every async query with no tenant set. EF Core opens only a connection that isn't already open, so one that the application opens and passes to the `DbContext` skips both callbacks: set its tenant yourself, and never reuse it for another tenant. On PostgreSQL, this needs a pool that keeps the session, such as Npgsql's own or PgBouncer in session mode. PgBouncer's transaction mode never supports `SET`, and each transaction can run on a different server connection, so a query can get no tenant or another client's. Behind it, set the tenant with `set_config(..., true)` inside an explicit transaction that holds every query, which a connection interceptor can't do. On SQL Server, set it with `sp_set_session_context` and `@read_only = 1`, so nothing can change the tenant until the connection returns to the pool. ([Jovanović: PostgreSQL row-level security with EF Core and Npgsql](https://www.milanjovanovic.tech/blog/postgres-row-level-security-with-ef-core), [Microsoft Learn: Interceptors](https://learn.microsoft.com/ef/core/logging-events-diagnostics/interceptors), [Microsoft Learn: Row-level security](https://learn.microsoft.com/sql/relational-databases/security/row-level-security), [dotnet/efcore: RelationalConnection.cs](https://github.com/dotnet/efcore/blob/38b28f589137695449b65df598ba4119eedfe20b/src/EFCore.Relational/Storage/RelationalConnection.cs#L621), [PgBouncer: Features](https://www.pgbouncer.org/features.html))

## Source redundancy

The benchmark numbers under [Writes](#writes) come from `milan-jovanovic`, and this repository hasn't reproduced them. `code-with-mukesh`, marked **Corroborate.**, also reports speed-ups, which this file leaves out because no unmarked source reports them. Both support the general rule: use set-based writes for set-based work, and accept losing change tracker behaviour. Before you quote a number, check it against the EF Core documentation on Microsoft Learn.

`shay-rojansky` has a recorded independence limit: he maintains Npgsql and works on Microsoft's EF Core team. This file cites him only for how Npgsql maps timestamps, under [Dates and times](#dates-and-times), as his notes allow. When to use each type comes from `jon-skeet`, in [datetime.md](datetime.md).

<!-- TODO: extend with query-side opinions (AsNoTracking defaults, split queries, projection over Include), migrations workflow, and connection resiliency. None of it is sourced yet. -->
