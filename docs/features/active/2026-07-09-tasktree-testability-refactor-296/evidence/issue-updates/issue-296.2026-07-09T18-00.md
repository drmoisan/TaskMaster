# Issue #296 — AC / DoD Reconciliation Mirror

- Issue: #296 (child of epic winforms-testability-refactor #295)
- Timestamp: 2026-07-09T18-00
- Feature folder: docs/features/active/2026-07-09-tasktree-testability-refactor-296/
- Branch: feature/tasktree-testability-refactor-296

## Acceptance Criteria (issue.md) — all satisfied

| AC | Status | Evidence |
|---|---|---|
| `ITaskTreeForm` exists, derives from `IForm`, `TaskTreeForm` implements it | PASS | TaskTree/ITaskTreeForm.cs (`ITaskTreeForm : IForm`); TaskTree/TaskTreeForm.cs (`: Form, ITaskTreeForm`); analyzers/nullable builds green (final-analyzers.md, final-nullable.md) |
| `TaskTreeController` depends on `ITaskTreeForm`, not concrete form | PASS | TaskTree/TaskTreeController.cs ctor `(IApplicationGlobals, ITaskTreeForm, TreeOfToDoItems, Action<string>)`; P3-T1 grep verified zero `TreeLv`/`OlvToDoID`/`SplitContainer1` in controller |
| Host-neutral logic separated from COM/WinForms | PASS | TaskTree/TaskTreeController.MoveLogic.cs (host-neutral move/tree logic against `ITreeVisual`); tests exercise it with no live control |
| No production file in `TaskTree` exceeds 500 lines | PASS | final-filesize.md (max 311; controller split 546 -> 206 + 295) |
| `TaskTree.Test` project exists, follows repo MSTest pattern, in solution | PASS | TaskTree.Test/*.csproj mirrors Tags.Test; added to TaskMaster.sln; tasktree-test-scaffold-build.md |
| No unit test constructs a live form/window or triggers a popup | PASS | All 37 tests use mocked `ITaskTreeForm`/`ITreeVisual` + recording `Action<string>` seam; no `Form`/`Control` constructed, no `Show()`/`ShowDialog()` |
| `TaskTree` project reaches >= 80% line coverage | PASS | final-coverage.md — TaskTree.dll 94.04% |
| Full C# toolchain passes with no regression | PASS | final-format.md, final-analyzers.md, final-nullable.md, final-coverage.md (clean single pass) |

## Test Conditions (issue.md) — satisfied

| Condition | Status | Evidence |
|---|---|---|
| Tree/business-logic units covered with pure inputs | PASS | TaskTreeControllerMoveLogicTests.cs (real `TreeOfToDoItems`/`IDList`) |
| Dialog/UI-bound paths covered via seams (no popups) | PASS | Recording `Action<string>` message-seam tests (desync fire / happy-path no-fire) |
| Event handler logic via mocked `ITaskTreeForm` | PASS | TaskTreeControllerTests.cs toggles/rebuild/resize/select against `Mock<ITaskTreeForm>` |

## Definition of Done (spec.md) — all satisfied

All 16 DoD checkboxes checked in spec.md, mapped to the AC evidence above plus:
- Invariants validated: MoveLogic tests assert both data-model state and `AddObject`/`RemoveObject` Verify.
- Caller unchanged: final-caller-unchanged.md (git diff of RibbonController.cs empty).
- Coverage delta / no-regression: coverage-delta.md (0% baseline -> 94.04%; new files >= 90%).

## `[ExcludeFromCodeCoverage]` Exemption Register — flagged for maintainer ratification

Plan register (ratified pattern): E1 `TaskTreeForm` (Form-derived), E2 `TreeListViewVisual`
(ObjectListView virtual-mode adapter), E3 `FormatRow` wrapper (type non-constructibility).

Three ADDITIONAL exemptions were applied during execution beyond the plan's E1/E2/E3 register,
each due to empirically-verified COM / live-control untestability. These are surfaced here for
maintainer ratification (deviation from the plan register, escalated in the executor report):

| # | Site | Justification (empirically verified) |
|---|---|---|
| E4 | `TaskTree/TaskTreeController.cs` — `ActivateOlItem` | Uses `dynamic` dispatch on the Outlook `Explorer`/selection; a Moq proxy throws `RuntimeBinderException` under `dynamic` binding, so the selectable/valid-type branches cannot execute against a mock without a live Outlook Explorer. Null-guard branch remains covered and NOT exempt. |
| E5 | `TaskTree/TaskTreeController.cs` — `ActivateOlItemAsync` | Same `dynamic`-dispatch obstacle as E4 (async form). Null-guard branch covered and NOT exempt. |
| E6 | `TaskTree/TaskTreeController.MoveLogic.cs` — `HandleModelDropped` drop routing | `RefreshObjects` NREs on the null `ListView` of a mock drop-event; confirmed via reflection probe. The `default`-case early return remains covered and NOT exempt. |

Testable seams remain non-exempt: `ResolveRowStyle`, all `MoveObjects*` logic, `FindChildByID`,
`IsValidType`, `HandleModelCanDrop`, and every `ITaskTreeForm`/`ITreeVisual`/`_showMessage`
consumer. Resulting coverage with E1-E6 applied: TaskTree.dll 94.04%, controller 95.65%,
move-logic 93.29%.

Ratification requested: maintainer to confirm E4/E5/E6 fall under the ratified COM interop /
live-control exemption (CLAUDE.md General Unit Test Policy §UT2 category c — Outlook Interop
event-handler paths depending on `Outlook.Application`/`Explorer` without an injectable seam,
and the ObjectListView live-control obstacle), consistent with the pre-ratified E1/E2/E3.
