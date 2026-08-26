# P5-T3 — Decision D7 Rung 1 GREEN (filing target preserved in owned files)

Timestamp: 2026-08-26T10-40

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~SelectedFolder_ChainResolvedToFullPath_RemainsPresentedStem" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p5-t3"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**RUNG 1 APPLIES AND IS DELIVERED.** The `P4-T1` artifact
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/other/p4-t1-d7-rung-verification.md`
records the line `D7 RUNG SELECTED: 1`, so this task executes and `P5-T4` and `P5-T5` are recorded
NOT APPLICABLE.

### Test result

TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p5-t3/results.trx`
records:

```
<Counters total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0"
          inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
          disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

| Test | Outcome | Duration |
|---|---|---|
| `SelectedFolder_ChainResolvedToFullPath_RemainsPresentedStem` | Passed | 398 ms |

`EXIT_CODE:` is `0`. The same test and filter were recorded RED before the change in
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p5-t2-d7-red.md`.

### `BreadcrumbSelectionMap.cs` is unmodified

```
git status --porcelain --untracked-files=all -- UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs
```

produces **no output**. The rung-1 condition — that the value read at
`BreadcrumbSelectionMap.cs:109` becomes the presented stem without writing that file — holds.

### What changed, and where

Both files written are OWNED.

1. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`. `BreadcrumbStateRow` gains an
   internal constructor overload taking the presented `filingTarget` alongside the resolved chain,
   and a private static `WithFilingTarget` that returns the chain with the LEAF segment's
   `FolderPath` replaced by that filing target, keeping the leaf's `Key`, `DisplayName` and
   `HasChildren`. The row itself stays immutable and every existing constructor is unchanged.
2. `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`. `SetSuggestionsAsync` now
   constructs the resolved suggestion row through that overload, passing the presented path it
   already holds in the local `path` (`row.Score.Value.FolderPath`).

Why this is confined to the filing value: the leaf segment's `FolderPath` is read in exactly one
place, `BreadcrumbSelectionMap.RowValue` (`:109`). Rendering reads `Chain[i].DisplayName`
(`BreadcrumbRenderProjection.cs:177`) and leaf expansion reads the leaf `Key`
(`FolderBreadcrumbBridgeRouter.cs:416` before this change), both preserved unchanged. The
enumeration of chain readers is recorded in section 5 of the `P4-T1` artifact.

The shape follows the landed Efc precedent `BreadcrumbRow.FilingTarget`
(`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:88`): an immutable per-row filing target held
independently of the resolved hierarchy paths.

### File sizes after the change

| File | Lines |
|---|---:|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 500 |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 487 |

Both at or under the 500-line limit.

### No regression in the surrounding test classes

A scoped run over every `UtilitiesCS.Test` class in the `OutlookObjects.Folder` namespace
(`/TestCaseFilter:FullyQualifiedName~OutlookObjects.Folder`, results written to the session
scratchpad) reported `Test Run Successful. Total tests: 727, Passed: 727`, so the substitution
breaks no existing selection, rendering or identity assertion.

Satisfies the AC-14 rung-1 criterion.
