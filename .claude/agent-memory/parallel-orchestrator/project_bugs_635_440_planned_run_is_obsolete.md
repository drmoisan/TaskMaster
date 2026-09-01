---
name: bugs-635-440-planned-run-is-obsolete
description: The parallel run bugs-635-440 was fully planned but never executed — both items (#635, #440) were later delivered by standalone PRs #688/#689 and both issues are CLOSED, so its kickoff artifact and plan branch are stale
metadata:
  type: project
---

`artifacts/orchestration/parallel-kickoff-bugs-635-440.md`, the branch
`parallel/bugs-635-440-plan`, its worktree `TaskMaster-wt/parallel-bugs-635-440-plan`, and the
manifest `docs/features/parallel/bugs-635-440/parallel.md` describe a run that never ran. Both of
its two items shipped independently instead:

- #635 — PR #688 `fix(635): settle the residual reflective-caller risk...`, merged
  2026-08-29T11:10:08Z, merge commit `5781a571`. Issue CLOSED/COMPLETED. Feature folder now under
  `docs/features/completed/` on main.
- #440 — PR #689, merge commit `ecdb1c84`. Issue CLOSED/COMPLETED.

Both item branches are ancestors of `origin/main` and their three-dot diffs against main are empty.
The planner state file `artifacts/orchestration/parallel-planner-state.json` no longer describes
this run at all; it was overwritten by the later `bugs-638-644-647` run.

**Why:** The kickoff artifact reads as a live, ready-to-execute run — "All items are prepared:
... preflight ALL CLEAR" — and its item table names real branches with real committed plans. That
is exactly the shape of a run awaiting `/parallel-run`, so it invites re-execution or re-admission
of its items. Confirmed 2026-08-31 when `/parallel-add 635` was rejected on those grounds.

**How to apply:** Do not admit #635 or #440 into any parallel run, and do not execute
`/parallel-run bugs-635-440`. More generally, treat a kickoff artifact as a claim about the past,
not evidence that its items are outstanding — the prepared item branch it names is precisely the
branch a standalone PR is most likely to have shipped from. Run the delivery pre-check in
[[verify-delivery-before-preparing-an-admission]] against the item, not the artifact. The stale
plan branch and worktree are candidates for cleanup, but nothing in the running
`bugs-638-644-647` run depends on them.
