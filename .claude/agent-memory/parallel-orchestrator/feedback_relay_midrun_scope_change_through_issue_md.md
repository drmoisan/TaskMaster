---
name: relay-midrun-scope-change-through-issue-md
description: With no SendMessage tool, a scope change for a running preparation child is relayed by committing it into the child's issue.md (and a superseded-scope banner on its research); the child adopted it on its next step
metadata:
  type: feedback
---

When the coordinator widens an item's scope while its preparation child is running, and no
SendMessage tool is available to this persona, write the change into the child's own
`docs/features/active/<folder>/issue.md` in its worktree and commit it with a pathspec. If the
research already landed with the old scope, add a short "SCOPE SUPERSEDED" banner under its scope
line too. Push.

**Why:** Observed 2026-10-02 on `/parallel-add 959` (maintainer folded #966 into #959 mid-preparation).
The child's spec author read issue.md as the requirements source, treated it as the later
instruction over its brief, ran a second research pass and rewrote the spec. No restart was needed.
The child attributed the commits to "another session", so say "parent parallel-orchestrator edit" in
the commit message body.

**How to apply:** Edit only `.md` files under the item's feature folder (Edit is allowed there).
Put the binding scope rule in issue.md so subagents see it without relay. Then poll; after the
child returns, grep the spec for the added items and for "out of scope"/"follow-up" wording to
confirm the widening landed. See [[resume-a-dead-preparation-child-dont-restart-it]].
