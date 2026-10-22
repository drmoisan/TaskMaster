# Baseline — Pre-Change Green Status of the Three Must-Pass Tests (P0-T17)

Timestamp: 2026-08-27T20-08

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose|FullyQualifiedName~SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired|FullyQualifiedName~Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry'
    '/Logger:trx;LogFileName=p0-t17.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/baseline/trx/p0-t17'
```

`$vstest` is the path recorded by P0-T5. `LogFileName=` and `/ResultsDirectory:` are both supplied, so
the TRX carries a deterministic name rather than the vstest default `<account>_<HOST>_<ts>.trx`.

EXIT_CODE: 0

Output Summary:

```
VSTest version 18.9.0 (x64)
A total of 1 test files matched the specified pattern.
  Passed PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose [55 ms]
  Passed Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry [56 ms]
  Passed SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired [3 ms]

Test Run Successful.
Total tests: 3
     Passed: 3
 Total time: 1.2340 Seconds
```

| Metric | Value |
| --- | ---: |
| Total | 3 |
| Passed | 3 |
| Failed | 0 |
| Skipped | 0 |

TRX artifact: `FF/evidence/baseline/trx/p0-t17/p0-t17.trx`. The TRX was post-processed to replace the
workspace path with the literal token `WS`, the machine name with `<host>`, and the account name with
`<account>`, so it carries no absolute host path, no account name, and no machine name.

These are the three tests AC-20, AC-21 and AC-22 require to pass with no edit to their test files.
Their pre-change green status is now established, so a later failure would be attributable to this
feature's production change rather than to a pre-existing condition.

Acceptance: `EXIT_CODE: 0` with exactly 3 passed, 0 failed, 0 skipped. PASS.
