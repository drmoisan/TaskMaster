# GREEN BEFORE THE FIX — #462 Repeated-Close Guard (P1-T4)

Timestamp: 2026-08-27T20-14

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce'
    '/Logger:trx;LogFileName=p1-t4.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p1-t4'
```

The test project was rebuilt (`/t:Rebuild`, EXIT_CODE 0) immediately before this run.

EXIT_CODE: 0

Output Summary:

```
  Passed CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce [67 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 1.2330 Seconds
```

| Metric | Value |
| --- | ---: |
| Tests run | 1 |
| Passed | 1 |
| Failed | 0 |

## Why this run matters

The guard passes **before** the #462 fix is applied. That is the point of running it now: it proves the
guard is meaningful in its own right rather than an artefact of the fix. If it were only green after
the fix, it could not distinguish "the fix preserved idempotent close" from "the fix created idempotent
close", and it would not detect a future regression that reintroduced research section 6.1 option A
(clearing the close flag on the successful-close path, which would let the second `SetDroppedDown(false)`
reach `_host.Close` and produce two `CloseReasons` entries).

P1-T6 re-runs this test together with the P1-T1 reopen test after the fix; the pair must both be green
there.

TRX artifact: `FF/evidence/regression-testing/trx/p1-t4/p1-t4.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0` with 1 passed, 0 failed. PASS.
