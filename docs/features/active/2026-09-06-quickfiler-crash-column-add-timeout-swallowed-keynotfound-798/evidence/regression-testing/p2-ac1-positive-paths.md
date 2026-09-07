# Phase 2 — AC1 positive-path regression tests (fail-before)

Timestamp: 2026-09-07T01-49
Task: [P2-T3] [expect-fail]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens.

## Tests added

- `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_AdderCompletesBeforeFirstDeadline_ReturnsWithoutThrowingAndStartsOneTask`
- `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_AdderCompletesBeforeSecondDeadline_ReturnsWithoutThrowingAndStartsOneTask`
- `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_AdderCompletesBeforeThirdDeadline_ReturnsWithoutThrowingAndStartsOneTask`

Each asserts that the call does not throw and that the adder invocation count is 1. Each drives the
deadlines through the P2-T1 arming barrier, firing exactly N-1 deadlines before releasing the adder
gate for the test whose adder completes before deadline N. A bare sequence of consecutive `Advance`
calls is not used in any of the three.

For N of 1 the barrier is not awaited and no deadline is fired. The plan's stated rationale for that
arrangement is corrected here: a timer normally **is** armed at the `TimeoutAfter` call, because
`Task.Run` leaves the task incomplete at that point, so the short-circuit the plan describes does not
apply. The arrangement is nevertheless correct as written, because the N of 1 test never advances the
fake clock, so the armed timer can never fire and the adder is the only thing that ends the call. The
plan's arrangement was implemented as specified rather than changed on the basis of its stated
rationale.

## Build

Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Test run

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t3 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~AddQfcColumnsAsync_AdderCompletesBefore&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p2-t3\<user>_<machine>_2026-09-07_01_49_55_net481.trx`

- total: 3
- passed: 1
- failed: 2
- total run time: 2.8139 s

## Observed status and observed adder invocation count

| Test | Observed status | Observed adder invocation count | Duration |
|---|---|---|---|
| `AddQfcColumnsAsync_AdderCompletesBeforeFirstDeadline_ReturnsWithoutThrowingAndStartsOneTask` | Passed | 1 | 213 ms |
| `AddQfcColumnsAsync_AdderCompletesBeforeSecondDeadline_ReturnsWithoutThrowingAndStartsOneTask` | Failed | 2 | 128 ms |
| `AddQfcColumnsAsync_AdderCompletesBeforeThirdDeadline_ReturnsWithoutThrowingAndStartsOneTask` | Failed | 3 | 6 ms |

The invocation count for the passing test is 1 because its `Be(1)` assertion succeeded; the counts
for the two failing tests are quoted directly from their failure messages.

Failure messages, quoted verbatim:

```
Expected invocations to be 1 because the first deadline must re-deadline the work already in flight, not start a second unit of work, but found 2.
```

```
Expected invocations to be 1 because two expired deadlines must re-deadline the work already in flight, not start two more units of work, but found 3.
```

In all three tests the `NotThrowAsync` assertion passed, so the two failures are the invocation-count
assertions and not incidental exceptions. That is the failure the plan predicts: the unfixed loop
starts a new `Task.Run` on each retry. No test hung; the four-minute blame hang timeout did not fire.

All three tests are required to pass in P3-T6.

Output Summary: 3 total, 1 passed, 2 failed. Observed adder invocation counts are 1, 2 and 3 for the
first-, second- and third-deadline tests respectively, matching the plan's stated expectation.
EXIT_CODE 1 matches ExpectedExitCode 1.
