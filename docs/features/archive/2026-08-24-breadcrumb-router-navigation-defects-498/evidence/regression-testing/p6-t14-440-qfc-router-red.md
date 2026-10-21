# P6-T14 — #440 Qfc router routing, fail-before (RED)

Timestamp: 2026-08-26T09-45

Command:

```
pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition|FullyQualifiedName~ArrowAsync_QfcRightOnSelectedParentNode_RoutesChildExpansion" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t14"; "EXIT_CODE: $LASTEXITCODE"'
```

EXIT_CODE: 1

ExpectedExitCode: 1

TRX: `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t14/results.trx`

Output Summary:

- TRX `<Counters total="2" executed="2" passed="0" failed="2" ... />`. Test Run Failed. This is the expected fail-before state for the Qfc router half of #440.
- `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition` — Failed. Cause: `Moq.MockException` — "Expected invocation on the mock once, but was 0 times" for `GetImmediateSubfoldersAsync(MidKey, ...)`. `FolderBreadcrumbBridgeRouter.FetchAndAttachSubfoldersAsync` still keys its provider query on `row.Chain[row.Chain.Count - 1].Key`, the LEAF, so the expansion that follows the parent-select queries the wrong node. Against the strict provider that is an unmatched invocation, which the method's broad catch converts into a `BridgeErrorMessage`.
- `ArrowAsync_QfcRightOnSelectedParentNode_RoutesChildExpansion` — Failed. Cause: "Expected type to be RenderMessage, but found BridgeErrorMessage" — the same leaf-keyed query fails, so no children are ever attached and no descent is possible.
