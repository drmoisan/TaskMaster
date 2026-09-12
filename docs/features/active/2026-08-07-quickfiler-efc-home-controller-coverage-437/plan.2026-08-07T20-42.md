# quickfiler-efc-home-controller-coverage — Plan

- **Issue:** #437
- **Parent epic:** #136 `quickfiler-per-file-coverage` (child F8, wave 1, band C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T20-42
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** `full-feature` — acceptance criteria are authoritative in **both** `spec.md` and
  `user-story.md` (AC1-AC15, identical text). `issue.md` is context only.

## Required References

- Policy order (`policy-compliance-order`): `CLAUDE.md` → `.claude/rules/general-code-change.md` →
  `.claude/rules/general-unit-test.md` → `.claude/rules/csharp.md`
- Requirements: `docs/features/active/2026-08-07-quickfiler-efc-home-controller-coverage-437/spec.md`,
  `.../user-story.md`
- Research (six per-file artifacts): `.../research/EfcHomeController.research.md`,
  `.../research/EfcHomeController.ExecuteMoves.research.md`,
  `.../research/EfcHomeController.Metrics.research.md`,
  `.../research/EfcHomeController.Timing.research.md`,
  `.../research/EfcHomeControllerDependencies.research.md`,
  `.../research/EfcHomeControllerDependencyFactories.research.md`
- Epic: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (Shared Design, F8, F9)

**All work must comply with these policies; do not duplicate their content here.**

## Path Conventions

- `<FEATURE>` = `docs/features/active/2026-08-07-quickfiler-efc-home-controller-coverage-437`
- `<PROD>` = `QuickFiler/Controllers`
- `<TEST>` = `QuickFiler.Test/Controllers`
- Evidence roots (non-overridable): `<FEATURE>/evidence/baseline/`, `<FEATURE>/evidence/qa-gates/`,
  `<FEATURE>/evidence/regression-testing/`. `artifacts/baselines/`, `artifacts/qa/`,
  `artifacts/qa-gates/`, `artifacts/coverage/`, `artifacts/evidence/` are rejected.
- Every command-step artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Baseline and final-QC test artifacts record **numeric** per-file line-rate and branch-rate.

## Standing Safety Constraints (apply to every task in this plan)

- **Modal-popup hazard.** `MoveFailureMessageAction` defaults to `text => MessageBox.Show(text)`.
  Every test that can reach `result == false` MUST assign a recording no-op to that seam. A modal
  popup hangs CI. This is restated in each affected task.
- **Parallelization hazard.** `scripts/vscode/TaskMaster.cli.runsettings` sets `<Scope>ClassLevel</Scope>`
  with `<Workers>0</Workers>`, so test **classes** run in parallel. Every new or modified test class
  that mutates the 16 `Production*` statics or `_defaultDependenciesFactory` MUST carry
  `[DoNotParallelize]` and a `[TestCleanup]` restoring state.
- **Never invoke** the `EfcViewerQueue.Dequeue`, `EfcDataModel.CreateAsync`, `FileIO2.WriteTextFile`,
  or `Production*Initializer` defaults. Assert identity via `.Method.Name` only.
- No live forms, no popups, no live Outlook store, no temp files, no external services, no
  `Thread.Sleep`, no `Task.Delay`, no real wall-clock waits. Suspension uses `TaskCompletionSource`.
- MSTest + Moq + FluentAssertions, Arrange-Act-Assert.
- **Do not** modify `coverage.config` or any shared build property file.
- **Do not** edit sibling-owned files: F9 (`EfcFormController.cs`, `EfcItemController.cs`,
  `EfcViewer.cs`) and F6 (`QfcExplorerController.cs`). Read-only construction inside a test is
  permitted.
- 500-line ceiling applies to **production and test** files. `QuickFiler.Test` is a legacy
  packages.config project: every new `.cs` needs an explicit `<Compile Include=...>` entry in
  `QuickFiler.Test/QuickFiler.Test.csproj` or it will not compile.
- **Do not fix** the documented latent defects (inert `_stopWatch`, `.Seconds` vs `.TotalSeconds`,
  non-atomic check-then-set, missing CSV separator, partial `xComma` sanitization,
  `NotImplementedException` overload, binding-time asymmetry). Where an existing assertion pins a
  defect (the `"RecipientSender"` concatenation in
  `EfcHomeControllerMetricsTests.BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`),
  preserve that assertion verbatim.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Reads

- [ ] [P0-T1] Read the four policy documents in required order and record the read
  - Order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`
  - Acceptance: `<FEATURE>/evidence/baseline/phase0-instructions-read.md` exists containing `Timestamp:`, `Policy Order:`, and the explicit four-file list
- [ ] [P0-T2] Verify the C# toolchain bootstrap before any formatter or coverage command runs
  - Confirm the .NET SDK resolves, run `dotnet tool restore` at repo root, and confirm `csharpier` and `dotnet-coverage` are runnable
  - Acceptance: `<FEATURE>/evidence/baseline/toolchain-bootstrap.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with the resolved csharpier and dotnet-coverage versions
- [ ] [P0-T3] Confirm F1's per-file coverage harness and ledger are present on the branch, or HALT
  - Verify `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` exists and locate F1's per-file coverage report harness script; record its exact repo-relative path
  - If either is absent, write the artifact with `EXIT_CODE: 1`, mark the plan BLOCKED, report blocked to the caller, and STOP. Do not improvise a substitute harness.
  - Acceptance: `<FEATURE>/evidence/baseline/f1-harness-presence.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, the ledger path, the harness path, and the F8 classification of all six files
- [ ] [P0-T4] Capture the baseline formatter state
  - Command: `dotnet tool run csharpier check .`
  - Acceptance: `<FEATURE>/evidence/baseline/csharpier-baseline.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`
- [ ] [P0-T5] Capture the baseline analyzer build state
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: `<FEATURE>/evidence/baseline/msbuild-analyzers-baseline.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with warning and error counts
- [ ] [P0-T6] Capture the baseline nullable/type-check build state
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: `<FEATURE>/evidence/baseline/msbuild-nullable-baseline.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`
- [ ] [P0-T7] Capture the coverage-enabled baseline test run and the per-file numbers for the six F8 files
  - Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-efc-home-controller-coverage-437\evidence\baseline\coverage-baseline.cobertura.xml`
  - Then run F1's per-file harness (path from task P0-T3) over that Cobertura output
  - Acceptance: `<FEATURE>/evidence/baseline/coverage-baseline.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, total pass/fail counts, and a table with **numeric** line-rate and branch-rate for `EfcHomeController.cs`, `EfcHomeController.ExecuteMoves.cs`, `EfcHomeController.Metrics.cs`, `EfcHomeController.Timing.cs`, `EfcHomeControllerDependencies.cs`, `EfcHomeControllerDependencyFactories.cs`. No placeholders.
- [ ] [P0-T8] Record the indicative sibling-branch baseline for comparison only
  - Values: `EfcHomeController.cs` 0.968481/0.890625; `ExecuteMoves.cs` 0.931624/0.833333; `Metrics.cs` 0.975904/0.916667; `Timing.cs` 1.0/0.666667; `DependencyFactories.cs` 0.957895/1.0; `Dependencies.cs` 0.94431/0.93617
  - Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
  - Acceptance: `<FEATURE>/evidence/baseline/indicative-baseline-424.md` records the table, the source path, and an explicit statement that these figures are INDICATIVE ONLY and that the task P0-T7 output is authoritative
- [ ] [P0-T9] Record the file-size baseline for every production and test file in scope
  - Six production files under `<PROD>` plus the seven `<TEST>/EfcHomeController*.cs` files
  - Acceptance: `<FEATURE>/evidence/baseline/file-size-baseline.md` lists each path with its current line count and remaining headroom against 500

### Phase 1 — Shared Test Infrastructure Extraction

- [ ] [P1-T1] Create the shared reflection test-support class
  - New file `<TEST>/EfcHomeControllerTestSupport.cs` containing `internal static class EfcHomeControllerTestSupport` with `SetPrivateField(object target, string fieldName, object value)` (using `BindingFlags.NonPublic | BindingFlags.Instance`), `CreateUninitialized<T>()` wrapping `FormatterServices.GetUninitializedObject`, and `InvokePrivateStatic(Type owner, string methodName, params object[] args)` (using `BindingFlags.NonPublic | BindingFlags.Static`), which task P2-T6 requires to reach `EfcHomeController.CreateDefaultDependencies`
  - Acceptance: file exists, is under 500 lines, and contains no `[TestClass]`
- [ ] [P1-T2] Wire `<TEST>/EfcHomeControllerTestSupport.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Controllers\EfcHomeControllerTestSupport.cs" />` entry exists alongside the other `Controllers\EfcHomeController*` entries
- [ ] [P1-T3] Create the shared fake-globals test-support class
  - New file `<TEST>/EfcHomeControllerTestFakes.cs` containing the `FakeApplicationGlobals`, `FakeFileSystemFolderPaths`, and staging-filenames fakes currently triplicated across `EfcHomeControllerMetricsTests.cs`, `EfcHomeControllerLifecycleTests.cs`, and `EfcHomeControllerTests.cs`
  - The three copies are not identical and the shared types must be a superset: preserve both a parameterless constructor (used by `<TEST>/EfcHomeControllerTests.cs` L23) and the `IFileSystemFolderPaths` constructor, and preserve the `SpecialFoldersAccessCount` counter from `<TEST>/EfcHomeControllerLifecycleTests.cs` L405-423 that L217 asserts as `Be(2)`. Dropping either silently weakens an existing assertion
  - Acceptance: file exists, is under 500 lines, contains no `[TestClass]`, and reads no disk path
- [ ] [P1-T4] Wire `<TEST>/EfcHomeControllerTestFakes.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Controllers\EfcHomeControllerTestFakes.cs" />` entry exists
- [ ] [P1-T5] Repoint `<TEST>/EfcHomeControllerMetricsTests.cs` at the shared helpers and delete its local duplicates
  - Acceptance: the file no longer declares its own reflection helper or fake-globals types; existing assertions (including the `"RecipientSender"` pin) are unchanged; the file's line count decreases
- [ ] [P1-T6] Repoint `<TEST>/EfcHomeControllerTests.cs` at the shared helpers and delete its local `SetField` and fake types
  - Acceptance: no local reflection helper or fake type remains; no existing assertion text changes
- [ ] [P1-T7] Repoint `<TEST>/EfcHomeControllerLifecycleTests.cs` at the shared helpers and delete its local fake types
  - Acceptance: no local fake type remains; the file's line count decreases from its task P0-T9 value
- [ ] [P1-T8] Repoint `<TEST>/EfcHomeControllerExecuteMovesTests.cs` at the shared `SetPrivateField` and delete its local copy
  - Acceptance: the private `SetPrivateField` declaration is removed and all call sites resolve to `EfcHomeControllerTestSupport`
- [ ] [P1-T9] Add `[DoNotParallelize]` to the existing `EfcHomeControllerDependenciesTestsProductionFactory` class
  - File `<TEST>/EfcHomeControllerDependenciesProductionFactoryTests.cs`, class declaration at line 17; its `[TestCleanup]` already calls `ResetProductionFactoriesForTesting()`
  - Acceptance: the attribute is present on the class and the file remains under 500 lines (AC11)
- [ ] [P1-T10] Add `[DoNotParallelize]` to the existing `EfcHomeControllerLifecycleTests` class
  - File `<TEST>/EfcHomeControllerLifecycleTests.cs`, class declaration at line 20; the class mutates `EfcHomeController._defaultDependenciesFactory` via `SetDefaultDependenciesFactory` at lines 48 and 82, and its `[TestCleanup]` at lines 22-26 already calls `ResetDefaultDependenciesFactory()`
  - Rationale: task P2-T3 introduces `EfcHomeControllerStaticFactoryTests`, a second class mutating the same static. Under `<Scope>ClassLevel</Scope>` / `<Workers>0</Workers>` an unmarked mutator still runs in the parallel bucket alongside a `[DoNotParallelize]` class, so every mutator must be marked
  - Acceptance: the attribute is present on the class and the file remains under 500 lines (AC11)
- [ ] [P1-T11] Verify the refactor compiles and the EFC test surface is still green
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` then `& (& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe") QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings /TestCaseFilter:"FullyQualifiedName~EfcHomeController"`
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase1-scoped-run.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with pass/fail counts, plus the post-refactor line count of every file touched in this phase (all < 500)

### Phase 2 — EfcHomeController.cs Coverage and Reproducibility

- [ ] [P2-T1] Consolidate the two duplicate default-dependency lambdas into one shared static readonly default (AC8)
  - File `<PROD>/EfcHomeController.cs`: introduce `private static readonly Func<EfcHomeControllerDependencies> DefaultDependenciesFactory = () => new EfcHomeControllerDependencies();`, initialize `_defaultDependenciesFactory` (L24-25) from it, and change `ResetDefaultDependenciesFactory` (L37) to assign the same shared instance
  - Acceptance: exactly one default-lambda body remains in the file; behavior is unchanged; per-file coverage for the file is no longer order-dependent
- [ ] [P2-T2] Collapse `ParentCleanup` to an expression-bodied get-only property
  - File `<PROD>/EfcHomeController.cs` L286-290 → `internal System.Action ParentCleanup => _parentCleanup;`. Retain the `private System.Action _parentCleanup;` field declaration at L285 unchanged — it is still assigned at L64 and L100 and read at L349
  - Rationale: the `private set` has zero in-repo callers and is unreachable; removal shrinks the coverage denominator rather than excluding a line. `internal` member, no `IFilerHomeController` obligation, no F9 impact.
  - Acceptance: the setter is gone, the file still compiles, and the file's line count drops
- [ ] [P2-T3] Create the static-entry-point contract test class
  - New file `<TEST>/EfcHomeControllerStaticFactoryTests.cs` with `[TestClass]`, `[DoNotParallelize]`, and `[TestCleanup]` calling `EfcHomeController.ResetDefaultDependenciesFactory()`
  - Acceptance: the shell compiles with zero test methods and carries both attributes (AC11)
- [ ] [P2-T4] Wire `<TEST>/EfcHomeControllerStaticFactoryTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Controllers\EfcHomeControllerStaticFactoryTests.cs" />` entry exists
- [ ] [P2-T5] Add `PublicConstructor_WithInjectedDefaultFactory_DelegatesToInternalOverload` (G1)
  - File `<TEST>/EfcHomeControllerStaticFactoryTests.cs`. Seam: `EfcHomeController.SetDefaultDependenciesFactory(() => probeDependencies)`; probe bundle supplies `DataModelFactory`, `ViewerFactory` (uninitialized `EfcViewer`), `KeyboardHandlerFactory` (`Mock<IQfcKeyboardHandler>`), `ExplorerControllerFactory` (`Mock<IQfcExplorerController>`), `FormControllerWithDataFactory`
  - Act: `new EfcHomeController(fakeGlobals, () => { }, Mock.Of<MailItem>())` — closes the public 3-arg constructor at L47-52
  - Assert: `controller.DataModel` is the probe model and `controller.InitType == (QfEnums.InitTypeEnum.Sort | QfEnums.InitTypeEnum.SortConv)`
- [ ] [P2-T6] Add `ResetDefaultDependenciesFactory_RestoresSharedDefault` (G2)
  - File `<TEST>/EfcHomeControllerStaticFactoryTests.cs`. Set a sentinel factory, call `ResetDefaultDependenciesFactory()`, then invoke the private static `CreateDefaultDependencies` via `EfcHomeControllerTestSupport`
  - Assert: the result is a non-null `EfcHomeControllerDependencies` whose `DataModelFactory` is non-null, proving the single shared default is installed
- [ ] [P2-T7] Add `SetDefaultDependenciesFactory_WithNull_ThrowsArgumentNullException` (G3)
  - File `<TEST>/EfcHomeControllerStaticFactoryTests.cs`. Assert `Throw<ArgumentNullException>()` with `ParamName == "factory"`
- [ ] [P2-T8] Add `CreateAsync_WithNullGlobals_ThrowsArgumentNullException` (G9)
  - File `<TEST>/EfcHomeControllerStaticFactoryTests.cs`. Use the internal `CreateAsync(globals, parentCleanup, dependencies, mail)` overload with `globals: null`
  - Assert: `await act.Should().ThrowAsync<ArgumentNullException>()`. Assert the exception type only unless `ParamName` is first confirmed against `UtilitiesCS/Extensions/NullExtensions.cs`
- [ ] [P2-T9] Add `CreateAsync_WithNullParentCleanup_ThrowsArgumentNullException` (G9)
  - File `<TEST>/EfcHomeControllerStaticFactoryTests.cs`; same shape with `parentCleanup: null`
- [ ] [P2-T10] Add `CreateAsync_WithNullDependencies_ThrowsArgumentNullException` (G9)
  - File `<TEST>/EfcHomeControllerStaticFactoryTests.cs`; same shape with `dependencies: null`
- [ ] [P2-T11] Add `LoadFinderAsync_WithNullGlobals_ThrowsArgumentNullException` (G9)
  - File `<TEST>/EfcHomeControllerStaticFactoryTests.cs`; internal `LoadFinderAsync` overload, `globals: null`
- [ ] [P2-T12] Add `LoadFinderAsync_WithNullParentCleanup_ThrowsArgumentNullException` (G9)
  - File `<TEST>/EfcHomeControllerStaticFactoryTests.cs`; `parentCleanup: null`
- [ ] [P2-T13] Add `LoadFinderAsync_WithNullDependencies_ThrowsArgumentNullException` (G9)
  - File `<TEST>/EfcHomeControllerStaticFactoryTests.cs`; `dependencies: null`
- [ ] [P2-T14] Add `Constructor_WithNullDependencies_ThrowsArgumentNullException` (G9)
  - File `<TEST>/EfcHomeControllerStaticFactoryTests.cs`. Act: `new EfcHomeController(fakeGlobals, () => { }, (EfcHomeControllerDependencies)null)` — closes the `dependencies.ThrowIfNull()` contract at L61
- [ ] [P2-T15] Add `HandleSelectionChangedAsync_WithNullSelection_TakesDummyDataModelPath` (G4)
  - File `<TEST>/EfcHomeControllerTests.cs`. Build the controller from the internal 4-arg constructor with a probe bundle; act `await controller.HandleSelectionChangedAsync(globals, null, QfEnums.InitTypeEnum.Find)`
  - Assert: it completes without throwing and `AsyncDataModelFactory` was **not** invoked, proving `CaptureSelectionSnapshot(null)` produced an empty non-null list
- [ ] [P2-T16] Create the Finder-run and controller-state test class
  - New file `<TEST>/EfcHomeControllerRunStateTests.cs` with `[TestClass]` and a private probe dependency bundle built on `EfcHomeControllerTestSupport`/`EfcHomeControllerTestFakes`
  - Acceptance: the shell compiles with zero test methods; it does not mutate any `Production*` static
- [ ] [P2-T17] Wire `<TEST>/EfcHomeControllerRunStateTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Controllers\EfcHomeControllerRunStateTests.cs" />` entry exists
- [ ] [P2-T18] Add `Run_WithFindInitTypeAndNoMail_ShowsViewerThroughInjectedSeam` (G5)
  - File `<TEST>/EfcHomeControllerRunStateTests.cs`. Build via the internal `LoadFinderAsync` overload with an empty `SelectionLoader` so `InitType == Find` and `DataModel.Mail` is null; assign a capturing `ViewerShowAction` **and** a capturing `MessageBoxShowAction`
  - Assert: the viewer-show capture fired once and the message-box capture never fired — this both closes the `_dataModel?.Mail is null && InitType.HasFlag(Find)` arm and guarantees no popup can be raised
- [ ] [P2-T19] Add `RunAsync_WithFindInitTypeAndNoMail_ShowsViewerThroughInjectedSeam` (G5)
  - File `<TEST>/EfcHomeControllerRunStateTests.cs`. Same arrangement; assign `ViewerShowAsyncAction` returning `Task.CompletedTask` and a capturing `MessageBoxShowAction`
  - Assert: the async viewer-show capture fired once and the message-box capture never fired. No `Task.Delay` and no wall-clock wait.
- [ ] [P2-T20] Add `StopWatch_WithMailBearingConstruction_IsAllocated` (G6)
  - File `<TEST>/EfcHomeControllerRunStateTests.cs`. Construct through the internal 4-arg constructor with a mail-bearing data model
  - Assert: `controller.StopWatch.Should().NotBeNull()` — a state-transition assertion pinning that allocation happens only on the data-bearing path
- [ ] [P2-T21] Add `StopWatch_WithoutMail_IsNull` (G6)
  - File `<TEST>/EfcHomeControllerRunStateTests.cs`. Construct with a data model whose `Mail` is null
  - Assert: `controller.StopWatch.Should().BeNull()`
- [ ] [P2-T22] Add `UiSyncContext_PropagatesFormViewerContext` (G7)
  - File `<TEST>/EfcHomeControllerRunStateTests.cs`. Set a non-null `SynchronizationContext` on the uninitialized probe `EfcViewer` before returning it from `ViewerFactory`
  - Assert: `controller.UiSyncContext.Should().BeSameAs(thatContext)` — pins propagation, not a live context. No form is constructed or shown.
- [ ] [P2-T23] Extend `Cleanup_ClearsControllerFieldsAndInvokesParentCleanup` with a `ParentCleanup` getter assertion (G8)
  - File `<TEST>/EfcHomeControllerLifecycleTests.cs`. Before calling `Cleanup()`, assert `controller.ParentCleanup.Should().BeSameAs(injectedCleanup)`
  - Acceptance: the existing assertions are unchanged and the file remains under 500 lines
- [ ] [P2-T24] Add `Constructor_WithMail_InvokesFactoriesInOrder` (G10)
  - File `<TEST>/EfcHomeControllerSeamTests.cs`. Build via the internal 4-arg constructor with a mail-bearing data model and a call-recording probe bundle
  - Assert: `probe.Calls.Should().ContainInOrder("viewer", "keyboard", "explorer", "form-with-data")` and `"form-without-data"` was never recorded
- [ ] [P2-T25] Verify Phase 2 compiles, runs green, and respects the file-size ceiling
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` then the scoped vstest run from task P1-T11
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase2-scoped-run.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and the line counts of `<PROD>/EfcHomeController.cs` and every test file touched in this phase (all < 500)

### Phase 3 — EfcHomeController.Timing.cs Branch Coverage

- [ ] [P3-T1] Widen the four Timing helpers from `private static` to `internal static`
  - File `<PROD>/EfcHomeController.Timing.cs`: `DescribeSynchronizationContext` (L9), `DescribeStartupOverlapState` (L14), `BuildFirstSelectionTimingContext` (L19), `LogFirstSelectionTiming` (L27)
  - `QuickFiler/Properties/AssemblyInfo.cs` already declares `[assembly: InternalsVisibleTo("QuickFiler.Test")]`; this removes the need for reflection. Zero runtime behavior change, zero public-API change.
  - Acceptance: all four are `internal static` and the solution builds
- [ ] [P3-T2] Extract `BuildFirstSelectionTimingMessage` as a pure function
  - File `<PROD>/EfcHomeController.Timing.cs`: add `internal static string BuildFirstSelectionTimingMessage(string phase, IApplicationGlobals globals, int selectedItemCount, string details)` carrying the existing `detailSegment` (L34) and `phaseLabel` (L35-37) logic plus the interpolation, and reduce `LogFirstSelectionTiming` to `logger.Debug(BuildFirstSelectionTimingMessage(phase, globals, selectedItemCount, details));`
  - Acceptance: emitted message text is byte-identical to today's for the same inputs; no mutable static and no log appender is introduced; the file stays under 500 lines
- [ ] [P3-T3] Create the Timing test class
  - New file `<TEST>/EfcHomeControllerTimingTests.cs` with `[TestClass]`; no static mutation, so no `[DoNotParallelize]` is required
  - Acceptance: the shell compiles with zero test methods
- [ ] [P3-T4] Wire `<TEST>/EfcHomeControllerTimingTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Controllers\EfcHomeControllerTimingTests.cs" />` entry exists
- [ ] [P3-T5] Add `DescribeStartupOverlapState_WithNonNullEvents_ReturnsCorrelated` (T1)
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. Seam: `Mock<IApplicationGlobals>(MockBehavior.Loose)` with `SetupGet(g => g.Events).Returns(Mock.Of<TaskMaster.IAppEvents>())`
  - Assert: result equals `"correlated"` — this is the arm no test has ever executed
- [ ] [P3-T6] Add `DescribeStartupOverlapState_WithNullEvents_ReturnsUnknown` (T1)
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. `Mock<IApplicationGlobals>` with `Events` returning null; assert `"unknown"`
- [ ] [P3-T7] Add `DescribeStartupOverlapState_WithNullGlobals_ReturnsUnknown` (T1)
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. Pass `null` directly, exercising the `?.` arm distinctly from the `Events == null` arm; assert `"unknown"`
- [ ] [P3-T8] Add `DescribeSynchronizationContext_WithNullContext_ReturnsNullLiteral` (T2)
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. Pass `null` **explicitly** rather than reading ambient `SynchronizationContext.Current`; assert the result equals `"null"`
- [ ] [P3-T9] Add `DescribeSynchronizationContext_WithSuppliedContext_ReturnsFullTypeName` (T2)
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. Pass `new SynchronizationContext()` explicitly; assert the result equals `typeof(SynchronizationContext).FullName`. Never depend on `SynchronizationContext.Current`.
- [ ] [P3-T10] Add `BuildFirstSelectionTimingMessage_WithNullDetails_OmitsDetailSegment` (T3)
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. `details: null`; assert the message does not contain the ` | ` separator following the context block
- [ ] [P3-T11] Add `BuildFirstSelectionTimingMessage_WithWhitespaceDetails_OmitsDetailSegment` (T3)
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. `details: "   "`; same assertion. Together with task P3-T10 this closes the `string.IsNullOrWhiteSpace` true arm.
- [ ] [P3-T12] Add `BuildFirstSelectionTimingMessage_WithDetails_AppendsDetailSegment` (T3)
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. `details: "elapsedMs=42"`; assert the message ends with `" | elapsedMs=42"`. Never assert an exact `elapsedMs` produced by production code.
- [ ] [P3-T13] Add `BuildFirstSelectionTimingMessage_WithAlreadyPrefixedPhase_DoesNotDoublePrefix` (T3)
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. Phase starting with `"[First-selection timing]"`; assert the prefix appears exactly once
- [ ] [P3-T14] Add `BuildFirstSelectionTimingMessage_WithUnprefixedPhase_AddsPrefix` (T3)
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. Unprefixed phase; assert the message starts with `"[First-selection timing] "`
- [ ] [P3-T15] Add `LogFirstSelectionTiming_WithValidArguments_EmitsWithoutThrowing`
  - File `<TEST>/EfcHomeControllerTimingTests.cs`. Direct `internal static` call (no reflection); assert it does not throw, covering the reduced `logger.Debug(...)` statement
- [ ] [P3-T16] Migrate `BuildFirstSelectionTimingContext_WhenEventsUnavailable_ReportsUnknownOverlapState` from `<TEST>/EfcHomeControllerTests.cs` (L137-160) into `<TEST>/EfcHomeControllerTimingTests.cs`
  - Replace `Type.GetMethod("BuildFirstSelectionTimingContext", BindingFlags.NonPublic | BindingFlags.Static)` + `Invoke` with a direct `internal static` call
  - Determinism: keep the existing `Contains("selectedItemCount=2")`, `Contains("startupOverlapState=unknown")`, and `Contains("threadId=")` assertions verbatim — never assert an exact managed thread id
  - Acceptance: the test passes in its new home and no longer appears in `<TEST>/EfcHomeControllerTests.cs`
- [ ] [P3-T17] Migrate `LogFirstSelectionTiming_AcceptsUnprefixedPhaseWithoutThrowing` from `<TEST>/EfcHomeControllerTests.cs` (L162-189) into `<TEST>/EfcHomeControllerTimingTests.cs`
  - Same reflection-to-direct-`internal static`-call replacement for `LogFirstSelectionTiming`
  - Acceptance: the test passes in its new home; no reflection into any Timing member remains anywhere in `QuickFiler.Test`; `<TEST>/EfcHomeControllerTests.cs` line count decreases from its task P0-T9 value
- [ ] [P3-T18] Verify Phase 3 clears the branch floor for `EfcHomeController.Timing.cs`
  - Command: the coverage run from task P0-T7 with output `<FEATURE>/evidence/qa-gates/coverage-phase3.cobertura.xml`, then F1's per-file harness
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase3-timing-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, and the numeric branch-rate for `EfcHomeController.Timing.cs` at **>= 0.75** (AC2), plus the file's line count (< 500)

### Phase 4 — EfcHomeController.ExecuteMoves.cs Coverage

- [ ] [P4-T1] Create the ExecuteMoves async-state test class
  - New file `<TEST>/EfcHomeControllerExecuteMovesStateTests.cs` with `[TestClass]`, built on `EfcHomeControllerTestSupport`
  - Acceptance: the shell compiles with zero test methods
- [ ] [P4-T2] Wire `<TEST>/EfcHomeControllerExecuteMovesStateTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Controllers\EfcHomeControllerExecuteMovesStateTests.cs" />` entry exists
- [ ] [P4-T3] Add `ExecuteMovesAsync_WithSuccessfulMove_DrivesCoreAndResetsGuard` (T1a)
  - File `<TEST>/EfcHomeControllerExecuteMovesStateTests.cs`. Uninitialized controller; inject `_formController`, `_dataModel`, `_globals` via the shared helper; `MoveToFolderAsyncAction` returns `Task.FromResult(true)`; `MoveMetricsAction` records
  - **Safety: assign a recording no-op to `MoveFailureMessageAction`** even though this test expects success — its default is `MessageBox.Show`, which would hang CI
  - Assert: the move recorder fired once, the metrics recorder fired once, and `controller.TryBeginExecuteMoves()` returns `true` afterwards, proving the `finally` reset ran. Closes lines 39-41, 43-45 and the line-33 true arm.
- [ ] [P4-T4] Add `ExecuteMovesAsync_WhenMoveSeamFaults_ResetsGuardThroughFinally` (T1b)
  - File `<TEST>/EfcHomeControllerExecuteMovesStateTests.cs`. Same arrangement but `MoveToFolderAsyncAction` returns `Task.FromException<bool>(new InvalidOperationException("move failed"))` (a pre-faulted task, not a synchronous throw)
  - **Safety: `MoveFailureMessageAction` must be overridden with a recorder.**
  - Assert: `await act.Should().ThrowAsync<InvalidOperationException>()` then `controller.TryBeginExecuteMoves().Should().BeTrue("the finally block must reset _isExecuting even when the move seam faults")`. Assert the observable consequence, not the private field.
- [ ] [P4-T5] Add `ExecuteMovesCoreAsync_WhenGlobalsClearedDuringAwait_UsesPreAwaitCapture` (T3)
  - File `<TEST>/EfcHomeControllerExecuteMovesStateTests.cs`. `MoveToFolderAsyncAction` returns `tcs.Task` from a test-owned `TaskCompletionSource<bool>`; start the core without awaiting; set `_globals` to null via the shared helper; then `tcs.SetResult(true)` and await
  - **Safety: `MoveFailureMessageAction` overridden with a recorder.**
  - Assert: the metrics recorder received the **original** globals instance (`BeSameAs`) and nothing threw. No `Thread.Sleep`, no `Task.Delay`.
- [ ] [P4-T6] Add `ExecuteMovesCoreAsync_WhenFormOptionsMutateDuringAwait_UsesPreAwaitCapture` (T4)
  - File `<TEST>/EfcHomeControllerExecuteMovesStateTests.cs`. Same `TaskCompletionSource` arrangement; mutate `_formController.MoveConversation` and the router selection between start and completion
  - **Safety: `MoveFailureMessageAction` overridden with a recorder.**
  - Assert: the recorded move request and the metrics `selectedFolder` still carry the pre-await values
- [ ] [P4-T7] Add `HandleMoveResult_WithNoInjectedMetricsAction_FallsBackToQuickFileMetricsWrite` (T2)
  - File `<TEST>/EfcHomeControllerExecuteMovesTests.cs`. Uninitialized controller with `MoveMetricsAction` left null; build `IApplicationGlobals` → `FS` → `Filenames` with `SetupGet(f => f.EmailSession).Returns("session.csv")`; pass `movedItems: new List<MailItemHelper>()` so `Metrics.cs` line 18 short-circuits before touching the null `_stopWatch`
  - **Safety: `MoveFailureMessageAction` overridden with a recorder** (this method can reach the failure arm).
  - Assert: `names.VerifyGet(n => n.EmailSession, Times.Once)` and the call does not throw. Closes line 141 and the line-135 false arm.
- [ ] [P4-T8] Add `MoveToFolderAsync_WithNoInjectedAction_FallsBackToDataModel` (T5)
  - File `<TEST>/EfcHomeControllerExecuteMovesTests.cs`. Uninitialized controller; leave `MoveToFolderAsyncAction` null; set `_dataModel` to `FormatterServices.GetUninitializedObject(typeof(EfcDataModel))`
  - Safety rationale to record in the test comment: `EfcDataModel.MoveToFolderAsync` returns `false` on its first statement when `MailInfo` is null (an uninitialized model has a null `_conversationResolver`), so no COM object is touched, no store is opened, and no file is written
  - **Safety: assign a recording no-op to `MoveFailureMessageAction`** before the act. `MoveToFolderAsync` does not itself reach `HandleMoveResult`, so no popup is reachable, but the standing constraint and the task P8-T7 audit require the assignment in every test whose observed result is `false`
  - Assert: `result.Should().BeFalse()`. Closes the line-94 false arm.
- [ ] [P4-T9] Verify Phase 4 runs green and both ExecuteMoves test files respect the 500-line ceiling
  - Command: the scoped vstest run from task P1-T11
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase4-scoped-run.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and the line counts of `<TEST>/EfcHomeControllerExecuteMovesTests.cs` and `<TEST>/EfcHomeControllerExecuteMovesStateTests.cs` (both < 500, AC9)

### Phase 5 — EfcHomeController.Metrics.cs Coverage

- [ ] [P5-T1] Add `QuickFileMetricsWrite_WithNonEmptyMovedList_ForwardsStopwatchElapsedSeconds` (T1)
  - File `<TEST>/EfcHomeControllerMetricsTests.cs`. Reuse the existing `CreateController(specialFolders, writer)` (fixed `metricsNowFactory`, recording `metricsLineWriter`), then set the private `_stopWatch` field to `new Stopwatch()` via `EfcHomeControllerTestSupport.SetPrivateField` — the helper passes `mail: null`, so the field is otherwise null
  - Act: `controller.QuickFileMetrics_WRITE("metrics.csv", "Archive", moved)` with a single-element `List<MailItemHelper>`
  - Assert: the writer received exactly one call; `Filename == "metrics.csv"`; `FolderRoot == "C:/Users/Test/Documents"`; the emitted line contains `",0,0.00,"`, proving the value flowed through line 23 rather than the early return
  - Determinism: a never-started `Stopwatch` returns `TimeSpan.Zero` unconditionally — no timer, sleep, delay, or wall-clock read (AC5)
- [ ] [P5-T2] Add `BuildQuickFileMetricLines_WithThreeMovedItems_PreservesOrderAndSharedPrefix` (T2)
  - File `<TEST>/EfcHomeControllerMetricsTests.cs`. Call the `internal static` builder directly with a fixed `DateTime`, `elapsedSeconds: 120`, and three helpers with distinct subjects
  - Assert: three lines in input order (O1); identical `"07/04/2026,01:05,"` timestamp prefix on all three (O2); all three carry `",40,0.67,"` from `120 / 3 = 40` and `(40 / 60d).ToString("##0.00") == "0.67"` (O3)
  - Do not "correct" the pinned `"RecipientSender"` concatenation behavior anywhere in this file
- [ ] [P5-T3] Verify Phase 5 closes the single `Metrics.cs` gap
  - Command: the coverage run from task P0-T7 with output `<FEATURE>/evidence/qa-gates/coverage-phase5.cobertura.xml`, then F1's per-file harness
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase5-metrics-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, the numeric line-rate for `EfcHomeController.Metrics.cs`, and confirmation that line 23 is now hit

### Phase 6 — EfcHomeControllerDependencies.cs Coverage

- [ ] [P6-T1] Create the dependencies-selection test class
  - New file `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs` with `[TestClass]`, `[DoNotParallelize]`, and `[TestCleanup]` calling `EfcHomeControllerDependencies.ResetProductionFactoriesForTesting()` (AC11)
  - Acceptance: the shell compiles with zero test methods and carries both attributes
- [ ] [P6-T2] Wire `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Controllers\EfcHomeControllerDependenciesSelectionTests.cs" />` entry exists
- [ ] [P6-T3] Add `LoadSelection_WithNullGlobals_ThrowsArgumentNullException` (G-D1)
  - File `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs`. Act: `EfcHomeControllerDependencies.LoadSelection(null, null)`
  - Assert: `Throw<ArgumentNullException>().Where(e => e.ParamName == "globals")`. Closes L402-405.
- [ ] [P6-T4] Add `LoadSelection_WithMixedOutlookSelection_ReturnsOnlyMailItems` (G-D2)
  - File `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs`. Mock chain: `globals.SetupGet(x => x.Ol.App).Returns(app.Object)`; `app.Setup(a => a.ActiveExplorer()).Returns(explorer.Object)`; `explorer.Setup(e => e.Selection).Returns(selection.Object)`; `selection.Setup(s => s.Count).Returns(3)`; `selection.As<IEnumerable>().Setup(s => s.GetEnumerator()).Returns(new List<object> { mail1, new object(), mail2 }.GetEnumerator())`
  - Precedent: `<TEST>/QfcHomeControllerTests.cs` L44-47 and `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.TrainSelection.cs` L59-90
  - Assert: `result.Should().Equal(mail1, mail2)`, proving both the `x is MailItem` filter lambda (L420) and retention order. No COM object is created.
- [ ] [P6-T5] Add `LoadSelection_WithEmptySelection_ReturnsEmptyAndNeverEnumerates` (G-D3)
  - File `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs`. Same chain with `selection.Setup(s => s.Count).Returns(0)`
  - Assert: `result.Should().BeEmpty()` **and** `selection.As<IEnumerable>().Verify(s => s.GetEnumerator(), Times.Never)` — the `Verify` is what makes this a branch test rather than a duplicate of task P6-T4
- [ ] [P6-T6] Add `LoadSelection_WithSingleItemSelection_ReturnsThatItem` (G-D4)
  - File `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs`. `Count = 1`, one `Mock<MailItem>(MockBehavior.Loose)` in the enumerator
  - Assert: `result.Should().ContainSingle().Which.Should().BeSameAs(mail)` — the lower boundary of the `> 0` comparison
- [ ] [P6-T7] Add `MetricsNowFactory_Default_ReturnsCurrentTimeWithinBounds` (G-D5)
  - File `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs`. Capture `before`, invoke `new EfcHomeControllerDependencies().MetricsNowFactory()`, capture `after`
  - Assert: `value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after)` — a bounded-interval assertion requiring no sleep. Record in the test summary comment that the production line under test **is** the default clock adapter, so a bounded assertion is the only way to execute it.
- [ ] [P6-T8] Add `DataModelFactory_WhenProductionFactoryReplacedAfterConstruction_UsesReplacement` (G-D6)
  - File `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs`. Reset the statics, construct `new EfcHomeControllerDependencies()` **first**, then assign a sentinel to `ProductionDataModelFactory`, then invoke `deps.DataModelFactory(...)`
  - Assert: the sentinel's value is returned, pinning invocation-time binding for the six late-bound defaults
- [ ] [P6-T9] Add `DataModelFactory_WhenOverrideInjected_IgnoresLaterProductionFactoryReplacement` (G-D7)
  - File `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs`. Construct with an explicit `dataModelFactory`, swap the static afterwards, invoke
  - Assert: the injected delegate still wins, pinning the `??` precedence rule at L66
- [ ] [P6-T10] Add `AsyncDataModelFactory_WhenProductionFactoryReplacedAfterConstruction_KeepsOriginal`
  - File `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs`. Construct first, then swap `ProductionAsyncDataModelFactory`
  - Assert delegate identity via `.Method.Name` only — **never invoke** the default (`EfcDataModel.CreateAsync` starts a real async Outlook data load). This pins the eager-binding half of the asymmetry at L67.
- [ ] [P6-T11] Add `ViewerFactory_WhenProductionFactoryReplacedAfterConstruction_KeepsOriginal`
  - File `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs`. Same shape for `ProductionViewerFactory` (L68)
  - Assert identity via `.Method.Name` only — **never invoke** the default (`EfcViewerQueue.Dequeue` constructs a real `EfcViewer` form)
- [ ] [P6-T12] Add `DelegateProperties_ReturnTheSameInstanceOnRepeatedReads` (G-D8)
  - File `<TEST>/EfcHomeControllerDependenciesSelectionTests.cs`. For each of the eleven get-only delegate properties, assert `deps.X.Should().BeSameAs(deps.X)` — pins "resolved once in the constructor, never re-created"
- [ ] [P6-T13] Verify Phase 6 runs green and closes the `LoadSelection` gaps
  - Command: the coverage run from task P0-T7 with output `<FEATURE>/evidence/qa-gates/coverage-phase6.cobertura.xml`, then F1's per-file harness
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase6-dependencies-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, the numeric line-rate and branch-rate for `EfcHomeControllerDependencies.cs`, and the line count of the new test file (< 500)

### Phase 7 — EfcHomeControllerDependencyFactories.cs Coverage

- [ ] [P7-T1] Create the dependency-factories test class
  - New file `<TEST>/EfcHomeControllerDependencyFactoriesTests.cs` with `[TestClass]`, `[DoNotParallelize]`, and `[TestCleanup]` calling `EfcHomeControllerDependencies.ResetProductionFactoriesForTesting()` (AC11)
  - Acceptance: the shell compiles with zero test methods and carries both attributes
- [ ] [P7-T2] Wire `<TEST>/EfcHomeControllerDependencyFactoriesTests.cs` into `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a `<Compile Include="Controllers\EfcHomeControllerDependencyFactoriesTests.cs" />` entry exists
- [ ] [P7-T3] Add `ProductionExplorerControllerConstructor_AfterReset_CreatesQfcExplorerController` (G-F1)
  - File `<TEST>/EfcHomeControllerDependencyFactoriesTests.cs`. Reset the statics; build `globals.SetupGet(x => x.Ol.App).Returns(app.Object)` and `app.Setup(a => a.ActiveExplorer()).Returns(explorer.Object)`; obtain `homeController` via `EfcHomeControllerTestSupport.CreateUninitialized<EfcHomeController>()`
  - Act: `EfcHomeControllerDependencies.ProductionExplorerControllerConstructor(QfEnums.InitTypeEnum.Find, globals.Object, homeController)` — closes L149-156, the file's one uncovered method
  - Assert: `Should().BeOfType<QfcExplorerController>()` and `app.Verify(a => a.ActiveExplorer(), Times.Once)`. `QfcExplorerController.cs` is F6-owned and is constructed read-only; no F6 file is edited.
- [ ] [P7-T4] Add `ResetProductionFactoriesForTesting_AfterAllSixteenAreReplaced_RestoresEveryDefault` (G-F2)
  - File `<TEST>/EfcHomeControllerDependencyFactoriesTests.cs`. Assign a distinguishable sentinel to all 16 `Production*` statics, call `ResetProductionFactoriesForTesting()`, then assert restoration
  - Assert via `.Method.Name` identity only for the thirteen named-method defaults — `ProductionDataModelFactory` → `"CreateProductionDataModel"`, `ProductionDataModelConstructor` → `"CreateProductionDataModelInstance"`, `ProductionAsyncDataModelFactory` → `"CreateAsync"`, `ProductionViewerFactory` → `"Dequeue"`, and the same pattern for the remaining nine
  - The three lambda-valued defaults (`ProductionFormControllerWithDataInitializer` L80, `ProductionFormControllerWithoutDataInitializer` L92, `ProductionDataFieldsInitializer` L105) have compiler-generated `Method.Name` values that must not be asserted; assert their restoration with `Should().NotBeSameAs(sentinel)` reference inequality instead. Still never invoke them (AC10)
  - **Must not invoke** `ProductionViewerFactory`, `ProductionAsyncDataModelFactory`, or any `Production*Initializer` (AC10)
- [ ] [P7-T5] Add `CreateProductionFormControllerWithData_ReturnsInitializerResultAfterConstructor` (G-F3)
  - File `<TEST>/EfcHomeControllerDependencyFactoriesTests.cs`. Replace `ProductionFormControllerWithDataConstructor` to return instance `A` and record `"ctor"`; replace `ProductionFormControllerWithDataInitializer` to assert its argument is `A`, return instance `B`, and record `"init"`; invoke via `new EfcHomeControllerDependencies().FormControllerWithDataFactory(...)`
  - Assert: result `BeSameAs(B)` and `calls.Should().Equal("ctor", "init")`. Both statics are replaced, so no real `EfcFormController` method is called.
- [ ] [P7-T6] Add `CreateProductionFormControllerWithoutData_ReturnsInitializerResultAfterConstructor` (G-F4)
  - File `<TEST>/EfcHomeControllerDependencyFactoriesTests.cs`. Same shape for the without-data path (L249-257)
- [ ] [P7-T7] Add `CreateProductionDataFields_ReturnsInitializerResultNotInputController` (G-F5)
  - File `<TEST>/EfcHomeControllerDependencyFactoriesTests.cs`. Have `ProductionDataFieldsInitializer` return a **different** instance from its input
  - Assert: the returned value is the initializer's instance, not the input controller — the existing test returns the same object, so propagation is not currently proven
- [ ] [P7-T8] Add `CreateProductionDataModel_WhenConstructorReplacedAfterDelegateCapture_UsesReplacement` (G-F6)
  - File `<TEST>/EfcHomeControllerDependencyFactoriesTests.cs`. Capture the composition-layer delegate from `new EfcHomeControllerDependencies()`, then swap `ProductionDataModelConstructor`, then invoke
  - Assert: the swap took effect, pinning the two-layer seam's late-binding contract
- [ ] [P7-T9] Add `ProductionConstructors_InvokedTwice_ReturnDistinctInstances`
  - File `<TEST>/EfcHomeControllerDependencyFactoriesTests.cs`. Use replaced `*Constructor` statics returning fresh objects; invoke twice
  - Assert: the two results are not reference-equal, pinning the "no memoization" invariant against a future caching change
- [ ] [P7-T10] Record the CCN-1 residual as an accepted, unclosed gap (AC7)
  - The five one-statement initializer closure bodies at `<PROD>/EfcHomeControllerDependencyFactories.cs` lines 80, 92, 105, 125, 128 are pass-throughs into `[ExcludeFromCodeCoverage]`-marked F9 members and are **left uncovered by design**. Do not edit `EfcFormController.cs`. Do not add `[ExcludeFromCodeCoverage]` and do not touch `coverage.config`.
  - Acceptance: `<FEATURE>/evidence/qa-gates/ccn1-residual.md` records `Timestamp:`, the five line numbers, the F9 members each reaches, the reason each is unreachable under the unit-test policy, and the file's line-rate with the residual included
- [ ] [P7-T11] Verify Phase 7 runs green and closes the factories gaps
  - Command: the coverage run from task P0-T7 with output `<FEATURE>/evidence/qa-gates/coverage-phase7.cobertura.xml`, then F1's per-file harness
  - Acceptance: `<FEATURE>/evidence/qa-gates/phase7-factories-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, the numeric line-rate and branch-rate for `EfcHomeControllerDependencyFactories.cs`, and the new test file's line count (< 500)

### Phase 8 — Final QC Loop and Coverage Verification

The four command steps below are unconditional. If any step fails or changes files, fix and
**restart from task P8-T1**; the recorded artifacts must come from a single clean pass. `SKIPPED` is
not a valid outcome for any task in this phase.

- [ ] [P8-T1] Run the formatter and record the result
  - Commands: `dotnet tool run csharpier format .` then `dotnet tool run csharpier check QuickFiler QuickFiler.Test`
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-csharpier.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and whether `format` modified any file (if it did, restart at task P8-T1)
- [ ] [P8-T2] Run the analyzer build and record the result
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-msbuild-analyzers.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with warning/error counts compared against task P0-T5
- [ ] [P8-T3] Run the nullable/type-check build and record the result
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-msbuild-nullable.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` compared against task P0-T6
- [ ] [P8-T4] Run the coverage-enabled test suite and record numeric per-file coverage
  - Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-efc-home-controller-coverage-437\evidence\qa-gates\coverage-final.cobertura.xml`, then F1's per-file harness over that output
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-coverage.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with total pass/fail counts and **numeric** line-rate and branch-rate for all six F8 files. No placeholders.
- [ ] [P8-T5] Verify the per-file coverage deltas against the thresholds (AC1, AC2)
  - Build a table for all six files with baseline line-rate/branch-rate (from task P0-T7), post-change line-rate/branch-rate (from task P8-T4), and the delta
  - Assert: line-rate >= 0.80 for all six files; branch-rate >= 0.75 for `EfcHomeController.Timing.cs`; no file's line-rate is below its task P0-T7 baseline
  - Acceptance: `<FEATURE>/evidence/qa-gates/per-file-coverage-verification.md` contains the table, the pass/fail verdict per file, and an explicit statement that the numbers come from F1's harness on F8's branch (not from the `...-424` indicative figures)
- [ ] [P8-T6] Verify the 500-line ceiling for every production and test file in scope (AC9)
  - Files: the six under `<PROD>` plus every `<TEST>/EfcHomeController*.cs`
  - Acceptance: `<FEATURE>/evidence/qa-gates/file-size-verification.md` lists each path with its final line count and a PASS/FAIL against 500, compared to `<FEATURE>/evidence/baseline/file-size-baseline.md`
- [ ] [P8-T7] Verify the test-safety and parallelization invariants by inspection (AC10, AC11, AC12)
  - Confirm every new or modified test that can reach `result == false` assigns `MoveFailureMessageAction`; confirm `EfcViewerQueue.Dequeue`, `EfcDataModel.CreateAsync`, `FileIO2.WriteTextFile`, and the `Production*Initializer` defaults are never invoked; confirm every static-mutating class carries `[DoNotParallelize]` plus a restoring `[TestCleanup]`; confirm no `Thread.Sleep`, `Task.Delay`, temp file, live form, popup, or live Outlook store appears in any new test
  - Acceptance: `<FEATURE>/evidence/qa-gates/test-safety-audit.md` lists each check, the search performed, and the result
- [ ] [P8-T8] Verify no sibling-owned or shared build file was modified (AC14)
  - Command: `git diff --name-only origin/epic/quickfiler-per-file-coverage-integration...HEAD`
  - Assert: no entry for `QuickFiler/Controllers/EfcFormController.cs`, `QuickFiler/Controllers/EfcItemController.cs`, `QuickFiler/Viewers/EfcViewer.cs`, `QuickFiler/Controllers/QfcExplorerController.cs`, `coverage.config`, or any `*.props`/`*.targets`
  - Acceptance: `<FEATURE>/evidence/qa-gates/scope-boundary-verification.md` records the command, the full changed-file list, and the verdict
- [ ] [P8-T9] Promote the deferred items to GitHub issues and record their numbers (AC13)
  - Items: mid-batch cancellation in `ExecuteMovesAsync` (C3), and the seven latent defects — inert `_stopWatch`, `.Seconds` vs `.TotalSeconds`, non-atomic `TryBeginExecuteMoves` check-then-set, missing CSV field separator, inconsistent `xComma` sanitization, the `NotImplementedException` overload, and the eager-vs-invocation-time binding asymmetry
  - Use the MCP promotion lifecycle; do not fix any of them in this feature
  - Acceptance: `<FEATURE>/evidence/issue-updates/deferred-items-promotion.md` records `Timestamp:` and one issue number and URL per item; the numbers are written back into AC13 in `spec.md`
- [ ] [P8-T10] Check off AC1-AC15 in `<FEATURE>/spec.md`
  - Per the `acceptance-criteria-tracking` skill: mark each criterion only when its cited evidence artifact exists and supports it; leave unmet criteria unchecked with a stated reason
  - Acceptance: all fifteen checkboxes in `spec.md` carry a verdict and each cites its evidence artifact path
- [ ] [P8-T11] Check off AC1-AC15 in `<FEATURE>/user-story.md` in step with `spec.md`
  - Acceptance: the two files' AC states are identical; any divergence is a failure
- [ ] [P8-T12] Write the final status summary and confirm a clean single-pass toolchain run
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-status.md` records `Timestamp:`, the four command steps of the final pass with their `EXIT_CODE: 0` values, the AC1-AC15 status table, and an explicit statement that tasks P8-T1 through P8-T4 all passed within one uninterrupted loop

## Test Plan

- **Unit (MSTest + Moq + FluentAssertions, Arrange-Act-Assert):** 45 named test methods across
  Phases 2-7, one per atomic task, plus the two migrated Timing tests.
- **New test files:** `EfcHomeControllerTestSupport.cs`, `EfcHomeControllerTestFakes.cs`,
  `EfcHomeControllerStaticFactoryTests.cs`, `EfcHomeControllerRunStateTests.cs`,
  `EfcHomeControllerTimingTests.cs`, `EfcHomeControllerExecuteMovesStateTests.cs`,
  `EfcHomeControllerDependenciesSelectionTests.cs`, `EfcHomeControllerDependencyFactoriesTests.cs` —
  each requiring an explicit `<Compile Include=...>` entry in `QuickFiler.Test/QuickFiler.Test.csproj`.
- **Modified test files:** `EfcHomeControllerTests.cs`, `EfcHomeControllerMetricsTests.cs`,
  `EfcHomeControllerLifecycleTests.cs`, `EfcHomeControllerSeamTests.cs`,
  `EfcHomeControllerExecuteMovesTests.cs`, `EfcHomeControllerDependenciesProductionFactoryTests.cs`.
- **Integration / manual:** none. No live Outlook, no live form, no popup, no external service.
- **Coverage evidence:**
  - Baseline: `<FEATURE>/evidence/baseline/coverage-baseline.md` and
    `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml`
  - Indicative reference: `<FEATURE>/evidence/baseline/indicative-baseline-424.md`
  - Post-change: `<FEATURE>/evidence/qa-gates/final-coverage.md` and
    `<FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml`
  - Comparison: `<FEATURE>/evidence/qa-gates/per-file-coverage-verification.md`

## Open Questions / Notes

- **Upstream gate.** F1's per-file harness and
  `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` must exist on the branch
  before execution. Task P0-T3 is a hard halt gate: if either is absent, report BLOCKED and stop rather
  than substituting an aggregate-coverage measurement.
- **Accepted residual.** CCN-1's five initializer closure bodies stay uncovered by design
  (task P7-T10). Closing them would require an F9 design change that the epic's additive-only
  constraint bars.
- **Accepted residual.** The three host-bound default lambda bodies in `EfcHomeController.cs`
  (`ViewerShowAction`, `ViewerShowAsyncAction`, `MessageBoxShowAction`) are line-level irreducible
  items inside a testable file. No `[ExcludeFromCodeCoverage]` is added and `coverage.config` is not
  modified.
- **Informational coupling (CCN-2).** Task P7-T3 constructs the F6-owned `QfcExplorerController`
  read-only. If F6 changes that constructor's signature or removes the `ActiveExplorer()` call, the
  mock setup in that one test needs a one-line update.
- **Latent defects.** None are fixed here. Where an existing assertion pins a defect (the
  `"RecipientSender"` concatenation), the assertion is preserved rather than "corrected".
