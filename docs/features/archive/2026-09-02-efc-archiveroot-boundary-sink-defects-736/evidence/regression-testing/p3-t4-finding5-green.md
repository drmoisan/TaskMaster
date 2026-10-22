# P3-T4 — Finding 5 breadcrumb boundary: recorded GREEN after the reroute

Timestamp: 2026-09-04T00-07

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcFormControllerTests.BindBreadcrumbRowsAsync|FullyQualifiedName~EfcFormControllerTests.Issue439BindBreadcrumbRowsAsync" "/Logger:trx;LogFileName=p3-t4.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p3-t4
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 0

## TRX results

Total **2**, passed **2**, failed **0**. `Test Run Successful.` The TRX names both methods:

| Test method | Outcome | Duration |
|---|---|---|
| `Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter` | Passed | 00:00:00.2412601 |
| `BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow` | Passed | 00:00:00.0035088 |

The pre-existing positive test still passes unchanged, and the negative sibling that P3-T2 recorded
red is now green.

## P3-T3's observations

The reroute replaced the single `logger.Error` statement inside `BindBreadcrumbRowsAsync`'s general
exception arm with a `TryReportBoundaryFault` invocation carrying the same message and exception.
Measured after the change, in `QuickFiler/Controllers/EfcFormController.cs`:

- `TryReportBoundaryFault` occurrence count: **9**, exactly one greater than the 8 P2-T9 established.
- `catch (` line count: still **12**.
- Inside the `BindBreadcrumbRowsAsync` member (post-change span lines 1038-1055): exactly **one**
  `logger.Debug` call and **zero** `logger.Error` calls.
- The `catch (OperationCanceledException)` arm above it is left byte-identical, still reading
  `logger.Debug("Breadcrumb bind canceled.");`.
- File line count: **1247**, within the D7 budgeted ceiling of 1330.

## TRX inventory

Exactly **one** TRX file exists under this task's results directory: `p3-t4.trx`.

Output Summary: build exited 0; the two-method run exited 0 with TRX total 2, passed 2, failed 0,
naming both breadcrumb tests. The controller file's `TryReportBoundaryFault` count is 9, its
`catch (` count is unchanged at 12, and `BindBreadcrumbRowsAsync` now carries one `logger.Debug` and
zero `logger.Error` calls.
