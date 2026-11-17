# Writing style for opinions

- **Put the opinion first.** State the recommendation in the first sentence. The rationale follows, and the sources come last.
- **Make it precise enough to apply mechanically.** An agent should be able to turn an opinion into an edit or a `review-project` finding without interpreting it. A rule that names no setting, type, or threshold isn't finished.
- **Show code when it's clearer than prose.** Keep examples minimal and idiomatic for the declared `targets`. When an opinion replaces an old idiom, show a before-and-after pair. The same applies to `templates/`: example code files are welcome alongside configuration.
- **Make code samples compile against the declared `targets`.**
- **Use a table when the same attributes repeat across items.** Options, versions, or frameworks compared on the same few points read faster as rows than as parallel paragraphs, as in [datetime.md](../opinions/datetime.md). Keep the reasoning in sentences: a cell holds a fact, not the argument for it.
- **Group sections by topic, and add a new one to its group.** Don't sort them alphabetically or append them to the end of the page. In `templates/`, sort `PackageVersion` and `PackageReference` entries by name, ignoring case, within their `ItemGroup`, and `just validate-template-order` checks it. For the rule, see [awesome-humans/website: Writing style: Order](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md#order).
- **Keep one topic per file under `opinions/`.** Keep each file under about 300 lines, and split it when it grows past that.
- **Give transition guidance an expiry date,** such as for a migration off a retired tool, with the `Review by` marking that [awesome-humans/website: Writing style: Content](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md#content) defines. `audit-freshness` reports a marking whose date has passed.
