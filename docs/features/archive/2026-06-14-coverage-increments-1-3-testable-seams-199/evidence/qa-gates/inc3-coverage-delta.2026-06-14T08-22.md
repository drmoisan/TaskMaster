# Increment 3 — Coverage Delta

Timestamp: 2026-06-14T08-22

Command: dotnet-coverage merge (TestResults *.coverage) --output-format cobertura -> artifacts/csharp/inc3.cobertura.xml; per-method line analysis

EXIT_CODE: 0

## Baseline

Production-only baseline (post-#197, 197-COV-001): 71.65%.
Pre-feature TaskMaster assembly line-rate (artifacts/csharp/coverage-firstparty.cobertura.xml):
0.2578 (25.78%).

## Covered-line results on the named TaskMaster seams

- AppStagingFilenames: every targeted method (get/set ConditionalReminders, get/set CommonWords,
  get/set EmailInfoStagingFile, InitProp) at line-rate 1.0. The seven untargeted sibling properties
  (SubjectMap, CtfInc, CtfMap, EmailSessionTemp, EmailSession, MovedMails, RecentsFile) share the
  IDENTICAL getter/setter shape as the covered ConditionalReminders/CommonWords; the distinct code
  paths (lazy InitProp getter, Settings.Default+Save setter, and the Save-less EmailInfoStagingFile
  setter) are all exercised. The covered-line count on AppStagingFilenames increased from the
  pre-feature baseline.
- AppQuickFilerSettings: the four remaining properties (MoveEntireConversation, SaveAttachments,
  SavePictures, SaveEmailCopy) get/set at line-rate 1.0; combined with the pre-existing
  HighConfidence tests, all six properties are covered when the full TaskMaster.Test suite runs
  (P4-T4).
- AppFileSystemFolderPaths.MatchBestSpecialFolder: NOT covered — Flag-and-Stop gap
  (evidence/other/matchbestspecialfolder-gap.2026-06-14T08-22.md). The method cannot be reached in
  isolation without filesystem mutation (every accessible constructor runs LoadFolders ->
  Directory.CreateDirectory) or a new production seam, both prohibited.

## New/changed-code coverage

The new code is the two test files; every production method they target is at 100% line-rate. The
new-code (test file) line-rate is 1.0. The only Increment 3 target NOT delivered is
MatchBestSpecialFolder, which is the documented Flag-and-Stop gap, not a new-code coverage shortfall.

## Disposition

- Covered-line count on AppStagingFilenames and AppQuickFilerSettings INCREASED.
- No regression on changed lines (test-only addition; Settings.Default snapshot/restore prevents
  machine-state mutation).
- New-code coverage on all targeted, reachable methods = 100%.
- MatchBestSpecialFolder is a Flag-and-Stop gap (filesystem/seam dependency), explicitly authorized
  by the plan's Flag-and-Stop rule; not a remediation trigger for this task.

Outcome: PASS for AppStagingFilenames and AppQuickFilerSettings; MatchBestSpecialFolder
flagged-and-stopped (no production change).
