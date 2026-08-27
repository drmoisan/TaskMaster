# P6-T16 — #440 Qfc router routing, pass-after (GREEN)

Timestamp: 2026-08-26T09-55

Command:

```
pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition|FullyQualifiedName~ArrowAsync_QfcRightOnSelectedParentNode_RoutesChildExpansion" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t16"; "EXIT_CODE: $LASTEXITCODE"'
```

EXIT_CODE: 0

TRX: `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t16/results.trx`

Output Summary:

- TRX `<Counters total="2" executed="2" passed="2" failed="0" ... />`. Test Run Successful.
- `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition` — Passed (213 ms).
- `ArrowAsync_QfcRightOnSelectedParentNode_RoutesChildExpansion` — Passed (8 ms).
- Fix under test: `FolderBreadcrumbBridgeRouter.FetchAndAttachSubfoldersAsync` now keys its provider query on the row's ACTIVE node (`row.ActiveSegment ?? row.Chain[row.Chain.Count - 1]`) instead of unconditionally on the leaf, so the expansion that follows a parent-select queries the selected node. The `UnhandledArrowMessage` emission in `ArrowAsync` is unchanged and remains the fall-through.

Satisfies AC-17.
