# P0-T12 — Pre-Change Source Census

Timestamp: 2026-09-01T08-11

Command: a PowerShell census over the two files, using `Get-Content -LiteralPath <path>` for line
counts, `-match` against the stated anchored regular expressions for anchored counts, and ordinal
`String.IndexOf` scanning for simple-match (occurrence, not line) counts.

EXIT_CODE: 0

## `UtilitiesCS/Threading/TimeOutTask.cs`

| # | Measurement | Expected by plan | **Measured** | Match |
| --- | --- | --- | --- | --- |
| 1 | Total line count | 993 | **993** | yes |
| 2 | Anchored `^\s*catch \(TaskCanceledException\)\s*$` | 9 | **9** | yes |
| 3 | Anchored `^\s*catch \(TimeoutException\)\s*$` | 4 | **4** | yes |
| 4 | Anchored `^\s*catch \(System\.Exception e\)\s*$` | 10 | **10** | yes |
| 5 | Simple-match `OperationCanceledException` | 0 | **0** | yes |
| 6 | Anchored `^\s*using var timeoutSource = new CancellationTokenSource\(milliseconds\);\s*$` | 9 | **9** | yes |
| 7 | Simple-match `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` | 2 | **2** | yes |
| 8 | Simple-match `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))` | 1 | **1** | yes |
| 9 | Simple-match bare token `timeoutSourceFactory` | 5 | **5** | yes |

## `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`

| # | Measurement | Expected by plan | **Measured** | Match |
| --- | --- | --- | --- | --- |
| 10 | `(Get-Content -LiteralPath <path>).Count` | 387 | **387** | yes |
| 11 | File-level simple-match `CancellationToken.None` | 16 | **16** | yes |

## Supplementary Pre-Change Readings (context for later gates, not part of the acceptance tuple)

| Measurement | Measured |
| --- | --- |
| Simple-match `when (e is TaskCanceledException \|\| e is TimeoutException)` in the production file | 0 |
| Anchored `^\s*using var timeoutSource = \(\s*$` in the production file | 1 |
| Simple-match `Task.Delay` in the test file | 0 |
| Simple-match `Thread.Sleep` in the test file | 0 |
| Simple-match `Thread.SpinWait` in the test file | 0 |
| Simple-match `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException` in the test file | 0 |
| Simple-match `milliseconds: 30_000` in the test file | 0 |
| Simple-match `timeoutSourceFactory: timeoutSourceFactory` in the test file | 0 |

The anchored `^\s*using var timeoutSource = \(\s*$` count of 1 is the pre-existing `Func<TResult>`
seam. P1-T1 adds a second such construction, which is why P3-T7 defines L_CTOR as the **second** such
line number in ascending order.

## Agreement Statement

**Measured tuple: 993, 9, 4, 10, 0, 9, 2, 1, 5, 387, 16.**
**Plan-stated expectation: 993, 9, 4, 10, 0, 9, 2, 1, 5, 387, 16.**

**All eleven values agree. There is no disagreement to report, and no later census gate requires
adjustment.** Every Phase 1, Phase 2, and Phase 3 census gate is expressed as a delta from these
numbers and is therefore anchored to a verified baseline.

Output Summary: The pre-change census of `UtilitiesCS/Threading/TimeOutTask.cs` and
`UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` returned exactly the eleven values
the plan expects. The production file is 993 lines with 23 catch clauses (9 `TaskCanceledException`,
4 `TimeoutException`, 10 `System.Exception e`) and no exception filter clause; `OperationCanceledException`
is absent; the `timeoutSourceFactory` seam exists on the `Func<TResult>` sibling only, contributing
2 parameter declarations, 1 coalesce construction, and 5 bare-token occurrences. The test file is
387 lines with 16 `CancellationToken.None` occurrences and none of the three banned timing APIs.

Acceptance: met. All recorded values equal the plan's stated expectation.
