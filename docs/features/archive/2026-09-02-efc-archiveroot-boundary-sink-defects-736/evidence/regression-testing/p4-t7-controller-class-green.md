# P4-T7 — Whole `EfcFormControllerTests` class green across both partial files

Timestamp: 2026-09-04T00-12

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcFormControllerTests" "/Logger:trx;LogFileName=p4-t7.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\regression-testing\p4-t7
```

Build EXIT_CODE: 0 (`Build succeeded.`, `0 Warning(s)`, `0 Error(s)`)

EXIT_CODE: 0

## TRX results

Total **29**, passed **29**, failed **0**. `Test Run Successful.`

## The total is exactly nine greater than the baseline

The P0-T7 artifact records an `EfcFormControllerTests` total of **20**. This run reports **29**, which
is 20 + 9. The nine added results account exactly for:

| Source task | Methods added | Count |
|---|---|---|
| P2-T2 | the two overload-containment tests plus the null-sink and throwing-sink tests | 4 |
| P2-T7 | the two classification tests | 2 |
| P3-T1 | the breadcrumb negative sibling | 1 |
| P4-T3 | the two default-sink tests | 2 |
| | **Total** | **9** |

Every one of the 20 pre-existing results still passes, so nothing in the original file regressed when
it gained the `partial` keyword and the sibling partial was added beside it.

## TRX inventory

Exactly **one** TRX file exists under this task's results directory: `p4-t7.trx`. No MSTest
deployment directory was created, because the run passed.

Output Summary: the whole `EfcFormControllerTests` class, across both partial files, runs 29 total,
29 passed, 0 failed. That total is exactly nine greater than the 20 the P0-T7 baseline records,
accounting for the four methods from P2-T2, the two from P2-T7, the one from P3-T1, and the two from
P4-T3.
