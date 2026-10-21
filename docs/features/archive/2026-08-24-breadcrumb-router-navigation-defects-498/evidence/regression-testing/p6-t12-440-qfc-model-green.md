# P6-T12 — #440 Qfc state-model transitions, pass-after (GREEN)

Timestamp: 2026-08-26T09-25

Command:

```
pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~LeftArrow_QfcMultiSegmentRow_SelectsParentNode|FullyQualifiedName~RightArrow_QfcSelectedParentNode_ExpandsIntoChildren" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t12"; "EXIT_CODE: $LASTEXITCODE"'
```

EXIT_CODE: 0

TRX: `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t12/results.trx`

Output Summary:

- TRX `<Counters total="2" executed="2" passed="2" failed="0" ... />`. Test Run Successful.
- `LeftArrow_QfcMultiSegmentRow_SelectsParentNode` — Passed (42 ms).
- `RightArrow_QfcSelectedParentNode_ExpandsIntoChildren` — Passed (1 ms).
- Fix under test: `BreadcrumbStateRow` gained the #440 selected-node state (`ActiveSegmentIndex`, `ActiveSegment`, `ActiveSegmentHasSubfolders`, `ActivateSegment`, `GetActiveChild`, `TryExpandActiveSegment`) in the new sibling file `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs`, and `BreadcrumbStateModel.LeftArrow` / `RightArrow` now attempt the tree transition before the pre-existing behavior.
- Collateral test update recorded for the audit trail: `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` in `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` (a file this plan owns and writes under `P6-T13`) now presses Left twice. Its purpose is unchanged — an unhandled Left still reaches the `UnhandledArrowMessage` fall-through — but under the #440 contract the first Left consumes the one available parent-select transition. No must-not-write test class was modified.
- Scoped verification of the surrounding suite: `FullyQualifiedName~Breadcrumb` over `UtilitiesCS.Test` reports Total 291, Passed 291, Failed 0.
