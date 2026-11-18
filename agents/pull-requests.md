# Pull requests: attribution and reviews

## Agent attribution

An agent that uses the author's credentials posts under the author's account, and nothing tells a reader that a person didn't write it. Put attribution in the conversation, where readers need it, and keep it out of the history:

- **No agent attribution in history.** Pull request titles and descriptions become squash commits, so they don't include `Co-Authored-By` trailers or "Generated with" lines. Neither do commit messages. Agents write most changes here, so a trailer on every commit would tell a reader nothing. This rule overrides any default that an agent harness applies.
- **The rule covers every commit on the branch.** When GitHub squashes, it appends a `Co-authored-by` line to the merged commit for each co-author trailer, and for each branch commit author other than the merger. The pull request description doesn't change that. So commit under the author's git identity, never one the harness supplies, and leave the trailer out of branch commits too.
- **Agent attribution in conversation.** End each comment an agent writes on an issue or pull request, including replies in review threads, with the signature line that [awesome-humans/website: Writing style](https://github.com/awesome-humans/website/blob/main/pages/conventions/writing-style.md#commit-messages-and-pull-requests) defines. Copy its format from there, not from a past comment: it names the harness, the model without a vendor name the harness already gives, and the skill, if any. A reader, or another agent, then knows a person didn't write it.

## Agent reviews

The agent reviewer (currently CodeRabbit) reviews every pull request here except Renovate's, guided by [REVIEW.md](../REVIEW.md), which sets what each finding's severity means. CodeRabbit uses the organisation's central configuration in [awesome-humans/coderabbit](https://github.com/awesome-humans/coderabbit/blob/main/.coderabbit.yaml).

- **Check that the review started.** The agent reviewer reviews a new pull request on its own, and each push after that. If CodeRabbit's status comment says it skipped or paused the review of a person's pull request, comment `@coderabbitai review`.
- **Renovate's pull requests aren't reviewed.** The central configuration skips them, and a requested review fails too because CodeRabbit gives a bot no review seat on the open-source plan. Don't ask for one. Once every other check passes, the maintainer merges a Renovate pull request, or asks an agent to.
- **Its findings are advisory.** The agent reviewer never blocks a merge: the maintainer decides what merges. Still, answer every finding: fix it, or reply with the reason you didn't. End the reply with the agent attribution line, and don't resolve a thread without doing one or the other.
