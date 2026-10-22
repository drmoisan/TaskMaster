# [P3-T2] [expect-fail] Idle Iterations Must Wait Through the Injected Clock (Issue #448)

Timestamp: 2026-08-26T10-22

Task: [P3-T2] — `[expect-fail]`; a failing test is the expected outcome of this task only.
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` — added
`UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay`, plus two supporting members in a new
`Issue #448` region:

- `private sealed class CountingTimeProvider : FakeTimeProvider`, which counts delay requests by
  overriding `CreateTimer` (the `TimeProvider.Delay` extension routes through it) and never advances
  time on its own, so a consumer that parks on a delay stays parked instead of spinning.
- `private QfcFormController ArrangeUndoConsumer(TimeProvider clock, Func<IMovedMailInfo, Task> processor = null)`,
  which assigns the clock, sets `UndoConsumerStarter = body => body()` so the consumer runs inline,
  and optionally assigns `UndoItemProcessor`. Extracting this helper up front is what `[P3-T7]`
  prescribes and keeps the file under the 500-line cap.

The test does **not** await the returned consumer task. The pre-fix loop never terminates, so
awaiting it would produce a hang rather than a usable RED state (D5). Running the consumer inline
means control returns at its first `await`, which is exactly where the assertion is meaningful.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay" "/Logger:trx;LogFileName=p3-t2.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p3-t2"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t2/p3-t2.trx`

Counters: total 1, executed 1, passed 0, **failed 1**, error 0, timeout 0, aborted 0.

- `UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay` = **Failed**, with the
  assertion-failure message:

  `Expected clock.DelayRequests to be greater than or equal to 1 because an idle iteration must wait
  through the injected TimeProvider, not Task.Delay, but found 0 (difference of -1).`

  This is an assertion failure against a compiling tree, not a compile error and not a timeout: the
  `timeout` and `aborted` counters are both 0.

## Test-host residue

Command: `pwsh -NoProfile -File scripts\vscode\TestProcessCleanup.ps1`
EXIT_CODE: 0

Command: `Get-Process -Name vstest.console,testhost,testhost.x86,testhost.net48 -ErrorAction SilentlyContinue`
Result: **0 processes remaining**. No `vstest` or `testhost` process survived the scoped run, so the
runaway pre-fix consumer died with its test host as intended.

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test name and outcome unchanged. A case-insensitive search for the
account name and the machine name across the feature folder returns no match.

## Output Summary

RED state achieved by assertion, as required. The pre-fix `UndoConsumer` waits on
`await Task.Delay(200)`, so the injected clock records 0 delay requests. `[P3-T3]` rewrites the loop
to wait through `TimeProvider.Delay` and turns this test green.
