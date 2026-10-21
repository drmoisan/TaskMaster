# [P14-T7] Fix-order audit (AC-17)

Timestamp: 2026-08-26T16-34

Command:

```
git log --reverse --format='%h|%s' 61edc19b..HEAD --first-parent
git show --name-only --format='' <sha>          # per commit
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Twenty commits exist on `bug/qfc-collection-controller-defects-468` since the merge base `61edc19b`.
Eighteen were made by this plan; two are merges from the epic integration branch. **The fix sequence
matches D1 exactly**, and the dead-code removal commit `63eebd47` carries exactly one `.cs` path.

D1 fix order, from
`docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md`:

> `#468` dead-code removal first, then `#474-1`, then `#286`, then `#469-3`, then `#473-2`,
> `#469-1/-2`, `#470-2`, `#470-1`, `#470-3`, `#471`, `#473-1`, `#469-4`, `#474-2`.

## Commit sequence, in order

| # | SHA | Message | Defect in D1 order | Code paths | Docs paths |
|---|---|---|---|---|---|
| 1 | `c6723e9f` | `docs(468): phase 0 baseline and toolchain bootstrap` | — (Phase 0) | none | 13 |
| 2 | `63eebd47` | `fix(468): remove unreachable load paths and the dead _templateTlp field` | **#468** (1st) | `QuickFiler/Controllers/QfcCollectionController.cs` | 12 |
| 3 | `122dcd8d` | `fix(474): retype _parent to IQfcFormController and drop the runtime downcast` | **#474-1** (2nd) | `QfcCollectionController.TestSupport.cs`, `QfcCollectionControllerDarkModeTests.cs`, `QfcCollectionControllerDefects468Tests.cs`, `QuickFiler.Test.csproj`, `QfcCollectionController.cs` | 13 |
| 4 | `fbe5b3a6` | `fix(286): restore the reentrancy counter on the exceptional exit path` | **#286** (3rd) | `QfcCollectionControllerDefects468Tests.cs`, `QfcCollectionController.cs` | 11 |
| 5 | `d512fcfe` | `fix(469): replace the unordered move collection with an ordered snapshot` | **#469-3** (4th) | `QfcCollectionControllerDefects468MoveTests.cs`, `QfcCollectionControllerTests.cs`, `QuickFiler.Test.csproj`, `QfcCollectionController.cs` | 9 |
| 6 | `8637aaa8` | `fix(473): stop swallowing cancellation and double-logging one root failure` | **#473-2** (5th) | `QfcCollectionControllerDefects468MoveTests.cs`, `QfcCollectionController.cs` | 12 |
| 7 | `137ee307` | `fix(469): correct the diagnostics array length and guard before dereference` | **#469-1 / #469-2** (6th) | `QfcCollectionControllerDefects468MoveTests.cs`, `QfcCollectionController.cs` | 35 |
| 8 | `62322433` | `fix(470): derive the conversation insertion count from a single source of truth` | **#470-2** (7th) | `QfcCollectionControllerDefects468ConversationTests.cs`, `QuickFiler.Test.csproj`, `QfcCollectionController.cs` | 9 |
| 9 | `40381135` | `fix(470): handle a missing conversation original explicitly instead of subscripting` | **#470-1** (8th) | `QfcCollectionControllerDefects468ConversationTests.cs`, `QfcCollectionController.cs` | 12 |
| 10 | `ffc10ff9` | `fix(470): skip groups with no controller or viewer in SetVisualDigits` | **#470-3** (9th) | `QfcCollectionControllerDefects468ConversationTests.cs`, `QfcCollectionController.cs` | 9 |
| 11 | `6cac5a82` | `refactor(471): extract the shared panel-height arithmetic behind ShrinkByRows` | **#471 seam** (10th) | `QfcCollectionController.cs` **only** | 0 |
| 12 | `f733506a` | `fix(471): shrink the item panel on conversation collapse` | **#471** (10th) | `QfcCollectionControllerDefects468Tests.cs`, `QfcCollectionControllerLayout.StaTests.cs`, `QuickFiler.Test.csproj`, `QfcCollectionController.cs` | 14 |
| 13 | `97604063` | `refactor(473): extract DrainBackgroundLoadingTasksAsync from the duplicated drain sites` | **#473-1 seam** (11th) | `QfcCollectionController.cs` **only** | 0 |
| 14 | `505cab92` | `fix(473): drain background loading tasks through an atomic bag swap` | **#473-1** (11th) | `QfcCollectionControllerDefects468Tests.cs`, `QfcCollectionController.cs` | 12 |
| 15 | `613e88c3` | `docs(469): document the retained stackMovedItems contract and consume the parameter` | **#469-4** (12th) | `QfcCollectionControllerDefects468MoveTests.cs`, `QfcCollectionController.cs`, `QuickFiler/Interfaces/IQfcCollectionController.cs` | 0 |
| 16 | `4938779a` | `refactor(474): split the move-readiness evaluation from its notification` | **#474-2 seam** (13th) | `QfcCollectionController.cs` **only** | 0 |
| 17 | `48c9ad8f` | `docs(468): commit phase 11-13 QA-gate and regression evidence` | — (evidence catch-up) | `QfcCollectionControllerDefects468Tests.cs` | 11 |
| 18 | `7f0e7a2b` | merge of `origin/epic/quickfiler-bug-family-integration` | — (integration) | n/a | n/a |
| 19 | `ef907908` | merge of `origin/epic/quickfiler-bug-family-integration` | — (integration) | n/a | n/a |
| 20 | `5f8026aa` | `fix(474): make move readiness inspectable without presenting a dialog` | **#474-2** (13th) | none | 5 |

## D1 conformance

| D1 position | Defect | Commit(s) | In order? |
|---|---|---|---|
| 1 | `#468` | `63eebd47` | yes |
| 2 | `#474-1` | `122dcd8d` | yes |
| 3 | `#286` | `fbe5b3a6` | yes |
| 4 | `#469-3` | `d512fcfe` | yes |
| 5 | `#473-2` | `8637aaa8` | yes |
| 6 | `#469-1` and `#469-2` | `137ee307` | yes |
| 7 | `#470-2` | `62322433` | yes |
| 8 | `#470-1` | `40381135` | yes |
| 9 | `#470-3` | `ffc10ff9` | yes |
| 10 | `#471` | `6cac5a82` (seam) then `f733506a` (fix) | yes |
| 11 | `#473-1` | `97604063` (seam) then `505cab92` (fix) | yes |
| 12 | `#469-4` | `613e88c3` | yes |
| 13 | `#474-2` | `4938779a` (seam) then `5f8026aa` (verification) | yes |

The sequence matches D1 with no reordering, no defect skipped, and no defect fixed twice.

## Seam-before-fix cadence (D15)

Each of the three AC-20 seams is committed **separately from and immediately before** the defect fix
that lands on top of it, and each seam commit's path list is `QuickFiler/Controllers/QfcCollectionController.cs`
alone with zero docs paths:

| Seam | Seam commit | Paths | Fix commit that follows |
|---|---|---|---|
| `ShrinkByRows` | `6cac5a82` | 1 (`QfcCollectionController.cs`) | `f733506a` |
| `DrainBackgroundLoadingTasksAsync` | `97604063` | 1 (`QfcCollectionController.cs`) | `505cab92` |
| readiness predicate + `_notifyNotReady` | `4938779a` | 1 (`QfcCollectionController.cs`) | `5f8026aa` |

## Dead-code isolation (the explicit acceptance clause)

`git show --name-only 63eebd47` yields 13 paths. Filtering to `.cs` and `.csproj`:

```
QuickFiler/Controllers/QfcCollectionController.cs
```

**Exactly one `.cs` path, and no `.csproj` path.** The remaining 12 paths are all under
`docs/features/active/qfc-collection-controller-defects-468/`: the Phase 1 evidence artifacts, the
plan, and `spec.md`. The file renumbering caused by removing twelve members is therefore a single
reviewable hunk in a single source file, which is what D15 requires.

## Deviations from the planned cadence, recorded truthfully

Three departures from the plan's literal commit cadence exist in this history. None reorders a fix.

1. **`137ee307` carries 35 docs paths** rather than the ~11 a single phase produces. It absorbed the
   host-identifier sanitisation sweep recorded in
   `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`, which rewrote sixteen previously
   committed TRX files. That sweep is a scoped remediation, not a plan task; its own artifact records
   why it was necessary and confirms that no `<Counters>` value changed.

2. **`48c9ad8f` is not a planned commit.** It is an evidence catch-up made by an earlier session,
   absorbing the Phase 11-13 evidence plus the two `TryGetMoveReadiness` tests that P13-T4 and P13-T5
   call for. Its presence means the source for those two tests was already committed when this
   executor session resumed; both were verified against their acceptance criteria rather than
   re-authored. It also carries one path outside `docs/` and outside the owned set —
   `.claude/agent-memory/orchestrator/completion-gate-receipt-shapes.md` — which is agent memory, not
   product code, and is explicitly permitted by the P14-T10 scope-lock formulation.

3. **`5f8026aa` carries no `.cs` path**, because the fix and its tests were already committed at
   `4938779a` and `48c9ad8f` respectively. The commit therefore carries the Phase 13 evidence and the
   plan check-offs under the message the plan mandates for P13-T8.

Commits 18 and 19 are merges from `origin/epic/quickfiler-bug-family-integration`, performed by the
orchestrator to bring this branch level with the epic integration branch before the final QA loop.
They introduce sibling feature 498's changes. They are integration operations, not fixes, and they do
not appear in the D1 sequence.
