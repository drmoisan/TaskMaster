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

## `[ExcludeFromCodeCoverage]` Exemption Register — final (remediation pass 1, 2026-07-09T23-26)

Feature-review (remediation-inputs.2026-07-09T23-09.md) flagged E4/E5/E6 as three Blocking findings:
each was an exemption placed on a TESTABLE seam rather than an irreducible host-bound residual. The
remediation replaced each with a mockable seam plus real tests and REMOVED the attribute. The final
register contains only the four legitimate exemptions below.

### Final exemptions (four — all legitimate host-bound/thin residuals)

| # | Site | Category | Justification |
|---|---|---|---|
| E1 | `TaskTree/TaskTreeForm.cs` (type) | b | Form-derived WinForms host surface. |
| E2 | `TaskTree/TreeListViewVisual.cs` (type) | b/c | Minimal ObjectListView host adapter; two-line delegations to a live virtual-mode control. |
| E3 | `TaskTree/TaskTreeController.cs` — `FormatRow` | c | Thin residual event-handler wrapper; `FormatRowEventArgs`/`OLVListItem` are not constructible from tests. The strikeout decision is extracted into the covered `ResolveRowStyle`. |
| E6-residual | `TaskTree/TaskTreeController.MoveLogic.cs` — `HandleModelDropped` | b/c | Thin residual wrapper only: builds E2 adapters from live `e.ListView`/`e.SourceListView` and calls `e.RefreshObjects()` (NREs without a live control). Routing extracted to covered `RouteDrop`; post-drop filter/sort extracted to covered `ApplyPostDropView`. |

### Removed exemptions (remediation pass 1)

| # | Site | Fix | Coverage now |
|---|---|---|---|
| E4 | `ActivateOlItem` | `dynamic item` -> `object item` with typed `DisplayOutlookItem` dispatch (`is MailItem`/`is TaskItem` -> `Display()`); Explorer selection binds against the mockable `Outlook.Explorer` interface. New tests cover selectable/Display branches and the caller valid-type path `TreeLvActivateItem`. | Controller.cs 100% line |
| E5 | `ActivateOlItemAsync` | Same `object`-seam fix (async form); `Task.Run` wrapping unaffected. New tests cover the async selectable/Display/Activate branches and `TreeLvActivateItemAsync` valid-type path. | Controller.cs 100% line |
| E6 | `HandleModelDropped` drop routing | Extracted the `switch (e.DropTargetLocation)` routing into the covered `RouteDrop(ITreeVisual, ITreeVisual, ModelDropEventArgs)` over the mockable `ITreeVisual` seam (ModelDropEventArgs constructed via the tests' `DropArgs` reflection helper). New tests cover every `DropTargetLocation` enum value. Only the thin `RefreshObjects`/adapter-construction residual keeps the attribute. | MoveLogic.cs 94.54% line |

Testable seams remain non-exempt: `ResolveRowStyle`, `RouteDrop`, `ApplyPostDropView`, all
`MoveObjects*` logic, `FindChildByID`, `IsValidType`, `HandleModelCanDrop`, `ActivateOlItem(Async)`,
and every `ITaskTreeForm`/`ITreeVisual`/`_showMessage` consumer. Resulting coverage after removal of
E4/E5/E6: TaskTree.dll 96.34% line / 91.49% branch (was 94.04%); controller 100% line; move-logic
94.54% line. 51 tests pass. No `[ExcludeFromCodeCoverage]` remains on a testable seam.
