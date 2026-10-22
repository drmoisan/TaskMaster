# [P5-T12] RC1 and RC2 pass-after evidence

Timestamp: 2026-08-28T01-00
Task: [P5-T12]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcItemControllerCleanupTests|FullyQualifiedName~FormDarkMode_OnAllFieldsNullController|FullyQualifiedName~FormActiveTheme_OnAllFieldsNullController|FullyQualifiedName~FormLoadTheme_OnAllFieldsNullController|FullyQualifiedName~FormCleanup_CalledTwice|FullyQualifiedName~FormCleanup_InvokesParentCleanupExactlyOnce" "/Logger:trx;LogFileName=rc1-pass.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p5-t12` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 0

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

## Result — TRX `<Counters>`, verbatim

```
total="13" executed="13" passed="13" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **13** (non-zero, per the non-vacuity rule). Failed: **0**.

## Enumerated result names and outcomes

| # | Result name | Outcome |
|---|---|---|
| 1 | `FormDarkMode_OnAllFieldsNullController_ReturnsFalseAndDoesNotThrow` | Passed |
| 2 | `FormActiveTheme_OnAllFieldsNullController_ReturnsBackingFieldAndDoesNotThrow` | Passed |
| 3 | `FormLoadTheme_OnAllFieldsNullController_DoesNotThrow` | Passed |
| 4 | `FormCleanup_CalledTwice_DoesNotThrow` | Passed |
| 5 | `FormCleanup_InvokesParentCleanupExactlyOnce` | Passed |
| 6 | `Cleanup_OnFiveArgumentConstructedController_DoesNotThrow` | Passed |
| 7 | `Cleanup_CalledTwice_DoesNotThrow` | Passed |
| 8 | `Cleanup_NullsButtonsField` | Passed |
| 9 | `Cleanup_DisposesTimerBeforeNullingIt` | Passed |
| 10 | `ApplyReadEmailFormat_AfterCleanup_DoesNotThrow` | Passed |
| 11 | `SubjectSenderAndTo_ReadFromItemInfo_AndAreInertAfterCleanup` | Passed |
| 12 | `ItemDarkMode_OnNullGlobalsController_ReturnsFalseAndDoesNotThrow` | Passed |
| 13 | `ItemActiveThemeAndLoadTheme_OnNullThemesController_DoNotThrow` | Passed |

Thirteen distinct result names, all with outcome `Passed`. Every one of the thirteen was recorded red in
the two fail-before artifacts `rc1-form-fail.md` (5 of 5 failed) and `rc1-item-fail.md` (8 of 8 failed),
so the pair is a complete fail-before / pass-after set for RC1 and RC2.

## What the pass demonstrates

- **RC1 accessor contract.** `EfcFormController.DarkMode`, `.ActiveTheme` and `.LoadTheme()`, and
  `EfcItemController.DarkMode`, `.ActiveTheme` and `.LoadTheme()` are all readable on a torn-down
  controller. The `params object[]` dependency array is no longer materialised on the null path.
- **RC1 lifecycle contract.** Both `Cleanup()` methods are callable on a partially constructed
  controller and are idempotent. `EfcFormController._parentCleanup` is invoked exactly once across two
  consecutive `Cleanup()` calls and is null afterwards.
- **RC1 field contract.** `EfcItemController._buttons` is null after `Cleanup()` returns, and
  `Subject`, `Sender` and `To` all read the cached `_itemInfo` model and stay inert after teardown.
- **RC2 timer contract.** The injected timer is disposed before its field is nulled;
  `Timer.Change(0, Timeout.Infinite)` on the captured instance throws `ObjectDisposedException`. The
  timer was armed with `Timeout.Infinite` for both the due time and the period, so it could never fire
  and no test waited on it.
- **RC2 callback contract.** `ApplyReadEmailFormat(null)` on a freshly cleaned controller returns
  silently without side effect, following the 484 post-teardown shape of an early return that does not
  log.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p5-t12/rc1-pass.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The `/InIsolation`
`Deploy_*` scratch tree written into that directory was deleted.

Output Summary: PASS. 13 executed, 13 passed, 0 failed, EXIT_CODE 0. All thirteen RC1/RC2 regression
tests are green after the guards, having been recorded red before them.
