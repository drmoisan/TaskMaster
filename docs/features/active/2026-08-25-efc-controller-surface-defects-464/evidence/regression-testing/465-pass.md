# [P7-T12] #465 pass-after evidence — RC8, RC9 and RC7

Timestamp: 2026-08-28T01-24
Task: [P7-T12]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~MatchesForSearchText|FullyQualifiedName~WithTrashRow|FullyQualifiedName~ActionDeleteAsync_AwaitedTwice|FullyQualifiedName~IsBannerRow|FullyQualifiedName~IsSelectableFolder|FullyQualifiedName~Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter" "/Logger:trx;LogFileName=465-pass.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p7-t12` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 0

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

## Result — TRX `<Counters>`, verbatim

```
total="7" executed="7" passed="7" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **7** (non-zero, per the non-vacuity rule). Failed: **0**.

## Enumerated result names and outcomes

| # | Result name | Outcome | Remedy |
|---|---|---|---|
| 1 | `MatchesForSearchText_WithRepresentativeInput_ReturnsExpectedMatches` | Passed | RC8 |
| 2 | `WithTrashRow_AppliedTwice_YieldsExactlyOneTrashRow` | Passed | RC9 |
| 3 | `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` | Passed | RC9 |
| 4 | `IsBannerRow_ClassifiesByTheFourCharacterPrefix` | Passed | RC7 |
| 5 | `IsBannerRow_NullOrShortRow_ReturnsFalseWithoutThrowing` | Passed | RC7 |
| 6 | `IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` | Passed | RC7 |
| 7 | `Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter` (pre-existing) | Passed | non-regression |

Seven distinct result names, all `Passed`, matching the task's expected count exactly.

Row 7 is the pre-existing breadcrumb bind-boundary test. Its pass **proves the `BindFolderRows`
restructuring did not disturb the breadcrumb bind boundary**: `BindFolderRows` now passes its own
`rows` parameter to `BindBreadcrumbRowsAsync` instead of the field, and the router still receives the
archive root and renders the expected segments.

## Fail-before / pass-after pairing

| Row | Fail-before artifact | Pre-change outcome |
|---|---|---|
| 2, 3 | `465c-fail.md` | both **Failed**, each reporting two trash rows where one was expected |
| 4, 5 | `465d-fail.md` | both **Failed** — misclassified three-equals row; `ArgumentOutOfRangeException` on a short row |
| 6 | `465d-fail.md` | **Passed** before and after; pins an upstream-delivered (#614) non-regression property |
| 1 | none | RC8 has no behavioural fail-before; see the `[P7-T14]` exception dossier and the structural offset in `465-source-structure.md` |
| 7 | none | pre-existing non-regression guard |

## What the pass demonstrates

- **RC8 (#465 B).** The extracted pure matching helper returns the delegate result verbatim, yields an
  empty array for a null delegate or a null result, and passes an empty string through when the search
  text is null. The control read now happens on the UI thread before any `Task.Run`.
- **RC9 (#465 C).** `WithTrashRow` is idempotent, and awaiting `ActionDeleteAsync()` twice leaves
  exactly one trash row in `_folderRows` with both original rows intact. Retention moved out of
  `BindFolderRows` into `BindSourceFolderRows` and `ApplyDeleteGesture`.
- **RC7 (#465 D).** `IsBannerRow` classifies by `BreadcrumbRowBuilder.BannerPrefix` using
  `StartsWith` under `StringComparison.Ordinal`, never `Substring`, so a short row cannot throw. Both
  EFC classification sites route through it and agree.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p7-t12/465-pass.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The `/InIsolation`
`Deploy_*` scratch tree written into that directory was deleted.

Output Summary: PASS. 7 executed, 7 passed, 0 failed, EXIT_CODE 0. All three #465 remedies are green,
and the pre-existing breadcrumb bind-boundary test confirms the `BindFolderRows` restructuring did not
disturb that boundary.
