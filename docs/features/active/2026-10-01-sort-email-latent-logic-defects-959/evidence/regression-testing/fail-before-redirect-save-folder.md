# Fail-Before: RedirectSaveFolder Re-Rooting Defect (P4-T9) — STOP: FAIL-BEFORE WRONG REASON

Timestamp: 2026-10-03T11-28
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_AttachmentSaving_Tests" "/ResultsDirectory:coverage\test-results\959\p4-t9" "/Logger:trx;LogFileName=p4-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: The run is red with exactly the expected failing row (RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths) and ten passing rows, and the failure is the re-rooting defect (the alternate save path's directory is still the origin folder). The MESSAGE does NOT contain the plan's required substring `Sortemail959Sandbox\origin`: FluentAssertions' string-difference message truncates the left part of both strings to `…59Sandbox\origin`, so the literal is absent from both the console output and the TRX message text. Acceptance item 4 is not met, and under the Expect-fail wrong-reason branch this is the stop label FAIL-BEFORE WRONG REASON. P4-T9 is not checked off.

```
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
COUNTERS total=11 executed=11 passed=10 failed=1
RESULT_COUNT: 11
RESULT SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer = Passed
RESULT SaveAttachment_WhenFileDoesNotExist_SavesDirectly = Passed
RESULT RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths = Failed
RESULT Cleanup_Files_ResetsEveryPromptSession = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave = Passed
RESULT SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting = Passed
RESULT SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly = Passed
RESULT SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath = Passed
RESULT SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer = Passed
MESSAGE RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths :: Expected Path.GetDirectoryName(helper.FilePathSaveAlt) to be a match with the expectation, but it differs at index 23:
               (actual)
  "…59Sandbox\origin"
  "…59Sandbox\destination"
               (expected)
```

The MESSAGE above is the TRX `ErrorInfo/Message` text. The payload printed it with the ellipsis and the arrow glyphs mangled by the console encoding (`.` and blanks). The TRX file holds `"…59Sandbox\origin"` and `"…59Sandbox\destination"` (a U+2026 ellipsis), which was confirmed by a read-only search of the TRX. No host path appears in the message.

## Acceptance (P4-T9, all five required)

1. EXIT_CODE non-zero and equal to ExpectedExitCode (1 = 1): met.
2. COUNTERS total=11 executed=11 passed=10 failed=1: met.
3. The Failed row is exactly RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths, and the ten Passed rows are the other NAMES-TAS-FINAL names: met.
4. The MESSAGE line contains `Sortemail959Sandbox\origin`: NOT MET. The message carries `59Sandbox\origin` after the FluentAssertions truncation ellipsis. The full literal is not printed because FluentAssertions shortens a long string difference to a window around the first differing index (index 23 here).
5. Every SANDBOX value False: met.

Stop label: FAIL-BEFORE WRONG REASON (P4-T9). The executor did not adjust the test, the gate or the substring. A planner amendment is required to decide whether the substring gate is restated against the printed form (for example `59Sandbox\origin`, which occurs exactly in the observed message). On a re-run after that amendment, this fixed-name file is rewritten.
