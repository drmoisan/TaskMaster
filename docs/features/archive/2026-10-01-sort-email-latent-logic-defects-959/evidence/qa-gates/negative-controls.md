# Negative Controls (PD-7)

Timestamp: 2026-10-06T15-12
ITERATION: 1
Command: pwsh -NoProfile -Command with Set-Location to the item worktree and one Test-Path -LiteralPath per artifact over the nine artifacts named below (the six fail-before artifacts under evidence/regression-testing/ and the three control artifacts p4-t13-control-backup.2026-10-03T12-14.md, p4-t14-control-applied.2026-10-03T12-15.md and p4-t15-control-restored.2026-10-03T12-17.md), printing ARTIFACTS-PRESENT
EXIT_CODE: 0 (the payload's process exit code)
Output Summary: ARTIFACTS-PRESENT: 9/9. Each of the six code fixes was observed red with its regression test present and the fix absent, with exactly the failing-row set of the Test Inventory table and the named MESSAGE substring, and each was closed by a green pass-after section. The structural test was proven discriminating by the mutation control, and the restore reproduced the fixed file byte for byte.

- ARTIFACTS-PRESENT: 9/9

## Red-first observations

| Fix | Failing rows observed | Passing rows | MESSAGE substring observed | Fail-before artifact | EXIT_CODE (ExpectedExitCode) | Pass-after section that closed it |
| --- | --- | --- | --- | --- | --- | --- |
| L1 (SaveCase combined case labels) | `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No]`, `[NoToAll]`, `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes]`, `[YesToAll]` | `SaveCase_WhenAnswerIsEmpty_DoesNotSave` | `but was 0 times` (all four rows) | regression-testing/fail-before-save-case.md | 1 (1) | `## Pass-after (P1-T11)`: 5 of 5 passed |
| L3 phase one (Cleanup_Files misses the alternate-name answer) | `Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsAltName]` | the other three NAMES-TAS-P1 rows | `but found` and `YesToAll` (`... but found YesNoToAllResponse.YesToAll {value: 4}.`) | regression-testing/fail-before-cleanup-files-phase-one.md | 1 (1) | `## Pass-after (P1-T12)`: 4 of 4 passed |
| L4 and the header (WriteCSV_StartNewFileIfDoesNotExist) | `WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader`, `WriteCSV_WhenFileExists_DoesNotWrite` | none | first test `differs at index 0`; second test `NullReferenceException` | regression-testing/fail-before-write-csv.md | 1 (1) | `## Pass-after (P2-T10)`: 2 of 2 passed |
| L2 (unbounded retry under a held YesToAll) | `TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear` | the eleven NAMES-T | `InvalidOperationException` (the tripwire sentinel) | regression-testing/fail-before-try-save-retry.md | 1 (1) | `## Pass-after (P3-T6)`: 12 of 12 passed (and TST1 14 of 14) |
| Re-rooting (RedirectSaveFolder alternate path) | `RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths` | the other ten NAMES-TAS-FINAL | `GetDirectoryName(helper.FilePathSaveAlt)`, `origin"` and `destination"` (all three; revision 1.8) | regression-testing/fail-before-redirect-save-folder.md (ITERATION 2) | 1 (1) | `## Pass-after (P4-T11)`: 11 of 11 passed |
| EfcDataModel (prompt-state reset when the filer throws) | `MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates` | the other two NAMES-TEF | `but found 0` | regression-testing/fail-before-efc-filer-cleanup.md | 1 (1) | `## Pass-after (P5-T9)`: 3 of 3 passed, and the eleven archive-root tests passed with `ART-PORCELAIN: EMPTY` |

Every failing-row set above equals the corresponding row of the plan's Test Inventory table (P1-T6, P1-T7, P2-T6, P3-T3, P4-T9, P5-T7).

## Mutation control (structural test)

The one test that cannot be observed red-first, `Cleanup_Files_ResetsEveryPromptSession`, received a mutation control: one element (`AttachmentsAltNamePrompt`) removed from `AllPromptSessions` by Edit E-A-CONTROL-MUTATE, the test observed failing, and the element restored by the inverse Edit E-A-CONTROL-RESTORE.

- FIX-HASH-A: 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2 (p4-t13-control-backup.2026-10-03T12-14.md)
- MUTATED-HASH-A: 0870C1B3D12C2F36F4098E0585680D09092C8A5F3E7B4B9DB51FE1E483496122 (p4-t14-control-applied.2026-10-03T12-15.md)
- RESTORED-HASH-A: 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2 (p4-t15-control-restored.2026-10-03T12-17.md, RESTORE-ROUTE: EDIT)
- Failing row under the mutation: `Cleanup_Files_ResetsEveryPromptSession` (COUNTERS total=11 executed=11 passed=10 failed=1, VSTEST_EXIT_CODE 1, ExpectedExitCode 1)
- MESSAGE substring: `but found 3` (`Expected resetTargets to contain 4 item(s), but found 3: ...`)
- Restored run: `COUNTERS total=11 executed=11 passed=11 failed=0`

RESTORED-HASH-A equals FIX-HASH-A, and MUTATED-HASH-A differs from both.

## Acceptance (P6-T17, all three required)

1. ARTIFACTS-PRESENT: 9/9: met.
2. Every row of the first section names a non-zero EXIT_CODE (1 in each) and a failing-row set equal to the Test Inventory table: met.
3. The second section shows RESTORED-HASH-A equal to FIX-HASH-A and MUTATED-HASH-A different from both: met.
