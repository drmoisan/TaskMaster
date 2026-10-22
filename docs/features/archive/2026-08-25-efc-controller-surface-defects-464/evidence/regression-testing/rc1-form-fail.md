# [P5-T3] RC1 form-side fail-before evidence

Timestamp: 2026-08-28T00-53
Task: [P5-T3] [expect-fail]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~FormDarkMode_OnAllFieldsNullController|FullyQualifiedName~FormActiveTheme_OnAllFieldsNullController|FullyQualifiedName~FormLoadTheme_OnAllFieldsNullController|FullyQualifiedName~FormCleanup_CalledTwice|FullyQualifiedName~FormCleanup_InvokesParentCleanupExactlyOnce" "/Logger:trx;LogFileName=rc1-form-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p5-t3` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 1
ExpectedExitCode: 1

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

**Recorded platform-switch substitution.** The plan writes the project-level build as
`"/p:Platform=Any CPU"`. Against `QuickFiler.Test\QuickFiler.Test.csproj` that spaced form fails with
`MSB error: The BaseOutputPath/OutputPath property is not set for project 'QuickFiler.Test.csproj'`,
because the project defines the `AnyCPU` platform and not `Any CPU`. The unspaced `/p:Platform=AnyCPU`
is used for every project-level intermediate build in this plan. Solution-level gate commands keep the
spaced `"/p:Platform=Any CPU"` form verbatim, and this substitution is confined to decision D3
intermediate builds, which are never cited as analyzer or nullable gates.

## Result — TRX `<Counters>`, verbatim

```
total="5" executed="5" passed="0" failed="5" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **5** (non-zero, per the non-vacuity rule). Failed: **5**.

## Enumerated result names, outcomes, and failure exception types

| # | Result name | Outcome | Exception type | Faulting production line |
|---|---|---|---|---|
| 1 | `FormDarkMode_OnAllFieldsNullController_ReturnsFalseAndDoesNotThrow` | Failed | `System.NullReferenceException` | `EfcFormController.get_DarkMode()`, `EfcFormController.cs:275` |
| 2 | `FormActiveTheme_OnAllFieldsNullController_ReturnsBackingFieldAndDoesNotThrow` | Failed | `System.ArgumentNullException` | `Initializer.DependenciesNotNull` under `strict: true` from `EfcFormController.cs:255` |
| 3 | `FormLoadTheme_OnAllFieldsNullController_DoesNotThrow` | Failed | `System.NullReferenceException` | `EfcFormController.get_DarkMode()` reached from `LoadTheme()`, `EfcFormController.cs:266` |
| 4 | `FormCleanup_CalledTwice_DoesNotThrow` | Failed | `System.NullReferenceException` | `EfcFormController.Cleanup()`, `EfcFormController.cs:189` |
| 5 | `FormCleanup_InvokesParentCleanupExactlyOnce` | Failed | `System.NullReferenceException` | `EfcFormController.Cleanup()`, `EfcFormController.cs:189` |

Five distinct result names, all with outcome `Failed`. This is the fail-before evidence for the
form-side RC1 defects: the unguarded `_globals.Ol` dereference in the `DarkMode` getter's eagerly
materialised `params object[]` argument array, the `strict: true` dependency check in the `ActiveTheme`
getter, the unguarded `_themes` indexer in `LoadTheme`, and the unguarded `_globals.Ol.PropertyChanged`
detach at the head of `Cleanup()`.

Failure 2's exception type is `ArgumentNullException` rather than `NullReferenceException` because
`Initializer.DependenciesNotNull(strict: true, ...)` throws rather than returning `false` when a
dependency is null (`UtilitiesCS/HelperClasses/Initializer.cs:310-322`). The defect is the same
class — an accessor that cannot be read on a torn-down controller — and the fail-before signal is
equally discriminating.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p5-t3/rc1-form-fail.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The
`/InIsolation` `Deploy_*` scratch tree written into that directory was deleted.

Output Summary: EXPECT-FAIL SATISFIED. 5 executed, 5 failed, EXIT_CODE 1 against ExpectedExitCode 1. All
five RC1 form-side accessor and lifecycle tests are red against the unguarded pre-change source, with the
faulting production line recorded for each.
