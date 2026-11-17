# Maintaining: index, commits, pull requests, diagrams

- **List every new file under `opinions/` in `README.md`, in two places:**
  - A bullet under [Scope](../README.md#scope), in the form `- **[Title](opinions/<file>.md):** one line on what it covers`. Sort the bullets by title, ignoring case, and keep the unlinked **Libraries** bullet last.
  - An entry in the [Repository layout](../README.md#repository-layout) tree, sorted by file name.

  The two orders differ: `architecture.md` is titled "Application architecture". When you rename or remove a file, update both places in the same commit. The same goes for a skill directory and its row in the [Maintenance via skills](../README.md#maintenance-via-skills) table, which is sorted by skill name. `scripts/validate-readme-index.cs` fails the pull request if an entry is missing, out of order, or points at a file that doesn't exist.

- **Follow [Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/) for commit messages and pull request titles.** A pull request squash-merges under its title, so the title becomes the commit subject in `git log`. Use the form `<type>[(scope)]: <description>`, in lowercase and the imperative. The scope is optional, as in `chore(deps)` or `docs(review-project)`.

  | Type       | Use for                                                  |
  | ---------- | -------------------------------------------------------- |
  | `docs`     | Opinion, template, and guidance changes                  |
  | `fix`      | Bug fixes in scripts and workflows                       |
  | `feat`     | New features in scripts and workflows                    |
  | `ci`       | Workflow plumbing                                        |
  | `chore`    | Dependencies and housekeeping                            |
  | `research` | Staging or discarding a research topic (repository-only) |

  A skill that sets its own title says so in its `SKILL.md`, as `research-topic` and `resolve-research` do.

- **Fill in the pull request template a skill names.** The templates are the organisation's, in [awesome-humans/.github: `PULL_REQUEST_TEMPLATE/`](https://github.com/awesome-humans/.github/tree/main/PULL_REQUEST_TEMPLATE), and each skill's "Open a pull request" step names its own. Fill in every section, and pass the result to `gh pr create --body-file`. If a section doesn't apply, write "Doesn't apply" and the reason, instead of deleting it. The description becomes the commit message, so follow [Writing style: Commit messages and pull requests](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md#commit-messages-and-pull-requests). Delete the template's guidance comment, and leave out tables, checkboxes, and inline links. End with a `Closes: #<number>` or `Refs: #<number>` trailer for each related issue, written as `awesome-humans/<repository>#<number>` for an issue in another repository. A person opening a pull request in the browser gets the default template, which links the others.

- **Update a diagram in the same commit as the skill or lifecycle it shows.** `README.md` has diagrams of the maintenance pipeline and of the research and source-admission lifecycles. They're for orientation only, and the prose here and in the `SKILL.md` files stays canonical. `scripts/export-diagrams.cs` draws them as SVG into `assets/diagrams/`. To change one, edit the data at the top of the diagram's class, rerun the script, and commit both. Never edit an SVG by hand: CI reruns the script and fails on any difference.
