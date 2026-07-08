# Remediation No-Exemption Precheck

Timestamp: 2026-07-04T18:31:13Z
Command: Select-String over issue #236 production files, coverage.config, TaskMaster.runsettings, and scripts/vscode/TaskMaster.cli.runsettings for coverage exemptions, target-name exclusions, and threshold weakenings
EXIT_CODE: 0
Output Summary: No issue #236 coverage exemption, target-name exclusion, or coverage configuration weakening was found before remediation edits.

SearchFiles:
- QuickFiler/Controllers/EfcHomeController.cs
- QuickFiler/Controllers/EfcHomeController.Metrics.cs
- QuickFiler/Controllers/EfcHomeController.Timing.cs
- QuickFiler/Controllers/EfcHomeControllerDependencies.cs
- QuickFiler/Helper Classes/EfcViewerQueue.cs
- QuickFiler/Helper Classes/ItemViewerQueue.cs
- QuickFiler/Helper Classes/QfcThemeHelper.cs
- QuickFiler/Helper Classes/QfcThemeControlSet.cs
- QuickFiler/Helper Classes/ViewerQueueCore.cs
- QuickFiler/Helper Classes/TlpCellSnapShot.cs
- coverage.config
- TaskMaster.runsettings
- scripts/vscode/TaskMaster.cli.runsettings

ExemptionPattern: ExcludeFromCodeCoverage|ExcludeByAttribute|ExcludeByFile|ModulePath|CompanyName|PublicKeyToken|coverage.*(exclude|ignore)|ignore.*coverage
TargetConfigPattern: EfcViewerQueue|ItemViewerQueue|QfcThemeHelper|EfcHomeController|TlpCellStates
ThresholdPattern: threshold|fail-under|line-rate|80\.00|90\.00|Minimum|CoverageThreshold

## Exemption Hits
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:12: <ModulePaths>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:14: <ModulePath>.*Deedle.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:15: <ModulePath>.*FSharp.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:16: <ModulePath>.*Castle\.Core.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:17: <ModulePath>.*FluentAssertions.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:18: <ModulePath>.*Moq.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:19: <ModulePath>.*Microsoft\.Testing.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:20: <ModulePath>.*MSTest.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\coverage.config:22: </ModulePaths>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:14: <ModulePaths>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:16: <ModulePath>.*Deedle.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:17: <ModulePath>.*FSharp.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:18: <ModulePath>.*Castle\.Core.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:19: <ModulePath>.*FluentAssertions.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:20: <ModulePath>.*Moq.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:21: <ModulePath>.*Microsoft\.Testing.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:22: <ModulePath>.*MSTest.*</ModulePath>
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.runsettings:24: </ModulePaths>

## Target Name Hits In Coverage Configuration
none

## Threshold / Coverage Setting Hits
none

PrecheckResult: PASS
