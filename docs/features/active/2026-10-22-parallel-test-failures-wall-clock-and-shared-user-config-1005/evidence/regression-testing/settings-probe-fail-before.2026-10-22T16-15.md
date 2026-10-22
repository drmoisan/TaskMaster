# Settings-Saving Probe - Fail Before (Issue #1005)

Timestamp: 2026-10-22T16-15
Command: vstest.console.exe TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskTreeControllerMoveLogicTests.MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam&TestCategory!=LiveOutlook|FullyQualifiedName~AppQuickFilerSettingsTests.HighConfidenceModeEnabled_WhenSetTrue_ReadsBackTrue&TestCategory!=LiveOutlook|FullyQualifiedName~AppStagingFilenamesTests.ConditionalReminders_Setter_RoundTripsThroughSettingsDefault&TestCategory!=LiveOutlook|FullyQualifiedName~AppAutoFileObjectsCoverageExpansionTests.ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues&TestCategory!=LiveOutlook" /ResultsDirectory:SCRATCH\issue-1005-results-p1t6-fail-before "/Logger:trx;LogFileName=issue-1005-p1t6-fail-before.trx"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Expected failure observed. 4 total, 4 executed, 0 passed, 4 failed. Each of the four probe-set tests fails on the probe assertion and records saves on the test thread: MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam 1 save, HighConfidenceModeEnabled_WhenSetTrue_ReadsBackTrue 1 save, ConditionalReminders_Setter_RoundTripsThroughSettingsDefault 1 save, ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues 6 saves. No other assertion is the reported failure. Each counted save was cancelled by the probe, so no settings file was written.

## Preceding build (ANALYZE)

- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0
- Result: `0 Warning(s)`, `0 Error(s)`. The probe code (P1-T1 to P1-T5) compiles against the unmodified production code; TaskTree.Test compiles under C# 7.3.

## Filter note

The `TestCategory!=LiveOutlook` clause is attached to each `FullyQualifiedName~` alternative because `&` binds tighter than `|` in the vstest filter grammar; a single trailing clause would apply to the last alternative only. The selected test set is the four named tests.

## Trx-derived results (raw trx kept under SCRATCH, not committed)

| Test | Outcome | Failure message |
| --- | --- | --- |
| `TaskTreeControllerMoveLogicTests.MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` | Failed | Expected saves to be 0 because the test persisted settings to the shared per-host settings file through IDList, but found 1 (difference of 1). |
| `AppQuickFilerSettingsTests.HighConfidenceModeEnabled_WhenSetTrue_ReadsBackTrue` | Failed | Expected saves to be 0 because the test persisted settings to the shared per-host settings file, but found 1 (difference of 1). |
| `AppStagingFilenamesTests.ConditionalReminders_Setter_RoundTripsThroughSettingsDefault` | Failed | Expected saves to be 0 because the test persisted settings to the shared per-host settings file, but found 1 (difference of 1). |
| `AppAutoFileObjectsCoverageExpansionTests.ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues` | Failed | Expected saves to be 0 because the test persisted settings to the shared per-host settings file, but found 6 (difference of 6). |
