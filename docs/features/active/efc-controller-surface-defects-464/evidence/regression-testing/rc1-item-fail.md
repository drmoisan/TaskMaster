# [P5-T5] RC1 item-side fail-before evidence

Timestamp: 2026-08-28T00-57
Task: [P5-T5] [expect-fail]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcItemControllerCleanupTests" "/Logger:trx;LogFileName=rc1-item-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p5-t5` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 1
ExpectedExitCode: 1

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

The unspaced `/p:Platform=AnyCPU` substitution is recorded in
`evidence/regression-testing/rc1-form-fail.md` and applies to every decision-D3 intermediate build.

## Result — TRX `<Counters>`, verbatim

```
total="8" executed="8" passed="0" failed="8" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **8** (non-zero, per the non-vacuity rule). Failed: **8**.

## Enumerated result names and outcomes

| # | Result name | Outcome | Failure reason |
|---|---|---|---|
| 1 | `Cleanup_OnFiveArgumentConstructedController_DoesNotThrow` | **Failed** | `System.ArgumentNullException` from the unconditional `Buttons.ForEach` at `EfcItemController.cs:224`; `_buttons` is null because `Initialize` was never run |
| 2 | `Cleanup_CalledTwice_DoesNotThrow` | **Failed** | `System.ArgumentNullException` from the same unguarded `Buttons.ForEach` at `EfcItemController.cs:224` |
| 3 | `Cleanup_NullsButtonsField` | **Failed** | the `Cleanup()` call threw before reaching any assertion; `_buttons` is never nulled by the pre-change body |
| 4 | `Cleanup_DisposesTimerBeforeNullingIt` | **Failed** | the `Cleanup()` call threw before reaching the timer; the pre-change body nulls `_timer` at `:244` without disposing it |
| 5 | `ApplyReadEmailFormat_AfterCleanup_DoesNotThrow` | **Failed** | the preparatory `Cleanup()` threw; `ApplyReadEmailFormat` itself has no null-collaborator guard |
| 6 | `SubjectSenderAndTo_ReadFromItemInfo_AndAreInertAfterCleanup` | **Failed** | assertion failure: `observedSubject` did not match the injected `_itemInfo.Subject`, because `Subject` reads `_itemViewer.LblSubject.Text` at `EfcItemController.cs:578` instead of the cached model |
| 7 | `ItemDarkMode_OnNullGlobalsController_ReturnsFalseAndDoesNotThrow` | **Failed** | `System.NullReferenceException` from the eagerly materialised `params object[]` argument array in `EfcItemController.get_DarkMode()` at `EfcItemController.cs:408` |
| 8 | `ItemActiveThemeAndLoadTheme_OnNullThemesController_DoNotThrow` | **Failed** | `System.ArgumentNullException` from `Initializer.DependenciesNotNull(strict: true, ...)` reached from `EfcItemController.get_ActiveTheme()` at `EfcItemController.cs:361` |

Eight distinct result names, all with outcome `Failed`. The plan requires at least rows 1, 3, 4, 5, 6 and
8 to be red; all eight are.

Row 6 fails as an assertion rather than an exception, which is the discriminating shape for that defect:
`Sender` and `To` already read `_itemInfo`, `Subject` alone reads a control, and the injected
`Mock<MailItemHelper>` makes the divergence observable without a live Outlook item. The mock never assigns
`MailItemHelper.UnRead`, whose setter writes through to `Item.Save()`.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p5-t5/rc1-item-fail.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`; the substitution was
applied case-insensitively because vstest writes the `storage=` attribute in lower case). The
`/InIsolation` `Deploy_*` scratch tree written into that directory was deleted.

Output Summary: EXPECT-FAIL SATISFIED. 8 executed, 8 failed, EXIT_CODE 1 against ExpectedExitCode 1. All
eight RC1/RC2 item-side tests are red against the unguarded pre-change source, with the faulting
production line recorded for each.
