# [P1-T19] Cover the Two Exception Branches of `IterateQueueAsync`

Timestamp: 2026-08-26T09-39

Task: [P1-T19]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` — added three tests that reach the
exception branches of `QuickFiler/Controllers/QfcHomeController.Iteration.cs`. Those branches were
`:38-52` at base and are now `:42-56`, shifted by the four lines `[P1-T15]` added to the caller;
the code in them is unchanged.

| Test | Arrangement | Branch reached |
| --- | --- | --- |
| `IterateQueueAsync_DequeueThrowsOperationCanceled_SwallowsAndReturns` | dequeue throws `OperationCanceledException` | `catch (OperationCanceledException)` |
| `IterateQueueAsync_DequeueThrowsWhenTokenCancelled_SwallowsAndReturns` | dequeue cancels the controller token from inside its callback, then throws `InvalidOperationException` | `catch (System.Exception)` with `Token.IsCancellationRequested` true |
| `IterateQueueAsync_DequeueThrowsWhenTokenNotCancelled_Rethrows` | dequeue throws `InvalidOperationException` with no cancellation pending | `catch (System.Exception)` `else` — rethrow |

The second test cancels from **inside** the callback, as the task requires: the entry guard
`Token.ThrowIfCancellationRequested()` sits outside the `try`, so a token already cancelled at entry
escapes the method uncaught and never reaches the `IsCancellationRequested` branch. The controller's
private `_token` field is assigned through the file's `SetPrivateField` helper so the test and the
production code share one `CancellationTokenSource`.

All three are green in both the pre-fix and post-fix states and are NOT tagged `[expect-fail]`.

### Reuse of `ArrangeIterate` and the 500-line cap

The task requires reusing `ArrangeIterate` so the file stays under the 500-line cap. After
`[P1-T15]`, `[P1-T16]` and `[P1-T17]` the file stood at 513 lines, over the cap, so this task also
performed the reductions below. None changes the behaviour of any test.

- `ArrangeIterate` gained `outcome` (`Func<Task<QfcDequeueBatch>>`), which replaces the dequeue
  result outright and is what lets each exception test arrange a throwing dequeue in one call.
- `ArrangeIterate`'s `quantity` and `timeOut` matchers became optional, defaulting to the new
  `AnyValue` matcher. Every call site that passed `q => true, t => true` now omits them. The two
  pinned call sites are unchanged and still reproduce their concrete arguments: `q => q == 8` with
  `t => t == 2000` (line 270), and `q => q == itemsPerIteration` with
  `itemsPerIteration: itemsPerIteration` (lines 339-340). The base `It.IsAny<int>()` timeout on the
  second of those is now expressed as the `AnyValue` default, which is the same matcher semantics.
- Call sites use tuple deconstruction instead of three separate local assignments.
- Four raw `_controller.GetType().GetField(...).SetValue(...)` blocks were replaced by one
  `SetPrivateField(string, object)` helper.
- The two repeated queue verifications became `VerifyCompleteAdding` and `VerifyEnqueue` helpers.
  Their `times` parameter is `Func<Times>`, not `Times`, because `Times.Never` and `Times.Once` are
  static methods in Moq 4 and a method group cannot bind to a `Times` parameter.
- The three `mockDataModel.Verify` expressions that `[P1-T15]` retargeted are **untouched**: each
  still names `DequeueNextItemGroupWithOutcomeAsync` inline with its `Times.Never`, `Times.Once`
  and `Times.Once` argument, so `[P1-T15]`'s acceptance still holds against the current tree.
- XML doc comments on the helpers and on the tests added by `[P1-T16]`, `[P1-T17]` and this task
  were shortened. Assertion reason strings were left byte-identical so the failure message quoted in
  the `[P1-T16]` artifact remains reproducible; the `p1-t16` and `p1-t17` TRX files were re-run
  against this final tree and reproduce their recorded outcomes exactly.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~QfcHomeControllerIterationTests" "/Logger:trx;LogFileName=p1-t19.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t19"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t19/p1-t19.trx`

TRX counters: `total="13" executed="13" passed="12" failed="1"`.

The three named tests are recorded **Passed**:

```
Passed IterateQueueAsync_DequeueThrowsOperationCanceled_SwallowsAndReturns
Passed IterateQueueAsync_DequeueThrowsWhenTokenCancelled_SwallowsAndReturns
Passed IterateQueueAsync_DequeueThrowsWhenTokenNotCancelled_Rethrows
```

The single failed test is `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding`,
which `[P1-T16]` tagged `[expect-fail]` and which stays Failed until `[P2-T7]` lands the
`SourceExhausted` guard. No other test failed.

Command: `(Get-Content "QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs").Count`
EXIT_CODE: 0

| File | Condition | Post-change | Result |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | at most 500 | **494** | PASS |

## Output Summary

The two exception branches of `IterateQueueAsync` are now driven by three passing tests, which is
what makes the `[P5-T7]` changed-file coverage gate on
`QuickFiler/Controllers/QfcHomeController.Iteration.cs` reachable rather than merely demanding: the
eleven uncovered occurrences recorded at base (`lines=80 covered=69 rate=86.25`) are all inside
those branches. Format EXIT_CODE 0, compile EXIT_CODE 0, scoped run EXIT_CODE 1 with 13 executed,
12 passed and the single expected `[expect-fail]` failure. The file finishes at 494 of the 500-line
cap.
