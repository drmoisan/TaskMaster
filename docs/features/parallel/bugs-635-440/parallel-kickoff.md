# Parallel Kickoff: bugs-635-440

Planned by parallel-planner on 2026-08-29T06:30:00Z. All items are prepared: promoted, active
folders created, research complete, spec and user-story written, atomic plans approved, preflight
ALL CLEAR, blast radii declared and V1/V2-clear. Planning state:
artifacts/orchestration/parallel-planner-state.json (run branch: parallel/bugs-635-440-plan).

Both items are `full-bug` work mode, so `spec.md` is present and `user-story.md` is deliberately
absent on each item branch.

Both items sit in cohort 0 because their declared blast radii do not conflict. With
max_concurrency 2 the whole run is a single cohort executed as one concurrency batch.

The planning_commit recorded below is the head of parallel/bugs-635-440-plan at the moment this
artifact was authored, which is the run-manifest commit; the commit that adds this artifact is its
child. Each plan-hash is the git blob object id of that plan on its own pushed item branch, so it
can be re-derived without checking the branch out.

## Invocation Prompt

Run `/parallel-run bugs-635-440` to execute this run, or paste the prompt below.

Use the parallel-orchestrator subagent to execute the prepared run whose manifest is
docs/features/parallel/bugs-635-440/parallel.md on the plan-home branch parallel/bugs-635-440-plan.
Each item resumes at atomic execution from its committed plan-path on its own pushed feature branch
rather than re-planning, and each item opens its own pull request against main.

## Item Summary

| issue_num | feature_folder | cohort | complexity | branch | plan-path |
| --- | --- | --- | --- | --- | --- |
| 440 | docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440 | 0 | C3 | bug/breadcrumb-left-right-arrow-parent-child-navigation-440 | docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440/plan.2026-08-29T00-22.md |
| 635 | docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635 | 0 | C3 | bug/issue-468-residual-reflective-caller-risk-635 | docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/plan.2026-08-29T00-23.md |

## Integrity

planning_commit: e1d010f1660492307b683dc2531e6e4beab12f34

| plan-path | plan-hash |
| --- | --- |
| docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440/plan.2026-08-29T00-22.md | 7092e7c34f93c5c20594068066a8436de8580851 |
| docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/plan.2026-08-29T00-23.md | 17eb80a8c794ef5a63960ea9d69bc8a44d3e4472 |

## Before Executing

1. Re-check the three branches named in the manifest section "Contention this run cannot see".
   They hold in-flight work on adjacent QuickFiler and coverage surfaces that cohort scheduling
   cannot observe.
2. Expect a fresh-worktree msbuild rebuild to fail with CS0006 until the stale analyzer-include
   paths are reconciled with packages.config. The item 440 plan carries a bootstrap workaround.
3. The second pull request to merge may need a trivial append-order conflict resolution in
   .claude/agent-memory/atomic-planner/MEMORY.md and .claude/agent-memory/task-researcher/MEMORY.md.
