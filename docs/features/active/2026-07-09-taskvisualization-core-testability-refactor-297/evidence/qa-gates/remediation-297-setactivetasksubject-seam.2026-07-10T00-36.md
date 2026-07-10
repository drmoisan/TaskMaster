# Remediation QA Gates — `setActiveTaskSubject` Seam (#297 remediation pass 1)

- Timestamp: 2026-07-10T00-36
- Branch: `remediate-297` (pushed to `feature/taskvisualization-core-testability-refactor-297`)
- Scope: Blocking-1 from `remediation-inputs.2026-07-10T00-17.md` — `SetFlag(Taskname)` /
  `Shortcut_ReadingNews` left uncovered despite a feasible seam.

## Fix summary

- Added optional-with-default constructor parameter `Action<string> setActiveTaskSubject = null`
  to both `TaskController` constructors (`TaskVisualization/TaskController.cs`).
- `InitializeSeams` now wires `_setActiveTaskSubject = setActiveTaskSubject ?? (v => _active.TaskSubject = v);`
  (new private field `Action<string> _setActiveTaskSubject`), mirroring the existing
  `_showWarning` / `_mailItemHelperFactory` seam pattern.
- `SetFlag`'s `Taskname` case (`TaskVisualization/TaskController.Actions.cs`) now calls
  `_setActiveTaskSubject(value)` instead of writing `_active.TaskSubject = value` directly.
- Test fixture helpers `TaskControllerFixtures.BuildController` / `BuildControllerOver` gained a
  matching optional `setActiveTaskSubject` parameter so tests can inject a capturing delegate.
- `FlagTasks.cs` was not edited (uses named arguments and does not set the new trailing optional
  parameter — zero-edit compatible).

## New tests

- `SetFlag_Taskname_WritesSubjectAndFacade` — injects a capturing delegate, calls
  `SetFlag("New Subject", Enums.FlagsToSet.Taskname)`, asserts the captured value and the
  `TaskNameText` facade write.
- `Shortcut_ReadingNews_SetsAllFlagsAndFocusesDuration` — injects a capturing delegate, calls
  `Shortcut_ReadingNews()`, asserts Context/Projects on `Active`, the captured Taskname
  (`"READ: Original Subject"`), the Worktime facade write, and
  `view.Mock.Verify(v => v.FocusDuration(), Times.Once)`.

Both replace prior "not unit-tested here" skip-comments in
`TaskVisualization.Test/TaskControllerActionsTests.cs`.

## Toolchain commands (single clean pass)

1. Format:
   - Command: `dotnet tool run csharpier format .`
   - EXIT_CODE: 0
   - Output Summary: `Formatted 1341 files in 4579ms.` Only the 5 intentionally-touched files
     changed (`git status --short` confirmed no incidental reformatting elsewhere). Verified
     clean with a follow-up `dotnet tool run csharpier check TaskVisualization TaskVisualization.Test`
     -> `Checked 44 files in 899ms.` EXIT_CODE 0.

2. Analyzer build:
   - Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true -nologo -v:quiet`
   - EXIT_CODE: 0
   - Output Summary: 0 errors. Only pre-existing baseline warnings (CS0618 obsolete
     `AsyncEnumerable` calls, CS8632 nullable-annotation-context warnings in unrelated files,
     CS0649/CS0169/CS0067 unused-field/event warnings, one CS4014 in `OK_Action` pre-existing).
     One new pre-existing-pattern warning is expected: none introduced by this change (grep of
     the touched files' compiler output shows no new diagnostics).

3. Nullable / TreatWarningsAsErrors build:
   - Commands (in order, matching the repo's documented forced-nullable-Rebuild + Debug-restore
     sequence):
     a. `MSBuild.exe TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -nologo -v:quiet`
        -> EXIT_CODE 1 (84 pre-existing errors confined to vendored `SVGControl` /
        `UtilitiesSwordfish`, unrelated to this change; matches the known repo-wide nullable
        debt documented for this repo).
     b. `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -nologo -v:quiet`
        (Debug restore) -> EXIT_CODE 0, 0 errors.
     c. `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -m -nologo -v:quiet`
        (incremental gate command matching the plan's baseline command) -> EXIT_CODE 0, 0 errors
        (up-to-date no-op, matching the established baseline behavior recorded in
        `evidence/baseline/baseline-nullable.2026-07-09T22-00.md`).
   - Output Summary: Final incremental nullable/TWAE gate: EXIT_CODE 0, 0 errors. No new nullable
     diagnostics from the `setActiveTaskSubject` seam change.

4. Test (coverage):
   - Command: `vstest.console.exe TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll /InIsolation /Settings:TaskVisualization.Test/coverage.runsettings`
   - EXIT_CODE: 0
   - Output Summary: `Total tests: 106. Passed: 106.` (104 pre-existing + 2 new). No flaky
     `PhysicalFileInfoAdapter` test present in this assembly's suite (that test lives in a
     different project); no retry was needed.
   - Coverage (Cobertura, `TestResults/<guid>/*.cobertura.xml`):
     - `TaskVisualization` package: line-rate 85.36%, branch-rate 78.28% (repo-run aggregate,
       above the 80% line floor).
     - `TaskVisualization.TaskController` (partial in `TaskController.Actions.cs`): line-rate
       98.39%, branch-rate 91.30%.
     - Confirmed via per-line hit data: `TaskController.Actions.cs` lines 299-306
       (`Shortcut_ReadingNews`) all `hits="1"`; lines 384-389 (`SetFlag` `Taskname` case,
       including line 386 `_setActiveTaskSubject(value);`) all `hits="1"`. Both methods flagged
       Blocking in the review are now measured and covered.

## File-size compliance

- `TaskVisualization/TaskController.cs`: 330 lines (was 312).
- `TaskVisualization/TaskController.Actions.cs`: 490 lines (was 490 — net 0 change; only the
  `Taskname` case body line was replaced in place).
- `TaskVisualization/TaskController.Accelerator.cs`: 500 lines (untouched).
- `TaskVisualization.Test/TaskControllerActionsTests.cs`: 452 lines (was 420).
- `TaskVisualization.Test/TaskControllerFixtures.cs`: 195 lines (was 191).
- All files remain <= 500 lines.

## Scope confirmation

- `git diff --stat` against the branch tip shows exactly 5 changed files: `TaskController.cs`,
  `TaskController.Actions.cs`, `TaskControllerActionsTests.cs`, `TaskControllerFixtures.cs`, and
  the exemption-inventory evidence doc. `TaskVisualization/FlagTasks.cs` is absent from the diff
  (zero-edit, as required).
