# P4-T12 — Determinism audit of the added test code (AC12)

Timestamp: 2026-09-01T20-20
Command: `Select-String -Path 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs' -SimpleMatch '<literal>'` for each of `Thread.Sleep`, `Task.Delay`, `SpinWait` and `DateTime.Now`
EXIT_CODE: 0

## The four banned-API searches

| Literal | Matches across both files |
| --- | --- |
| `Thread.Sleep` | 0 |
| `Task.Delay` | 0 |
| `SpinWait` | 0 |
| `DateTime.Now` | 0 |

**All four return zero**, so AC12's determinism requirement holds. `Thread.Sleep`, `Task.Delay` and `DateTime.Now` are named as banned in test code by `.claude/rules/general-unit-test.md`; `SpinWait` is included because a spin-wait is a polling loop by another name.

## The zero results are not vacuous

A zero-match search proves nothing unless the same search form demonstrably reaches the files it claims to have scanned. Two control literals were searched with the identical multi-file `Select-String -SimpleMatch` invocation:

| Control literal | Matches |
| --- | --- |
| `TaskCompletionSource` | 1 |
| `HarnessController` | 6 |

Both return non-zero, and `HarnessController` appears in both files, so the multi-file path argument genuinely resolves to both files and the search mechanism works. The four zeros above are therefore real absences rather than an artifact of a mistyped path, an unreadable file, or a search that matches nothing by construction.

## The only wait in the pump-hosted test

Quoted verbatim from the test body, `QfcItemController.InitializationTests.Part3.cs` line 370:

    System.Exception fault = await observed.Task.ConfigureAwait(false);

`observed` is a `TaskCompletionSource<System.Exception>` created with `TaskCreationOptions.RunContinuationsAsynchronously` and completed from the sink callback via `observed.TrySetResult(e)`. The wait is therefore on a deterministic completion signal raised by the code under test, not on wall-clock time and not on a polling interval. There is exactly one such wait, and it is the only wait in the test.

The test carries `[Timeout(PumpTimeoutMs)]`, where `PumpTimeoutMs` is the existing 60000 constant shared by the pump-hosted tests in this class. That attribute is not a wait mechanism: its documented role in this file is to convert a genuine deadlock in production code into a test failure rather than a CI hang. If the sink is never invoked the test fails on timeout, which is the correct diagnosis of a broken observation path rather than a masked one.

## The other three added tests

The remaining three added tests contain no wait at all and are effectively synchronous:

- `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault` awaits the guard directly. Because the supplied `SynchronizationContext` is installed as current before the Act, the await at `ViewerSetup.cs:64` continues inline and the mocked seam faults immediately. It carries no `[Timeout]` because it uses no pump.
- `WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing` is fully synchronous: it invokes the default sink delegate and asserts it does not throw.
- `InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink` cancels its `CancellationTokenSource` before the Act, so `Token.ThrowIfCancellationRequested()` throws before any seam call is reached. There is no timing dependency: the cancellation is observed on the first statement of `InitializeWebViewAsync`.

## Ambient-state hygiene

The directly-awaited test installs a `SynchronizationContext` as `SynchronizationContext.Current` during Arrange and restores the previous value in a `finally`. The restore is load-bearing for test independence rather than cosmetic: without it the installed context would leak onto a pooled thread and could reach an unrelated test running later, which would violate the independence and determinism requirements of `.claude/rules/general-unit-test.md`. No test mutates any other ambient or static state.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
