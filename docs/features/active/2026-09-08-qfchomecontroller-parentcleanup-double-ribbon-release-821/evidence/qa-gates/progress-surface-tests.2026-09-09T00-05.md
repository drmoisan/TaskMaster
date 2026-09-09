# Phase 6 — The ten new progress-surface tests pass

Timestamp: 2026-09-09T13-57
Task: [P6-T8]

Source of these results: the `[P6-T5]` captured run, EXIT_CODE 0, 7208 total, 7208 passed, 0 failed.

## The five in `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`

| # | Test | Result |
|---|---|---|
| 1 | `RequestCancel_WhenSourceIsNull_ThrowsInvalidOperationExceptionWithMessage` | **Passed** |
| 2 | `RequestCancel_WhenSourceIsDisposed_DoesNotThrow` | **Passed** |
| 3 | `CancelButton_Click_WhenSourceIsNull_DoesNotThrowOutOfTheHandler` | **Passed** |
| 4 | `CancelButton_Click_WhenSourceIsDisposed_DoesNotThrowOutOfTheHandler` | **Passed** |
| 5 | `SetCancellationTokenSource_WithNull_DoesNotEnableButton` | **Passed** |

## The five in `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs`

| # | Test | Result |
|---|---|---|
| 6 | `RequestCancel_WhenSourceIsNull_ThrowsInvalidOperationExceptionWithMessage` | **Passed** |
| 7 | `RequestCancel_WhenSourceIsDisposed_DoesNotThrow` | **Passed** |
| 8 | `CancelButtonClick_WhenSourceIsNull_DoesNotThrowOutOfTheHandler` | **Passed** |
| 9 | `CancelButtonClick_WhenSourceIsDisposed_DoesNotThrowOutOfTheHandler` | **Passed** |
| 10 | `SetCancellationTokenSource_WithNull_DoesNotEnableButton` | **Passed** |

## Result lines located in the captured output, verbatim

```text
  Passed RequestCancel_WhenSourceIsNull_ThrowsInvalidOperationExceptionWithMessage [1 ms]
  Passed RequestCancel_WhenSourceIsNull_ThrowsInvalidOperationExceptionWithMessage [< 1 ms]
  Passed RequestCancel_WhenSourceIsDisposed_DoesNotThrow [5 ms]
  Passed RequestCancel_WhenSourceIsDisposed_DoesNotThrow [1 ms]
  Passed CancelButton_Click_WhenSourceIsNull_DoesNotThrowOutOfTheHandler [1 ms]
  Passed CancelButton_Click_WhenSourceIsDisposed_DoesNotThrowOutOfTheHandler [1 ms]
  Passed CancelButtonClick_WhenSourceIsNull_DoesNotThrowOutOfTheHandler [1 ms]
  Passed CancelButtonClick_WhenSourceIsDisposed_DoesNotThrowOutOfTheHandler [1 ms]
  Passed SetCancellationTokenSource_WithNull_DoesNotEnableButton [1 ms]
  Passed SetCancellationTokenSource_WithNull_DoesNotEnableButton [1 ms]
```

Three names are shared between the two test classes —
`RequestCancel_WhenSourceIsNull_ThrowsInvalidOperationExceptionWithMessage`,
`RequestCancel_WhenSourceIsDisposed_DoesNotThrow` and
`SetCancellationTokenSource_WithNull_DoesNotEnableButton` — and each returned exactly **2** matches,
one per class, both prefixed `Passed`. The four handler tests carry class-specific names, matching each
file's own established style (`CancelButton_Click_` in the viewer file, `CancelButtonClick_` in the
pane file), and each returned exactly **1** match, prefixed `Passed`. Ten result lines in total.

## Coverage of the invariant's three outcomes

| Outcome | Viewer test | Pane test |
|---|---|---|
| Throws `InvalidOperationException` with a diagnosable message | 1 | 6 |
| Returns quietly after owner disposal | 2 | 7 |
| Nothing escapes the handler, surface closed or disposed — null case | 3 | 8 |
| Nothing escapes the handler, surface closed or disposed — disposed case | 4 | 9 |
| Accept-point guard leaves the button disabled for a null source | 5 | 10 |

The tests are deterministic: no `Thread.Sleep`, no `Task.Delay`, no sleep, no retry and no timing
tolerance. Each disposed source is disposed synchronously on the statement before the act, and no
temporary file is created.

Output Summary: all ten names are listed above and each is reported **passed** in the `[P6-T5]` run —
five in `ProgressViewer_Tests.cs` and five in `ProgressPane_Tests.cs`.
