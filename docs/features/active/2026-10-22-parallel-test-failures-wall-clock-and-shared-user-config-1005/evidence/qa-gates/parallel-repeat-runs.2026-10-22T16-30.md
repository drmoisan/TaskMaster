# Parallel Repeat Runs (P6-T4)

Timestamp: 2026-10-22T16-30
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~DfDeedle_COM_Tests&TestCategory!=LiveOutlook|FullyQualifiedName~ApplicationSettingsStoreTests&TestCategory!=LiveOutlook|FullyQualifiedName~IDListGetNextToDoIDTests&TestCategory!=LiveOutlook|FullyQualifiedName~TaskTreeControllerMoveLogicTests&TestCategory!=LiveOutlook|FullyQualifiedName~TaskTreeControllerRouteDropTests&TestCategory!=LiveOutlook|FullyQualifiedName~AppQuickFilerSettingsTests&TestCategory!=LiveOutlook|FullyQualifiedName~AppQuickFilerSettingsRemainingPropertiesTests&TestCategory!=LiveOutlook|FullyQualifiedName~AppStagingFilenamesTests&TestCategory!=LiveOutlook|FullyQualifiedName~AppAutoFileObjectsCoverageExpansionTests&TestCategory!=LiveOutlook|FullyQualifiedName~RibbonControllerTests&TestCategory!=LiveOutlook" /ResultsDirectory:SCRATCH\issue-1005-results-p6t4-combined-<n> "/Logger:trx;LogFileName=issue-1005-p6t4-combined-<n>.trx" (5 times), then one invocation per class in reverse order with the same runsettings and /InIsolation
EXIT_CODE: 0
Output Summary: 15 runs, 0 failures. Five consecutive combined runs of the ten affected classes in one vstest invocation: 116 passed, 0 failed each. Ten solo runs, one class per process, in reverse register order: every run passed with 4 to 26 tests matched. AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing elapsed 2.1 ms to 2.7 ms across the six runs that include DfDeedle_COM_Tests (no wall-clock deadline).

## Configuration

- Runsettings: `scripts/vscode/TaskMaster.cli.runsettings` (MSTest `Workers` 0, `Scope` ClassLevel: classes run in parallel within each assembly).
- Isolation: `/InIsolation` on every invocation.
- Preceding build (ANALYZE, once): `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, EXIT_CODE 0, `0 Warning(s)`, `0 Error(s)`.
- Raw trx documents are kept under `SCRATCH` and are not committed; the rows below are derived from them.

## Combined runs (all ten classes, one vstest invocation)

Class set: `DfDeedle_COM_Tests`, `ApplicationSettingsStoreTests`, `IDListGetNextToDoIDTests`, `TaskTreeControllerMoveLogicTests`, `TaskTreeControllerRouteDropTests`, `AppQuickFilerSettingsTests`, `AppQuickFilerSettingsRemainingPropertiesTests`, `AppStagingFilenamesTests`, `AppAutoFileObjectsCoverageExpansionTests`, `RibbonControllerTests`.

| Run | Exit | Total | Passed | Failed | `AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` elapsed |
| --- | --- | --- | --- | --- | --- |
| 1 | 0 | 116 | 116 | 0 | 00:00:00.0023057 |
| 2 | 0 | 116 | 116 | 0 | 00:00:00.0023350 |
| 3 | 0 | 116 | 116 | 0 | 00:00:00.0021357 |
| 4 | 0 | 116 | 116 | 0 | 00:00:00.0026975 |
| 5 | 0 | 116 | 116 | 0 | 00:00:00.0025364 |

## Solo runs (one class per vstest invocation, reverse register order)

| Order | Class | Assembly | Exit | Total | Passed | Failed | Happy-path elapsed |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | `RibbonControllerTests` | TaskMaster.Test | 0 | 17 | 17 | 0 | n/a |
| 2 | `AppAutoFileObjectsCoverageExpansionTests` | TaskMaster.Test | 0 | 10 | 10 | 0 | n/a |
| 3 | `AppStagingFilenamesTests` | TaskMaster.Test | 0 | 10 | 10 | 0 | n/a |
| 4 | `AppQuickFilerSettingsRemainingPropertiesTests` | TaskMaster.Test | 0 | 6 | 6 | 0 | n/a |
| 5 | `AppQuickFilerSettingsTests` | TaskMaster.Test | 0 | 6 | 6 | 0 | n/a |
| 6 | `TaskTreeControllerRouteDropTests` | TaskTree.Test | 0 | 7 | 7 | 0 | n/a |
| 7 | `TaskTreeControllerMoveLogicTests` | TaskTree.Test | 0 | 17 | 17 | 0 | n/a |
| 8 | `IDListGetNextToDoIDTests` | ToDoModel.Test | 0 | 13 | 13 | 0 | n/a |
| 9 | `ApplicationSettingsStoreTests` | UtilitiesCS.Test | 0 | 4 | 4 | 0 | n/a |
| 10 | `DfDeedle_COM_Tests` | UtilitiesCS.Test | 0 | 26 | 26 | 0 | 00:00:00.0023218 |

Result: PASS. Zero failures in all fifteen runs, and every run matched at least one test.
