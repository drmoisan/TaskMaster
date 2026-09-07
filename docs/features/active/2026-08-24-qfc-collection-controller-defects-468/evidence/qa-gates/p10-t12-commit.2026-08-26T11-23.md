# [P10-T12] Commit of the issue #471 panel-height fix

Timestamp: 2026-08-26T11-23

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs            QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs            QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs            QuickFiler.Test/QuickFiler.Test.csproj            docs/features/active/qfc-collection-controller-defects-468
git commit -m "fix(471): shrink the item panel on conversation collapse"
git show --name-only HEAD
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Commit `f733506ab59de423cb7cd6e9834938d1906af1ab` —
`fix(471): shrink the item panel on conversation collapse`. 18 paths.

## Committed path list, classified against the owned file set

| Path | Owned-set member |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `<CTRL>` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs` | D12 test file 5 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | D12 test file 2 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | D13 insertion point |
| `docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md` | plan of record |
| `docs/features/active/qfc-collection-controller-defects-468/spec.md` | AC source (AC-11 checked off) |
| 12 paths under `<FEATURE>/evidence/` | evidence artifacts and TRX files |

**Out-of-scope set: empty.** No path outside the owned file set appears. In particular
`QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
and `QuickFiler/Controllers/EfcFormController.cs` do not appear.

## Two deliberately-deferred paths absorbed here

Per the plan's commit cadence, the P9-T5 evidence artifact
(`evidence/qa-gates/p9-t5-commit.2026-08-26T11-03.md`) and the P9-T5 plan checkbox edit could not be
committed by P9-T5 itself, which records the commit it is describing, nor by P10-T3, whose acceptance
restricts the commit to `<CTRL>` alone. This commit absorbs both. No extra unplanned commit was
created for them.

## Staging hygiene

Every `git add` used an explicit pathspec. `.claude/agent-memory/**` and `.claude/state/**` are
dirty in this worktree, are not owned by this feature, and remain unstaged and uncommitted:
`git status --porcelain` after this commit shows exactly
` M .claude/agent-memory/orchestrator/completion-gate-receipt-shapes.md` and `?? .claude/state/`,
and nothing else.

## Phase 10 commit pair

| Commit | Subject | Paths |
|---|---|---|
| `6cac5a82` | `refactor(471): extract the shared panel-height arithmetic behind ShrinkByRows` | 1 (`<CTRL>`) |
| `f733506a` | `fix(471): shrink the item panel on conversation collapse` | 18 |

D15 requires the seam and the fix to be separate commits. They are. The seam commit's tree is the
one at which P10-T2 measured 958 passed — identical to P9-T4 — which is the evidence that the
extraction changed no observable behaviour.
