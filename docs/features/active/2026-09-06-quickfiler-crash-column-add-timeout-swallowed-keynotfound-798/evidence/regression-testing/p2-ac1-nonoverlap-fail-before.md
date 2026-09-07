# Phase 2 — AC1 non-overlap regression test (fail-before)

Timestamp: 2026-09-07T01-46
Task: [P2-T1] [expect-fail]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens.

## Test added

`UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce`

The test injects a column adder that counts its invocations, signals entry, then blocks, and drives
the three production deadlines through the deterministic arming barrier declared in the same test
class. It asserts the adder is invoked exactly once across all three deadlines.

### Arming barrier

The barrier is a `TimeProvider` that forwards `GetUtcNow`, `GetTimestamp`, `LocalTimeZone`,
`TimestampFrequency` and `CreateTimer` to an inner `FakeTimeProvider` unchanged, and completes a
`TaskCompletionSource` after forwarding `CreateTimer`. Forwarding rather than intercepting is
required so the inner provider still owns and fires the timer.

Three consecutive `Advance` calls are not used and are prohibited for this task and for P2-T2,
P2-T3 and P2-T4. Each deadline is armed relative to the clock only after the previous proxy faults,
so consecutive advances would move the clock past deadlines the production loop has not yet created,
the returned task would never complete, and the test would hang until the four-minute blame timeout
rather than failing.

Two further properties of the implementation are load-bearing and were implemented as specified:

- the adder gate is released in a `finally`, not on the success path, because the expected
  fail-before outcome is an assertion failure and a success-path release would leave thread-pool
  threads blocked for the life of the test process;
- the barrier signal is completed with `TrySetResult` and never `SetResult`, because the production
  loop can arm one more timer than the surrounding prose predicts and a second `SetResult` on the
  same instance throws where an unobserved `TrySetResult` is inert.

## Build

Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

The project-scoped build is used for per-task observation only. The solution-wide analyzer build
required by this phase runs in P2-T11.

## Test run

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t1 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~AddQfcColumnsAsync_ThreeDeadlines&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the `ShellUtilities` / `SysImageListHelper` / `OSBrowser` extension because
P0-T8 recorded `SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p2-t1\<user>_<machine>_2026-09-07_01_46_42_net481.trx`

- total: 1
- passed: 0
- failed: 1
- duration of the failing test: 272 ms
- total run time: 2.1435 s

## Observed failure

Status: **Failed**, as expected.

Observed adder invocation count: **3**.

Failure message, quoted verbatim:

```
Expected probe.Invocations to be 1 because the column-add work must be started once and re-deadlined, not restarted on every retry, but found 3.
```

The failure is the one the plan predicts and not an incidental one: the assertion that failed is the
invocation-count assertion, the call returned normally so the barrier drove all three deadlines, and
the observed count of 3 is the direct consequence of the unfixed loop starting a new `Task.Run` on
every retry. The test did not hang; the four-minute blame hang timeout did not fire.

Output Summary: 1 total, 0 passed, 1 failed. `AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce`
failed with an observed adder invocation count of 3 against an expected 1, which is the AC1
fail-before condition. EXIT_CODE 1 matches ExpectedExitCode 1.
