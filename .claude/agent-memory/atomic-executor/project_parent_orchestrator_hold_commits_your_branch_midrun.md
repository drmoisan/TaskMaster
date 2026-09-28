---
name: parent-orchestrator-hold-commits-your-branch-midrun
description: A parallel-run orchestrator can commit YOUR in-progress evidence onto your branch while you are still executing, so your next push reports an unfamiliar parent commit — verify authorship before treating it as a foreign write
metadata:
  type: project
---

The parent orchestrator in a parallel run may commit your uncommitted working-tree files onto your
own branch mid-execution, without telling you. Your next `git push` then reports a range starting at
a SHA you never created.

Observed 2026-09-13, issue #816: after my Phase 3 push landed `96fa0ca3c`, my Phase 4 push printed
`77cf1ab9e..0376e147c`. `77cf1ab9e` was `wip(816): parent-side hold commit of Phase 4 QA gate
evidence`, authored "Dan Moisan" at 23:44:27 — mid-Phase-4 — containing exactly the six evidence
artifacts I had already Written (P4-T1 through P4-T6) plus my own plan check-offs. It committed my
content, added nothing, deleted nothing, and caused no conflict.

**Why this is not automatically benign:** the same mechanism could commit a half-written artifact, or
sweep a path outside your plan's scoped pathspec set. The hold commit in #816 stayed inside the
feature folder, but nothing in the mechanism guarantees that.

**How to apply:**
- When a push range starts at an unexpected SHA, run
  `git log --oneline -8` then `git show --stat --format="%H%n%an%n%ci%n%s" <sha>` BEFORE concluding
  anything. Check three things: the file list (is it your content?), the timestamp (does it fall
  inside your run?), and whether any path lies outside your plan's pathspec set.
- If every file is one you authored, continue without remediation and note it in the final report.
  Do NOT reset, revert, or rebase — that would discard work the parent deliberately preserved.
- If a path OUTSIDE the plan's scoped pathspec set appears, that is a real scope breach: stop and
  report it rather than absorbing it into your own commit.
- A terminal `git status --porcelain` gate still passes normally afterwards, because the hold commit
  only moves your own pending content from the worktree into history.

Related: [[project_midplan_commit_breaks_deletion_staging_and_porcelain_spans]],
[[project_concurrent_executor_same_worktree]]
