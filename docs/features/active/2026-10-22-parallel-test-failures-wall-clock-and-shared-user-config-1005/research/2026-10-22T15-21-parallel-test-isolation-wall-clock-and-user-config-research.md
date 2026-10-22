# Research: Parallel test failures from wall-clock deadlines and shared user.config (Issue #1005)

- Issue: #1005
- Branch: bug/parallel-test-failures-wall-clock-and-shared-user-config-1005
- Work Mode: full-bug
- Requirement source: `issue.md` section `## Acceptance Criteria` (AC1 through AC7)
- Filename timestamp basis: the researcher has no clock tool; the stamp was taken as the UTC equivalent of the feature-folder plan stamp (America/New_York) plus the research interval. The orchestrator may rename the file if an exact write time is required.
- Evidence tags: [V-code] read in the repository by this research; [V-web] confirmed from Microsoft documentation or the dotnet/runtime source; [I] inferred, needs confirmation by the orchestrator.

## 1. Summary of Findings

1. DfDeedle (AC1): there is no production seam gap. `DfDeedle.AddQfcColumnsAsync` already accepts `columnAdder` and `timeProvider`, `FakeTimeProvider` is already referenced and already used by `DfDeedle_COM_Tests`, and the two offending tests only fail to pass the seam. The fix is test-only.
2. IDList (AC2, AC3): `IDList.GetNextToDoID` writes `Properties.Settings.Default.MaxLengthOfID` and calls `Save()` whenever the generated ID is longer than the cached `_maxIDLength`. A freshly constructed `IDList` has `_maxIDLength == 0`, so the first ID it produces always triggers `Save()`. Nine TaskTree.Test methods reach this path.
3. AC4: the entire repository has 26 production `Save()` calls on an `ApplicationSettingsBase` instance across 6 files. Fifteen are reached by tests (Numeric Derivation Evidence below). One more `Save()` is called directly by test code (`AppAutoFileObjectsCoverageExpansionTests.TestCleanup`).
4. Recommended design: one public settings-store abstraction in UtilitiesCS (`ISettingsStore`: indexer plus `Save()`). Its production adapter (`ApplicationSettingsStore`) wraps any `ApplicationSettingsBase`. Each consumer receives the store per instance through a constructor overload. Parameterless constructors default to the adapter over that assembly's `Properties.Settings.Default`, so production behavior is unchanged. Tests inject an in-memory fake or a Moq mock. No static mutable seam is used.
5. No human coverage-exemption decision is required. The adapter can be fully covered with a test subclass of `ApplicationSettingsBase` that overrides the indexer and `Save()`, so no test touches disk.

## 2. Current State Analysis

### 2.1 DfDeedle.AddQfcColumnsAsync [V-code]

- Declaration (`UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`, file under `#nullable enable`):
  `internal static async Task AddQfcColumnsAsync(Table table, MAPIFolder folder, CancellationToken token, int counter, Action<object, object>? columnAdder = null, TimeProvider? timeProvider = null)`
- `columnAdder`: when null, uses the production `AddQfcColumns((Table)t, (MAPIFolder)f)`. It is typed over `object` because of CS1769 (embedded interop types).
- Retry loop: the work starts once with `Task.Run(() => adder(table, folder), token)`. For `attempt` from `counter` up to `AttemptLimit` (3), it awaits `work.TimeoutAfter(3000, timeProvider)`.
  - Success returns.
  - `TaskCanceledException` or `TimeoutException` returns when the token is cancelled; otherwise the loop re-arms the deadline on the same task.
  - When all deadlines are spent and the token is not cancelled, it throws `TimeoutException("The column add step timed out after 9000 ms for folder '<name>'...")`.
- `TimeoutTask.TimeoutAfter` (`UtilitiesCS/Threading/TimeOutTask.cs`) returns the task unchanged when it has already completed. Otherwise it arms `(timeProvider ?? TimeProvider.System).CreateTimer(...)`, and the continuation disposes the timer. With a `FakeTimeProvider` that is never advanced, the deadline can never fire, so completion depends only on the work.
- `DfDeedleQfcColumnTimeoutTests` (UtilitiesCS.Test) passes `probe.Adder` (a gated `Action<object, object>`) and an `ArmingBarrierTimeProvider` (in-repo fake under `UtilitiesCS.Test/TestHelpers`). It fires each 3000 ms deadline with `barrier.Advance`. The class carries `[DoNotParallelize]` because of process-wide log4net state, not because of settings.
- Package availability: `UtilitiesCS.Test/packages.config` lists `Microsoft.Extensions.TimeProvider.Testing` 10.11.0 and `Microsoft.Bcl.TimeProvider` 10.0.13, and `UtilitiesCS.Test.csproj` carries matching `<Reference>`/`HintPath` entries. `DfDeedle_COM_Tests.cs` already has `using Microsoft.Extensions.Time.Testing;` and uses `new FakeTimeProvider()` in its `GetEmailDataInViewAsync` test. No new NuGet dependency is needed.
- Every `AddQfcColumnsAsync` call site:

| Call site | Passes columnAdder | Passes timeProvider | Wall-clock bound |
| --- | --- | --- | --- |
| `DfDeedle.GetEmailDataInViewAsync` (production) | no | forwards caller's `timeProvider` | only if caller passes none |
| `DfDeedle_COM_Tests.AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` | no | no | yes (the reported failure) |
| `DfDeedle_COM_Tests.AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully` | no | no | no in practice: `Task.Run` with a pre-cancelled token returns an already-cancelled task, so `TimeoutAfter` short-circuits. It still matches the AC4 "without its seams" search and is fixed for uniformity. |
| `DfDeedleQfcColumnTimeoutTests` (all five tests, through `StartColumnAdd`) | yes | yes (`ArmingBarrierTimeProvider`) | no |

- Indirect callers through `GetEmailDataInViewAsync`:
  - `DfDeedle_COM_Tests` (one test) passes `timeProvider: new FakeTimeProvider()`.
  - `DfDeedleEtlTimeoutTests` (two tests) pass `barrier` or `new FakeTimeProvider()`.
  - The production caller `QfcDatamodel.GetEmailsInViewDfAsync` (`QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`) passes no `timeProvider`. Its one test, `QfcDatamodelRethrowTests.GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack`, throws a sentinel from `Explorer.CurrentView` during table acquisition, upstream of `AddQfcColumnsAsync`, so it never reaches the column add. This is reported as not in the AC4 family (see §7 for a related out-of-scope note).
- Conclusion: no production seam gap exists, and AC1 is satisfied by a test-only change.

### 2.2 IDList and settings persistence in ToDoModel [V-code]

- `ToDoModel/Data Model/ID/IDList.cs`: `public class IDList : SerializableList<string>, IIDList`.
- Constructors:
  - `()`
  - `(IList<string>)`
  - `(IEnumerable<string>)`
  - `(string filename, string folderpath)`
  - `(string, string, Outlook.Application)` `[ExcludeFromCodeCoverage]`
  - `(string, string, CSVLoader<string>, string, bool)`
  - `(… , Outlook.Application)` `[ExcludeFromCodeCoverage]`
  - The filename constructors deserialize through the `SerializableList` base. All of them are ordinary constructors, so a field initializer runs on every construction path.
- `GetNextToDoID(string strSeed)`:
  1. Validates the seed (`ThrowIfNullOrEmpty`).
  2. Increments in base 36 until it finds an ID not in the list, then adds it.
  3. When `strMaxID.Length > _maxIDLength`, updates `_maxIDLength`, sets `Properties.Settings.Default.MaxLengthOfID = (int)_maxIDLength;` and calls `Properties.Settings.Default.Save();`.
  4. Calls `Serialize()` when `Filepath is not null`. `SerializableList.Serialize()` is a no-op when `Filepath == ""`, which is the default.
- `GetNextToDoID()` seeds from `this.Max()`.
- `_maxIDLength` starts at 0 and is set lazily only by the `MaxLengthOfID` getter or by `RefreshIDList`. A list built with `new IDList()` therefore saves on its first generated ID.
- No other code in IDList or ToDoModel writes or saves `Properties.Settings`. `ToDoItem` reads `Properties.Settings.Default.MaxLengthOfID` (read only). `ToDoDefaults` reads `Properties.Settings.Default`. The generated setter in `ToDoModel/Properties/Settings.Designer.cs` is `this["MaxLengthOfID"] = value;`, so setting through the `ApplicationSettingsBase` indexer is behaviorally identical.
- Production construction:
  - `AppToDoObjects.LoadIDList` uses `new IDList(FnameIDList, appData, Parent.Ol.App)`.
  - `AppToDoObjects.LoadIdListFromDisk` uses `new IDList(ids) { Filename, Folderpath }`.
- Callers of `GetNextToDoID`:
  - `TreeOfToDoItems.AssignId`, `ReNumberChildrenIDs`, `AddChild`
  - `ToDoItem` (through its `IdList` property)
  - `ToDoEvents`
  - `FileOperationsPST`
  - `TaskTreeController.MoveObjectsToSibling` (root-target branch)
  - `TaskVisualization.AutoCreateProject`
  - `TaskTreeController.MoveObjectsToSibling` (child-target branch) reaches it through `ReNumberChildrenIDs((IDList)_globals.TD.IDList)`.
  - `MoveObjectsToChildren` reaches it through `_dataModel.AddChild(x, target, _globals.TD.IDList)`.
- Test reach:
  - TaskTree.Test `MakeController` in both `TaskTreeControllerMoveLogicTests` and `TaskTreeControllerRouteDropTests` sets `mockTD.Setup(x => x.IDList).Returns(new IDList())`, which is a real, fresh list.
  - `ToDoModel.Test` `IDListGetNextToDoIDTests` avoids `Save()` by pre-reading `MaxLengthOfID` (`CreateListWithCachedMaxLength`), and its null and empty seed tests throw before the save. That is a workaround, not a seam.
  - All other tests use `Mock<IIDList>` (TaskVisualization.Test `AutoCreateProjectTests`, ToDoModel.Test `ToDoItemCoverageExpansionTests`).
- Language versions and project shape:
  - ToDoModel: `<LangVersion>latest</LangVersion>`.
  - ToDoModel.Test and TaskTree.Test: no `LangVersion` element, so the .NET Framework default applies: C# 7.3. No nullable annotations, target-typed `new()`, `is not`, switch expressions, or `using` declarations.
  - TaskMaster: `preview`. TaskMaster.Test: `latest`. UtilitiesCS: `12.0`. UtilitiesCS.Test: `Latest`.
  - Nullable is per-file opt-in through `#nullable enable`.
  - All are legacy non-SDK projects, so every new `.cs` file needs a `<Compile Include>` entry in its `.csproj`.
- Internals visibility:
  - ToDoModel grants `InternalsVisibleTo("ToDoModel.Test")` only (declared in `ToDoItem.cs`), not TaskTree.Test.
  - UtilitiesCS grants UtilitiesCS.Test and ToDoModel.Test, not ToDoModel or TaskMaster.
  - TaskMaster grants TaskMaster.Test.
  - TaskTree grants TaskTree.Test.
  - Therefore the store interface must be `public` in UtilitiesCS, and the IDList injection constructor must be `public`.

### 2.3 TaskMaster settings writers [V-code]

| File | Members that write and save | Pattern |
| --- | --- | --- |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` (68 lines) | `MoveEntireConversation`, `SaveAttachments`, `SavePictures`, `SaveEmailCopy`, `HighConfidenceModeEnabled`, `HighConfidenceThreshold` (internal setters) | getter reads `Settings.Default.X`; setter writes it and calls `Settings.Default.Save()` |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` (139 lines) | `ConditionalReminders`, `CommonWords`, `SubjectMap`, `CtfInc`, `CtfMap`, `EmailSessionTemp`, `EmailSession`, `MovedMails`, `RecentsFile` | lazy getter from `Settings.Default`; setter writes the backing field and `Settings.Default`, then calls `Save()`. `EmailInfoStagingFile` does not save. |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (853 lines, partial) | `LngConvCtPwr`, `Conversation_Weight`, `SmithWatterman_MatchScore`, `SmithWatterman_MismatchScore`, `SmithWatterman_GapPenalty`, `MaxRecents` | field `_defaults = Properties.Settings.Default`; setter writes and calls `_defaults.Save()`. `_defaults` is also read for about 20 file-name settings. |
| `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` | `ApplyJunkFolderSelections`, `LoadJunkPotential` (after the user picks a folder), `LoadJunkCertain` (after the user picks a folder) | `Properties.Settings.Default.Save()` |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | `DarkMode` setter | `Properties.Settings.Default.Save()` |

The other settings classes have no `Save()` call: UtilitiesCS, QuickFiler, TaskVisualization, and Tags (`My.Settings.Save()` appears only inside a commented-out VB block in `Tags/My Project/Settings.Designer.cs`).

## 3. Why All Settings Classes Share One user.config

- `ApplicationSettingsBase.Save()` calls `OnSettingsSaving` and then, unless cancelled, `base.Save()`. That call reaches the provider's `SetPropertyValues` [V-web, `ApplicationSettingsBase.Save` reference page and dotnet/runtime source]. The default `LocalFileSettingsProvider` writes user-scoped values [V-web].
- The user.config location is derived from the host identity (company, executable or AppDomain evidence, version), not from the settings class. Each settings class is a section group keyed by its full type name inside that one file [I; standard .NET Framework behavior, not tool-verified here]. The maintainer's failure path, `...\TestSourceHost__...\user.config`, is consistent with a host-derived location.
- Consequences:
  - Within one testhost process, `ToDoModel.Properties.Settings`, `TaskMaster.Properties.Settings`, and any other settings class read and rewrite the same file.
  - The configuration system checks whether the file changed since it was loaded and throws `ConfigurationErrorsException: The configuration file has been changed by another program` when a concurrent writer has replaced it.
  - When Test Explorer runs several test assemblies in parallel testhost processes from the same host executable, those processes resolve the same path, so cross-assembly races are possible too [I].
  - `TaskMaster.runsettings` and `scripts/vscode/TaskMaster.cli.runsettings` set `Workers=0`, `Scope=ClassLevel`, so classes within one assembly run concurrently.
- Side effect: the snapshot and restore in the TaskMaster test `TestCleanup` methods restores values in memory only. The last test-mutated value that was saved stays in the host's user.config.

## 4. Candidate Approaches

### Selected: per-instance `ISettingsStore` (UtilitiesCS) with an `ApplicationSettingsBase` adapter

- New public interface `UtilitiesCS.ISettingsStore`:
  - `object this[string propertyName] { get; set; }`
  - `void Save();`
  - XML docs state that `Save` persists every pending value, mirroring `ApplicationSettingsBase`.
- New public sealed adapter `UtilitiesCS.ApplicationSettingsStore : ISettingsStore`:
  - Constructor takes an `ApplicationSettingsBase` and throws `ArgumentNullException` on null.
  - The indexer forwards to `settings[propertyName]`, and `Save()` forwards to `settings.Save()`.
- Consumers hold `private readonly ISettingsStore _settingsStore`. Parameterless or existing constructors default it to `new ApplicationSettingsStore(Properties.Settings.Default)` of their own assembly, and a new overload accepts an `ISettingsStore`. Property names use `nameof(Properties.Settings.X)` to stay tied to the generated class.
- Advantages:
  - One small abstraction is reused by two assemblies (ToDoModel and TaskMaster).
  - Injection is per instance, so there is no shared static seam.
  - The production default is byte-for-byte equivalent, because generated property setters are `this[name] = value`.
  - It matches the repo's DI-seam preference order (interface first) in `.claude/rules/csharp.md`.
  - The getters can also read through the store, which removes the in-memory `Settings.Default` mutation from the affected TaskMaster tests.
- Limitations: indexer access is untyped (casts at the call sites). This is acceptable for the small, fixed member set.

### Rejected alternatives (brief)

- Static settable default persistence delegate or `SerializableList`-style static seam: shared mutable static state across parallel tests, prohibited by the item constraints.
- Redirecting user.config or installing a custom `SettingsProvider`: global, and either touches the file system or mutates process-wide state.
- `[DoNotParallelize]`, `Workers=1`, retries, or longer timeouts: prohibited (AC6).
- Pre-reading `MaxLengthOfID` in tests (the current `IDListGetNextToDoIDTests` workaround): it leaves the `Save()` path reachable, does not work on empty lists (`Max()` throws), and is not applicable to TaskTree's `MakeController`.
- An IDList-specific `Action<int>` delegate: it would solve only IDList and would require a second abstraction for the TaskMaster writers.

## 5. Recommended Changes

### 5.1 AC1: DfDeedle tests (test-only)

- `AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing`:
  - Pass `timeProvider: new FakeTimeProvider()` and never advance it. Keep the production adder (`columnAdder` null) so the default-adder branch remains exercised; the deadline can then never fire.
  - Await the task with `await` in an `async Task` test instead of `GetAwaiter().GetResult()`.
  - Assert completion without exception.
  - Optionally strengthen the test by verifying the mocked `Columns.Add("SentOn")` call.
- `AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully`: pass `timeProvider: new FakeTimeProvider()` as well.
- File size: `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` is 848 lines, already above the 500-line cap, and must not grow. Mark the class `partial` and move the `AddQfcColumnsAsync` section (both tests) into a new `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs`. The new file keeps access to the private `BuildTableMock` and `BuildFolderWithUdp` helpers, and the original file shrinks. Add a `<Compile Include>` entry in `UtilitiesCS.Test.csproj`.

### 5.2 AC2, AC3: IDList seam

- Add to `IDList`:
  - `private readonly ISettingsStore _settingsStore = new ApplicationSettingsStore(Properties.Settings.Default);`. The field initializer runs for every constructor, including the deserializing and Outlook-bound ones.
  - New public constructors `IDList(IEnumerable<string> ids, ISettingsStore settingsStore)` and optionally `IDList(ISettingsStore settingsStore)`. Each assigns the injected store after a null guard.
- In `GetNextToDoID`, replace the two settings lines with:

```csharp
_settingsStore[nameof(Properties.Settings.MaxLengthOfID)] = _maxIDLength;
_settingsStore.Save();
```

  The redundant `(int)` cast is dropped because `_maxIDLength` is already `int`.
- No change to `IIDList`, `TreeOfToDoItems`, `TaskTreeController`, or `AppToDoObjects`. The production default is identical.
- TaskTree.Test (C# 7.3): change `MakeController` in both test classes to `new IDList(new List<string>(), settingsStore.Object)`, where `settingsStore` is a `Mock<ISettingsStore>`. The `(IDList)` cast in `MoveObjectsToSibling` still succeeds because the object is still an `IDList`. `TaskTree.Test.csproj` already references UtilitiesCS (the test files use the `UtilitiesCS` namespace) and `System.Configuration`.
- ToDoModel.Test (C# 7.3): add tests for the persistence branch:
  - The first ID on a fresh list sets `MaxLengthOfID` to the new length and saves once.
  - A shorter ID does not save.
  - A rollover to a longer ID saves again.
  - A null store throws `ArgumentNullException`.
  - Convert `CreateListWithCachedMaxLength` to inject a mock store, and remove the cache-priming workaround.

### 5.3 AC4: TaskMaster writers reached by tests

- `AppQuickFilerSettings`:
  - Add the field and constructors `AppQuickFilerSettings()` (default adapter) and `AppQuickFilerSettings(ISettingsStore settingsStore)`.
  - Getters return `(bool)_settingsStore[nameof(Settings.X)]` or `(double)...`.
  - Setters assign through the store and call `_settingsStore.Save()`.
  - `ApplicationGlobals` keeps `new AppQuickFilerSettings()`, unchanged.
- `AppStagingFilenames`: same pattern for all ten properties. The getters' `InitProp` reads go through the store. The nine saving setters use the store. Converting all nine keeps the class internally consistent, although only two are reached by tests.
- `AppAutoFileObjects`:
  - Add `ISettingsStore _settingsStore` (default adapter) and the overload `AppAutoFileObjects(IApplicationGlobals parent, ISettingsStore settingsStore)`.
  - Route only the six scalar properties through the store. The file-name reads on `_defaults` are untouched.
  - File size: the main file is 853 lines. Move the six scalar properties into a new partial `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` so the main file shrinks, and add a `<Compile Include>` entry in `TaskMaster.csproj`.
- `AppOlObjects` (`ApplyJunkFolderSelections`, `LoadJunkPotential`, `LoadJunkCertain`, `DarkMode`): not reached by any test (see §6), so not in the AC4 family. Recommend no change in this item to keep the COM-bound class out of scope; noted as an optional follow-up.

### 5.4 Test-side changes for AC4

- New `TaskMaster.Test/AppGlobals/InMemorySettingsStore.cs` (C# latest): a dictionary-backed `ISettingsStore` with a `SaveCount` property and seeding through the indexer. Add `<Compile Include>` in `TaskMaster.Test.csproj`.
- `AppQuickFilerSettingsTests`, `AppQuickFilerSettingsRemainingPropertiesTests`, `AppStagingFilenamesTests`:
  - Construct the system under test with an `InMemorySettingsStore`.
  - Assert round-trips against the fake store, and assert that `SaveCount` incremented for saving setters (and did not for `EmailInfoStagingFile`).
  - Remove the `Settings.Default` snapshot and restore. Remove `[DoNotParallelize]` from `AppQuickFilerSettingsTests`, whose only shared state was `Settings.Default`.
- `RibbonControllerTests`:
  - `CreateController` sets `_quickFilerSettings` to `new AppQuickFilerSettings(store)` with a seeded fake.
  - Assertions read the fake store instead of `Settings.Default`.
  - Remove the snapshot and restore.
  - `[DoNotParallelize]` removal: the class doc attributes the attribute to the settings snapshot. The rest of the 452-line class and the `RibbonControllerTests.Engines.cs` partial must be read by the executor for other process-global state before removal. Keep the attribute if any other shared static is found and record why.
- `AppAutoFileObjectsCoverageExpansionTests`:
  - `CreateSutWithSpecialFolders` passes an `InMemorySettingsStore` seeded with the six scalar values.
  - `ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues` asserts against the fake.
  - Delete the `TestInitialize` and `TestCleanup` snapshot, including the direct `defaults.Save()` call.

### 5.5 Seam unit tests

New `UtilitiesCS.Test/HelperClasses/Settings/ApplicationSettingsStoreTests.cs` with a nested `RecordingApplicationSettings : ApplicationSettingsBase`. It overrides `public override object this[string]` and `public override void Save()` (both overridable per the dotnet/runtime source [V-web]), stores values in a dictionary, and counts saves. Because both overridden members bypass the base, no provider is invoked and no file is read or written. The tests cover:

- the null constructor argument
- get forwarding
- set forwarding
- save forwarding

Add a `<Compile Include>` entry.

## 6. Behavior Semantics and Requirements Mapping

- Success:
  - No unit test calls `ApplicationSettingsBase.Save()` on any `Properties.Settings.Default`.
  - No test awaits a wall-clock deadline on `AddQfcColumnsAsync`.
  - Production paths save exactly when they saved before (same condition, same property, same value).
- Ordering rule preserved in `GetNextToDoID`: add the ID, then persist the max length if it grew, then serialize the list.
- Edge cases:
  - An empty list's first ID always persists (unchanged).
  - A null store passed explicitly is rejected at construction (fail fast). Default constructors never hold null.
  - `AppStagingFilenames.EmailInfoStagingFile` must remain non-persisting.

| AC | Design element |
| --- | --- |
| AC1 | §5.1 test changes |
| AC2 | §5.2 IDList store with default adapter |
| AC3 | TaskTree.Test `MakeController` injection in both classes. `IDListGetNextToDoIDTests` converted. No other test reaches a real `IDList.GetNextToDoID`. |
| AC4 | §5.3 and §5.4 changes plus the evidence search (Numeric Derivation Evidence below) |
| AC5 | §8 fail-before strategy |
| AC6 | parallel CLI run with `TaskMaster.cli.runsettings`. No serialization added. `[DoNotParallelize]` removed where it existed only for settings. |
| AC7 | §9 coverage plan plus the full toolchain |

### Test-to-site reach map (production `Save()` sites reached by tests)

| Production site | Reaching test methods |
| --- | --- |
| IDList.cs:GetNextToDoID | TaskTreeControllerMoveLogicTests: `MoveObjectsToSibling_RootTarget_RemovesFromRootsAndReseeds`, `MoveObjectsToSibling_RootNotInRoots_FiresMessageSeam`, `MoveObjectsToSibling_ChildTarget_InsertsIntoParentChildren`, `MoveObjectsToChildren_RootSource_ReparentsAndRemovesFromRoots`, `MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam`, `MoveObjectsToChildren_NonRootSource_ReparentsUnderTarget`. TaskTreeControllerRouteDropTests: `RouteDrop_Item_RoutesToChildren_ReturnsTrue`, `RouteDrop_AboveItem_InsertsSiblingBeforeTarget_ReturnsTrue`, `RouteDrop_BelowItem_InsertsSiblingAfterTarget_ReturnsTrue` |
| AppQuickFilerSettings.cs:MoveEntireConversation | AppQuickFilerSettingsRemainingPropertiesTests.`MoveEntireConversation_Setter_RoundTripsThroughSettingsDefault` |
| AppQuickFilerSettings.cs:SaveAttachments | AppQuickFilerSettingsRemainingPropertiesTests.`SaveAttachments_Setter_RoundTripsThroughSettingsDefault` |
| AppQuickFilerSettings.cs:SavePictures | AppQuickFilerSettingsRemainingPropertiesTests.`SavePictures_Setter_RoundTripsThroughSettingsDefault` |
| AppQuickFilerSettings.cs:SaveEmailCopy | AppQuickFilerSettingsRemainingPropertiesTests.`SaveEmailCopy_Setter_RoundTripsThroughSettingsDefault` |
| AppQuickFilerSettings.cs:HighConfidenceModeEnabled | AppQuickFilerSettingsTests.`HighConfidenceModeEnabled_WhenSetTrue_ReadsBackTrue`. RibbonControllerTests: `ToggleHighConfidenceMode_FlipsStoredValue`, `SetHighConfidenceModeForLaunch_True_EnablesMode`, `StandardLaunchAfterHighConfidenceLaunch_DoesNotEnableMode` |
| AppQuickFilerSettings.cs:HighConfidenceThreshold | AppQuickFilerSettingsTests.`HighConfidenceThreshold_WhenSet_ReadsBackSameValue`. RibbonControllerTests.`SetHighConfidenceThresholdText_WithValidPercentage_PersistsProbability` |
| AppStagingFilenames.cs:ConditionalReminders | AppStagingFilenamesTests.`ConditionalReminders_Setter_RoundTripsThroughSettingsDefault` |
| AppStagingFilenames.cs:CommonWords | AppStagingFilenamesTests.`CommonWords_GetterAfterSet_ReturnsCachedValueWithoutReReadingSettings` |
| AppAutoFileObjects.cs (all six scalar setters) | AppAutoFileObjectsCoverageExpansionTests.`ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues` |

Test code that calls `Save()` directly: `AppAutoFileObjectsCoverageExpansionTests.TestCleanup` (`defaults.Save()`). It runs after every test in that class.

Unreached production sites (not in the family):

- `AppStagingFilenames` `SubjectMap`, `CtfInc`, `CtfMap`, `EmailSessionTemp`, `EmailSession`, `MovedMails`, and `RecentsFile` setters: no test assigns them, and `AppFileSystemFolderPaths` only constructs the class.
- `AppOlObjects.ApplyJunkFolderSelections`: the only test caller (`StoreWrapperControllerTests`) uses a `RecordingOlObjects` fake in UtilitiesCS.Test.
- `AppOlObjects.LoadJunkPotential`: no test.
- `AppOlObjects.LoadJunkCertain`: `AppOlObjectsTests.LoadJunkCertain_KeepsStoredValue_WhenReplacementSelectionIsCancelled` returns null before the save because `PickFolder` returns null.
- `AppOlObjects.DarkMode`: the QuickFiler.Test callers set `Ol.DarkMode` on `Mock<IOlObjects>`.

## Numeric Derivation Evidence

Complete Family: IDList.cs:GetNextToDoID, AppQuickFilerSettings.cs:MoveEntireConversation, AppQuickFilerSettings.cs:SaveAttachments, AppQuickFilerSettings.cs:SavePictures, AppQuickFilerSettings.cs:SaveEmailCopy, AppQuickFilerSettings.cs:HighConfidenceModeEnabled, AppQuickFilerSettings.cs:HighConfidenceThreshold, AppStagingFilenames.cs:ConditionalReminders, AppStagingFilenames.cs:CommonWords, AppAutoFileObjects.cs:LngConvCtPwr, AppAutoFileObjects.cs:Conversation_Weight, AppAutoFileObjects.cs:SmithWatterman_MatchScore, AppAutoFileObjects.cs:SmithWatterman_MismatchScore, AppAutoFileObjects.cs:SmithWatterman_GapPenalty, AppAutoFileObjects.cs:MaxRecents
Exhaustive Search Scope: the entire repository, every tracked *.cs file in all production projects (UtilitiesCS, ToDoModel, TaskMaster, QuickFiler, TaskTree, TaskVisualization, Tags, SVGControl) and all test projects (*.Test), covering every ApplicationSettingsBase subclass (the six Settings.Designer.cs classes) and every receiver form of Save() on them (Settings.Default.Save(), Properties.Settings.Default.Save(), and a cached field or local such as _defaults.Save() or defaults.Save())
Inclusion Rules: a production (non-test) member whose body calls Save() on an ApplicationSettingsBase instance, and that at least one test method executes up to that call under the test's own arrangement, traced through the concrete call chain
Exclusion Rules: Save() on Outlook items, views, project data, or other non-settings receivers; commented-out code (including the VB block in Tags/My Project/Settings.Designer.cs); production Save() sites that no test reaches (seven AppStagingFilenames setters, AppOlObjects.JunkFolders.cs ApplyJunkFolderSelections, LoadJunkPotential, LoadJunkCertain, and AppOlObjects.cs DarkMode); test-code Save() calls (AppAutoFileObjectsCoverageExpansionTests.TestCleanup, recorded separately and fixed)
Primary Search Strategy: forward from production. A repository-wide content search of all *.cs for the regular expression `\.Save\(\)` produced every Save() call (Outlook and settings mixed). Filtering to settings receivers through a search for `ApplicationSettingsBase` declarations and the field forms `Properties.Settings _defaults` and `= Properties.Settings.Default;` gave 26 settings sites. Each enclosing member was then traced to its test callers by searching test projects for its setter assignment or caller method, which yielded IDList.cs:GetNextToDoID, AppQuickFilerSettings.cs:MoveEntireConversation, AppQuickFilerSettings.cs:SaveAttachments, AppQuickFilerSettings.cs:SavePictures, AppQuickFilerSettings.cs:SaveEmailCopy, AppQuickFilerSettings.cs:HighConfidenceModeEnabled, AppQuickFilerSettings.cs:HighConfidenceThreshold, AppStagingFilenames.cs:ConditionalReminders, AppStagingFilenames.cs:CommonWords, AppAutoFileObjects.cs:LngConvCtPwr, AppAutoFileObjects.cs:Conversation_Weight, AppAutoFileObjects.cs:SmithWatterman_MatchScore, AppAutoFileObjects.cs:SmithWatterman_MismatchScore, AppAutoFileObjects.cs:SmithWatterman_GapPenalty, AppAutoFileObjects.cs:MaxRecents
Cross-check Search Strategy: backward from tests. A content search of every *.Test project for `Settings\.Default`, `new AppQuickFilerSettings`, `new AppStagingFilenames`, `new AppAutoFileObjects`, `new AppOlObjects`, `new IDList`, `IDList\)\.Returns`, `GetNextToDoID`, the RibbonController toggle and threshold helpers, and the DarkMode and junk-folder members. Each matching test was then traced forward through production call chains (TaskTreeController.MoveObjectsToSibling, TaskTreeController.MoveObjectsToChildren, TreeOfToDoItems.AddChild, TreeOfToDoItems.ReNumberChildrenIDs, RibbonController.Intelligence helpers) to the Save() it executes, which yielded IDList.cs:GetNextToDoID, AppQuickFilerSettings.cs:MoveEntireConversation, AppQuickFilerSettings.cs:SaveAttachments, AppQuickFilerSettings.cs:SavePictures, AppQuickFilerSettings.cs:SaveEmailCopy, AppQuickFilerSettings.cs:HighConfidenceModeEnabled, AppQuickFilerSettings.cs:HighConfidenceThreshold, AppStagingFilenames.cs:ConditionalReminders, AppStagingFilenames.cs:CommonWords, AppAutoFileObjects.cs:LngConvCtPwr, AppAutoFileObjects.cs:Conversation_Weight, AppAutoFileObjects.cs:SmithWatterman_MatchScore, AppAutoFileObjects.cs:SmithWatterman_MismatchScore, AppAutoFileObjects.cs:SmithWatterman_GapPenalty, AppAutoFileObjects.cs:MaxRecents
Primary Member Set: IDList.cs:GetNextToDoID, AppQuickFilerSettings.cs:MoveEntireConversation, AppQuickFilerSettings.cs:SaveAttachments, AppQuickFilerSettings.cs:SavePictures, AppQuickFilerSettings.cs:SaveEmailCopy, AppQuickFilerSettings.cs:HighConfidenceModeEnabled, AppQuickFilerSettings.cs:HighConfidenceThreshold, AppStagingFilenames.cs:ConditionalReminders, AppStagingFilenames.cs:CommonWords, AppAutoFileObjects.cs:LngConvCtPwr, AppAutoFileObjects.cs:Conversation_Weight, AppAutoFileObjects.cs:SmithWatterman_MatchScore, AppAutoFileObjects.cs:SmithWatterman_MismatchScore, AppAutoFileObjects.cs:SmithWatterman_GapPenalty, AppAutoFileObjects.cs:MaxRecents
Cross-check Member Set: IDList.cs:GetNextToDoID, AppQuickFilerSettings.cs:MoveEntireConversation, AppQuickFilerSettings.cs:SaveAttachments, AppQuickFilerSettings.cs:SavePictures, AppQuickFilerSettings.cs:SaveEmailCopy, AppQuickFilerSettings.cs:HighConfidenceModeEnabled, AppQuickFilerSettings.cs:HighConfidenceThreshold, AppStagingFilenames.cs:ConditionalReminders, AppStagingFilenames.cs:CommonWords, AppAutoFileObjects.cs:LngConvCtPwr, AppAutoFileObjects.cs:Conversation_Weight, AppAutoFileObjects.cs:SmithWatterman_MatchScore, AppAutoFileObjects.cs:SmithWatterman_MismatchScore, AppAutoFileObjects.cs:SmithWatterman_GapPenalty, AppAutoFileObjects.cs:MaxRecents
Primary Count: 15
Cross-check Count: 15
Member-set Comparison: the normalized primary and cross-check member sets are identical (15 equal members, no member present in only one set). The 26 total production settings Save() sites reconcile as 15 reached plus 11 unreached (seven AppStagingFilenames setters, three AppOlObjects.JunkFolders.cs members, one AppOlObjects.cs DarkMode setter), and the per-file totals are 1 + 6 + 9 + 3 + 1 + 6 = 26.

The AC4 DfDeedle half is a separate enumeration. Two tests call `AddQfcColumnsAsync` without either seam (the HappyPath and PreCancelledToken tests in `DfDeedle_COM_Tests`). The five `DfDeedleQfcColumnTimeoutTests` pass both seams. All three `GetEmailDataInViewAsync` test callers pass `timeProvider`.

## 7. Secondary Inventory (report only) and Scope Recommendations

| Item | Shared state | Saves? | Recommendation |
| --- | --- | --- | --- |
| `AppQuickFilerSettingsTests` `[DoNotParallelize]` | `Settings.Default` high-confidence values | via SUT | Same root cause; fixed in this item (store injection, remove attribute) |
| `RibbonControllerTests` `[DoNotParallelize]` | `Settings.Default` high-confidence values | via SUT | Same root cause; fix here. Remove the attribute only after confirming no other static state in the class. |
| `AppQuickFilerSettingsRemainingPropertiesTests`, `AppStagingFilenamesTests`, `AppAutoFileObjectsCoverageExpansionTests` (in-memory snapshot and restore) | `Settings.Default` | yes | Same root cause; fixed here |
| `ApplicationGlobalsStartupTimingTests` (per-method `[DoNotParallelize]`) and `ApplicationGlobalsTests` | `Settings.Default.StartupTimingEnabled`, in memory | no | Same family (shared static settings) but a different production path that this item does not touch. File a follow-up issue through the MCP promotion lifecycle. |
| `AppEventsTests` | `Settings.Default.EventsHooked`, in memory | no | Follow-up issue (same as above) |
| `AppAutoFileObjectsFolderPredictorTests` | `Settings.Default.UseLcppnPredictor`, in memory | no, but the old `AppAutoFileObjectsCoverageExpansionTests.TestCleanup` save could persist it concurrently | The concurrent-persist risk disappears with this item. The in-memory mutation goes to the follow-up issue. |
| `AppOlObjectsTests` (`JunkPotential`, `OlJunkCertain`) | in memory | no | Follow-up issue |
| `AppToDoObjectsTests` (`PropertyValues` injection into `Settings.Default`) | in memory | no | Follow-up issue |
| `DfDeedleQfcColumnTimeoutTests`, `DfDeedleEtlTimeoutTests`, and other UtilitiesCS.Test `[DoNotParallelize]` classes | log4net, UI-thread, and store-wrapper statics | no | Unrelated to settings; out of scope |
| `QfcDatamodel.GetEmailsInViewDfAsync` calls `GetEmailDataInViewAsync` without `timeProvider` | wall clock in production only | n/a | Its one test throws before any deadline. Not a test defect, so no action. |

No `Workers=1`, retry, `Thread.Sleep`, or `Task.Delay` usage in a test project exists to cope with shared settings. Workers settings are `0` in both runsettings files and in the `[assembly: Parallelize]` of UtilitiesCS.Test.

## 8. Fail-Before Strategy (AC5)

- IDList (TaskTree.Test): add a deterministic shared-settings probe assertion to `MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` before any production change.
  - Resolve `ToDoModel.Properties.Settings.Default` by reflection: `typeof(IDList).Assembly.GetType("ToDoModel.Properties.Settings")`, then the public static `Default` property cast to `ApplicationSettingsBase`.
  - Subscribe a `SettingsSaving` handler that records only when `Environment.CurrentManagedThreadId` equals the test thread's ID, and sets `e.Cancel = true`. `ApplicationSettingsBase.Save` skips `base.Save()` when cancelled [V-web], so the fail-before run writes nothing to user.config.
  - Unsubscribe in `finally`.
  - Assert that no save was recorded. Before the fix: fails (one save on the test thread). After `MakeController` injects the mock store: passes, and the mock verifies `Save()` once.
  - The handler is thread-attributed and removed in `finally`, so concurrent classes are not affected after the fix.
- TaskMaster setters: apply the same probe through `TaskMaster.Properties.Settings.Default.SettingsSaving` (accessible through `InternalsVisibleTo`) on one representative test per class: `AppQuickFilerSettingsTests`, `AppStagingFilenamesTests`, `AppAutoFileObjectsCoverageExpansionTests`. Run before and after.
- ToDoModel.Test seam tests: the new persistence-branch tests fail before the seam exists (compile failure). Prefer the runtime probe above as the recorded fail-before, and record the compile-stage result only as supplementary.
- DfDeedle: the original failure needs real wall-clock elapse under load and cannot be reproduced deterministically without timers. Record a `fail-before-exception.<timestamp>.md` dossier under `evidence/regression-testing/` with `WhyFailingRunImpossible`. Use the AC4 search as the repeatable alternative proof: it finds two seamless calls before and zero after, with the maintainer's captured exception text quoted from `issue.md`.

## 9. Testing and Coverage Implications (AC7)

- New seam code:
  - `ISettingsStore` is interface-only and absent from Cobertura.
  - `ApplicationSettingsStore` reaches 100% through `RecordingApplicationSettings`.
  - The IDList, AppQuickFilerSettings, AppStagingFilenames, and AppAutoFileObjects scalar members are covered by tests that inject fakes.
  - The default-constructor initializers (`new ApplicationSettingsStore(Properties.Settings.Default)`) execute in any test that uses a parameterless constructor. Construction reads nothing from disk until a property is read.
  - No `[ExcludeFromCodeCoverage]` and no human exemption decision are required.
- Changed lines: every rewritten getter and setter line is executed by the converted tests, so changed-line coverage does not drop.
- Parallel validation (AC6): run the coverage task with `scripts/vscode/TaskMaster.cli.runsettings` (`Workers=0`, `ClassLevel`). Run the affected classes repeatedly in that configuration. CI passes no runsettings, so CI does not exercise class-level parallelism. The local parallel run is the evidence.

## 10. File Change List and Production Estimate (Q7)

Production C# files (7):

1. `UtilitiesCS/HelperClasses/Settings/ISettingsStore.cs` (new)
2. `UtilitiesCS/HelperClasses/Settings/ApplicationSettingsStore.cs` (new)
3. `ToDoModel/Data Model/ID/IDList.cs`
4. `TaskMaster/AppGlobals/AppQuickFilerSettings.cs`
5. `TaskMaster/AppGlobals/AppStagingFilenames.cs`
6. `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (scalar properties removed)
7. `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` (new partial)

Build files (2): `UtilitiesCS/UtilitiesCS.csproj`, `TaskMaster/TaskMaster.csproj`. The `.csproj` files of ToDoModel, TaskTree, and QuickFiler are unchanged.

Test files:

- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` (made `partial`, section removed)
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs` (new)
- `UtilitiesCS.Test/HelperClasses/Settings/ApplicationSettingsStoreTests.cs` (new)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs`
- optionally `ToDoModel.Test/Data Model/ID/IDListMaxLengthPersistenceTests.cs` (new) with `ToDoModel.Test/ToDoModel.Test.csproj`
- `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs`
- `TaskTree.Test/TaskTreeControllerRouteDropTests.cs`
- `TaskMaster.Test/AppGlobals/InMemorySettingsStore.cs` (new)
- `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs`
- `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsRemainingPropertiesTests.cs`
- `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs`
- `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs`
- `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`
- `TaskMaster.Test/TaskMaster.Test.csproj`

Routing note: seven production C# files exceed the direct-path budget. Route through the orchestrator's large path (per `csharp-change-budget-router`).

## Automation Feasibility

No human interaction is required. Every step runs from the command line:

- the format, analyzer, and type-check toolchain
- the MSTest coverage run with `TaskMaster.cli.runsettings`
- the fail-before probes
- the AC4 searches

The issue's Visual Studio Test Explorer re-run is an optional manual confirmation. The CLI parallel run with the same `Workers=0`/`ClassLevel` settings replaces it as evidence. No coverage-exemption ratification is needed.

## Pre-Mortem

- Review checklists: A reviewer may flag the `public` widening of `IDList` (new constructor) and the new public `ISettingsStore`. Mitigation: the plan states that UtilitiesCS and ToDoModel grant no `InternalsVisibleTo` to the consuming assemblies (ToDoModel, TaskMaster, TaskTree.Test), so `public` is the minimum. XML docs state the contract. A reviewer may also flag `[DoNotParallelize]` removal on `RibbonControllerTests` without proof. Mitigation: a plan task reads the whole class and its partial for other statics and records the finding.
- Coverage: New seam members could fall below 90% if the adapter is tested only through Settings.Default, which is not allowed. Mitigation: `RecordingApplicationSettings` test subclass covers `ApplicationSettingsStore` fully. Persistence-branch tests cover both IDList branches. The coverage comparison reads per-file figures from the post-processed Cobertura projection.
- Mirror parity: Not applicable. No `.claude`, `.agents`, `.github/skills`, or bundled mirror file is changed, and the execution-environment brief states no fact this item alters.
- File size: `DfDeedle_COM_Tests.cs` (848 lines) and `AppAutoFileObjects.cs` (853 lines) are already over 500. Mitigation: both are made smaller by moving sections into new partial files. `RibbonControllerTests.cs` (452 lines) and `TaskTreeControllerMoveLogicTests.cs` (414 lines) must stay at or under 500. The plan records physical line counts (not `Measure-Object -Line`) before and after.
- Test isolation: The fail-before probe subscribes to a static `SettingsSaving` event. Mitigation: the handler is thread-attributed, cancels only to prevent disk writes, and is removed in `finally`. Tests create no temp files. No test reads or writes user.config after the fix. The new in-memory fake is per-test-instance.
- CI-only behavior: CI runs vstest without a runsettings file, so class-level parallelism is not exercised there, and a regression could pass CI while failing in Test Explorer. Mitigation: record a local parallel run with `TaskMaster.cli.runsettings` as AC6 evidence. Changes are pure C# with no path or OS assumptions, and the legacy projects need explicit `<Compile Include>` entries for the new files, which CI's cold build would otherwise catch as missing types.
- Evidence: The DfDeedle fail-before is not reproducible deterministically, and an absent dossier would block AC5. Mitigation: a `fail-before-exception.<timestamp>.md` dossier plus the AC4 before-and-after search outputs under `evidence/regression-testing/`. The probe-based fail-before runs for IDList and the TaskMaster setters are recorded there too. Evidence files carry `Timestamp`, `Command`, `EXIT_CODE`, and `Output Summary`, and no raw TRX or coverage document is committed.

## 11. Items for the Orchestrator to Confirm

- [I] The cross-process sharing of the testhost user.config path (§3) is inferred from documented `LocalFileSettingsProvider` behavior and the reported path. It does not change the design.
- [V-web] The `ApplicationSettingsBase` indexer and `Save()` overridability and the `SettingsSaving` cancel semantics were read from the dotnet/runtime source, which mirrors the .NET Framework reference source. Confirm on net481 when the first adapter test runs.
