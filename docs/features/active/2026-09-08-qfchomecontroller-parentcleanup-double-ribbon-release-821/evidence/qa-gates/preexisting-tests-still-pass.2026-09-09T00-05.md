# Phase 6 — Pre-existing tests the spec pins still pass

Timestamp: 2026-09-09T13-56
Task: [P6-T7]

Source of these results: the `[P6-T5]` captured run, EXIT_CODE 0, 7208 total, 7208 passed, 0 failed.

## The seven pinned tests

| # | Test | Result line, verbatim | What it pins |
|---|---|---|---|
| 1 | `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup` | `Passed Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup [59 ms]` | A throwing teardown stage still reaches the callback |
| 2 | `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` | `Passed Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource [< 1 ms]` | Cleanup nulls the token-source field |
| 3 | `Cleanup_ClearsControllerFieldsAndInvokesParentCleanup` | `Passed Cleanup_ClearsControllerFieldsAndInvokesParentCleanup [< 1 ms]` | The Efc boolean-flag assertion still compiles and holds |
| 4 | `CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick` | `Passed CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick [500 ms]` | The `CancelSource` setter's existing enabling behaviour is unchanged |
| 5 | `CancelPath_WhenInvoked_CancelsTokenSource` | `Passed CancelPath_WhenInvoked_CancelsTokenSource [1 ms]` | The viewer's cancel path still cancels a live source |
| 6 | `CancelSource_SetterAndGetter_RoundTripAssignedValue` | `Passed CancelSource_SetterAndGetter_RoundTripAssignedValue [1 ms]` | The property still round-trips |
| 7 | `CancelButtonClick_WhenInvoked_CancelsTokenSource` | `Passed CancelButtonClick_WhenInvoked_CancelsTokenSource [16 ms]` | The pane's cancel path still cancels a live source |

**All seven are reported passed.** Each name returned exactly one match in the captured output, and
every match is prefixed `Passed`.

## Why row 1 is the direct proof AC15 requires

`Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup` sets up a datamodel mock whose `Cleanup()`
throws `InvalidOperationException`, then asserts that `controller.Cleanup()` does not propagate and
that the parent-cleanup mock was still invoked exactly once. It passes only if the two
`catch (System.Exception e)` blocks remain separate and neither encloses the `finally` — restructuring
the try/catch/finally to "be safe" would break it.

This is the behavioural half of AC15. The structural half is recorded by `[P2-T5]`: exactly two
`catch (System.Exception e)` matches, still at lines 382 and 399, unmoved from the `[P0-T14]`
baseline, and `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` absent from every change
span.

## Rows 4 to 7 and the two behaviour changes this fix makes

Rows 4 and 5 pass against the rewritten `SetCancellationTokenSource`, which now delegates to the
`CancelSource` property setter, and against the rewritten `CancelButton_Click`, which now closes in a
`finally`. Row 7 passes against the pane's equivalent rewrite. Their continued passing shows the
success path — outcome 1 of the invariant, "requests cancellation" — is unaffected: a live source is
still cancelled, and the button is still enabled when a source is assigned.

Output Summary: all seven pinned pre-existing tests are reported **passed** in the `[P6-T5]` run.
