# GREEN — #462 Full Drop-Down Regression Surface (P1-T7)

Timestamp: 2026-08-27T20-17

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbPendingOpenCloseTests|FullyQualifiedName~BreadcrumbDropDownLifecycleTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests'
    '/Logger:trx;LogFileName=p1-t7.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p1-t7'
```

EXIT_CODE: 0

Output Summary:

```
Test Run Successful.
Total tests: 48
     Passed: 48
 Total time: 2.9392 Seconds
```

| Metric | Value |
| --- | ---: |
| Total | 48 |
| Passed | 48 |
| Failed | 0 |
| Skipped | 0 |

`vstest.console.exe` prints a `Failed:` line and a `Skipped:` line only when the respective count is
non-zero; neither line appears, so both are 0.

This surface covers all four test classes that exercise `CloseCore`, `RequestOpen` and the drop-down
open/close state machine, including the two tests AC-20 and AC-21 name
(`PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` and
`SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired`) and the seven further
`CloseCore`-exercising tests the research document's section 3.1 table enumerates. Option D of research
section 6.1 passes all of them, as that table predicted.

TRX artifact: `FF/evidence/regression-testing/trx/p1-t7/p1-t7.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0`, 0 failed, 0 skipped, and a passed count greater than 20 (48). PASS.
