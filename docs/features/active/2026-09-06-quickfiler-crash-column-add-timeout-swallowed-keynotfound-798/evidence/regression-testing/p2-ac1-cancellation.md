# Phase 2 — AC1 cancellation guard test

Timestamp: 2026-09-07T01-51
Task: [P2-T4]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens.

## Test added

`UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_CancellationRequestedMidLoop_DoesNotThrowTimeoutExhausted`

A never-completing injected adder is arranged, the first deadline is driven through the P2-T1 arming
barrier, and the token is cancelled after that first advance. The test then asserts that the awaited
call does not throw a `TimeoutException` whose message contains the literal `column add`, so user
cancellation continues to be silent and is not converted into a user-facing dialog.

This is a guard test, not a fail-before test: today's code returns normally on cancellation, so it
passes before the fix and must still pass after it. Tagging it `[expect-fail]` would be false, and it
is correctly untagged in the plan.

### Deadline drain after cancellation

After the cancellation, the test releases any deadline the production loop still has armed, using the
same arming barrier, until the call completes. The drain is bounded by the two deadlines that can
remain in the three-deadline budget and is guarded by `Task.WhenAny` against the call completing
without arming a further timer. Without the drain the test would wait forever on a call that the
production loop has not yet been given the opportunity to exit, which would convert an intended pass
into a four-minute blame hang. The drain does not weaken the assertion: the adder still never
completes, so the loop can only exit through its cancellation check.

## Build

Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Test run

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t4 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~AddQfcColumnsAsync_CancellationRequestedMidLoop&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 0
ExpectedExitCode: 0

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p2-t4\<user>_<machine>_2026-09-07_01_51_41_net481.trx`

- total: 1
- passed: 1
- failed: 0
- duration: 156 ms
- total run time: 1.8868 s

## Observed status

Status: **Passed**.

The awaited call completed without surfacing a `column add` timeout after the token was cancelled
mid-loop. The test did not hang; the four-minute blame hang timeout did not fire.

Because this test passes at Phase 2, it is not a member of `BASELINE_FAILURE_SET`. The same test is
required to be passing again in P3-T6.

Output Summary: 1 total, 1 passed, 0 failed.
`AddQfcColumnsAsync_CancellationRequestedMidLoop_DoesNotThrowTimeoutExhausted` passed, confirming that
user cancellation is silent today and giving P3-T2 a guard against converting it into a user-facing
timeout. EXIT_CODE 0 matches ExpectedExitCode 0.
