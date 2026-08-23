# P1-T7 — WinFormsPumpHost Seam Self-Tests

Issue: #230
Task: [P1-T7]

## Step 1 — Build

- Timestamp: 2026-08-07T22-05
- Command: `MSBuild.exe QuickFiler.Test/QuickFiler.Test.csproj -t:Build -p:Configuration=Debug -p:Platform="AnyCPU" -v:m`
- EXIT_CODE: 0
- Output Summary: Build succeeded, 0 errors. Output produced at
  `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`. This also satisfies the P1-T2
  clause that the new `<Compile Include>` entries build: both
  `TestSupport\WinFormsPumpHost.cs` and `TestSupport\WinFormsPumpHostTests.cs`
  compiled into the assembly.

## Step 2 — Filtered test run (D6 command form)

- Timestamp: 2026-08-07T22-05
- Command:
  ```powershell
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~WinFormsPumpHostTests"
  ```
- EXIT_CODE: 0
- Output Summary: **Total tests: 13 — Passed: 13, Failed: 0.** Total time 1.6906
  seconds. Test parallelization ClassLevel, Workers 24. Resolved vstest:
  `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
  (VSTest 18.8.0 x64).

### Executed tests

| Test | Result | Time | Covers |
|---|---|---|---|
| `Constructor_WhenHostStarts_CapturesWinFormsContextOnADistinctThread` | Passed | 108 ms | API shape / readiness handshake (S-AC1) |
| `InvokeAsyncAction_WhenPosted_RunsOnThePumpThread` | Passed | 5 ms | Thread identity, `InvokeAsync(Action)` (S-AC2) |
| `InvokeAsyncFactory_WhenPosted_RunsOnThePumpThreadAndReturnsTheValue` | Passed | 2 ms | Thread identity + return value, `InvokeAsync<T>` (S-AC2) |
| `RunAsyncVoid_WhenPosted_StartsAndResumesOnThePumpThread` | Passed | 4 ms | Start and post-await resume on the pump (S-AC2) |
| `RunAsyncResult_WhenPosted_RunsOnThePumpThreadAndReturnsTheValue` | Passed | 5 ms | Thread identity + unwrapped result, `RunAsync<T>` (S-AC2) |
| `AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread` | Passed | 3 ms | `await host.SyncContext` via `UiThread.GetAwaiter` (S-AC2, U-AC1) |
| `BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread` | Passed | 49 ms | **WPF-dispatcher interop smoke test** (S-AC3) |
| `InvokeAsync_WhenWorkThrows_FaultsTheAwaitedTaskWithTheOriginalException` | Passed | 65 ms | Synchronous-throw fault channel (S-AC2) |
| `RunAsyncVoid_WhenWorkFaults_SurfacesTheOriginalUnwrappedException` | Passed | 3 ms | Async fault, unwrapped (S-AC2) |
| `RunAsyncResult_WhenWorkFaults_SurfacesTheOriginalUnwrappedException` | Passed | 4 ms | Async fault, generic overload (S-AC2) |
| `PostingMembers_AfterStop_FaultWithObjectDisposedException` | Passed | 3 ms | Post-after-stop fails fast, all four members (S-AC2) |
| `Dispose_CalledTwice_IsANoOp` | Passed | 2 ms | Idempotent disposal (S-AC2) |
| `StopAsync_WhenThePumpLoopRecordedAnException_RethrowsIt` | Passed | 4 ms | `Application.ThreadException` recorder rethrown at `StopAsync` (S-AC2) |

## Verified properties

- **S-AC1 (host class / API shape / net481).** `WinFormsPumpHost` is an
  `internal sealed class ... : IDisposable` with a readiness-handshake
  constructor, `SyncContext`, `ThreadId`, both `InvokeAsync` overloads, both
  `RunAsync` overloads, `StopAsync`, and an idempotent `Dispose`. It compiles for
  `TargetFrameworkVersion v4.8.1` and contains no `init` accessor, `record`, or
  `record struct` (D4). File is 482 lines.
- **S-AC2 (seam self-tests).** Thread identity for all four posting members and
  for `await host.SyncContext`; both fault channels on the awaited task;
  post-after-stop `ObjectDisposedException`; double-`Dispose` no-op; recorded
  `Application.ThreadException` rethrown by `StopAsync` — all covered and passing.
- **S-AC3 (WPF-interop smoke test).** `BothMarshalRoutes_...` proves a WPF
  `Dispatcher` created on the pump thread is serviced by the WinForms message loop
  (`Dispatcher.FromThread(pump).InvokeAsync` returns the pump thread id) and that
  `await host.SyncContext` resumes on the same thread. The interop
  `Initialize(bool)`'s tail relies on is therefore established before any
  controller test depends on it.
- **U-AC1 (deterministic await without touching the MSTest context).** No test
  calls `SynchronizationContext.SetSynchronizationContext` on the MSTest thread and
  the host never mutates `WindowsFormsSynchronizationContext.AutoInstall`. All
  waits are on `ManualResetEventSlim`, `TaskCompletionSource<T>` with
  `RunContinuationsAsynchronously`, or the member's own returned `Task`. No
  `Thread.Sleep`, `Task.Delay`, or polling appears in either file.
- **U-AC2 (usage contract demonstrated by the self-test file).** The file shows
  construction, `using`/`finally` release, all four posting members, awaiting the
  context directly, both fault channels, `StopAsync` fault surfacing, and the
  post-after-stop and double-dispose edges — the complete contract, adoptable by
  example.
