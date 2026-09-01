---
name: defer-dirty-worktree-cleanup-never-force
description: A merged item's worktree is often dirty with unrelated build repair — defer cleanup and record why, never force it; and an out-of-band continuation worktree is structurally unremovable
metadata:
  type: feedback
---

When an item reaches `merged`, do not assume its worktree cleanup is a formality. Check
`git status --porcelain` in that worktree first. If it is dirty, leave it, keep `merge_status`
at `merged`, and record the assessment on the item — never reach for `-f`.

**Why:** On run bugs-638-644-647, item 637's tracked worktree carried 17 uncommitted
modifications spanning every `.csproj` plus `.claude/state/powershell-batch-budget.default.json`.
None of it was the item's delivery — the branch was already an ancestor of `origin/main`. It was a
local repair of stale analyzer include paths after a Dependabot bump (Meziantou.Analyzer
3.0.156 to 3.0.194, Roslynator.Analyzers 4.16.0 to 5.0.0) plus a UTF-8 BOM strip on each
`.csproj`, i.e. the open issue #597 surface. Discarding another context's uncommitted build repair
is not the run's decision to make, and a plain removal already refuses a dirty tree, so the
fail-safe is doing its job — do not defeat it. This is the third distinct reason a cleanup can be
blocked, after the untracked-worktree case in [[free-item-branches-by-detaching]] and the locked
case in [[issue-merge-and-removal-commands-bare]].

**A merged item's DELIVERY worktree may not be the one `items[]` tracks, and that one is
structurally unremovable.** Item 637 was delivered from a continuation worktree/branch pair created
against the same agent id (`agent-af95f0a8159ff28fa-wt/2026-08-31T08-39`), while `items[].worktree_path`
still named the original (`agent-af95f0a8159ff28fa`, parked at its pre-delivery tip). The
continuation path appears in no `items[]` record, so both removal gates fail closed on it exactly as
they do for a planner worktree. There is no in-band way to clean it up; report it rather than
hunting for one. Note the paths are SIBLINGS, not nested — the `-wt` suffix makes a different
directory — so the parent's removability is independent of it.

**How to apply:**

- Assess, then record the deferral in a durable field (an item-level `cleanup_note`) and in
  `parallel-status.md`, so the next session sees a decision rather than an omission. "Not performed"
  and "deliberately deferred, for this reason" read very differently on resume.
- Cleanup is not on the critical path in `open` mode: there is no auto-completion gate, so a
  deferred removal blocks nothing. Do not trade a destructive action for tidiness.
- The per-edge cohort barrier is satisfied by `merged` alone, so a deferred removal never holds back
  a conflicting later-cohort neighbour either.

See [[parallel-run-execution-playbook]].
