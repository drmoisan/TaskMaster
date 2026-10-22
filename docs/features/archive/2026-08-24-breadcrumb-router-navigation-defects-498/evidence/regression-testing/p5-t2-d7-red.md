# P5-T2 — Qfc Filing-Target Regression RED (fail-before)

Timestamp: 2026-08-26T10-24

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~SelectedFolder_ChainResolvedToFullPath_RemainsPresentedStem" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p5-t2"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

**RED as required.** This is an `[expect-fail]` task; the failing result is the intended outcome and
is the fail-before evidence for the decision-D7 ladder. A passing result here would have been a
failure of the task.

TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p5-t2/results.trx`
records:

```
<Counters total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0"
          inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
          disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

| Test | Outcome |
|---|---|
| `SelectedFolder_ChainResolvedToFullPath_RemainsPresentedStem` | Failed |

**Observed value: the store-qualified path.** The recorded failure message, with the carriage-return
entities collapsed, reads:

```
Expected router.GetSelectedFolder() to be a match with the expectation because the filing target
is the presented stem, not the resolved leaf path, but it differs at index 0:
    (actual)
  "\Inbox\Projects\Apollo"
  "Projects\Apollo"
    (expected)
```

The presented stem is `Projects\Apollo`; the observed selected-folder value is
`\Inbox\Projects\Apollo`, the full store-qualified path carried by the resolved chain's leaf
segment.

Cause, named: with the decision-D5 resolution delivered in Phase 4, the Qfc row is now a resolved
suggestion row rather than a scored fallback, so
`BreadcrumbSelectionMap.RowValue` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs:109`)
takes its `IsSuggestion` branch and returns `row.Chain[row.Chain.Count - 1].FolderPath`. That leaf
segment is the one the provider returned, so its `FolderPath` is the full path rather than the
presented stem. The rung-1 preservation is applied by `P5-T3`; the read-only analysis selecting
rung 1 is recorded in
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/other/p4-t1-d7-rung-verification.md`.

Test run summary reported by vstest: `Test Run Failed. Total tests: 1, Failed: 1`.
