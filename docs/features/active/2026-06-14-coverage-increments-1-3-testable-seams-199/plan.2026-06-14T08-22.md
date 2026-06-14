# coverage-increments-1-3-testable-seams - Atomic Implementation Plan

- **Issue:** #199
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-14T08-22
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Scope and Mode

Full-mode test-coverage refactor. Test additions only across three existing `.Test`
projects (`ToDoModel.Test`, `QuickFiler.Test`, `TaskMaster.Test`). No production behavior
change. The comparison baseline for net coverage increase is the post-#197 production-only
rate of **71.65%** (authority-scoped exception 197-COV-001).

Authoritative requirements source: `spec.md` (this folder). Issue: #199 (canonical
throughout).

## Hard Constraints (apply to every test-authoring task)

- MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`), Moq, FluentAssertions only. No
  xUnit/NUnit.
- Arrange-Act-Assert; descriptive names; clear failure messages; independent, isolated,
  deterministic.
- No temp files; no external dependencies; no live Outlook process; no WinForms message
  loop; no mutable global state; no timing/sleep hacks. Async paths use
  synchronously-completing delegates.
- Zero production change. No edit to any `*.cs` production file, `*.csproj`, `*.props`,
  `*.targets`, `coverage.config`, `TaskMaster.runsettings`, or the coverage pipeline. No
  `[ExcludeFromCodeCoverage]` added or removed; the #197 exemption boundary is unchanged.
- The `internal` `SetAndSave<T>` overloads are reachable via the existing
  `InternalsVisibleTo("ToDoModel.Test")` (confirmed in
  `ToDoModel/Data Model/ToDo/ToDoItem.cs`); no production seam is required to reach them.
- New/changed code targets >= 90% line coverage; no coverage regression on changed lines.
- C# per-batch budget: at most 3 test files per batch handled by the csharp-typed-engineer
  worker. Tasks are grouped to honor this.

## Flag-and-Stop Rule (scope-change, not a silent edit)

If any target seam cannot be exercised without introducing a new production seam
(interface, injection point, wrapper) that is not already present in source — for example
the `ProjectEntry.SetProjectId` malformed-ID path if it routes through a static
`MyBox`/MessageBox call with no existing injectable seam — the executor MUST halt that task,
restrict the test to branches reachable without the new seam, record the gap as a deviation
in `evidence/other/`, and stop for maintainer direction. Introducing a new production seam
is a scope change and is prohibited as a silent edit.

## Evidence Locations (canonical, non-overridable)

All evidence is written under `<FEATURE>/evidence/<kind>/` where `<FEATURE>` is
`docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199`:

- Policy-read and baseline command artifacts: `evidence/baseline/`
- Per-phase QA loop artifacts (csharpier, analyzers, nullable/TWAE, MSTest): `evidence/qa-gates/`
- Coverage measurement and per-increment delta comparison artifacts: `evidence/qa-gates/`
- Deviation/flag-and-stop dossiers: `evidence/other/`
- Issue update mirrors: `evidence/issue-updates/`

Non-canonical paths (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`,
`artifacts/evidence/`, etc.) are forbidden for evidence output. The roadmap baseline source
`artifacts/csharp/coverage-firstparty.cobertura.xml` is a read-only input, not an evidence
output location.

All artifacts use ISO-8601 timestamps `yyyy-MM-ddTHH-mm`. Each command-step artifact MUST
include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Coverage artifacts
MUST include numeric coverage headline values.

---

### Phase 0 — Baseline Capture and Seam Verification

- [x] [P0-T1] Read policy files in required order and record an evidence artifact at `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`. Acceptance: artifact exists with all three fields and the four files listed.
- [x] [P0-T2] Capture csharpier baseline by running `dotnet tool run csharpier --check .` and write `evidence/baseline/csharpier-baseline.2026-06-14T08-22.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records the exit code and whether any files would be reformatted.
- [x] [P0-T3] Capture analyzer baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `evidence/baseline/analyzers-baseline.2026-06-14T08-22.md` with the four required fields. Acceptance: artifact records build result and analyzer diagnostic count.
- [x] [P0-T4] Capture nullable/TreatWarningsAsErrors baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `evidence/baseline/nullable-baseline.2026-06-14T08-22.md` with the four required fields. Acceptance: artifact records build result and any nullable warnings.
- [x] [P0-T5] Capture the post-#197 production-only coverage baseline by recording the authoritative figure (71.65%) and its source artifact `artifacts/csharp/coverage-firstparty.cobertura.xml` into `evidence/baseline/coverage-baseline.2026-06-14T08-22.md` with `Timestamp:`, `Command:` (the read/derivation step), `EXIT_CODE:`, and `Output Summary:` containing the numeric headline `Production-only baseline: 71.65%` and the per-assembly figures for `ToDoModel`, `QuickFiler`, and `TaskMaster` from the source. Acceptance: artifact records the 71.65% numeric baseline and source path.
- [x] [P0-T6] Verify the Increment 1 target seams are present and measured (not #197-exempt): confirm `internal void SetAndSave<T>` (four overloads) in `ToDoModel/Data Model/ToDo/ToDoLoader.cs`, `GetNextToDoID(string)` and Outlook-free constructors in `ToDoModel/Data Model/ID/IDList.cs`, `SetProjectId`/`CompareTo` in `ToDoModel/Data Model/Project/ProjectEntry.cs`, `BaseChanger` in `ToDoModel/Data Model/ID/BaseChanger.cs`, and `InternalsVisibleTo("ToDoModel.Test")`. Record findings in `evidence/baseline/seam-verification-todomodel.2026-06-14T08-22.md`. Acceptance: artifact confirms each member exists with file/line and that none carry `[ExcludeFromCodeCoverage]`.
- [x] [P0-T7] Verify the Increment 2 target seams are present and measured: confirm `KaChar`/`KaCharAsync` in `QuickFiler/Controllers/KaChar.cs`, `KaKey`/`KaKeyAsync` in `QuickFiler/Controllers/KaKey.cs`, `KaStringAsync` in `QuickFiler/Controllers/KaStringAsync.cs`, `KbdActions<>` in `QuickFiler/Controllers/KbdActions.cs`, `FilerQueue` in `QuickFiler/Controllers/FilerQueue.cs`, `QfcQueue` in `QuickFiler/Controllers/QfcQueue.cs`, and that none touch Outlook in the targeted paths. Record findings in `evidence/baseline/seam-verification-quickfiler.2026-06-14T08-22.md`. Acceptance: artifact confirms each member exists with file/line and no `[ExcludeFromCodeCoverage]`.
- [x] [P0-T8] Verify the Increment 3 target seams are present and measured: confirm `AppStagingFilenames` in `TaskMaster/AppGlobals/AppStagingFilenames.cs`, `MatchBestSpecialFolder(string)` in `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`, and the remaining pure properties in `TaskMaster/AppGlobals/AppQuickFilerSettings.cs`. Record findings in `evidence/baseline/seam-verification-taskmaster.2026-06-14T08-22.md`, noting that `AppStagingFilenames` and `AppQuickFilerSettings` read/write the static `TaskMaster.Properties.Settings.Default` singleton directly and expose NO injectable settings type; the established, maintainer-accepted test approach (per existing `AppQuickFilerSettingsTests.cs`) is to snapshot `Settings.Default` in `[TestInitialize]` and restore it in `[TestCleanup]`, exercising the type via its parameterless constructor. Acceptance: artifact confirms each member exists with file/line, records that no injectable settings seam exists (snapshot/restore is the seam), and that none carry `[ExcludeFromCodeCoverage]`.

---

### Phase 1 — Increment 1: ToDoModel.Test (Batch A: ToDoLoader + IDList)

- [x] [P1-T1] Add MSTest tests for `ToDoLoader.SetAndSave<T>` covering all four overloads in a new test file under `ToDoModel.Test/` (e.g. `ToDoModel.Test/Data Model/ToDo/ToDoLoaderSetAndSaveTests.cs`): positive (`objectSetter` invoked with supplied value; `objectSaver` overload invokes the saver; `ref` overload assigns the new value), negative (null `objectSetter` path; null `objectSaver` path as guard behavior, not an unguarded NRE), and edge (read-only guard path; value equal to existing value). Use lambda delegates and Moq for delegate verification; FluentAssertions for assertions. Acceptance: all four overloads exercised with positive/negative/edge methods, file builds, tests pass.
- [x] [P1-T2] Add MSTest tests for `IDList.GetNextToDoID(string)` in a new test file under `ToDoModel.Test/` (e.g. `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs`), constructing `IDList` only via the Outlook-free constructors `IDList()`, `IDList(IList<string>)`, or `IDList(IEnumerable<string>)`: positive base case (no collision), edge ID-already-present loop (collision forces increment), edge length-boundary rollover (single base-36 digit advancing to two digits; assert produced length and value), and negative null/empty seed (assert the documented behavior verified from source, do not assume). Acceptance: all four scenarios present, no Outlook constructor used, tests pass.

### Phase 1 — Increment 1: ToDoModel.Test (Batch B: ProjectEntry + BaseChanger)

- [x] [P1-T3] Add MSTest tests for `ProjectEntry` in a new test file under `ToDoModel.Test/` (e.g. `ToDoModel.Test/Data Model/Project/ProjectEntryTests.cs`): `SetProjectId` positive (valid ID set), negative (null newID), and the malformed-ID validation path restricted to branches reachable without invoking a dialog; `CompareTo` positive/edge (equal, different, ordinal ordering, prefix) and negative (null argument). Use plain `ProjectEntry` instances. If the malformed-ID path requires a static `MyBox`/MessageBox call with no existing injectable seam, apply the Flag-and-Stop Rule: restrict to dialog-free branches and record the gap in `evidence/other/projectentry-malformed-gap.2026-06-14T08-22.md`. Acceptance: positive/negative/edge methods present for both members; any dialog-dependent gap is flagged, not silently edited; tests pass.
- [x] [P1-T4] Add MSTest tests for `BaseChanger` remaining uncovered branches in a new or extended test file under `ToDoModel.Test/` (e.g. extend `BaseChangerTests` or add `ToDoModel.Test/Data Model/ID/BaseChangerRemainingBranchesTests.cs`): positive (representative conversions across supported bases), edge (zero, single-digit, base-boundary rollover, maximum supported digit), negative/error (invalid base or invalid input character per the method contract). Acceptance: previously-uncovered branches exercised; positive/edge/negative methods present; tests pass.

### Phase 1 — Increment 1: QA Loop and Coverage

- [x] [P1-T5] Run csharpier formatting `dotnet tool run csharpier .`; if it changes files, restart the loop from this task. Write `evidence/qa-gates/inc1-csharpier.2026-06-14T08-22.md` with the four required fields. Acceptance: final pass reports no formatting changes (exit 0).
- [x] [P1-T6] Run analyzers `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `evidence/qa-gates/inc1-analyzers.2026-06-14T08-22.md` with the four required fields. Acceptance: build succeeds with no analyzer errors.
- [x] [P1-T7] Run nullable/TWAE `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `evidence/qa-gates/inc1-nullable.2026-06-14T08-22.md` with the four required fields. Acceptance: build succeeds with no warnings-as-errors.
- [x] [P1-T8] Run the ToDoModel.Test suite with coverage via `vstest.console.exe <ToDoModel.Test assembly path> /EnableCodeCoverage` (or `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to ToDoModel.Test). Write `evidence/qa-gates/inc1-mstest-coverage.2026-06-14T08-22.md` with the four required fields and numeric coverage headline. Acceptance: all new Increment 1 tests pass; artifact records pass count and ToDoModel coverage percent.
- [x] [P1-T9] Compute and record the Increment 1 covered-line delta in `evidence/qa-gates/inc1-coverage-delta.2026-06-14T08-22.md` reporting baseline production-only coverage (71.65%), post-Increment-1 production-only coverage, the covered-line increase on the named ToDoModel seams, and the new/changed-code coverage percentage (target >= 90%). Acceptance: artifact shows the named seams' covered-line count increased, new-code coverage >= 90%, and no regression on changed lines; if new-code coverage < 90% or any value is unavailable, mark remediation-required (not PASS).

---

### Phase 2 — Increment 2: QuickFiler.Test (Batch A: KaChar/KaCharAsync + KaKey/KaKeyAsync + KaStringAsync)

- [x] [P2-T1] Add MSTest tests for `KaChar` and `KaCharAsync` (both in `QuickFiler/Controllers/KaChar.cs`) in a new test file under `QuickFiler.Test/` (e.g. `QuickFiler.Test/Controllers/KaCharTests.cs`): positive (construction with valid char and delegate; stored key/delegate retained; action dispatches to the supplied delegate when invoked); async variant `KaCharAsync` awaited `Func<…, Task>` invoked and completes using a synchronously-completing delegate (no `Task.Delay`/`Sleep`); negative (null delegate / null key per the constructor contract); edge (equality/identity semantics if defined; default/boundary char values). Acceptance: positive/async/negative/edge methods present; no timing dependency; tests pass.
- [x] [P2-T2] Add MSTest tests for `KaKey` and `KaKeyAsync` (both in `QuickFiler/Controllers/KaKey.cs`) in a new test file under `QuickFiler.Test/` (e.g. `QuickFiler.Test/Controllers/KaKeyTests.cs`) with the same positive/async/negative/edge scenario set as P2-T1, using a synchronously-completing delegate for the async variant. Acceptance: positive/async/negative/edge methods present; no timing dependency; tests pass.
- [x] [P2-T3] Add MSTest tests for `KaStringAsync` (`QuickFiler/Controllers/KaStringAsync.cs`) in a new test file under `QuickFiler.Test/` (e.g. `QuickFiler.Test/Controllers/KaStringAsyncTests.cs`): positive (construction with valid string and delegate; stored values retained), async (awaited `Func<…, Task>` invoked and completes via a synchronously-completing delegate), negative (null delegate / null key), edge (default/boundary string values; equality/identity if defined). Acceptance: positive/async/negative/edge methods present; no timing dependency; tests pass.

### Phase 2 — Increment 2: QuickFiler.Test (Batch B: KbdActions + FilerQueue + QfcQueue)

- [x] [P2-T4] Add MSTest tests for the remaining `KbdActions<>` branches (`QuickFiler/Controllers/KbdActions.cs`) in a new or extended test file under `QuickFiler.Test/` (e.g. `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs`): positive (register then resolve an action by key), negative (resolve a missing key; register a duplicate key asserting documented behavior), edge (empty registry; removal of a present and an absent key; state after clear; enumeration path). Pure collection management, no Outlook. Acceptance: previously-uncovered registry branches exercised; positive/negative/edge methods present; tests pass.
- [x] [P2-T5] Add MSTest tests for `FilerQueue` pure paths (`QuickFiler/Controllers/FilerQueue.cs`) in a new test file under `QuickFiler.Test/` (e.g. `QuickFiler.Test/Controllers/FilerQueueTests.cs`): positive (enqueue then dequeue preserves order/contents), edge/state-transition (empty-queue dequeue/peek behavior; count after enqueue/dequeue sequence; state after clear), negative (invalid item per the method contract if applicable). Only pure queue-management paths; no Outlook/WinForms. Acceptance: positive/edge/negative methods present; tests pass.
- [x] [P2-T6] Add MSTest tests for `QfcQueue` pure paths (`QuickFiler/Controllers/QfcQueue.cs`) in a new test file under `QuickFiler.Test/` (e.g. `QuickFiler.Test/Controllers/QfcQueueTests.cs`): positive (enqueue/dequeue and pure queue-management operations), edge/state-transition (empty-queue behavior; count tracking; ordering invariants), negative (documented invalid-input behavior if applicable). Only pure queue-management paths; no Outlook/WinForms. Acceptance: positive/edge/negative methods present; tests pass.

### Phase 2 — Increment 2: QA Loop and Coverage

- [x] [P2-T7] Run csharpier formatting `dotnet tool run csharpier .`; if it changes files, restart from this task. Write `evidence/qa-gates/inc2-csharpier.2026-06-14T08-22.md` with the four required fields. Acceptance: final pass reports no formatting changes (exit 0).
- [x] [P2-T8] Run analyzers `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `evidence/qa-gates/inc2-analyzers.2026-06-14T08-22.md` with the four required fields. Acceptance: build succeeds with no analyzer errors.
- [x] [P2-T9] Run nullable/TWAE `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `evidence/qa-gates/inc2-nullable.2026-06-14T08-22.md` with the four required fields. Acceptance: build succeeds with no warnings-as-errors.
- [x] [P2-T10] Run the QuickFiler.Test suite with coverage via `vstest.console.exe <QuickFiler.Test assembly path> /EnableCodeCoverage` (or the pipeline script scoped to QuickFiler.Test). Write `evidence/qa-gates/inc2-mstest-coverage.2026-06-14T08-22.md` with the four required fields and numeric coverage headline. Acceptance: all new Increment 2 tests pass; artifact records pass count and QuickFiler coverage percent.
- [x] [P2-T11] Compute and record the Increment 2 covered-line delta in `evidence/qa-gates/inc2-coverage-delta.2026-06-14T08-22.md` reporting prior coverage, post-Increment-2 production-only coverage, the covered-line increase on the named QuickFiler seams, and new/changed-code coverage (target >= 90%). Acceptance: artifact shows the named seams' covered-line count increased, new-code coverage >= 90%, no regression on changed lines; otherwise mark remediation-required.

---

### Phase 3 — Increment 3: TaskMaster.Test (Batch A: AppStagingFilenames + MatchBestSpecialFolder + AppQuickFilerSettings)

- [x] [P3-T1] Add MSTest tests for `AppStagingFilenames` (`TaskMaster/AppGlobals/AppStagingFilenames.cs`) in a new test file under `TaskMaster.Test/` (e.g. `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs`) using the repo-established `Settings.Default` snapshot/restore pattern (capture affected `Settings.Default` values in `[TestInitialize]`, restore in `[TestCleanup]`) and the parameterless `new AppStagingFilenames()` constructor; do NOT introduce a production injection seam: positive (each property getter returns the value persisted in `Settings.Default`; setter round-trips through the backing field and `Settings.Default`), negative/edge (null/empty persisted value behavior per the property contract; the `EmailInfoStagingFile` setter which does not call `Save()`; the `InitProp` lazy-init path). Do not read the live filesystem or create temp files. Acceptance: positive/negative/edge methods present; `Settings.Default` snapshot/restore used so machine state is not mutated; tests pass.
- [x] [P3-T2] Add MSTest tests for `AppFileSystemFolderPaths.MatchBestSpecialFolder(string)` — FLAG-AND-STOP: method unreachable in isolation without filesystem mutation or a new production seam; gap recorded in evidence/other/matchbestspecialfolder-gap.2026-06-14T08-22.md, no production change made (`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`) in a new test file under `TaskMaster.Test/` (e.g. `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs`): positive (input path matching a known special folder returns that folder), edge (longest-prefix/best-match selection when multiple candidates partially match; case sensitivity per the method contract; trailing-separator normalization), negative (no-match input asserting the documented no-match result; null/empty path). Pure LINQ/string matching; no filesystem read. Acceptance: positive/edge/negative methods present; tests pass.
- [x] [P3-T3] Add MSTest tests for the remaining pure properties of `AppQuickFilerSettings` (`TaskMaster/AppGlobals/AppQuickFilerSettings.cs`) in the existing or a new test file under `TaskMaster.Test/AppGlobals/`, extending the established `Settings.Default` snapshot/restore pattern already used by `AppQuickFilerSettingsTests.cs` (no production injection seam): positive (get/set round-trips for `MoveEntireConversation`, `SaveAttachments`, `SavePictures`, `SaveEmailCopy` via the `internal` setters reachable through `InternalsVisibleTo("TaskMaster.Test")`), edge/negative (default values per each property contract). Do not read the live settings store via uncontrolled state; snapshot and restore every touched `Settings.Default` value. Acceptance: positive/edge/negative methods present for the previously-uncovered properties; `Settings.Default` snapshot/restore used; tests pass.

### Phase 3 — Increment 3: QA Loop and Coverage

- [x] [P3-T4] Run csharpier formatting `dotnet tool run csharpier .`; if it changes files, restart from this task. Write `evidence/qa-gates/inc3-csharpier.2026-06-14T08-22.md` with the four required fields. Acceptance: final pass reports no formatting changes (exit 0).
- [x] [P3-T5] Run analyzers `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `evidence/qa-gates/inc3-analyzers.2026-06-14T08-22.md` with the four required fields. Acceptance: build succeeds with no analyzer errors.
- [x] [P3-T6] Run nullable/TWAE `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `evidence/qa-gates/inc3-nullable.2026-06-14T08-22.md` with the four required fields. Acceptance: build succeeds with no warnings-as-errors.
- [x] [P3-T7] Run the TaskMaster.Test suite with coverage via `vstest.console.exe <TaskMaster.Test assembly path> /EnableCodeCoverage` (or the pipeline script scoped to TaskMaster.Test). Write `evidence/qa-gates/inc3-mstest-coverage.2026-06-14T08-22.md` with the four required fields and numeric coverage headline. Acceptance: all new Increment 3 tests pass; artifact records pass count and TaskMaster coverage percent.
- [x] [P3-T8] Compute and record the Increment 3 covered-line delta in `evidence/qa-gates/inc3-coverage-delta.2026-06-14T08-22.md` reporting prior coverage, post-Increment-3 production-only coverage, the covered-line increase on the named TaskMaster seams, and new/changed-code coverage (target >= 90%). Acceptance: artifact shows the named seams' covered-line count increased, new-code coverage >= 90%, no regression on changed lines; otherwise mark remediation-required.

---

### Phase 4 — Final QA Loop and Net Coverage Verification

- [x] [P4-T1] Run the full-solution csharpier check `dotnet tool run csharpier .` across the whole repository; if it changes any file, fix and restart this final loop from this task. Write `evidence/qa-gates/final-csharpier.2026-06-14T08-22.md` with the four required fields. Acceptance: single final pass reports no formatting changes (exit 0).
- [x] [P4-T2] Run analyzers + code style `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `evidence/qa-gates/final-analyzers.2026-06-14T08-22.md` with the four required fields. Acceptance: build succeeds with no analyzer errors in the final pass.
- [x] [P4-T3] Run nullable + warnings-as-errors `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `evidence/qa-gates/final-nullable.2026-06-14T08-22.md` with the four required fields. Acceptance: build succeeds with no warnings-as-errors in the final pass.
- [x] [P4-T4] Run the full MSTest suite across all three `.Test` projects with coverage via `vstest.console.exe <all three .Test assembly paths> /EnableCodeCoverage` (or `scripts/vscode/Invoke-MSTestWithCoverage.ps1`). Write `evidence/qa-gates/final-mstest-coverage.2026-06-14T08-22.md` with the four required fields and the numeric production-only coverage headline. Acceptance: all tests pass; artifact records total pass count and post-feature production-only coverage percent.
- [x] [P4-T5] Record the final net coverage comparison in `evidence/qa-gates/final-coverage-comparison.2026-06-14T08-22.md` reporting the post-#197 baseline (71.65%), the post-feature production-only coverage, the net change, and the aggregate new/changed-code coverage across all three increments (target >= 90%). Acceptance: artifact shows a measured net increase versus 71.65% and aggregate new-code coverage >= 90%; if no net increase or any required value is unavailable, the outcome is remediation-required (not PASS).
- [x] [P4-T6] Verify the no-production-change and exemption-boundary invariants by running `git diff --name-only` against the merge base and confirming only files under `ToDoModel.Test/`, `QuickFiler.Test/`, `TaskMaster.Test/`, and the feature `evidence/` folder changed; confirm no `[ExcludeFromCodeCoverage]` add/remove, and no edit to `coverage.config`, `TaskMaster.runsettings`, or pipeline scripts. Write `evidence/qa-gates/final-invariant-check.2026-06-14T08-22.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` and the changed-file list. Acceptance: artifact confirms zero production/config/pipeline changes; any production change is a flag-and-stop.
- [x] [P4-T7] Check off the spec and issue acceptance criteria as delivered/verified and record an issue-update mirror at `evidence/issue-updates/issue-199.2026-06-14T08-22.md` with `Timestamp:`, exact text, and `PostedAs:`. Acceptance: every spec AC maps to evidence (see AC-to-task map below) and the mirror artifact exists.

---

### Phase 5 — Authorized Seams to Close AC1/AC3 Gaps

Scope-change phase. The maintainer chose option B (remediation-inputs.2026-06-14T15-10.md)
and AUTHORIZED minimal production seams to close the two previously-deferred Flag-and-Stop
coverage gaps. Phases 0-4 remain complete and are not re-opened. The spec "zero
production change" Non-Goal is lifted ONLY for the two narrow seams in this phase.

Phase-5 hard constraints (in addition to the plan-wide Hard Constraints above):
- Production changes are limited to the two authorized seams: (a) `UtilitiesCS` assembly
  attribute `[assembly: InternalsVisibleTo("ToDoModel.Test")]` (plus a settable internal
  `MyBox` seam only if verification shows the existing `MyBox.DialogInvoker` is not test-
  settable); (b) the `TaskMaster` `MatchBestSpecialFolder` pure-helper extraction (or, as a
  fallback, an `internal` settable `SpecialFolders` seam / test-only constructor that skips
  `LoadFolders` plus `[assembly: InternalsVisibleTo("TaskMaster.Test")]`). `TaskMaster.Test`
  visibility is already present (`TaskMaster/ThisAddIn.cs`, `TaskMaster/Properties/AssemblyInfo.cs`).
- Any production change beyond these two seams is a flag-and-stop: halt the task, record the
  proposed change in `evidence/other/`, and stop for maintainer direction. Do not silently
  widen scope.
- No production behavior change. The `MyBox` dialog seam defaults to the real dialog in
  production (`MyBox.DialogInvoker` get returns `RealDialogInvoker` when unset). The
  `MatchBestSpecialFolder` matching semantics (substring `path.Contains`, longest-`Value`
  wins, returns `Key`, null on empty/no-match) must be byte-for-byte preserved.
- C# per-batch budget honored: Batch A and Batch B each touch at most 3 production files and
  at most 3 test files.
- Deterministic tests; MSTest + Moq + FluentAssertions; AAA; no temp files; no external
  deps; no live Outlook; no WinForms message loop. Tests inject the dialog stub via
  `MyBox.DialogInvoker` and never show a modal dialog.
- All evidence under `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/<kind>/`
  with ISO-8601 timestamps `2026-06-14T15-10`; each command-step artifact records
  `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; coverage artifacts record
  numeric headline values.

#### Phase 5 — Batch A: Gap 1 (AC1) — ProjectEntry dialog branches (UtilitiesCS seam + ToDoModel.Test)

- [x] [P5-T1] Verify the `MyBox.DialogInvoker` seam is a settable injection point usable from a test by reading `UtilitiesCS/Dialogs/MyBox.cs` and confirming the property `internal static Func<MyBoxViewer, DialogResult> DialogInvoker { get; set; }` exists, is settable, and defaults to the real dialog when unset. Record the finding in `evidence/baseline/p5-seam-verification-mybox.2026-06-14T15-10.md` with `Timestamp:`, `Command:` (the read/verification step), `EXIT_CODE:`, and `Output Summary:`. Acceptance: artifact confirms the settable internal `DialogInvoker` seam exists with file/line and that no new `MyBox` seam is required; if the seam is NOT test-settable, the artifact records that a minimal settable internal property is required and flags whether the change exceeds an assembly attribute + a single settable internal property (flag-and-stop if larger).
- [x] [P5-T2] Add `[assembly: InternalsVisibleTo("ToDoModel.Test")]` to `UtilitiesCS/Properties/AssemblyInfo.cs` immediately after the existing `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` line. This is the sole authorized `UtilitiesCS` production change (assembly attribute only). Acceptance: the attribute is present exactly once, `UtilitiesCS` still compiles, and no `MyBox` member visibility or behavior is otherwise changed; any need for a new `MyBox` seam beyond this attribute is flagged per P5-T1 and stopped.
- [x] [P5-T3] Add MSTest tests for the `ProjectEntry` dialog-dependent branches in `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs` (new file), injecting a deterministic `MyBox.DialogInvoker` stub in `[TestInitialize]` and restoring it in `[TestCleanup]` so no WinForms message loop runs. Cover: the `SetProjectId` malformed-ID validation branch (`newID.Length != 4` shows the error dialog and returns false; stub returns `DialogResult.OK`); the change-confirmation branch (`newID != ProjectID` routes through `ChangeId`; stub returns `DialogResult.Yes` confirming the change and `DialogResult.No` cancelling, asserting `ProjectID` updated vs unchanged); and the `CompareTo` length tie-break (equal ordinal prefix, shorter `ProjectID` returns -1, longer returns 1) using plain `ProjectEntry` instances with no dialog. Use Moq/lambda for the dialog stub and the `_idUpdate` action; FluentAssertions for assertions. Acceptance: malformed-ID, change-confirmation (Yes and No), and CompareTo length-tie-break methods present and passing; no real dialog shown; file builds.

#### Phase 5 — Batch B: Gap 2 (AC3) — MatchBestSpecialFolder seam (TaskMaster) + TaskMaster.Test

- [x] [P5-T4] Introduce the smallest seam for `AppFileSystemFolderPaths.MatchBestSpecialFolder` in `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` by extracting the pure matching logic into a static helper that takes the folder collection as a parameter (e.g. `internal static string MatchBestSpecialFolder(IReadOnlyDictionary<string, string> specialFolders, string path)`), and have the existing instance method `MatchBestSpecialFolder(string path)` delegate to it passing `SpecialFolders`. Preserve exact semantics: null/empty collection returns null; select entries where `path.Contains(value)`, order by descending `value.Length`, return the matched `Key` (or null when none). Do NOT add filesystem access. If the pure-helper extraction is not feasible without altering behavior, fall back to an `internal` settable `SpecialFolders` setter (or `internal` test-only constructor that skips `LoadFolders`) — `[assembly: InternalsVisibleTo("TaskMaster.Test")]` is already present in `TaskMaster/Properties/AssemblyInfo.cs` and `TaskMaster/ThisAddIn.cs`. Any change beyond one of these two options is flag-and-stop, recorded in `evidence/other/p5-matchbest-seam-deviation.2026-06-14T15-10.md`. Acceptance: the chosen seam compiles, the instance method still returns identical results for the same inputs, and the diff is limited to the single extraction/setter; the no-behavior-change invariant is documented in-code with a brief comment.
- [x] [P5-T5] Add MSTest tests for `AppFileSystemFolderPaths.MatchBestSpecialFolder` in `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs` (new file), driving the seam from P5-T4 with an in-memory folder collection (no filesystem access, no `LoadFolders`, no temp files): positive (a path containing a known special-folder value returns that folder's key); best-match/longest-prefix (two candidate values both contained in the path, the longer value's key wins); case sensitivity and trailing-separator behavior exactly as the method body defines (`string.Contains` is ordinal/case-sensitive; assert observed semantics, do not assume normalization); negative (no-match path returns null; null/empty collection returns null; null/empty input path per the method contract). Use FluentAssertions. Acceptance: positive, best-match, case/separator, and no-match/null methods present and passing; no filesystem access; file builds.

#### Phase 5 — Spec and AC re-pointing

- [x] [P5-T6] Update `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/spec.md` to lift the zero-production-change Non-Goal for ONLY the two authorized narrow seams: amend the "Any change to production code" Non-Goal (lines ~172-173) and the "No production behavior change" Invariant (lines ~37-38) with an explicit carve-out naming the two seams (UtilitiesCS `InternalsVisibleTo("ToDoModel.Test")` + optional `MyBox` settable seam; TaskMaster `MatchBestSpecialFolder` pure-helper extraction or `SpecialFolders` test seam), noting no runtime behavior change. Re-point AC1 (Increment 1 `ProjectEntry`) and AC3 (Increment 3 `MatchBestSpecialFolder`) and the Definition of Done line about the two Flag-and-Stop gaps (lines ~288-291) so they reference the Phase 5 task IDs and are marked fully delivered. Also correct the stale non-canonical evidence path `evidence/coverage/` (lines ~248, 302 context) to the canonical `evidence/qa-gates/`. Acceptance: spec carve-out names exactly the two seams, states no behavior change, AC1/AC3 and the DoD gap line reference Phase 5 tasks, and no `evidence/coverage/` path remains.

#### Phase 5 — QA Loop and Coverage (per-batch and final)

- [x] [P5-T7] After Batch A and Batch B, run csharpier formatting `dotnet tool run csharpier .`; if it changes files, fix and restart this QA loop from this task. Write `evidence/qa-gates/p5-csharpier.2026-06-14T15-10.md` with the four required fields. Acceptance: final pass reports no formatting changes (exit 0).
- [x] [P5-T8] Run analyzers `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `evidence/qa-gates/p5-analyzers.2026-06-14T15-10.md` with the four required fields. Acceptance: build succeeds with no analyzer errors.
- [x] [P5-T9] Run nullable/TWAE `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `evidence/qa-gates/p5-nullable.2026-06-14T15-10.md` with the four required fields. Acceptance: build succeeds with no warnings-as-errors.
- [x] [P5-T10] Run the `ToDoModel.Test` and `TaskMaster.Test` suites with coverage via `vstest.console.exe <ToDoModel.Test assembly path> <TaskMaster.Test assembly path> /EnableCodeCoverage` (or `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to those two projects). Write `evidence/qa-gates/p5-mstest-coverage.2026-06-14T15-10.md` with the four required fields and numeric coverage headline. Acceptance: all new Phase 5 tests pass; artifact records pass count and the ToDoModel and TaskMaster coverage percents.
- [x] [P5-T11] Compute and record the Phase 5 coverage delta in `evidence/qa-gates/p5-coverage-delta.2026-06-14T15-10.md` reporting the prior #199 production-only coverage (from `evidence/qa-gates/final-coverage-comparison.2026-06-14T08-22.md`), the post-Phase-5 production-only coverage, the covered-line increase on `ProjectEntry` (malformed-ID, ChangeId, CompareTo tie-break) and `AppFileSystemFolderPaths.MatchBestSpecialFolder` (plus the extracted helper), and the new/changed-code coverage percentage (target >= 90%) for the seams added in P5-T2/P5-T4. Acceptance: artifact shows coverage strictly increased vs the prior #199 state, new/changed-code coverage >= 90%, and no regression on changed lines; if new-code coverage < 90% or any value is unavailable, mark remediation-required (not PASS).
- [x] [P5-T12] Verify the production-change boundary by running `git diff --name-only` against the merge base and confirming the only production files changed are `UtilitiesCS/Properties/AssemblyInfo.cs` (assembly attribute) and `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` (seam), plus optionally `UtilitiesCS/Dialogs/MyBox.cs` only if P5-T1 required a settable seam; all other changes are confined to `ToDoModel.Test/`, `TaskMaster.Test/`, `spec.md`, the plan, and the feature `evidence/` folder. Confirm no `[ExcludeFromCodeCoverage]` add/remove and no edit to `coverage.config`, `TaskMaster.runsettings`, or pipeline scripts. Write `evidence/qa-gates/p5-invariant-check.2026-06-14T15-10.md` with the four required fields and the changed-file list. Acceptance: artifact confirms production changes are limited to the two authorized seams; any other production change is a flag-and-stop.
- [x] [P5-T13] Check off AC1 and AC3 (and the two-gap Definition of Done line) in `spec.md` as fully delivered and record an issue-update mirror at `evidence/issue-updates/issue-199.2026-06-14T15-10.md` with `Timestamp:`, the exact text, and `PostedAs:`. Acceptance: AC1 and AC3 map to Phase 5 evidence (see AC-to-task map below), the two Flag-and-Stop DoD items are resolved, and the mirror artifact exists.

---

## Acceptance Criteria to Task Map

- Spec AC "Increment 1 (ToDoModel)" / AC1 → P1-T1, P1-T2, P1-T3, P1-T4 (tests); P1-T8 (pass); P1-T9 (covered-line increase). Previously-deferred `ProjectEntry` dialog branches (malformed-ID, change-confirmation, CompareTo tie-break) now fully covered by Phase 5: P5-T2 (UtilitiesCS seam), P5-T3 (tests), P5-T10 (pass), P5-T11 (covered-line increase). AC1 fully PASS.
- Spec AC "Increment 2 (QuickFiler)" → P2-T1, P2-T2, P2-T3, P2-T4, P2-T5, P2-T6 (tests); P2-T10 (pass); P2-T11 (covered-line increase).
- Spec AC "Increment 3 (TaskMaster)" / AC3 → P3-T1, P3-T2, P3-T3 (tests); P3-T7 (pass); P3-T8 (covered-line increase). Previously-deferred `AppFileSystemFolderPaths.MatchBestSpecialFolder` now fully covered by Phase 5: P5-T4 (seam), P5-T5 (tests), P5-T10 (pass), P5-T11 (covered-line increase). AC3 fully PASS.
- Spec AC "All tests comply with General + C# Unit Test Policy (MSTest/Moq/FluentAssertions, AAA, deterministic, no temp files, no external deps, no live Outlook/WinForms, no timing/sleep; positive/negative/edge/error scenarios)" → Hard Constraints section + every test task P1-T1..P3-T3.
- Spec AC "New/changed code achieves >= 90% line coverage; no regression on changed lines" → P1-T9, P2-T11, P3-T8, P4-T5.
- Spec AC "No exempted COM/VSTO/WinForms code un-exempted or tested; no `[ExcludeFromCodeCoverage]` change; coverage.config/runsettings/pipeline unchanged" → P0-T6, P0-T7, P0-T8 (seam verification), P4-T6 (invariant check).
- Spec AC "No production behavior change; required new seam is flagged-and-stopped" → Flag-and-Stop Rule, P1-T3, P4-T6. Phase 5 supersedes the test-only constraint for ONLY the two maintainer-authorized seams (option B): no runtime behavior change is preserved (P5-T4 semantics preservation, P5-T12 boundary check); any change beyond the two seams remains flag-and-stop.
- Spec AC "Authorized seams close AC1/AC3 gaps; coverage strictly increases vs prior #199 state; no production behavior change" (Phase 5 cycle) → P5-T1, P5-T2, P5-T3 (Gap 1 / AC1); P5-T4, P5-T5 (Gap 2 / AC3); P5-T6 (spec re-point); P5-T7..P5-T9 (toolchain); P5-T10 (pass); P5-T11 (coverage delta, strict increase + >= 90% new code); P5-T12 (production-change boundary); P5-T13 (AC check-off + issue mirror).
- Spec AC "Full C# toolchain passes in a single final pass" → P4-T1, P4-T2, P4-T3, P4-T4.
- Spec AC "Production-only coverage re-measured and recorded showing net increase vs 71.65%" → P0-T5 (baseline), P4-T4, P4-T5.

## Notes

- Per-batch budget honored: Phase 1 splits into Batch A (2 files) and Batch B (2 files);
  Phase 2 into Batch A (3 files) and Batch B (3 files); Phase 3 is a single Batch A (3
  files). No batch exceeds 3 test files. Phase 5 Batch A touches 2 production files
  (`UtilitiesCS/Properties/AssemblyInfo.cs`; optional `UtilitiesCS/Dialogs/MyBox.cs`) and 1
  test file (`ProjectEntryDialogBranchesTests.cs`); Phase 5 Batch B touches 1 production file
  (`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`) and 1 test file
  (`AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs`). Each batch is within the
  <=3 production + <=3 test file budget.
- Phase 5 is a maintainer-authorized scope change (option B per
  `remediation-inputs.2026-06-14T15-10.md`). Phases 0-4 remain complete and are not
  re-opened. The "Zero production change" plan-wide Hard Constraint is overridden ONLY for
  the two narrow seams enumerated in Phase 5; all other production/config/pipeline files
  remain unchanged.
- Each phase that adds tests runs the full C# toolchain loop (csharpier -> analyzers ->
  nullable/TWAE -> MSTest with coverage) and includes fail-closed baseline/final/
  coverage-comparison evidence tasks.
- Exact test file names are illustrative; the executing engineer may place test methods in
  the most appropriate existing or new file per repo conventions, provided the per-batch
  file budget and the canonical evidence paths are honored.
