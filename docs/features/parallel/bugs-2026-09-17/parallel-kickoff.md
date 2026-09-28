# Parallel Kickoff: bugs-2026-09-17

Planned by parallel-planner on 2026-09-17T00:00:00Z. All items are prepared: promoted, active folders created,
research complete, spec and user-story written, atomic plans approved, preflight ALL CLEAR, blast
radii declared and V1/V2-clear. Planning state:
artifacts/orchestration/parallel-planner-state.json (run branch: parallel/bugs-2026-09-17-plan).

Base: origin/main at 91746d2e4776a59ee1db1856c5c490a009c4958b. Mode closed, max_concurrency 2. One
cohort at generation 0 holding both items; conflict_edges is empty. The two declared radii intersect
only on QuickFiler.Test/QuickFiler.Test.csproj, which the mergeable-path class in
config/blast-radius.json declines to score, so no path_overlap edge arises. Issue 899 was named at
intake but was not admitted; the manifest records the reason.

## Invocation Prompt

Run `/parallel-run bugs-2026-09-17` to execute this run, or paste the prompt below.

Use the parallel-orchestrator subagent to execute the prepared run whose manifest is
docs/features/parallel/bugs-2026-09-17/parallel.md on the plan-home branch parallel/bugs-2026-09-17-plan. Each item
resumes at atomic execution from its committed plan-path on its own pushed feature branch rather
than re-planning, and each item opens its own pull request against main.

## Item Summary

| issue_num | feature_folder | cohort | complexity | branch | plan-path |
| --- | --- | --- | --- | --- | --- |
| 895 | docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895 | 0 | C3 | bug/fsharp-core-hintpath-netstandard21-skew-895 | docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/plan.2026-09-16T23-27.md |
| 900 | docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900 | 0 | C3 | bug/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900 | docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/plan.2026-09-16T23-27.md |

## Integrity

planning_commit: fd1b799c66b5dc017922207313b9c5bcb527ba1f

| plan-path | plan-hash |
| --- | --- |
| docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/plan.2026-09-16T23-27.md | 50623e45d0f3d20975db351531a086b96cb879d8 |
| docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/plan.2026-09-16T23-27.md | 7b4d9d4704f65c5bf61149907dcd2498413736ef |

## Execution Notes

Item branch commits at planning time: 895 at f1c53f9b88fddb4c69920ff37b3acb13718f7983, 900 at
88dd92169aab3f2cc3bdc808e456fe6d4bc169c4. Both were created from origin/main at
91746d2e4776a59ee1db1856c5c490a009c4958b and pushed.

Both plans carry command tasks. A worktree-isolated execution session may have pwsh refused by the
Bash-tool isolation filter, in which case item 900's plan stops at task P0-T4 with
CHANNEL UNAVAILABLE. Both plans carry a two-rung channel probe rather than hard-coding an outcome.
Expect to run item children without worktree isolation, reusing each item's existing preparation
worktree and addressing it with git -C.

Each item's execution session needs artifacts/orchestration/orchestrator-state.json seeded with
issue-num, a docs/features/active feature-folder, route_id and path_selected, and lifecycle_ready
true, or item 900's plan stops at task P0-T3 with PRE-IMPLEMENTATION GATE NOT SEEDED.

Item 900's declared radius is materially over-broad and includes .claude/** plus the shared surface
.claude/settings.json, none of which its plan writes. The over-report was measured and does not
change this run's only pairwise verdict, but drift detection fails open against it, and the run
should not be reopened for /parallel-add until that radius is tightened.
