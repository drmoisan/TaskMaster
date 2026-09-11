---
name: orchestrator-reconciliation-merge-tracks-feature-docs
description: A plan that baselines the feature folder as "?? untracked" is false when the orchestrator's reconcile-against-origin/main merge already committed issue.md/spec.md/research/plan
metadata:
  type: project
---

When an orchestrator hands an executor a worktree it "just reconciled against
`origin/main`", the reconciliation merge commit has usually already **committed the
feature folder documents**. A plan authored before that merge baselines them as a
single untracked-directory line and is then factually wrong at execution time.

**Why:** Observed on #730 (2026-09-02). The plan's `[P0-T5]` acceptance demanded
`git status --porcelain` contain "exactly one line,
`?? docs/features/active/<slug>/`", describing issue.md, spec.md, research/, and the
plan file as "none of which are yet committed". `git ls-files` at the handed-over
HEAD listed all four as tracked, and porcelain instead showed
`M <plan>.md` plus `?? <feature>/evidence/`. The downstream `[P2-T1]` staging-scope
gate inherited the same false premise and became unsatisfiable, because it demanded
the four staged paths "plus the single pre-existing untracked-directory line ...
and no other entries".

Compounding it: the executor's own mandatory plan check-offs make the plan file
show as `M`, so any porcelain gate written as "no other entries" fails on the
executor's required behaviour. Same failure shape as
[[project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate]].

**How to apply:** At preflight, run `git ls-files <feature-folder>` and
`git status --porcelain` against the ACTUAL handed-over worktree before accepting
any task that asserts a tracked/untracked state for the feature folder. Do not
carry the planner's snapshot forward — the planner and executor observe different
worktrees ([[project_planner_and_executor_observe_different_worktrees]]). Prefer
path-scoped porcelain gates (`git status --porcelain -- <specific paths>`) over
whole-tree "and no other entries" gates, which cannot survive the plan file's own
check-offs.
