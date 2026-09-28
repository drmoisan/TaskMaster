---
name: midplan-commit-breaks-deletion-staging-and-porcelain-spans
description: A plan that commits at the end of Phase 1 makes its Phase 2 `git add -A -- <deleted path>` exit 128 and its porcelain spans print nothing; both read as failures but are not
metadata:
  type: project
---

When a plan commits mid-run (e.g. a Phase 1 commit) and its Phase 2 verification spans were authored
assuming an uncommitted worktree, two spans change behaviour and both look like failures:

- `git add -A -- <path-that-was-deleted-and-committed>` exits **128** with
  `fatal: pathspec '<path>' did not match any files`. `git add` errors when a pathspec matches nothing
  in the worktree AND nothing differing in the index. The deletion is already in HEAD, so there is
  nothing left to stage. This is not a deletion that failed to be captured.
- `git status --porcelain -- <committed path>` prints **nothing**, because porcelain compares worktree
  against index and both already match HEAD.

**Why:** Observed on issue #872 (2026-09-13). The plan's P2-T11 paired a porcelain span with an
anchored `git diff --name-status` precisely because "porcelain goes empty once the change is committed
and the anchored diff does not" — the plan anticipated the porcelain case but not the `git add -A`
case, which aborted the P2-T32 staging sequence partway.

**How to apply:** Do not treat either as a blocker and do not restructure the commit. Continue the
remaining staging spans and the commit; the anchored diff against the base commit is the span that
still carries the evidence. Record the exit-128 span verbatim in the terminal evidence artifact with
its cause, rather than suppressing it — a reviewer who sees a 128 with no explanation will read it as
a lost deletion. At preflight, flag any Phase 2 `git add -A` over paths a earlier phase already
committed. Related: [[project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate]],
[[project_preflight_mergebase_diff_gates_need_commit_cadence]].
