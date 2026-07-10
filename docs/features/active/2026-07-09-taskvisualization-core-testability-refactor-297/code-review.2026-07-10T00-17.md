# Code Quality Review — taskvisualization-core-testability-refactor (#297)

- Timestamp: 2026-07-10T00-17
- Branch: `feature/taskvisualization-core-testability-refactor-297` @ `5f8eea31`
- Base: `epic/winforms-testability-refactor-integration` (merge-base `3f04d50f`)

## Executive Summary

This is a large, carefully executed structural refactor. `TaskController` is decomposed from 1861 lines into six focused partial classes plus two pure host-neutral helpers, all within the 500-line limit. The `ITaskViewer` primitive facade and the `ITaskViewerControls` companion cleanly separate the testable core from live-control identity; three injectable seams (`ITagPromptService`, `Action<string>` notifier, `Func<MailItem,Task<MailItemHelper>>` factory) remove dialog/COM construction from the unit-test path. Naming, XML docs (with why-comments on exemptions), and formatting are consistent with repository style. Test design is disciplined: non-STA tests use `Mock<ITaskViewer>` only; STA tests are confined to dedicated `*.StaTests.cs` with `[STATestClass]`, construct real never-shown in-memory controls, dispose per test, and use no message pump.

One Blocking finding and three non-blocking observations follow.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking | `TaskVisualization/TaskController.Actions.cs` | `SetFlag` Taskname case (lines 384-389) and `Shortcut_ReadingNews` (lines 299-306) | Both left uncovered; a thin controller-level write-interception seam over the `_active.TaskSubject` write would make them assertable, and no ratified exemption covers them | Add an optional-with-default `Action<string>` seam for the TaskSubject write (default `v => _active.TaskSubject = v`), route the Taskname case through it, and add two tests asserting the captured value for `SetFlag("x", Taskname)` and `Shortcut_ReadingNews` | The feature's purpose is testability and it already uses this exact seam pattern (`_showWarning`, `_mailItemHelperFactory`); a testable seam left uncovered without a ratified exemption is the class of gap this review gates | `TaskControllerActionsTests.cs` lines 75-88, 185-187 (tests deliberately skipped); `exemption-inventory` line 37 counts these as uncovered-not-exempt; coverage-comparison shows Actions.cs 261/293 = 89.08% |
| Low (non-blocking) | `TaskVisualization/TaskController.Flags.cs` | `ApplyChanges` (lines 35-128) | Whole-method exemption also covers the in-loop host-neutral field-application block (lines 51-114), broader than the plan's "COM-iteration lines" wording | In a future iteration or #298, extract `ApplyActiveFieldsTo(ToDoItem)` and cover it directly, shrinking the exempt surface | The field-application logic is pure comparison/assignment; only its reachability is blocked by the non-terminating COM loop | Pre-authorized by plan line 172 and P7-T1(iv); extractable units already covered |
| Info | `TaskVisualization.Test/coverage.runsettings` | `<ModulePath>` include | Coverage collector scoped to `TaskVisualization.dll` only, so the reported figure is feature-scoped, not repo-wide | Keep as-is for this child; ensure #298/epic integration reports project-wide C# coverage | Feature-scoped measurement is correct for a wave-0 child; project-wide 80% is deferred to #298 per spec | `coverage.runsettings` lines 12-16 |
| Info | evidence | `coverage-comparison.2026-07-10T00-01.md` | Branch-coverage percentage not recorded; raw Cobertura (`artifacts/csharp/coverage.xml`) is gitignored and not committed, so line/branch figures are not independently re-parseable | Record the branch-coverage percentage in the coverage-comparison evidence (and consider committing the trimmed Cobertura or an lcov digest) | The local audit is the merge gate for this child PR (zero CI checks); numeric branch evidence strengthens the gate | evidence markdown reports line coverage only |

## Design & Structure Assessment

- Decomposition: partial-class split preserves the shared private accelerator state (`_xlCtrlsActive`, `_altActive`, `_altLevel`, `_activeNavGroup`) without widening visibility, and preserves every event-routed method name (`OK_Action`, `Cancel_Action`, `KeyboardHandler_*`, `Assign*`, `Shortcut_*`, `Today_Change`, etc.), so event routing is behavior-preserving. Confirmed against the diff.
- Seam confinement: the concrete `TaskViewer` cast (`Form` accessor) is confined to the live-handle residue; `ViewerControls` carries control identity; the testable core sees only `ITaskViewer`. This is a clean layering.
- Helpers: `TaskDurationParser` (returns `(bool ok, int minutes, string error)`) and `TaskPriorityMapper` (bidirectional `OlImportance`<->display) are pure, fully covered (100%), and reusable by #298.
- Error handling: `CaptureDuration` preserves the exact prior behavior (negative -> notifier + no state change; non-integer/empty -> `FormatException` propagates), documented in XML docs.

## Test Quality Assessment

- Isolation/determinism: non-STA controller tests construct no live controls; STA tests construct only never-shown in-memory controls disposed per test; no `Show`/`ShowDialog`/`DoEvents`/`Thread.Sleep`/`Task.Delay`/timers/`PostMessage` round-trips (scan clean). No `Form`-derived type constructed anywhere in tests.
- Scenario completeness: `AreCollectionsEqual` and both `ApplyChange` overloads cover positive/negative/edge/null. `TaskDurationParser`/`TaskPriorityMapper` cover positive/zero/negative/non-integer/empty and both mapping directions plus unknown-fallback. `Assign*` cover cancel-vs-select. STA tests measure the accelerator state machine and control-identity builders.
- Gap: the two methods in the Blocking finding are the only material testable-logic paths left unexercised; the executor documented the reason inline.

## Blocking Finding Detail (6b)

Root cause: `_active` is a concrete `readonly ToDoItem` (TaskController.cs line 271), set once from `_todo_list[0].DeepCopy()`. `ToDoItem.TaskSubject`'s setter is non-virtual and routes to `OutlookItemExtensions` reflection against the underlying interop `MailItem`; over a Moq proxy this raises `MissingMethodException` (production recovers via `catch(COMException) -> Subject` fallback, which needs a live COM object). Because `ToDoItem.TaskSubject` cannot be serviced by any injectable double within #297's scope (ToDoItem is owned by ToDoModel and out of scope to refactor), the only in-scope mechanism to cover `SetFlag(Taskname)` and `Shortcut_ReadingNews` is a controller-level write-interception seam.

Required fix (seam-based, minimal):
1. Add field `private Action<string> _setActiveTaskSubject;` and initialize it in `InitializeSeams` with an optional-with-default parameter: `_setActiveTaskSubject = setActiveTaskSubject ?? (v => _active.TaskSubject = v);` (add the optional param to both constructors, preserving `FlagTasks.cs` zero-edit compatibility).
2. In `SetFlag`'s `Taskname` case, replace `_active.TaskSubject = value;` with `_setActiveTaskSubject(value);` (leave `_viewer.TaskNameText = value;` unchanged).
3. Add `SetFlag_Taskname_WritesSubjectAndFacade` and `Shortcut_ReadingNews_SetsAllFlagsAndFocusesDuration` tests injecting a capturing delegate; assert the captured value(s) and `mock.Verify(v => v.FocusDuration())`.

This closes both uncovered methods and completes the feature's own seam pattern for the last interop-bound write.
