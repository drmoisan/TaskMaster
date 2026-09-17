# Parallel Run Status: bugs-2026-09-17

Generated projection of `artifacts/orchestration/parallel-orchestrator-state.json`. Regenerated in full at every documented boundary. Never hand-authored and never an input; the run manifest and the checkpoint are authoritative.

## Run

| field | value |
| --- | --- |
| `parallel_slug` | bugs-2026-09-17 |
| `mode` | closed |
| `max_concurrency` | 2 |
| `current_cohort` | 0 |
| `recolor_generation` | 0 |
| `last_updated` | 2026-09-17T01-07 |
| `next_step` | ITEM_895_IN_FLIGHT_AWAITING_CHILD_DONE |

Effective concurrency for this run is 1 by operator directive, not 2. Both items
carry command tasks and both run msbuild and vstest, so they are serialized. See the
`execution_policy_note` key of the checkpoint for the two independent grounds.

## Items

| issue_num | feature_folder | cohort | state | merge_status | pr_url | merge_commit_sha |
| --- | --- | --- | --- | --- | --- | --- |
| 895 | `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895` | 0 | in_flight | worktree_created | - | - |
| 900 | `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900` | 0 | scheduled | not_started | - | - |

### Item lifecycle timestamps

| issue_num | worktree_created_at | pr_opened_at | ci_green_at | merged_at | worktree_removed_at |
| --- | --- | --- | --- | --- | --- |
| 895 | 2026-09-17T01-07 | - | - | - | - |
| 900 | - | - | - | - | - |

### Item branches and worktrees

| issue_num | branch_name | worktree_path | complexity_band |
| --- | --- | --- | --- |
| 895 | `bug/fsharp-core-hintpath-netstandard21-skew-895` | `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a8bc4dc5978785885` | C3 |
| 900 | `bug/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900` | `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-acb02d4502ebff3b7` | C3 |

## Cohorts

| index | generation | item_keys |
| --- | --- | --- |
| 0 | 0 | 895, 900 |

## Conflict Edges

No conflict edges. The two declared radii intersect only on `QuickFiler.Test/QuickFiler.Test.csproj`, a member of the mechanically-mergeable path class, so no `path_overlap` edge arises.

## Mutations

None recorded.

## Drift Events

None recorded.

## Mergeable Conflicts Resolved

None recorded.
