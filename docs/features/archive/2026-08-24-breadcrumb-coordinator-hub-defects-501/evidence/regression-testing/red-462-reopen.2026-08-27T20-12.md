# RED — #462 Reopen After Successful Close (P1-T2) [expect-fail]

Timestamp: 2026-08-27T20-12

ExpectedExitCode: 1

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync'
    '/Logger:trx;LogFileName=p1-t2.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p1-t2'
```

The test project was rebuilt with
`& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"` (EXIT_CODE 0)
immediately before this run, so the assembly under test contains the newly authored test method.

EXIT_CODE: 1

Output Summary:

```
Test Run Failed.
Total tests: 1
     Failed: 1
 Total time: 1.3908 Seconds
```

| Metric | Value |
| --- | ---: |
| Tests run | 1 |
| Failed | 1 |
| Passed | 0 |

The observed exit code equals the declared `ExpectedExitCode`, so this gate is a PASS: a failing test
is the intended outcome of this task, and only of this task.

## Verbatim assertion-failure text

```
Expected harness.Host.Requests to contain 2 item(s) because the reopen must reach _host.OpenAsync a second time, but found 1: {
```

The observed `Requests` count is **1**, which is exactly the defect I-462.2 describes: `RequestOpen`'s
guard at `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:93-94` reads the still-set
`_closePending` together with the reopened `_host.IsOpen` and returns the `ClosedTask` sentinel, so
`_host.OpenAsync` is never reached a second time.

TRX artifact: `FF/evidence/regression-testing/trx/p1-t2/p1-t2.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: exactly 1 test run, 1 failed, 0 passed, and the failure message shows the observed
`Requests` count is 1. PASS.
