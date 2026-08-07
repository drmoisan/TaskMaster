# [P3-T4] Producer-Liveness Regression Test — PASS AFTER

- **Issue:** #424
- **Task:** [P3-T4]
- **Test:** `QuickFiler.Controllers.Tests.QfcDatamodelTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle`
- **Production state:** FIXED by `[P3-T3]` — the datamodel-owned `volatile bool _remainingLoadActive` replaces `_worker?.IsBusy` as the producer-liveness signal.

Timestamp: 2026-08-06T23-26

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle"`

EXIT_CODE: 0

Output Summary:

```
Passed DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle [156 ms]
Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 1.1831 Seconds
```

## Fail-before / pass-after pair

| Run | Producer-liveness signal | Observation | EXIT_CODE |
|---|---|---|---|
| `[P3-T2]` fail-before | `() => _worker?.IsBusy == true` | dequeue completed early with an empty batch while the loader was still producing (`Expected pending.IsCompleted to be False ... but found True`) | 1 |
| `[P3-T4]` pass-after | `() => _remainingLoadActive` | dequeue keeps polling while the loader is live, then exits on genuine exhaustion once the loader completes | 0 |

The same unmodified test method produces both results; only production code changed between them.

## Changes verified by this test

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`
- New field: `private volatile bool _remainingLoadActive;` — `volatile` because it is written on the worker thread and read by dequeue callers.
- `DequeueWithHighConfidenceGateAsync` now passes `() => _remainingLoadActive` as the gate's `sourceActive` signal (was `() => _worker?.IsBusy == true`).
- `WaitForQueue` now loops on `_remainingLoadActive` (was `_worker.IsBusy`).

`QuickFiler/Controllers/QfcDatamodel.cs`
- `_remainingLoadActive = true;` immediately before **both** `worker.RunWorkerAsync()` call sites — the issue #244 zero-batch short-circuit and the positive-batch path. Setting before the start closes the window where a dequeue running ahead of `Worker_DoWork` would see a false signal.
- `_remainingLoadActive = false;` in a `finally` wrapping `await RemainingEmailLoader(_token)` inside `Worker_DoWork`. Because the method is `async void`, this continuation is the only point that truthfully marks the end of production; the `finally` also covers the throwing path.

No other `BackgroundWorker` lifecycle rework was performed. `QfcDatamodel` remains `[ExcludeFromCodeCoverage]` and gained only flag set/clear plus wiring — no new decision logic, consistent with the spec's coverage boundary.

## AC 7 conformance

> "...remains true across the `async void` `Worker_DoWork` first-await boundary while `LoadRemainingEmailsToQueueAsync` is still producing, becomes false only after the loader completes (cleared in a `finally`), and is the signal consumed by `sourceActive` in `QfcDatamodel.QueueProcessing.cs` in place of `_worker?.IsBusy`."

All three clauses hold. The flag is cleared in a `finally` around the awaited loader invocation rather than inside `LoadRemainingEmailsToQueueAsync` itself (plan Decisions Record item 3). This is behaviorally identical for AC 7 — the `async void` continuation runs exactly when the awaited loader completes — and it is what makes the transition observable through the `RemainingEmailLoader` seam the spec's own Test Strategy prescribes, since tests substitute the loader and never execute the real method body.

## Toolchain state

| Step | Command | EXIT_CODE |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 (`Formatted 1480 files`) |
| Analyzers | `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (0 errors) |
| Nullable | `msbuild TaskMaster.sln ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 (0 errors) |

File sizes after this change: `QfcDatamodel.cs` 496 lines, `QfcDatamodel.QueueProcessing.cs` 150 lines — both within the 500-line limit (verified again in `[P5-T2]`).
