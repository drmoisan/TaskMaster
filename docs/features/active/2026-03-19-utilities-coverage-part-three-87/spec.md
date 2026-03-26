# 2026-03-19-utilities-coverage-part-three — Spec

- **Issue:** #87
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-23
- **Status:** In Progress
- **Version:** 0.2

## Overview

The UtilitiesCS library has 292 classes tracked by coverage tooling. Approximately 155 files have an explicit line-rate below 80% in the Cobertura report, plus ~16 `Designer.cs` auto-generated files, ~4 commented stubs, and ~40+ pure-interface files with no executable code. This gap means regressions in core shared code go undetected and the library cannot pass the repo-wide ≥80% coverage gate mandated by `general-unit-test.instructions.md`.

Previous feature work (issue #82, utilities-coverage-part-two) raised `OutlookObjects/Folder` to ≥80%. This third part extends coverage to every remaining production `.cs` file compiled by `UtilitiesCS.csproj`, preceded by a compliance and baseline-capture gate (Phase 0) and a reconciliation step that maps every sub-80 non-skip file to a specific implementation task or skip-evaluation task before any implementation resumes.

Research conducted on 2026-03-22 verified the actual public surfaces, behavioral seams, and UI/runtime coupling of all 89 ordered below-threshold files and confirmed where existing test homes can be extended instead of creating new files.


## Behavior

Add or extend MSTest unit tests in `UtilitiesCS.Test` so that every production `.cs` file compiled by `UtilitiesCS.csproj` reaches at least 80% line coverage, or is explicitly documented as a skip candidate with rationale. Tests must follow the repo's general and C#-specific unit test policies (MSTest + Moq + FluentAssertions, Arrange-Act-Assert, deterministic, no external dependencies, no temp files).

The work is organized into 90 phases:

- **Phase 0** — Compliance and baseline capture: read all policy files, capture baseline build/test/coverage state, produce a per-file coverage baseline, and run a reconciliation gate that maps every sub-80 non-skip file to an implementation or skip task before any Phase 1+ work resumes.

- **Phases 1–89** — File-by-file coverage uplift, ordered by a combination of research priority and the coverage inventory. Each implementation phase targets a single production file and includes test methods (in an existing or new test class) plus a csproj registration task. Each skip-evaluation phase documents the rationale for why the file is excluded.

  Implementation phases cover the following files (89 total in coverage inventory; 11 are skip-evaluation):
  - *Dialogs*: `FolderNotFoundViewer`, `InputBox`, `InputBoxViewer`, `MyBox`, `NotImplementedDialog`, `FunctionButton`, `MyBoxViewer`, `YesNoToAll`, `DelegateButton`
  - *EmailIntelligence*: `AutoFile`, `SortEmail`, `FilterOlFoldersController`, `FilterOlFoldersViewer`, `FolderInfoViewer`, `OSBrowser`, `FolderRemapController`, `FolderRemapViewer`, `FolderSelector`, `SubjectMapEncoder`, `SubjectMapMetrics`, `SubjectMapSco`, `EmailDataMiner`, `IntelligenceConfig`, `EmailFiler`, `FolderRemapTree`, `ClassifierGroupUtilities`, `PeopleScoDictionaryNew`, `SpamBayes`, `CorpusInherit`, `CategoryClassifierGroup`, `MulticlassEngine`, `Triage`, `OlFolderClassifierGroup`, `ActionableClassifierGroup`, `ManagerAsyncLazy`, `RecentsList`, `Triage_OlLogic`, `BayesianPerformanceMeasurement`, `ClassifierGroup (Obsolete)`
  - *Extensions*: `DfDeedle`, `DfMLNet`, `AsyncSerialization`, `WinFormsExtensions`
  - *HelperClasses*: `DvgForm`, `QfcTipsDetails`, `TipsController`, `OlvExtension`, `TableLayoutHelper`, `FileInfoWrapper`, `DirectoryInfoWrapper`, `FileSystemInfoWrapper`, `DispatchUtility`, `ThemeControlGroup`, `MouseDownFilter`, `FilePathHelper`
  - *ReusableTypeClasses*: `ConfigGroupBox`, `ConfigViewer`, `ConfigController`, `SCODictionary`, `ScBag`, `LockingObservableLinkedListNode`, `LockingObservableLinkedList`
  - *Threading*: `IdleActionQueue`, `IdleAsyncQueue`, `ProgressPane`, `ProgressViewer`, `AsyncMultiTasker`, `ProgressTrackerAsync`, `ProgressTrackerPane`, `ProgressTracker`, `ApplicationIdleTimer`, `UiThread`, `TimedDiskWriter`
  - *OutlookObjects*: `OlTableExtensions`, `StoreWrapperController`, `OlToDoTable`
  - *OneDriveHelpers*: `OneDriveDownloader`

  Skip-evaluation phases (10 files) with rationale:
  - **Phase 6** (`ConfusionViewer`) and **Phase 7** (`MetricChartViewer`): constructor-only WinForms designer shells with no meaningful non-designer logic.
  - **Phase 28** (`ProgressMultiStepViewer`): constructor-only progress form shell.
  - **Phase 31** (`ThreadMonitor`): relies on obsolete `Thread.Suspend`/`Thread.Resume` APIs and timing-sensitive diagnostics; deterministic unit tests are not feasible.
  - **Phase 33** (`FileIO2`): deprecated utility with direct file-system dependence and no injection seam; tests would require real disk I/O, violating the no-temp-files policy.
  - **Phase 35** (`ScreenHelper`): behavior depends on live machine monitor topology and active forms; static `Screen.AllScreens` has no injection seam.
  - **Phase 37** (`Theme`): broad UI/control graph and large mutable surface; unit coverage is low-value relative to the narrower `ThemeControlGroup` covered by Phase 60.
  - **Phase 58** (`ShellUtilities`) and **Phase 59** (`ShellUtilitiesStatic`): static Win32 shell interop and PInvoke icon extraction have no DI seam and are environment-dependent.
  - **Phase 79** (`SystemThemeDetector`): static registry reads have no DI seam; tests would couple to machine/user theme settings.

- **Phase 90** — Final QC: format (`csharpier`), analyzer build (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), nullable build (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`), test run with `/EnableCodeCoverage`, csproj registration audit, and coverage gate verification.


## Inputs / Outputs

- **Inputs:**
  - `coverage/coverage.cobertura.xml` — Cobertura XML from the most recent `Invoke-MSTestWithCoverage.ps1` run; used to identify files below the 80% line-rate gate and to measure uplift after each phase.
  - `UtilitiesCS/UtilitiesCS.csproj` — explicit `<Compile Include>` entries define the canonical set of production files that must be covered. Files not in the csproj (e.g., the orphaned `OutlookObjects/MailResolution.cs`) are excluded from scope.
  - `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — explicit `<Compile Include>` entries; every new test `.cs` file must be registered here or it will silently not compile.
  - Existing test files in `UtilitiesCS.Test/` — ~120+ files providing established mocking patterns, namespace conventions, and AAA scaffolding to extend.

- **Outputs:**
  - New and updated MSTest `.cs` files under `UtilitiesCS.Test/` (one test class per production file, namespace mirroring subfolder).
  - Updated `UtilitiesCS.Test.csproj` with `<Compile Include>` entries for every new test file.
  - Updated `coverage/coverage.cobertura.xml` after the final test-with-coverage run, showing all production files at ≥80% line-rate.
  - TRX test-result logs under `TestResults/` from `vstest.console.exe` runs.

- Config keys and defaults: None — no runtime configuration is introduced.
- Versioning or backward-compatibility constraints: No public API changes; test-only additions.

## API / CLI Surface

The only relevant commands are the QA toolchain commands run at the end of every phase and for the final Phase 90 QC pass:

- **Format**: `dotnet tool run csharpier .`
- **Analyzer build**: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- **Nullable/type-safety build**: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- **Test with coverage**: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

No new CLI commands or public APIs are introduced. This is a test-only change.

## Data & State

No new data sources, transformations, or persistence are introduced. This feature adds test-only code.

- Coverage state is read from `coverage/coverage.cobertura.xml` at baseline capture (Phase 0) and re-generated at the Phase 90 QC run.
- Evidence artifacts are written to subdirectories under `evidence/` within the feature folder:
  - `evidence/baseline/` — baseline build, test-coverage, per-file coverage, and reconciliation artifacts
  - `evidence/qa-gates/` — final QC pass artifacts (format, analyzer, nullable, test-coverage)

## Constraints & Risks

- `UtilitiesCS.Test` is an old-style explicit-include project: any new test file must be registered as a `<Compile Include="...">` entry in `UtilitiesCS.Test.csproj` or it silently fails to compile.
- Many classes have deep Outlook COM interop dependencies. All COM calls must be mocked via `Moq` (e.g., `Mock<Outlook.MailItem>`, `Mock<Outlook.MAPIFolder>`); no live Outlook profile is permitted.
- WinForms UI classes (dialogs, viewers, forms) must be instantiated on the test thread (STA context) and tested for state and event routing, not designer rendering.
- Static state in several classes (`NotImplementedDialog.StopAtNotImplemented`, `InputBoxViewer.DpiCalled`, idle queues) must be isolated and reset using `[TestInitialize]`/`[TestCleanup]` to prevent test pollution.
- File-system serialization must use `MemoryStream`/`StringWriter` injection; creation or use of temporary files is prohibited.
- `IApplicationGlobals` and loader dependencies in EmailIntelligence classifier groups must be satisfied via `Moq` interface mocks.
- Async tests must return `Task` (not `async void`) and must not rely on timing or `Thread.Sleep` for determinism.
- 11 files are proposed as skip-evaluation candidates (see Behavior section). Each is documented and checked off in the plan before the final QC pass. The skip list must not grow without new justification.
- The approximately 155 below-threshold files, 16 `Designer.cs` files, 4 stubs, and 40+ pure-interface files mean the total phase count is large; the plan sequences work by testability difficulty and existing test-home availability to minimize rework.
- Research confirmed that many exact test homes already exist in `UtilitiesCS.Test` and should be extended rather than duplicated.


## Implementation Strategy

### Phase Structure (90 phases)

Work is organized into atomic phases aligned with the plan (`plan.2026-03-22T21-00.md`). Phases are executed in order; Phase 0 must complete before any Phase 1+ work begins.

**Phase 0 — Compliance & Baseline Capture**
1. Read all repo policy files in the required order.
2. Capture baseline build state.
3. Capture baseline test results with coverage (`vstest.console.exe /EnableCodeCoverage`).
4. Record per-file baseline coverage for all UtilitiesCS files below 80%.
5. Reconcile every sub-80 non-skip file to an implementation or skip task (evidence artifact required).
6. Verify the revised plan checklist matches the reconciliation matrix before execution resumes.

**Phases 1–89 — File Coverage and Skip Evaluation**

Each implementation phase follows this structure:
- One or more `[TestMethod]`-annotated tests covering the declared acceptance criteria for the target file.
- Tests extend an existing test file where one is identified; a new test file is created only when no adjacent home exists.
- A registration task that verifies `<Compile Include="..." />` is present in `UtilitiesCS.Test.csproj`.

Skip-evaluation phases check off a documented rationale item. The 11 skip-evaluation phases are P6, P7, P28, P31, P32, P33, P35, P37, P58, P59, and P79 (see Behavior section for per-file rationale).

**Phase 90 — Final QC Pass**
1. Run `dotnet tool run csharpier .` — no formatting changes.
2. Run analyzer build — zero diagnostics.
3. Run nullable/type-safety build — zero warnings treated as errors.
4. Run `vstest.console.exe /EnableCodeCoverage` — all tests pass; UtilitiesCS line coverage ≥ 80%.
5. Confirm each non-skipped phase has a `<Compile Include="..." />` present in `UtilitiesCS.Test.csproj`.
6. Verify coverage meets or exceeds the 80% threshold; record follow-up note if any file remains below.

### Seam Patterns for COM / WinForms Mocking

- **COM interop (Outlook):** Use `Moq` to mock `Microsoft.Office.Interop.Outlook` interfaces (e.g., `Mock<Outlook.MailItem>`, `Mock<Outlook.MAPIFolder>`). Follow existing patterns in `OutlookItemTests`, `FolderWrapperStateTests`.
- **WinForms UI (dialogs, forms, viewers):** Test state mutations and event routing. Instantiate forms/controls on the test thread. Do not test designer rendering.
- **File-system serialization:** Use `MemoryStream`/`StringWriter` injection; never create temp files per repo policy.
- **IApplicationGlobals and loader dependencies:** Mock via `Moq` interface mock to isolate EmailIntelligence classifier groups from the full application context.
- **Static state:** Use `[TestInitialize]`/`[TestCleanup]` to save and restore static flags (e.g., `NotImplementedDialog.StopAtNotImplemented`, `InputBoxViewer.DpiCalled`, idle queue event handlers).
- **Async tests:** Return `Task`; use `TaskCompletionSource`-based fakes rather than `Thread.Sleep` for async delegate verification.

### Explicit csproj Registration Requirement

Every new test `.cs` file **must** be added as a `<Compile Include="...">` entry in `UtilitiesCS.Test.csproj`. The project is old-style explicit-include; files not registered silently fail to compile.

### Extending vs. Creating Test Files

The research scan confirmed many exact test homes already exist. Rule:
1. **Extend** the existing test file when an exact or adjacent test class is confirmed in `UtilitiesCS.Test`.
2. **Create** a new test file only when no adjacent home is available.

Known existing homes include (non-exhaustive):
- `UtilitiesCS.Test\Dialogs\DelegateButton_Tests.cs`, `FunctionButton_Tests.cs`, `InputBox_Test.cs`, `YesNoToAll_Tests.cs`
- `UtilitiesCS.Test\Extensions\AsyncSerialization_Tests.cs`, `WinFormsExtensions_Tests.cs`
- `UtilitiesCS.Test\HelperClasses\FilePathHelper_Tests.cs`, `TimedDiskWriterTests.cs`, `WindowsForms\ScreenAndTableLayoutTests.cs`
- `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedList_Tests.cs`, `LockingObservableLinkedListNode_Tests.cs`
- `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTable_Tests.cs`
- `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs`
- `UtilitiesCS.Test\Threading\UiThread_Tests.cs`, `ProgressTracker_Tests.cs`, `ApplicationIdleTimer_Tests.cs`

- Dependency changes: None. All required test packages (MSTest, Moq, FluentAssertions) are already present.
- Logging/telemetry additions: None.
- Rollout plan: Each phase is independently executable and verifiable. The final toolchain loop runs only at Phase 90.

## Definition of Done

- [ ] Every `.cs` file compiled by `UtilitiesCS.csproj` reaches ≥80% line coverage as reported by the Cobertura XML, or is explicitly documented as a skip candidate (with rationale) in the plan
- [x] All 11 skip-evaluation phases (P6, P7, P28, P31, P32, P33, P35, P37, P58, P59, P79) are checked off in the plan with documented rationale
- [x] No pre-existing tests are broken or removed
- [x] All new tests follow MSTest + Moq + FluentAssertions conventions (AAA pattern, deterministic, isolated, no external dependencies, no temp files)
- [x] All new test files are registered in `UtilitiesCS.Test.csproj` via `<Compile Include="...">` and verified in Phase 90-T5
- [x] Repository-wide line coverage does not regress below the Phase 0 baseline
- [x] C# toolchain loop passes clean in a single Phase 90 pass: `dotnet tool run csharpier .` → analyzer build → nullable build → `vstest.console.exe /EnableCodeCoverage`
- [x] Phase 0 evidence artifacts exist: `evidence/baseline/phase0-instructions-read.md`, `baseline-build.md`, `baseline-test-coverage.md`, `baseline-per-file-coverage.md`, `remaining-sub80-reconciliation.md`
- [x] Phase 90 QA evidence artifacts exist: `evidence/qa-gates/final-qc-format.md`, `final-qc-analyzers.md`, `final-qc-nullable.md`, `final-qc-test-coverage.md`
- [ ] Docs updated (feature folder status set to Complete; plan updated to show all tasks checked)

## Seeded Test Conditions (from potential)
- [ ] Positive and negative flows for each Dialogs file (button state, action routing, null/cancel paths)
- [ ] Encode/decode round-trips for SubjectMapEncoder and SubjectMapSco
- [ ] Chunk-size and ordering assertions for AsyncMultiTasker and EmailDataMiner
- [ ] COM interop mock verification for OlTableExtensions, OlToDoTable, StoreWrapperController
- [ ] Progress and cancellation wiring for ProgressTracker, ProgressTrackerAsync, ProgressTrackerPane, ProgressPane, ProgressViewer
- [ ] Event-routing and static-state isolation for IdleActionQueue, IdleAsyncQueue, ApplicationIdleTimer
- [ ] File-system wrapper property forwarding and null-inner handling for FileInfoWrapper, DirectoryInfoWrapper, FileSystemInfoWrapper
- [ ] Classifier-group creation, validation, and fallback paths for SpamBayes, CategoryClassifierGroup, OlFolderClassifierGroup, ActionableClassifierGroup, MulticlassEngine, Triage
