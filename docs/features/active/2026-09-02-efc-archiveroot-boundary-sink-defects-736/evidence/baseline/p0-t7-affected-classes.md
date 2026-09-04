# P0-T7 — Pre-change pass/fail baseline of the three affected test classes

Timestamp: 2026-09-03T23-37

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcDataModelArchiveRootTests|FullyQualifiedName~EfcFormControllerTests" "/Logger:trx;LogFileName=p0-t7-quickfiler.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\baseline\p0-t7
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~AppOlObjectsArchiveRootValidationTests&TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p0-t7-taskmaster.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\baseline\p0-t7
```

EXIT_CODE: 0 (both runs; `Test Run Successful.` in each case)

## Per-class total/passed/failed triples derived from the TRX results

| Class | Total | Passed | Failed |
|---|---|---|---|
| `QuickFiler.Test.Controllers.EfcDataModelArchiveRootTests` | **11** | **11** | **0** |
| `QuickFiler.Controllers.Tests.EfcFormControllerTests` | **20** | 20 | **0** |
| `TaskMaster.Test.AppGlobals.AppOlObjectsArchiveRootValidationTests` | **6** | **6** | **0** |

The 11/11/0 and 6/6/0 triples are the values this task's acceptance pins.

**The `EfcFormControllerTests` total of 20 is the figure P4-T7 compares against.** It exceeds the 16
`[TestMethod]` plus `[DataTestMethod]` attributes P0-T9 counted in that file because the single
`[DataTestMethod]` expands to five data rows at run time: 15 + 5 = 20. P4-T7 therefore expects a
total of 29, which is 20 plus the nine methods added by P2-T2 (four), P2-T7 (two), P3-T1 (one), and
P4-T3 (two).

Run-level roll-up: the QuickFiler run reported Total tests 31, Passed 31 (20 + 11); the TaskMaster
run reported Total tests 6, Passed 6.

## TRX inventory under this task's results directory

Exactly **two** files exist under
`docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t7`
and no others:

1. `p0-t7-quickfiler.trx`
2. `p0-t7-taskmaster.trx`

No MSTest deployment directory was produced, because both runs passed.

Output Summary: both baseline runs exited 0 with zero failures.
`EfcDataModelArchiveRootTests` 11/11/0; `AppOlObjectsArchiveRootValidationTests` 6/6/0;
`EfcFormControllerTests` total 20, failed 0. Exactly two TRX files exist under this task's results
directory.
