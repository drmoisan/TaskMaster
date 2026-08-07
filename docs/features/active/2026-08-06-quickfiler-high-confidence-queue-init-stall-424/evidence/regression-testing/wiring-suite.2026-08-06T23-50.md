# [P4-T8] Wiring Suites — Full Run

- **Issue:** #424
- **Task:** [P4-T8]
- **Scope:** `QfcHomeControllerRunAsyncHighConfidenceTests.cs` (partial of `QfcHomeControllerRunAsyncTests`), `QfcScanProgressBandMapperTests.cs`, both gate test files, and `QfcDatamodelTests.cs`

Timestamp: 2026-08-06T23-50

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcHomeControllerRunAsyncTests|FullyQualifiedName~QfcScanProgressBandMapperTests|FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests|FullyQualifiedName~QfcDatamodelTests"`

EXIT_CODE: 0

Output Summary:

```
Test Run Successful.
Total tests: 54
     Passed: 54
```

Filter clauses are joined with `|` because vstest 18.x rejects `OR` inside `/TestCaseFilter` (Decisions Record item 12).

## Pass counts per suite

| Suite | Tests | Result |
|---|---|---|
| `QfcStreamingDequeueConfidenceGateTests` (+ `.Part2.cs`) | 21 | all passed |
| `QfcDatamodelTests` | 12 | all passed |
| `QfcScanProgressBandMapperTests` | 11 | all passed |
| `QfcHomeControllerRunAsyncTests` (incl. the high-confidence partial) | 10 | all passed |
| **Total** | **54** | **54 passed, 0 failed** |

Individual results for the home-controller suite:

```
Passed HighConfidencePreFilterLoader_CanBeOverridden_ForTesting [213 ms]
Passed RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue [162 ms]
Passed RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload [54 ms]
Passed RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly [2 ms]
Passed RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand [20 ms]
Passed RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration [2 ms]
Passed Run_ExecutesCorrectly [4 ms]
Passed Run_HighConfidenceEnabled_DoesNotLoadUnfilteredInitialBatch [2 ms]
Passed RunAsync_ExecutesCorrectly [5 ms]
Passed Worker_RunWorkerCompleted_HandlesCompletionCorrectly [131 ms]
```

## Changes delivered in Phase 4

**[P4-T2] `QuickFiler/Controllers/QfcScanProgressBandMapper.cs`** (new, `internal sealed`, wired into `QuickFiler.csproj`). Constructor takes `Action<double, string> report` with an `ArgumentNullException` guard — `double` is required so `ProgressTracker.Report(double value, string jobName)` (`ProgressTracker.cs:121`) binds by method-group conversion; an `Action<int, string>` target would be CS0123. `Report(int scanned, int accepted, int quantity)` computes `min(30, round(30.0 * accepted / quantity))`, treats `quantity <= 0` as 0, clamps to `[0, 30]`, and holds the previous value if a computed value would decrease. No UI, thread, or COM references.

**[P4-T4] `QuickFiler/Interfaces/IQfcDatamodel.cs`** gains `DequeueNextItemGroupAsync(int quantity, int timeOut, TimeSpan firstBatchDeadline, Action<int, int, int> progress)`. The existing two-argument overload keeps its exact signature and delegates to the new one with `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` and a null sink. `QfcDatamodel` is the only implementer, and all 18 `Mock<IQfcDatamodel>` sites are loose-behavior, so no other implementer or strict mock required updating.

**Inherited-path deadlines (deliberate, per [P4-T4]):** the post-UI iteration call site (`QfcHomeController.Iteration.cs:23`, `DequeueNextItemGroupAsync(itemsPerIteration, 2000)`) and the legacy synchronous `DequeueNextItemGroup(int)` path both now inherit `DefaultFirstBatchDeadline` (12 s) through the delegation and parameter defaults. This is a behavior change, recorded rather than claimed to be a no-op. The `QfcHomeControllerIterationTests.cs:268` exact-argument pin is unaffected — it mocks `IQfcDatamodel`, so no real gate executes — and `[P5-T1]` verifies it still passes.

**[P4-T5] `QuickFiler/Controllers/QfcHomeController.cs`** `RunAsync` constructs the mapper over `progress.Report` and calls the new overload with `200` (O1 adopted at this pre-UI call site only), the default deadline, and `scanProgress.Report`. The added code is wiring only; all mapping logic lives in the mapper. `QfcHomeController.Iteration.cs` is untouched.

**[P4-T6]** The exact-argument mock/verify moved from `DequeueNextItemGroupAsync(itemsPerIteration, 1000)` to the four-argument overload with `200`, `DefaultFirstBatchDeadline`, and a non-null sink. The `InitEmailQueueAsync(0, ...)`-once assertion, the no-unfiltered-first-page assertion, and both disabled-mode overload-discipline tests are unchanged in behavior and still pass.

**[P4-T7]** Two new tests: the band-mapping test scripts a five-signal scan through the captured sink and asserts every report between the `(0, "Initializing Email Queue")` and `(30, "Initializing Qfc Items")` reports lies in `[0, 30]`, carries the scanning label, and is monotonically non-decreasing; the empty-batch test asserts the sink is still invoked, the empty list reaches `LoadItemsAsync`, and background iteration is still initiated.

## Toolchain state

| Step | Command | EXIT_CODE |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 (`Formatted 1482 files`) |
| Analyzers | `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (0 errors) |
| Nullable | `msbuild TaskMaster.sln ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 (0 errors) |
| Tests | scoped vstest run above | 0 (54/54) |

## File sizes after Phase 4

| File | Lines | Status |
|---|---|---|
| `QuickFiler/Controllers/QfcHomeController.cs` | 487 | within limit |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 177 | within limit |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 59 | within limit |
| `QuickFiler/Controllers/QfcScanProgressBandMapper.cs` | 78 | within limit |

`QfcStreamingDequeueConfidenceGateTests.Part2.cs` (584) and `QfcDatamodelTests.cs` (529) remain above the limit pending the pre-decided relocations in `[P5-T2]`.
