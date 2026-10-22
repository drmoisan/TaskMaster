# P4-T8 — Footprint and Scope Verification (Issue #751)

Timestamp: 2026-09-03T14-45

EXIT_CODE: 0 (all three commands)

## Command 1 — production file immutability

Command: `git diff f8414ee9..HEAD -- TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs`

Verbatim output:

```
(no output)
```

Output line count: **0**. The production file is byte-identical to its state at the branch point
`f8414ee9`.

## Command 2 — unscoped branch footprint

Command: `git diff --name-only f8414ee9..HEAD`

Verbatim output:

```
TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs
TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs
docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/issue.md
docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/plan.2026-09-03T11-48.md
docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/research/2026-09-03T09-45-terminal-notification-hook-sync-barrier-research.md
docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/spec.md
```

Path count: **6**. Every path is either one of the two named test files or begins with
`docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/`. Evaluated mechanically: the
count of paths matching neither condition is **0**.

This command deliberately carries **no pathspec**. A pathspec-restricted listing can only ever report the
directories it names, so it cannot detect a change in a production project directory the pathspec omitted.
The tracked top-level project directories in this repository are QuickFiler, SVGControl, Tags, TaskMaster,
TaskTree, TaskVisualization, ToDoModel, UtilitiesCS, and VBFunctions (each carrying a `.csproj`), their nine
`.Test` siblings, and the `.csproj`-less legacy directory TaskVisualizer. An unscoped listing covers all of
them and every other tracked path without depending on that enumeration remaining correct.

## Command 3 — unscoped porcelain companion

Command: `git status --porcelain`

Verbatim output:

```
 M docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/plan.2026-09-03T11-48.md
?? docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/
```

Line count: **2**. Evaluated mechanically: the count of printed lines naming a path under `TaskMaster/` or
`TaskMaster.Test/` is **0**.

The two lines are the plan file, modified to record Phase 0 through Phase 3 check-off state, and the
untracked `evidence/` directory holding this plan's artifacts. Both are inside the feature folder. P5-T13
commits the evidence directory and P5-T15 commits the final plan-file state.

The status companion is required and is **not** redundant with the diff: the diff goes blind to untracked
paths, and the status goes empty once a change is committed, so each alone is wrong in one state.

## Acceptance

| Required | Observed | Result |
|---|---|---|
| The first command produced no output at all | 0 lines | PASS |
| Every path the unscoped `--name-only` listing prints is one of the two named test files or begins with the feature-folder path | 6 of 6; 0 disallowed | PASS |
| The unscoped porcelain companion prints no line naming any path under `TaskMaster/` or `TaskMaster.Test/` | 0 such lines | PASS |

## Cross-check against P0-T9

P0-T9 recorded, before any edit made by this plan, `git diff --stat f8414ee9..HEAD` naming exactly four
paths: `issue.md`, `plan.2026-09-03T11-48.md`, the research record, and `spec.md` — all four beginning with
`docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/`.

**No path outside the two test files and outside the feature folder was present in the baseline diff.** The
pre-existing branch condition that would falsify spec AC4 independently of this plan's edits therefore does
**not** exist, and this task does not fail on that cross-check.

The branch diff grew from those four paths to six: the two `TaskMaster.Test/AppGlobals/` source files added
by the P2-T6 commit `be2a3c1f`.
