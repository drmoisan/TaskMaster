# Increment 3 — MSTest with Coverage (TaskMaster.Test)

Timestamp: 2026-06-14T08-22

Command: vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~AppStagingFilenamesTests|FullyQualifiedName~AppQuickFilerSettingsRemainingPropertiesTests"
(vstest 18.7.0; raw .coverage merged to artifacts/csharp/inc3.cobertura.xml, gitignored.)

EXIT_CODE: 0

## Output Summary

Total tests: 12. Passed: 12. Failed: 0. Total time: ~2.19s. Deterministic; Settings.Default
snapshot/restore in TestInitialize/TestCleanup so machine state is not mutated; no filesystem
access, no temp files. Breakdown: AppStagingFilenamesTests 6,
AppQuickFilerSettingsRemainingPropertiesTests 6.

Whole-class line-rate after this filtered Increment 3 run (inc3.cobertura.xml):
- TaskMaster.AppQuickFilerSettings: 0.6667 (66.67% in this filter — the HighConfidence pair is
  covered by the pre-existing AppQuickFilerSettingsTests, not included in this filtered run; the
  full P4-T4 suite run covers all six properties)
- TaskMaster.AppStagingFilenames: 0.3077 (30.77% — three of the ten identical-shaped properties plus
  InitProp are targeted; see delta note)
- TaskMaster.AppFileSystemFolderPaths: 0.0 (MatchBestSpecialFolder is the Flag-and-Stop gap; no
  tests, see evidence/other/matchbestspecialfolder-gap)

Targeted-method line-rate (every method the Increment 3 tests exercise is at 100%):
- AppStagingFilenames: get/set ConditionalReminders, get/set CommonWords, get/set
  EmailInfoStagingFile, InitProp — all 1.0
- AppQuickFilerSettings: get/set of MoveEntireConversation, SaveAttachments, SavePictures,
  SaveEmailCopy — all 1.0
