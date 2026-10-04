# How it works

Each ecosystem's repository maintains itself through agent skills. Nothing reaches an opinion without passing CI and the owner's review. The diagrams are for orientation only. The skills and the roster's admission criteria, linked from each ecosystem's pages, are canonical.

## Every change is a pull request

Each skill works on its own branch, and its changes land through a pull request. CI checks three things: the metadata on every opinion and template, that every cited source is on the roster and citable, and that every link resolves. The owner then reviews and merges the pull request.

Two skills only report. `audit-freshness` finds review dates that have drifted and version pins that lag. `verify-project` grades an external codebase against the opinions. Their findings, a scheduled release watch, and a monthly harvest reminder start the next run.

![Maintenance pipeline: each skill from what starts it, through CI and the owner's review, to the roster, research/, or the opinions and templates; audit-freshness and verify-project only report](assets/diagrams/maintenance-pipeline.svg)

## Opinions have to be earned

A source joins the roster on its track record: at least two years of sustained writing, with depth, accuracy, and an independent view. A source with less than two years, or that's been dormant for over a year, goes on the watch list. The watch list supplies leads, but is never cited. Two markings limit what a citable source can support. A source is vetted again whenever it goes quiet or a blocker clears.

![Source standing: vet-source sorts a candidate into citable, watch list or declined, and a citable source is unmarked, Corroborate or Discovery-only](assets/diagrams/source-standing.svg)

## Research is staged, never merged in place

A research topic saves the evidence on a subject in its own pull request. A second pull request then promotes it into the opinions and templates, or discards it. Either way, the topic is deleted, so a topic still on disk is unresolved by definition.

![Research lifecycle: research-topic saves research/{topic}.md, and resolve-research promotes, discards or partly promotes it](assets/diagrams/research-lifecycle.svg)

## House opinions

One person outranks the roster: each repository's owner. The owner's preferences are recorded separately, and always marked **House:** where they appear, so you can tell community practice from local convention.
