# [P2-T1] Discriminate the Gate Stop Reasons

Timestamp: 2026-08-26T09-51

Task: [P2-T1]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` — the four `DequeueAsync` exits now
report discriminated stop reasons instead of the `QfcDequeueStop.QuantitySatisfied` stub landed by
`[P1-T8]` (per D-Plan-1):

- degenerate `quantity <= 0` exit: `QfcDequeueStop.QuantitySatisfied` (unchanged).
- first-batch deadline exit (immediately after `LogDeadlineExpiry`): now
  `QfcDequeueStop.DeadlineExpired`.
- take-returned-null exit (`timeOut <= 0` or already-waited with an inactive source): now
  `QfcDequeueStop.SourceExhausted`.
- loop-completion exit (`accepted.Count == quantity`): `QfcDequeueStop.QuantitySatisfied`
  (unchanged).

`LogDeadlineExpiry` and `LogScore` message text is byte-for-byte unchanged.

## Verification

Command: `dotnet tool run csharpier check "QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs"`
EXIT_CODE: 1 (first pass; the edited `return` collapsed to one line under CSharpier)

Command: `dotnet tool run csharpier format "QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop|FullyQualifiedName~DequeueAsync_SourceDrained_ReportsSourceExhaustedStop" "/Logger:trx;LogFileName=p2-t1.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p2-t1"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t1/p2-t1.trx`

Counters: total 2, executed 2, **passed 2**, failed 0, error 0, timeout 0, aborted 0.

- `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` = **Passed**
  (was Failed at `[P1-T9]`).
- `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` = **Passed**
  (was Failed at `[P1-T10]`).

TRX hygiene: the TRX was scrubbed of the absolute worktree path, the account name and the machine
name, then re-parsed as XML; `<Counters .../>`, all test names and all outcomes survive the scrub
unchanged. A case-insensitive search for `danmoisan` or `megalodon4` across the feature folder
returns no match.

## Output Summary

Both `[expect-fail]` tests from Phase 1 transition Failed -> Passed. Format EXIT_CODE 0 after one
formatter rewrite, compile EXIT_CODE 0, scoped run EXIT_CODE 0 with 2 of 2 Passed and 0 Failed.
