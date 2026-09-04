# P0-T2 — Branch and commit baseline

Timestamp: 2026-09-03T23-31

Command:

```
git rev-parse --abbrev-ref HEAD
git status --porcelain
git merge-base origin/main HEAD
git diff --name-only origin/main...HEAD -- .claude/agent-memory
```

EXIT_CODE: 0

## Observation 1 — branch, porcelain, merge base

Branch (`git rev-parse --abbrev-ref HEAD`): `bug/efc-archiveroot-boundary-sink-defects-736`

Merge base (`git merge-base origin/main HEAD`), full 40-character SHA:
`66749143601aedb816c679b911f1042ffa3e86a5`

`git status --porcelain` span, enumerated line by line with its status code. The span is not empty;
both lines name paths under this feature folder, which satisfies this task's universal clause. Both
were produced by P0-T1, the immediately preceding task in this plan: the plan file check-off and the
newly created evidence subdirectory.

1. ` M docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/plan.2026-09-02T12-02.md` — status code ` M` (modified, unstaged)
2. `?? docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/` — status code `??` (untracked)

No line names a path outside this feature folder and outside `.claude/agent-memory/`. No drift is
recorded and no blocking condition is reported.

## Observation 2 — agent-memory paths already inside `origin/main...HEAD`

This is the observation P7-T2 and P7-T4 are evaluated against. The paths below were committed to this
branch by the earlier agents of this run (task-researcher, prd-feature, atomic-planner, orchestrator,
atomic-executor) before Phase 0 began, so they sit inside `origin/main...HEAD` and not in the working
tree. None of them is in the ratified eleven-path Write Set. D11's AC11 pathspec excludes
`.claude/**`, so none of them is visible to the scope gate, and they are therefore accounted for here
instead.

Output of `git diff --name-only origin/main...HEAD -- .claude/agent-memory`, enumerated individually:

1. `.claude/agent-memory/atomic-executor/MEMORY.md`
2. `.claude/agent-memory/atomic-executor/project_gitignore_star_log_blocks_committed_msbuild_log_evidence.md`
3. `.claude/agent-memory/atomic-executor/project_processed_cobertura_filenames_use_backslash.md`
4. `.claude/agent-memory/atomic-planner/MEMORY.md`
5. `.claude/agent-memory/atomic-planner/empty-porcelain-clause-is-unsatisfiable.md`
6. `.claude/agent-memory/atomic-planner/existence-is-not-retention-gate-committed-artifacts.md`
7. `.claude/agent-memory/atomic-planner/project_736_efc_archiveroot_boundary_sink_plan_seams.md`
8. `.claude/agent-memory/atomic-planner/trx-carries-host-tokens-in-two-casings.md`
9. `.claude/agent-memory/atomic-planner/worktree-root-breaks-dotclaude-exclusion.md`
10. `.claude/agent-memory/orchestrator/MEMORY.md`
11. `.claude/agent-memory/orchestrator/worktree-isolation-blocks-pwsh-per-agent-type.md`
12. `.claude/agent-memory/prd-feature/MEMORY.md`
13. `.claude/agent-memory/prd-feature/feedback_backticked_paths_are_the_change_footprint.md`
14. `.claude/agent-memory/prd-feature/feedback_invariant_and_trace_in_proposed_fix.md`
15. `.claude/agent-memory/task-researcher/MEMORY.md`
16. `.claude/agent-memory/task-researcher/project_efc736_archiveroot_boundary_sink.md`

Count: 16 paths. Every one of them is outside this item's ratified Write Set, and every one of them
is invisible to the AC11 scope gate because D11's pathspec carries `":(exclude).claude/**"`.

No expectation is placed on the value of HEAD, and none is recorded.

Output Summary: branch is `bug/efc-archiveroot-boundary-sink-defects-736`; merge base is
`66749143601aedb816c679b911f1042ffa3e86a5`; the porcelain span carries two lines, both under this
feature folder and both produced by P0-T1, so no drift is present; 16 `.claude/agent-memory/` paths
are already inside `origin/main...HEAD` and are enumerated above as the set P7-T2 and P7-T4 subtract
against.
