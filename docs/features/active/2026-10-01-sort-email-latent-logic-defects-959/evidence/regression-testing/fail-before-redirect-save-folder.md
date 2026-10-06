# Fail-Before: RedirectSaveFolder Re-Rooting Defect (P4-T9)

Timestamp: 2026-10-03T12-09
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_AttachmentSaving_Tests" "/ResultsDirectory:coverage\test-results\959\p4-t9" "/Logger:trx;LogFileName=p4-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1
ITERATION: 2
Output Summary: The run is red with exactly the expected failing row (RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths) and ten passing rows. The failure message names `Path.GetDirectoryName(helper.FilePathSaveAlt)` and shows the actual value ending `origin"` against the expected value ending `destination"`, so the alternate save path's directory is still the origin folder after the extraction step (the re-rooting defect). This rewrite supersedes the 2026-10-03T11-28 stop record (iteration 1), which survives only in its stop commit; the test, the command and the filter are unchanged, and the run is evaluated against the revision 1.8 acceptance text.

```
VSTEST_EXIT_CODE: 1
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=11 executed=11 passed=10 failed=1
RESULT_COUNT: 11
RESULT SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly = Passed
RESULT Cleanup_Files_ResetsEveryPromptSession = Passed
RESULT RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths = Failed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT SaveAttachment_WhenFileDoesNotExist_SavesDirectly = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain = Passed
RESULT SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer = Passed
MESSAGE RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths :: Expected Path.GetDirectoryName(helper.FilePathSaveAlt) to be a match with the expectation, but it differs at index 23:
               (actual)
  ".59Sandbox\origin"
  ".59Sandbox\destination"
               (expected)
```

The MESSAGE above is transcribed as the payload printed it. The console encoding rendered the U+2026 ellipsis as `.` and the two arrow glyphs as blanks; the vstest console section of the same run printed `"…59Sandbox\origin"` and `"…59Sandbox\destination"` with the arrows. No host path appears in the message.

## Acceptance (P4-T9, revision 1.8, all five required)

1. `EXIT_CODE:` non-zero and equal to `ExpectedExitCode:` (1 = 1): met.
2. `COUNTERS total=11 executed=11 passed=10 failed=1`: met.
3. The `Failed` row is exactly `RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths` and the ten `Passed` rows are the other `NAMES-TAS-FINAL` names: met.
4. The `MESSAGE` entry of the failed row contains all three backslash-free tokens `GetDirectoryName(helper.FilePathSaveAlt)`, `origin"` and `destination"`: met (all three occur in the transcribed message above).
5. Every `SANDBOX-` value is `False`: met.

Result: FAIL-BEFORE OBSERVED for the expected reason. P4-T9 is checked off.

## Pass-after (P4-T11)

Run: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_AttachmentSaving_Tests" "/ResultsDirectory:coverage\test-results\959\p4-t11" "/Logger:trx;LogFileName=p4-t11.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere), after Edit E-A-RR-SECOND (P4-T10).

```
PASS-AFTER-VSTEST_EXIT_CODE: 0
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
COUNTERS total=11 executed=11 passed=11 failed=0
RESULT SaveAttachment_WhenFileDoesNotExist_SavesDirectly = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly = Passed
RESULT SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer = Passed
RESULT Cleanup_Files_ResetsEveryPromptSession = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave = Passed
RESULT RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain = Passed
```

Acceptance (P4-T11, all three required): `PASS-AFTER-VSTEST_EXIT_CODE: 0`: met; `COUNTERS total=11 executed=11 passed=11 failed=0` with the eleven `RESULT` rows exactly `NAMES-TAS-FINAL`, each `= Passed`: met; every `SANDBOX-` value is `False`: met.
