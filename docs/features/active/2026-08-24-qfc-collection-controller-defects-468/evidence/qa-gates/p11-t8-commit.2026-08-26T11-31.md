# [P11-T8] Commit of the issue #473 defect 1 fix

Timestamp: 2026-08-26T11-31

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs            QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs            docs/features/active/qfc-collection-controller-defects-468
git commit -m "fix(473): drain background loading tasks through an atomic bag swap"
git show --name-only HEAD
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Commit `505cab9250a61dc89ae8b8555e9f34ce6f9dd348` —
`fix(473): drain background loading tasks through an atomic bag swap`. 14 paths.

## Committed path list, classified against the owned file set

| Path | Owned-set member |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `<CTRL>` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | D12 test file 2 |
| `docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md` | plan of record |
| `docs/features/active/qfc-collection-controller-defects-468/spec.md` | AC source (AC-12 checked off) |
| 10 paths under `<FEATURE>/evidence/` | evidence artifacts and TRX files |

**Out-of-scope set: empty.** No path outside the owned file set appears. In particular
`QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
and `QuickFiler/Controllers/EfcFormController.cs` do not appear. The csproj is unchanged in this
commit; Phase 11 added no new file.

The P10-T12 evidence artifact is absorbed here, because P10-T12 records the commit it describes and
P11-T3's acceptance restricted its own commit to `<CTRL>` alone. No extra unplanned commit was
created for it.

## Staging hygiene

Every `git add` used an explicit pathspec. `git status --porcelain` after this commit shows exactly
` M .claude/agent-memory/orchestrator/completion-gate-receipt-shapes.md` and `?? .claude/state/`,
both of which are dirty for reasons unrelated to this feature and neither of which was staged.

## Phase 11 commit pair

| Commit | Subject | Paths |
|---|---|---|
| `97604063` | `refactor(473): extract DrainBackgroundLoadingTasksAsync from the duplicated drain sites` | 1 (`<CTRL>`) |
| `505cab92` | `fix(473): drain background loading tasks through an atomic bag swap` | 14 |

D15 requires the seam and the fix to be separate commits. They are. The seam commit's tree measured
962 passed at P11-T2, identical to P10-T11, and is also the tree against which the P11-T4
fail-before run was executed.
