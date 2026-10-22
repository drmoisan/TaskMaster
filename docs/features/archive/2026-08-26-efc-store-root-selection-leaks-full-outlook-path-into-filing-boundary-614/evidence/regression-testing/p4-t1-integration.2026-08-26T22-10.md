# P4-T1 — Cross-Assembly Targeted Regression Run (remediation cycle 1, issue #614)

Timestamp: 2026-08-26T22-10

Preceded by:
`& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` — EXIT_CODE 0.

Command (1 of 3):
`& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p4-t1-qf.trx" "/ResultsDirectory:coverage\trx\p4-t1-qf"`

Command (2 of 3):
`& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation "/Logger:trx;LogFileName=p4-t1-tm.trx" "/ResultsDirectory:coverage\trx\p4-t1-tm"`

Command (3 of 3):
`& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~ArchiveStemContractTests|FullyQualifiedName~FolderConverterIssue614Tests|FullyQualifiedName~EmailFilerConfig_Tests|FullyQualifiedName~FolderPredictorTests|FullyQualifiedName~FolderConverter" "/Logger:trx;LogFileName=p4-t1-ut.trx" "/ResultsDirectory:coverage\trx\p4-t1-ut"`

Note on results directory: each run was given its own per-run subdirectory
(`coverage\trx\p4-t1-qf`, `-tm`, `-ut`) rather than one shared `coverage\trx\p4-t1`, so three
concurrent-safe TRX files exist rather than three files in one folder. This is a naming detail of
the gitignored scratch tree only; it changes no test scope, filter, or assertion.

EXIT_CODE: 0, 0, 0

## Output Summary

| Run | Scope | Total | Passed | Failed | Skipped | Exit |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| 1 | `QuickFiler.Test` (full assembly) | 982 | 982 | 0 | 0 | 0 |
| 2 | `TaskMaster.Test` (full assembly) | 381 | 381 | 0 | 0 | 0 |
| 3 | `UtilitiesCS.Test` (scoped to the five contract/converter/config/predictor classes) | 120 | 120 | 0 | 0 | 0 |
| **Total** | | **1483** | **1483** | **0** | **0** | |

All three runs reported `Test Run Successful.` **Zero failures in all three runs.** No rule-6 flake
(#594 / #592 / #586 / #584) was observed, so this artifact is not split and no `ExpectedExitCode`
declaration is required.

The `QuickFiler.Test` full-assembly run is the strongest local statement of the CR-1/CR-2 change's
blast radius: it contains `EfcSelectionGuardTests`, all three `BreadcrumbBridgeRouter*` classes, and
every `EfcFormController` / `EfcDataModel` test, and it is entirely green.

Raw TRX files were written to the gitignored `coverage\trx\` tree, not under `evidence/`.
