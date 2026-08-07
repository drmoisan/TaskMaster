# [P3-T2] Producer-Liveness Regression Test — FAIL BEFORE

- **Issue:** #424
- **Task:** [P3-T2] `[expect-fail]`
- **Test:** `QuickFiler.Controllers.Tests.QfcDatamodelTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle`
- **Test file:** `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`
- **Production state:** UNMODIFIED with respect to the liveness flag. `QfcDatamodel.QueueProcessing.cs:87` still reads `() => _worker?.IsBusy == true`; no `_remainingLoadActive` field exists. The test compiles against the pre-fix surface and references no new member.

Timestamp: 2026-08-06T23-20

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle"`

EXIT_CODE: 1

Output Summary:

```
Failed DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle [262 ms]
Error Message:
 Expected pending.IsCompleted to be False because the loader is still producing, so the gate
 must keep polling rather than treat an empty queue as an exhausted source and return an early
 partial batch, but found True.

Test Run Failed.
Total tests: 1
     Failed: 1
```

Assertion site: `QfcDatamodelTests.cs:387`.

## Why this is the latent defect

`Worker_DoWork` is `async void` (`QfcDatamodel.cs:173`). It returns to the `BackgroundWorker` at its first yielding await — `await RemainingEmailLoader(_token)` at `QfcDatamodel.cs:187` — so `BackgroundWorker.IsBusy` goes **false near the start of the load** while `LoadRemainingEmailsToQueueAsync` is still producing into the master queue.

`DequeueWithHighConfidenceGateAsync` passed that value to the gate as its producer-liveness signal:

```csharp
() => _worker?.IsBusy == true      // QfcDatamodel.QueueProcessing.cs:87
```

The gate treats `sourceActive == false` plus an empty queue as **source exhausted** and returns immediately. The test drives exactly that state and observes the early return: after two 200 ms poll intervals the dequeue task had already completed with an empty batch, even though the loader was still held open and producing.

This is the latent defect `spec.md` Root Cause Analysis and research §2.8 identify as one the fix must repair to be sound — both the new deadline exit and the pre-existing exhaustion exit depend on this signal being truthful.

## Determinism of the fail-before observation

The pre-fix `IsBusy == false` state is reached through an asynchronously posted `BackgroundWorker` completion, so it is not synchronously observable. The test makes the observation deterministic rather than racy:

1. `model.InitEmailQueue(0, worker)` — the issue #244 zero-batch short-circuit (`QfcDatamodel.cs:238-243`), which is COM-free and still calls `SetupWorker` + `RunWorkerAsync`.
2. `RemainingEmailLoader` is replaced with a delegate held open by a `TaskCompletionSource`, so the producer provably never finishes during the assertion window.
3. `loaderEntered.Task.Wait(TimeSpan.FromSeconds(5))` — bounded, event-driven proof that the worker actually reached the loader (the repo-established pattern at `QfcInitEmailQueueZeroBatchTests.cs:160-163`).
4. `WaitForState(() => !worker.IsBusy, ...)` — bounded `SpinWait.SpinUntil` state wait for the async-void return. Not a fixed sleep: it returns the instant the transition occurs and fails with a clear message if it never does.
5. Only then is `DequeueNextItemGroupAsync(1, 200)` invoked, with all poll intervals advanced through the injected `FakeTimeProvider`.

No `Thread.Sleep`, no `Task.Delay`, no wall-clock waits, no Outlook COM, no temp files.

## Toolchain state at capture

| Step | Command | EXIT_CODE |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 (`Formatted 1480 files`) |
| Analyzers | `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (0 errors) |

The failing result is a genuine behavioral assertion failure, not a compile or configuration artifact. The pass-after counterpart is recorded by `[P3-T4]`.
