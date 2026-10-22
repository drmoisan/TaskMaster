# Spec: Parallel test failures from wall-clock deadlines and shared user.config (Issue #1005)

- Issue: #1005
- Branch: bug/parallel-test-failures-wall-clock-and-shared-user-config-1005
- Work Mode: full-bug
- Last Updated: 2026-10-22
- Status: Draft
- Requirement source: `issue.md` `## Acceptance Criteria` (AC1 through AC7, maintainer-authored)
- Research: `research/2026-10-22T15-21-parallel-test-isolation-wall-clock-and-user-config-research.md` (feature-relative)

> **AC authority.** This item is in `full-bug` work mode. This `spec.md` is the sole authoritative acceptance-criteria source. AC1 through AC7 carry the meaning of the `issue.md` criteria unchanged, made more precise from the research; AC8 is added by the orchestrator. `user-story.md` exists only because the prd-feature stop hook requires it and contains no acceptance criteria.

## 1. Problem Statement

Two MSTest tests fail intermittently when the suite runs in parallel in Visual Studio Test Explorer:

1. `DfDeedle_COM_Tests.AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` calls `DfDeedle.AddQfcColumnsAsync(table, folder, token, 0)` without the existing `columnAdder` or `timeProvider` seam. The production retry loop then waits on three real 3000 ms deadlines for a `Task.Run` over Moq-backed COM mocks. Under parallel load the work misses the 9000 ms budget and the method throws `TimeoutException: The column add step timed out after 9000 ms for folder '(unknown folder)'`.
2. `TaskTreeControllerMoveLogicTests.MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` reaches `IDList.GetNextToDoID`, which writes `Properties.Settings.Default.MaxLengthOfID` and calls `Properties.Settings.Default.Save()`. All settings classes in one testhost share one host-derived `user.config`, so concurrent saves race and one fails with `ConfigurationErrorsException: Failed to save settings: The configuration file has been changed by another program`.

Both violate the unit-test isolation rules (UT1 independence and determinism; UT4 no mutable global state, no file I/O). Every test must pass when run in parallel.

## 2. Root Cause

- **DfDeedle (wall clock).** There is no production seam gap. `AddQfcColumnsAsync` (`UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`) already accepts `Action<object, object>? columnAdder` and `TimeProvider? timeProvider`. `TimeoutTask.TimeoutAfter` arms `(timeProvider ?? TimeProvider.System).CreateTimer(...)`, so with no provider the deadline is the real clock. The tests simply do not pass the seam. `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`) is already referenced by UtilitiesCS.Test and already used in `DfDeedle_COM_Tests`.
- **Settings persistence (shared file).** `ApplicationSettingsBase.Save()` persists through `LocalFileSettingsProvider` to a `user.config` whose location derives from the host identity, not from the settings class. Every production member that writes a setting and calls `Save()` with no injectable persistence seam therefore makes any test that reaches it write shared machine state. A fresh `IDList` has a cached max length of zero, so its first generated ID always saves.

The research enumerated every production `Save()` call on an `ApplicationSettingsBase` instance across the repository and traced each to its test callers (research section "Test-to-site reach map"). The sites reached by tests are:

| Production file | Members reached by tests |
| --- | --- |
| `ToDoModel/Data Model/ID/IDList.cs` | `GetNextToDoID` |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | `MoveEntireConversation`, `SaveAttachments`, `SavePictures`, `SaveEmailCopy`, `HighConfidenceModeEnabled`, `HighConfidenceThreshold` setters |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | `ConditionalReminders`, `CommonWords` setters |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | `LngConvCtPwr`, `Conversation_Weight`, `SmithWatterman_MatchScore`, `SmithWatterman_MismatchScore`, `SmithWatterman_GapPenalty`, `MaxRecents` setters |

In addition, test code calls `Save()` directly: `AppAutoFileObjectsCoverageExpansionTests.TestCleanup` invokes `defaults.Save()` after every test in that class.

Production `Save()` sites with no test reach (seven further AppStagingFilenames setters; AppOlObjects ApplyJunkFolderSelections, LoadJunkPotential, LoadJunkCertain, DarkMode) are not in the family and are not changed by this item (section 4).

## 3. Proposed Fix (Design)

### D1. DfDeedle tests (test-only)

- `AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing`: pass `timeProvider: new FakeTimeProvider()` and never advance it, keeping `columnAdder` null so the production default-adder branch stays exercised. Convert to an `async Task` test that awaits the call (no `GetAwaiter().GetResult()`). Assert completion without exception, and verify the mocked column-add call.
- `AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully`: pass `timeProvider: new FakeTimeProvider()` for uniformity (it matches the AC4 "without its seams" search, although it is not wall-clock bound in practice).
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` is already above the 500-line cap. Mark the class `partial` and move the `AddQfcColumnsAsync` section (both tests) into new `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs`, so the original file shrinks and does not grow. Add a `<Compile Include>` entry in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
- No production change to `DfDeedle`.

### D2. Settings-store seam (UtilitiesCS)

- New public interface `UtilitiesCS.ISettingsStore` in `UtilitiesCS/HelperClasses/Settings/ISettingsStore.cs`: `object this[string propertyName] { get; set; }` and `void Save();`. XML docs state that `Save` persists every pending value, mirroring `ApplicationSettingsBase`.
- New public sealed adapter `UtilitiesCS.ApplicationSettingsStore : ISettingsStore` in `UtilitiesCS/HelperClasses/Settings/ApplicationSettingsStore.cs`. The constructor takes an `ApplicationSettingsBase` and throws `ArgumentNullException` on null; the indexer forwards to `settings[propertyName]`; `Save()` forwards to `settings.Save()`.
- Both new files get `<Compile Include>` entries in `UtilitiesCS/UtilitiesCS.csproj`.
- `public` is the minimum visibility: UtilitiesCS grants no `InternalsVisibleTo` to ToDoModel, TaskMaster, or TaskTree.Test.

**Invariant.** Every consumer holds a non-null, per-instance `ISettingsStore`. Its production default is `new ApplicationSettingsStore(Properties.Settings.Default)` of that consumer's own assembly, which is behaviorally identical to the current code because generated settings setters are `this[name] = value`. Explicitly injected null is rejected at construction. No static mutable seam is introduced.

**Accept-to-persist trace (IDList).** A test constructs `new IDList(new List<string>(), store)` with a `Mock<ISettingsStore>` (non-null, accepted; null would throw `ArgumentNullException` at construction). `GetNextToDoID("0")` generates an ID longer than the cached zero length, assigns `store["MaxLengthOfID"] = newLength`, and calls `store.Save()` on the mock. Nothing reaches `ApplicationSettingsBase.Save()`, so no `ConfigurationErrorsException` can be thrown and `user.config` is not written. In production, the default adapter forwards the same assignment and `Save()` to `ToDoModel.Properties.Settings.Default`, which persists exactly as before. AC2 and AC3 pin this trace.

### D3. IDList (ToDoModel)

- In `ToDoModel/Data Model/ID/IDList.cs` add `private readonly ISettingsStore _settingsStore = new ApplicationSettingsStore(Properties.Settings.Default);` (a field initializer, so it runs on every existing constructor including the deserializing and Outlook-bound ones).
- Add public constructors `IDList(IEnumerable<string> ids, ISettingsStore settingsStore)` and `IDList(ISettingsStore settingsStore)`, each null-guarding the store.
- In `GetNextToDoID`, replace the two settings lines with an indexer assignment through `nameof(Properties.Settings.MaxLengthOfID)` and `_settingsStore.Save()`. Ordering is preserved: add the ID, persist the max length if it grew, then serialize.
- No change to `IIDList`, `TreeOfToDoItems`, `TaskTreeController`, or `AppToDoObjects`. ToDoModel.csproj is unchanged.

### D4. TaskMaster settings writers

- `TaskMaster/AppGlobals/AppQuickFilerSettings.cs`: add the store field, a parameterless constructor (default adapter) and an `AppQuickFilerSettings(ISettingsStore settingsStore)` overload. Getters read through the store with casts; setters write through the store and call `Save()` on it. `ApplicationGlobals` keeps `new AppQuickFilerSettings()` unchanged.
- `TaskMaster/AppGlobals/AppStagingFilenames.cs`: same pattern for all of its settings-backed properties, so the class is internally consistent. Saving setters save through the store; `EmailInfoStagingFile` remains non-persisting.
- `TaskMaster/AppGlobals/AppAutoFileObjects.cs`: add the store field (default adapter) and an `AppAutoFileObjects(IApplicationGlobals parent, ISettingsStore settingsStore)` overload. Route only the six scalar properties through the store; the file-name reads on `_defaults` are untouched. Because the file is already above 500 lines, move the scalar properties into new partial `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` so the main file shrinks; add a `<Compile Include>` entry in `TaskMaster/TaskMaster.csproj`.

### D5. Test changes

- New `UtilitiesCS.Test/HelperClasses/Settings/ApplicationSettingsStoreTests.cs` with a nested `RecordingApplicationSettings : ApplicationSettingsBase` that overrides the indexer and `Save()` (dictionary-backed, save counter), so no provider is invoked and no file is touched. Covers null constructor argument, get forwarding, set forwarding, save forwarding. `<Compile Include>` in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
- `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs` (C# 7.3): replace the `CreateListWithCachedMaxLength` cache-priming workaround with an injected `Mock<ISettingsStore>`, and add persistence-branch tests: first ID on a fresh list sets `MaxLengthOfID` and saves once; a shorter ID does not save; a rollover to a longer ID saves again; a null store throws `ArgumentNullException`. If the file would exceed 500 lines, place the new tests in a new `ToDoModel.Test/Data Model/ID/IDListMaxLengthPersistenceTests.cs` with a `<Compile Include>` in `ToDoModel.Test/ToDoModel.Test.csproj`.
- `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs` and `TaskTree.Test/TaskTreeControllerRouteDropTests.cs` (C# 7.3): `MakeController` returns `new IDList(new List<string>(), settingsStore.Object)` from `mockTD.Setup(x => x.IDList)`, where `settingsStore` is a `Mock<ISettingsStore>`.
- New `TaskMaster.Test/AppGlobals/InMemorySettingsStore.cs`: dictionary-backed `ISettingsStore` with a `SaveCount` property, one instance per test. `<Compile Include>` in `TaskMaster.Test/TaskMaster.Test.csproj`.
- `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs`, `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsRemainingPropertiesTests.cs`, `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs`: construct the system under test with an `InMemorySettingsStore`, assert round-trips against the fake, assert `SaveCount` increments for saving setters and not for `EmailInfoStagingFile`, and remove the `Settings.Default` snapshot and restore.
- `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (and, only if needed, `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs`): `CreateController` injects `new AppQuickFilerSettings(store)` with a seeded fake; assertions read the fake; the snapshot and restore are removed.
- `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs`: construct with an `InMemorySettingsStore` seeded with the scalar values, assert against the fake, and delete the `TestInitialize`/`TestCleanup` snapshot including the direct `defaults.Save()` call.

### D6. Removal of class-level `[DoNotParallelize]` (AC8)

- `AppQuickFilerSettingsTests`: the attribute's only reason is shared `Settings.Default` state, which D5 removes; delete it.
- `RibbonControllerTests`: before removal, read the entire class (both partial files) for other process-global static state (static fields, singletons, `UiThread`/dispatcher statics, log4net configuration, environment variables, other `Settings.Default` members). Record the finding in the feature's other-evidence folder. Remove the attribute only when the check finds no other shared static state; otherwise keep it and record which state requires it.

## Acceptance Criteria

The criteria lines below carry no counts, because the research record's numeric derivation block does not use the required label names (see the note under Assumptions); populations are enumerated by name instead.

- [x] AC1: `DfDeedle_COM_Tests.AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` no longer depends on wall-clock time: it passes a `FakeTimeProvider` that is never advanced through the existing `timeProvider` parameter of `AddQfcColumnsAsync` (with `columnAdder` left at the production default or supplied as a completed adder), is an awaited `async Task` test containing no `GetAwaiter().GetResult()`, `Thread.Sleep`, `Task.Delay` or other real wait, and passes deterministically. `AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully` also passes a `FakeTimeProvider`. No production file under UtilitiesCS Extensions changes for this criterion.
- [x] AC2: `IDList.GetNextToDoID` persists `MaxLengthOfID` only through an injected per-instance `ISettingsStore` (indexer assignment followed by `Save()` on the store). Every existing `IDList` constructor defaults the store to an `ApplicationSettingsStore` over `ToDoModel.Properties.Settings.Default`, so production writes and saves the same property under the same condition as before; new public constructors accept a store and throw `ArgumentNullException` for null. ToDoModel.Test unit tests prove: the first ID on a fresh list assigns the new length and saves exactly once; a shorter ID does not save; a longer ID saves again; a null store is rejected. The cache-priming workaround in `IDListGetNextToDoIDTests` is removed.
- [x] AC3: `TaskTreeControllerMoveLogicTests.MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam`, and every other test that reaches a real `IDList.GetNextToDoID` (every test that uses `MakeController` in `TaskTreeControllerMoveLogicTests` and in `TaskTreeControllerRouteDropTests`, plus `IDListGetNextToDoIDTests`), constructs its `IDList` with an injected store, so no test reaches `ApplicationSettingsBase.Save()` through `IDList`. The settings-saving probe described in Test Strategy records zero saves on the test thread for `MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` after the fix.
- [x] AC4: a search of all test projects for tests that reach a production settings `Save()` or that call `AddQfcColumnsAsync` without its seams is recorded under the feature's regression-testing evidence folder, both before and after the fix. Every match is fixed in this item: the `IDList.GetNextToDoID` site; the `AppQuickFilerSettings` setters `MoveEntireConversation`, `SaveAttachments`, `SavePictures`, `SaveEmailCopy`, `HighConfidenceModeEnabled` and `HighConfidenceThreshold`; the `AppStagingFilenames` setters `ConditionalReminders` and `CommonWords`; the `AppAutoFileObjects` setters `LngConvCtPwr`, `Conversation_Weight`, `SmithWatterman_MatchScore`, `SmithWatterman_MismatchScore`, `SmithWatterman_GapPenalty` and `MaxRecents`; the test-code `defaults.Save()` in `AppAutoFileObjectsCoverageExpansionTests`; and both seamless `AddQfcColumnsAsync` calls in `DfDeedle_COM_Tests`. Each of those production members saves through an injected `ISettingsStore` whose default is the adapter over `TaskMaster.Properties.Settings.Default` (or ToDoModel's for `IDList`), and each reaching test injects an in-memory fake or Moq mock. The after-fix search finds no test that reaches a settings `Save()` and no seamless `AddQfcColumnsAsync` call.
- [x] AC5: each fix is preceded by fail-before evidence recorded under the feature's regression-testing evidence folder: for the `IDList` path and for one representative test in each of `AppQuickFilerSettingsTests`, `AppStagingFilenamesTests` and `AppAutoFileObjectsCoverageExpansionTests`, a run of the settings-saving probe that fails before the production change and passes after it; for the DfDeedle path, where a deterministic failing run is impossible without real timers, a schema-valid fail-before-exception dossier stating why the failing run is impossible, quoting the maintainer's captured exception from `issue.md`, and citing the before-fix AC4 search result as the repeatable demonstration.
- [x] AC6: the full test suite passes in parallel, recorded as a local MSTest run with `scripts/vscode/TaskMaster.cli.runsettings` (class-level parallelism, workers unrestricted), plus repeated parallel runs of `DfDeedle_COM_Tests`, `TaskTreeControllerMoveLogicTests`, `TaskTreeControllerRouteDropTests` and the converted TaskMaster settings test classes. The fix adds no serial worker setting (such as a Workers value of one), no `[DoNotParallelize]`, no retries, no `Thread.Sleep` or `Task.Delay`, and no lengthened timeout, and no test writes or redirects `user.config` or creates a temporary file.
- [x] AC7: the full C# toolchain passes in one clean pass in this order: `dotnet tool run csharpier format .` (verified with `dotnet tool run csharpier check .`), the analyzer `msbuild TaskMaster.sln /t:Rebuild` with `EnableNETAnalyzers` and `EnforceCodeStyleInBuild`, the `msbuild TaskMaster.sln /t:Rebuild` with `TreatWarningsAsErrors` and without a Nullable override, and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. Changed lines lose no coverage against the merge-base baseline; `ApplicationSettingsStore` and every new or changed seam member in `IDList`, `AppQuickFilerSettings`, `AppStagingFilenames` and the `AppAutoFileObjects` scalar partial reach at least ninety percent line coverage, read from the post-processed Cobertura document and recorded in the committed coverage-comparison evidence (the committed JaCoCo projection carries package-level counters only); no `[ExcludeFromCodeCoverage]` is added. Every created or modified `.cs` file named in the Write Set is at or under the five-hundred-line ceiling, except `DfDeedle_COM_Tests.cs` and `AppAutoFileObjects.cs`, which were already above it at the merge base and must instead be smaller than at the merge base.
- [x] AC8: the class-level `[DoNotParallelize]` attribute is removed from `AppQuickFilerSettingsTests`. For `RibbonControllerTests`, a recorded check of the whole class (both partial files) for process-global static state other than the removed settings snapshot is committed under the feature's other-evidence folder; the attribute is removed when that check finds no other shared static state, and when it finds some, the attribute stays and the record names the state and why it requires serialization.

## 4. Scope and Non-Goals

**In scope (decided by the orchestrator):**

- The production settings `Save()` sites reached by tests, listed in section 2 and in AC4.
- Both seamless `AddQfcColumnsAsync` tests in `DfDeedle_COM_Tests`.
- The test-code `defaults.Save()` in `AppAutoFileObjectsCoverageExpansionTests`.
- The remaining settings-backed properties of `AppStagingFilenames` (converted for internal consistency of the class, although no test reaches their setters).
- Class-level `[DoNotParallelize]` removal on `AppQuickFilerSettingsTests` and, conditionally, `RibbonControllerTests` (AC8).

**Out of scope (a follow-up issue will be filed through the MCP promotion lifecycle):**

- Tests that mutate `Settings.Default` in memory without saving: StartupTimingEnabled (ApplicationGlobalsStartupTimingTests, ApplicationGlobalsTests), EventsHooked (AppEventsTests), UseLcppnPredictor (AppAutoFileObjectsFolderPredictorTests), JunkPotential and OlJunkCertain (AppOlObjectsTests), and the PropertyValues injection in AppToDoObjectsTests.
- The per-method `[DoNotParallelize]` in ApplicationGlobalsStartupTimingTests.

**Other non-goals:**

- Production settings `Save()` sites that no test reaches: AppOlObjects.JunkFolders.cs (ApplyJunkFolderSelections, LoadJunkPotential, LoadJunkCertain) and AppOlObjects.cs (DarkMode). No change.
- `[DoNotParallelize]` classes whose shared state is unrelated to settings (DfDeedleQfcColumnTimeoutTests, DfDeedleEtlTimeoutTests and other UtilitiesCS.Test classes guarding log4net, UI-thread or store-wrapper statics).
- QfcDatamodel.GetEmailsInViewDfAsync, which calls GetEmailDataInViewAsync without a time provider in production only; its one test throws before any deadline.
- Any change to `IIDList`, `TreeOfToDoItems`, `TaskTreeController`, `AppToDoObjects`, `ApplicationGlobals`, or the runsettings files.

## 5. Constraints

- All tests pass in parallel. No `Workers=1`, no `[DoNotParallelize]` added, no retries, no `Thread.Sleep`, `Task.Delay`, or longer timeouts.
- No temporary files in tests; no redirecting `user.config` to a temp path; no custom `SettingsProvider` installed process-wide.
- No new NuGet dependency (`Microsoft.Extensions.TimeProvider.Testing` is already referenced by UtilitiesCS.Test).
- No static mutable seam; the settings store is injected per instance.
- Bugfix workflow: fail-before evidence first (or a schema-valid fail-before-exception dossier where a deterministic failing run is impossible), then the minimal fix.
- MSTest, Moq, FluentAssertions. New seam code at least 90% coverage; changed lines lose no coverage.
- Every created or modified `.cs` file at or under 500 physical lines. Files already above 500 (`DfDeedle_COM_Tests.cs`, `AppAutoFileObjects.cs`) are split into partials and shrink; they are not grown. `.csproj` and Markdown files are exempt per `.claude/rules/general-code-change.md`.
- Legacy non-SDK projects: every new `.cs` file needs a `<Compile Include>` entry. TaskTree.Test, ToDoModel.Test and QuickFiler.Test are C# 7.3 (no nullable annotations, target-typed `new()`, `is not`, switch expressions or `using` declarations). UtilitiesCS is C# 12 with per-file `#nullable enable`.
- C# toolchain per CLAUDE.md, in order: `dotnet tool run csharpier format .`; `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`); `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
- TaskMaster CI checks that must pass on the PR: actionlint, build-analyzers, build-nullable, format-check, hygiene, mstest-coverage, pester. (The execution-environment skill's CI section describes upstream drm-copilot, not this repository.)
- Committed test evidence is projections only (CLAUDE.md "Committed Test Evidence Format"); no raw TRX or Cobertura document is committed.
- Evidence lives under the feature folder's `evidence/<kind>/` per the evidence-and-timestamp conventions.

## 6. Write Set

Production (C#):

- `UtilitiesCS/HelperClasses/Settings/ISettingsStore.cs` (new)
- `UtilitiesCS/HelperClasses/Settings/ApplicationSettingsStore.cs` (new)
- `ToDoModel/Data Model/ID/IDList.cs`
- `TaskMaster/AppGlobals/AppQuickFilerSettings.cs`
- `TaskMaster/AppGlobals/AppStagingFilenames.cs`
- `TaskMaster/AppGlobals/AppAutoFileObjects.cs`
- `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` (new partial)

Build files:

- `UtilitiesCS/UtilitiesCS.csproj`
- `TaskMaster/TaskMaster.csproj`

Tests:

- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs` (new)
- `UtilitiesCS.Test/HelperClasses/Settings/ApplicationSettingsStoreTests.cs` (new)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs`
- `ToDoModel.Test/Data Model/ID/IDListMaxLengthPersistenceTests.cs` (new, only if needed for the size ceiling) and `ToDoModel.Test/ToDoModel.Test.csproj`
- `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs`
- `TaskTree.Test/TaskTreeControllerRouteDropTests.cs`
- `TaskMaster.Test/AppGlobals/InMemorySettingsStore.cs` (new)
- `TaskMaster.Test/AppGlobals/SettingsSaveProbe.cs` (new; the shared thread-attributed settings-saving probe used by the three TaskMaster.Test probe tests, added by the plan's Decision D-A)
- `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs`
- `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsRemainingPropertiesTests.cs`
- `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs`
- `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs`
- `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`
- `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` (only if the AC8 check or the settings conversion requires it)
- `TaskMaster.Test/TaskMaster.Test.csproj`

Routing: seven production C# files exceed the direct-path budget; this item runs on the orchestrated large path.

## 7. Test Strategy

**Settings-saving probe (fail-before for AC3, AC5).** Before any production change, add to `MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` a probe that resolves `ToDoModel.Properties.Settings.Default` by reflection (`typeof(IDList).Assembly.GetType("ToDoModel.Properties.Settings")`, public static `Default`, cast to `ApplicationSettingsBase`), subscribes a `SettingsSaving` handler that records only when `Environment.CurrentManagedThreadId` equals the test thread's ID and sets `e.Cancel = true` (so the fail-before run writes nothing to `user.config`), unsubscribes in try-finally, and asserts no save was recorded. Before the fix it fails with one recorded save; after `MakeController` injects the mock store it passes, and the mock verifies `Save()` once. Apply the same probe through `TaskMaster.Properties.Settings.Default.SettingsSaving` (visible to TaskMaster.Test through `InternalsVisibleTo`) to one representative test in each of `AppQuickFilerSettingsTests`, `AppStagingFilenamesTests` and `AppAutoFileObjectsCoverageExpansionTests`. The probe is thread-attributed and always removed, so concurrent classes are unaffected.

**DfDeedle (AC1, AC5).** The original failure requires real elapsed time under load and cannot be reproduced deterministically without timers. Record a fail-before-exception dossier (`WhyFailingRunImpossible`) and use the AC4 search (seamless calls present before, absent after) as the repeatable demonstration.

**Seam tests (AC2, AC7).** `ApplicationSettingsStoreTests` through `RecordingApplicationSettings`; IDList persistence-branch tests; converted TaskMaster tests asserting against `InMemorySettingsStore` values and `SaveCount`.

**Parallel validation (AC6).** CI passes no runsettings, so CI does not exercise class-level parallelism. The local run of `scripts/vscode/Invoke-MSTestWithCoverage.ps1` with `scripts/vscode/TaskMaster.cli.runsettings` (`Workers=0`, `Scope=ClassLevel`) plus repeated runs of the affected classes is the evidence. The maintainer's Visual Studio Test Explorer re-run is an optional manual confirmation.

**Evidence locations** (feature-relative; every artifact carries `Timestamp`, `Command`, `EXIT_CODE`, `Output Summary`):

- `evidence/regression-testing/` — AC4 before and after search results; settings-saving probe fail-before and pass-after test-result summaries; DfDeedle fail-before-exception dossier.
- `evidence/other/` — RibbonControllerTests static-state check record (AC8).
- `evidence/baseline/` — merge-base coverage projection and summary.
- `evidence/qa-gates/` — final toolchain pass, parallel run test-result summaries, post-change coverage projection and summary, coverage comparison.

## 8. Assumptions and Open Points

- The cross-process sharing of the testhost `user.config` path is inferred from documented `LocalFileSettingsProvider` behavior and the reported path; it does not change the design.
- `ApplicationSettingsBase` indexer and `Save()` overridability and `SettingsSaving` cancel semantics were read from the dotnet/runtime source; confirm on net481 when the first adapter test runs. If `Save()` is not overridable on net481, the fallback is to test the adapter through a `SettingsSaving` handler that cancels the save, with the same zero-disk-write property.
- Numeric assertions: the research's `## Numeric Derivation Evidence` block uses the labels `Primary Search Strategy` and `Cross-check Search Strategy` rather than the required `Primary Search Strategy or Query Expression` and `Cross-check Search Strategy or Query Expression`, so it does not satisfy this spec's numeric-evidence contract. The acceptance criteria therefore assert no counts and enumerate the site population by name. The research's figures (fifteen reached sites, twenty-six total, two seamless calls) remain informational in this body only.

## 9. Risks

- Public surface widening (`ISettingsStore`, `ApplicationSettingsStore`, new `IDList` constructors): `public` is the minimum because the consuming assemblies have no `InternalsVisibleTo` grant from UtilitiesCS or ToDoModel.
- A regression could pass CI while failing in Test Explorer, because CI runs no class-level parallelism; mitigated by the recorded local parallel run.
- `RibbonControllerTests` may hold other shared static state; mitigated by the AC8 recorded check.
