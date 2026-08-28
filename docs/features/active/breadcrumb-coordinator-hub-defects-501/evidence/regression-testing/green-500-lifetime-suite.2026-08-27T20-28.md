# GREEN — #500 Lifetime-Half Regression Surface (P3-T7)

Timestamp: 2026-08-27T20-28

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~BreadcrumbCoordinatorUpgradeLifetimeTests|FullyQualifiedName~BreadcrumbCoordinatorLifecycleTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests'
    '/Logger:trx;LogFileName=p3-t7.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p3-t7'
```

EXIT_CODE: 0

Output Summary:

```
  Passed RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure [63 ms]
Test Run Successful.
Total tests: 45
     Passed: 45
```

| Metric | Value |
| --- | ---: |
| Total | 45 |
| Passed | 45 |
| Failed | 0 |
| Skipped | 0 |

`vstest.console.exe` prints a `Failed:` line and a `Skipped:` line only when the respective count is
non-zero; neither line appears in the output, so both are 0.

## Named test confirmation

`RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure` is confirmed **PASSED** in
this run (its `Passed` line is quoted verbatim above). That test is the one the spec's
`## Test Strategy` singles out as the throw-path contract: `RunSynchronous`'s existing
`catch { Abandon(lease); throw; }` must still abandon the lease and rethrow after the `TryRunCurrent`
restructure. Moving `action()` outside the lock did not disturb it — the throw still propagates out of
`TryRunCurrent` to `RunSynchronous`'s `catch`, which is why the research document predicted this test
would be unaffected.

The 45-test surface covers the lifetime type itself plus the three coordinator test classes that drive
it through `SetSuggestions`, `AddItems` and the guarded render post. It also re-confirms the 42 tests
from the P2-T4 pure-move proof, now with the lock narrowing applied on top.

TRX artifact: `FF/evidence/regression-testing/trx/p3-t7/p3-t7.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0`, 0 failed, 0 skipped, and
`RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure` named as passed. PASS.
