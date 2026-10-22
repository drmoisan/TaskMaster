# Final Toolchain: Tests with Coverage (P7-T4)

Timestamp: 2026-10-22T16-35
Command: pwsh -NoProfile -File <repo>\scripts\vscode\Invoke-MSTestWithCoverage.ps1
EXIT_CODE: 0
Output Summary: Test Run Successful. Total 7618, executed 7618, passed 7618, failed 0 (trx summary) under the class-level parallel runsettings. First-party coverage: lines 57627/66720 (86.37%), branches 14060/17431 (80.66%). All 4 probe-set tests and all 21 register tests are Passed, plus both converted DfDeedle tests.

## Runner route

- Route: the unscoped script `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, invoked through PowerShell 7 with an absolute script path. It runs `dotnet-coverage collect` around `vstest.console.exe` over every discovered `*.Test.dll`, applying `/Settings:scripts/vscode/TaskMaster.cli.runsettings` (MSTest `Workers` 0, reported as 24 workers, `Scope` ClassLevel), `/InIsolation`, `TestCategory!=LiveOutlook`, and the `\.claude\` worktree exclusion.
- Runsettings path: `scripts/vscode/TaskMaster.cli.runsettings`.
- Shell-icon carve-out: not used. The run did not stall (test time 34.9 s, script elapsed 95 s); the four shell-icon classes ran and passed.
- Same toolchain pass as `final-toolchain-format.2026-10-22T16-32.md`, `final-toolchain-analyzers.2026-10-22T16-32.md`, and `final-toolchain-typecheck.2026-10-22T16-33.md`.

## Trx summary (derived from the trx; raw trx not committed)

```
Test run outcome: Completed
Total 7618, executed 7618, passed 7618, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```

Baseline comparison: 7599 at P0-T7; 7618 now (19 new register tests: 4 adapter, 7 IDList, 2 AppQuickFilerSettings, 4 AppStagingFilenames, 2 AppAutoFileObjects).

## One-line first-party coverage summary

```
First-party coverage: lines 57627/66720 (86.37%), branches 14060/17431 (80.66%)
```

## Probe-set tests

| Test | Outcome |
| --- | --- |
| `TaskTreeControllerMoveLogicTests.MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` | Passed |
| `AppQuickFilerSettingsTests.HighConfidenceModeEnabled_WhenSetTrue_ReadsBackTrue` | Passed |
| `AppStagingFilenamesTests.ConditionalReminders_Setter_RoundTripsThroughSettingsDefault` | Passed |
| `AppAutoFileObjectsCoverageExpansionTests.ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues` | Passed |

## Register tests

| Test | Outcome |
| --- | --- |
| `ApplicationSettingsStoreTests.Constructor_NullSettings_ThrowsArgumentNullException` | Passed |
| `ApplicationSettingsStoreTests.Indexer_Get_ReturnsWrappedSettingsValue` | Passed |
| `ApplicationSettingsStoreTests.Indexer_Set_WritesWrappedSettingsValue` | Passed |
| `ApplicationSettingsStoreTests.Save_Invoked_ForwardsToWrappedSettingsOnce` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_FirstIdOnFreshList_AssignsLengthAndSavesOnce` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_IdNotLongerThanCachedLength_DoesNotSaveAgain` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_LongerIdAfterFirst_SavesAgain` | Passed |
| `IDListGetNextToDoIDTests.Constructor_EnumerableWithNullStore_ThrowsArgumentNullException` | Passed |
| `IDListGetNextToDoIDTests.Constructor_NullStoreOnly_ThrowsArgumentNullException` | Passed |
| `IDListGetNextToDoIDTests.Constructor_StoreOnly_SavesThroughInjectedStore` | Passed |
| `IDListGetNextToDoIDTests.Constructor_Default_UsesApplicationSettingsStore` | Passed |
| `AppQuickFilerSettingsTests.Constructor_Parameterless_UsesApplicationSettingsStore` | Passed |
| `AppQuickFilerSettingsTests.Constructor_NullSettingsStore_ThrowsArgumentNullException` | Passed |
| `AppStagingFilenamesTests.SavingSetters_EachAssignment_WritesStoreAndSavesOnce` | Passed |
| `AppStagingFilenamesTests.Getters_UnsetFields_ReadSeededStoreValues` | Passed |
| `AppStagingFilenamesTests.Constructor_Parameterless_UsesApplicationSettingsStore` | Passed |
| `AppStagingFilenamesTests.Constructor_NullSettingsStore_ThrowsArgumentNullException` | Passed |
| `AppAutoFileObjectsCoverageExpansionTests.Constructor_ParentOnly_UsesApplicationSettingsStore` | Passed |
| `AppAutoFileObjectsCoverageExpansionTests.Constructor_NullSettingsStore_ThrowsArgumentNullException` | Passed |

## Converted DfDeedle tests

| Test | Outcome |
| --- | --- |
| `DfDeedle_COM_Tests.AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` | Passed |
| `DfDeedle_COM_Tests.AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully` | Passed |

## Artifacts

- JaCoCo projection copy: `evidence/qa-gates/final-coverage-projection.2026-10-22T16-35.jacoco.xml`.
- Post-processed Cobertura copy: `SCRATCH` `issue-1005-final-cobertura.xml` (not committed). Trx copy: `SCRATCH` `issue-1005-final.trx` (not committed).
