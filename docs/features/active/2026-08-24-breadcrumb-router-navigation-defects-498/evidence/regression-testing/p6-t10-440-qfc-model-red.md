# P6-T10 — #440 Qfc state-model transitions, fail-before (RED)

Timestamp: 2026-08-26T09-05

Command:

```
pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~LeftArrow_QfcMultiSegmentRow_SelectsParentNode|FullyQualifiedName~RightArrow_QfcSelectedParentNode_ExpandsIntoChildren" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t10"; "EXIT_CODE: $LASTEXITCODE"'
```

EXIT_CODE: 1

ExpectedExitCode: 1

TRX: `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t10/results.trx`

Output Summary:

- TRX `<Counters total="2" executed="2" passed="0" failed="2" ... />`. Test Run Failed. This is the expected fail-before state for the Qfc surface of #440.
- `LeftArrow_QfcMultiSegmentRow_SelectsParentNode` — Failed. Cause: "Expected handled to be True because Left selects the parent node before the pre-existing path, but found False." `BreadcrumbStateModel.LeftArrow` today only resets the subfolder selection and calls `row.TryCollapseLeaf()`, which reports no-op on a row with no open expansion, so no parent-select transition exists.
- `RightArrow_QfcSelectedParentNode_ExpandsIntoChildren` — Failed. Cause: "Expected handled to be True because the descent transition selects the first child, but found False." `BreadcrumbStateModel.RightArrow` today re-expands a collapse or opens the leaf expansion and has no descent mechanism, so a Right on an already-expanded row reports unhandled.

Satisfies the AC-28 fail-before obligation for the Qfc surface.
