# [P0-T17] Design preconditions re-derived against the current tree

Timestamp: 2026-09-08T00-51

Each of the five preconditions below was re-derived directly against the working tree in this run, not read from a cited artifact.

## 1. The `InternalsVisibleTo` grant

File: `UtilitiesCS/Properties/AssemblyInfo.cs`. Lines 18 through 20 read:

```
18 : [assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
19 : [assembly: InternalsVisibleTo("UtilitiesCS.Test")]
20 : [assembly: InternalsVisibleTo("ToDoModel.Test")]
```

Line 19 carries `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` as stated. A search of that file for `QuickFiler.Test` returned 0 matches, so no line of it names `QuickFiler.Test`.

Observed result: **as stated.** Any `internal` seam added to `UiThread` by this delivery is reachable from `UtilitiesCS.Test` and is not reachable from `QuickFiler.Test`, which is why decision D3 adds no grant and every AC4 test lives in `UtilitiesCS.Test`.

## 2. Default apartment state under MSTest

File: `UtilitiesCS.Test/test.runsettings`, read in full:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!-- Global STA execution is intentionally disabled.
     Tests that require an STA apartment must opt in with MSTest's
     STATestMethod or STATestClass attributes so the rest of the suite can run
     under the default threading model and participate in parallel execution. -->
<RunSettings />
```

The file is `<RunSettings />` with a comment stating that global STA execution is intentionally disabled.

Search performed: `ExecutionThreadApartmentState` over `**/*.runsettings`. Result: **no match**.

Observed result: **as stated.** A plain `[TestMethod]` therefore runs MTA, so it supplies the AC1 rejection case directly, and `[STATestMethod]` / `[STATestClass]` supplies the acceptance case.

## 3. The no-live-Form assertion in `UtilitiesCS.Test`

File: `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs`, line 17:

```
public void ExecutingAssembly_ContainsNoFormDerivedType()
```

Observed result: **as stated.** No type added to `UtilitiesCS.Test` by this delivery may derive from `System.Windows.Forms.Form`. This binds `FakeUiCaptureSource` in [P2-T1] and `UiThreadStateScope` in [P1-T7].

## 4. `ThreadSafeSingleShotGuard` consumers

Search performed: `ThreadSafeSingleShotGuard` over `**/*.cs`. Result: **23 files**, enumerated:

`UtilitiesCS/Threading/UiThread.cs`; `UtilitiesCS/Threading/ThreadSafeSingleShotGuard.cs`; `UtilitiesCS/Threading/ProgressTracker.cs`; `UtilitiesCS/Threading/IdleAsyncQueue.cs`; `UtilitiesCS/Threading/IdleActionQueue.cs`; `UtilitiesCS/Threading/ApplicationIdleTimer.cs`; `UtilitiesCS/ReusableTypeClasses/TimedActions/TimedBatchAction.cs`; `UtilitiesCS/ReusableTypeClasses/TimedActions/TimedAsyncTask.cs`; `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/ScDictionary.cs`; `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/ScBag.cs`; `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableBase.cs`; `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs`; `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.Serialization.cs`; `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`; `UtilitiesCS/EmailIntelligence/Flags/FlagConsolidator.cs`; `UtilitiesCS/EmailIntelligence/Flags/FlagClassNoItem.cs`; `UtilitiesCS/EmailIntelligence/Bayesian/CorpusInherit.cs`; `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierShared.cs`; `UtilitiesCS.Test/Threading/ThreadSafeSingleShotGuard_Tests.cs`; `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`; `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs`; `TaskVisualization/FlagChangeTrainingQueue.cs`; `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`.

Observed result: **as stated, 23 files.** Removing `UiThread.cs` from that set still leaves 22 files referencing the type, which is why decision D2 retains it rather than deleting it.

## 5. Legacy non-SDK projects with explicit compile items

`UtilitiesCS/UtilitiesCS.csproj:1112` carries `<Compile Include="Threading\UiThread.cs" />`.
`UtilitiesCS/UtilitiesCS.csproj:1110` carries `<Compile Include="Threading\ThreadSafeSingleShotGuard.cs" />`, which is the sibling item [P1-T2] adds beside.
`UtilitiesCS.Test/UtilitiesCS.Test.csproj:503` carries `<Compile Include="Threading\UiThread_Tests.cs" />`.
`UtilitiesCS.Test/UtilitiesCS.Test.csproj:76` carries `<Compile Include="TestHelpers\UiThreadDispatcherScope.cs" />`, which is the sibling item [P1-T8] adds beside.

Observed result: **as stated.** Both projects are legacy non-SDK `packages.config` projects with explicit compile items, so a source file that is not listed does not compile and its tests silently do not exist.

## Result

DESIGN_PRECONDITION_FAILURES: 0
