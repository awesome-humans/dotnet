# Metadata

Three kinds of resource record this metadata, in the two syntaxes their file formats allow:

| Directory    | `targets` | `last-reviewed` | `sources` | Syntax             |
| ------------ | --------- | --------------- | --------- | ------------------ |
| `opinions/`  | Yes       | Yes             | Yes       | YAML frontmatter   |
| `templates/` | Yes       | Yes             | Yes       | First-line comment |
| `research/`  | Yes       | Yes             | Yes       | YAML frontmatter   |

**`opinions/` and `research/` use YAML frontmatter:**

```yaml
---
targets: [net11.0, csharp-15, fsharp-11]
last-reviewed: 2026-08-12 # last time content was verified against sources
sources: [andrew-lock, dotnet-blog] # ids from AWESOME-HUMANS.md, sorted
---
```

A `research/` topic has no status field. Promoting or discarding a topic deletes it, so a topic on disk is unresolved by definition, and a status could only ever read `open`. For details, see [research-topic](../skills/research-topic/SKILL.md) and [resolve-research](../skills/resolve-research/SKILL.md).

**`templates/` use a first-line comment header.** An XML, INI, or source file can't open with a `---` block and stay valid for the tools that read it. So the same fields go in a comment instead, separated by pipes after a fixed marker:

```xml
<!-- awesome-humans/dotnet template | targets: net11.0 | last-reviewed: 2026-08-12 | sources: ms-learn -->
```

```ini
# awesome-humans/dotnet template | targets: net11.0 | last-reviewed: 2026-08-12 | sources: ms-learn
```

```fsharp
// awesome-humans/dotnet template | targets: net11.0, fsharp-11 | last-reviewed: 2026-08-12 | sources: scott-wlaschin
```

Rules:

- **Don't record when a resource was used.** A `last-used` field is rejected. Writing a date on every read left uncommitted edits in every checkout an agent read from, and they conflicted with real changes to the same line. To find pruning candidates, `audit-freshness` counts what refers to each opinion and template instead.
- **Update `last-reviewed` only after you verify the content against its sources.**
- **Use `sources` IDs that exist in `AWESOME-HUMANS.md`,** either in the roster tables or as a house profile's reserved `house:<profile>` ID.
  - CI reads the `**Discovery-only.**` and `**Corroborate.**` markings from a source's section under `## Source notes`, not from its table row.
  - In `opinions/` and `templates/`, the source must also be citable. A watch-list source or a `**Discovery-only.**` source is rejected.
  - A file whose citable sources are all marked `**Corroborate.**` is rejected too, because an unmarked source has to stand beside them.
  - `research/` is exempt: a topic can cite unvetted material if the text flags it. For details, see [research-topic](../skills/research-topic/SKILL.md).
- **Sort `sources` by ID in ordinal order, with `house:<profile>` sorted like any other ID.** Write `[andrew-lock, dotnet-blog, house:simon-mckenna, ms-learn]`. The rule applies in all three directories and to both syntaxes.
- **Write dates in ISO 8601 (`YYYY-MM-DD`), in UTC.** Always use absolute dates, never relative ones. To get today's date, run `date -u +%F`. CI, GitHub, and the site build run in UTC, as [awesome-humans/website: Writing style](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md) describes.
- **Two template files are exempt from the header:** `templates/global.json` and `templates/example.slnf`, because JSON has no comment syntax. What they pin is audited against the latest releases instead. Smoke-testing the example projects leaves build output under `templates/` (`artifacts/`, `bin/`, and `obj/`, all gitignored). That output isn't a resource, and the check skips it.
- **`skills/` are deliberately exempt.** The [Agent Skills specification](https://agentskills.io/specification) defines their frontmatter, they're procedures and not reference material, and nothing reads a date from them. Use `git log` to see when a skill last changed.

CI runs `just validate-metadata` against both syntaxes, and a missing field, a relative date, or an unsorted `sources` list fails the pull request.
