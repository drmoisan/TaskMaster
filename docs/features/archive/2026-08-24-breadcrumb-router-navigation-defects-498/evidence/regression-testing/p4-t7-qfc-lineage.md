# P4-T7 — Qfc Lineage Resolution (multi-segment chain)

Timestamp: 2026-08-26T10-12

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~SetSuggestionsAsync_StrictProvider_ResolvesMultiSegmentChain" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p4-t7"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**GREEN.** TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p4-t7/results.trx`
records:

```
<Counters total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0"
          inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0"
          disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

| Test | Outcome | Duration |
|---|---|---|
| `SetSuggestionsAsync_StrictProvider_ResolvesMultiSegmentChain` | Passed | 260 ms |

Test home: `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`.

Strictness. The test uses a `MockBehavior.Strict` `IFolderHierarchyProvider` built by the new
`StemProviderMock` helper, which follows the existing `ProviderMock` pattern
(`FolderBreadcrumbBridgeRouterTests.cs:54-73`) but sets up `ResolveLeafKeyAsync` for the presented
archive-relative stem `Projects\Apollo` ONLY, and `GetAncestorChainAsync` for the resulting leaf key
only. A call with any other path form is an unmatched strict invocation and throws, so the test
cannot pass by resolving the wrong path form: `SetSuggestionsAsync` catches the throw and adds the
scored fallback row, whose `Chain` is empty and whose `IsSuggestion` is false, failing the
assertions. The run additionally verifies `ResolveLeafKeyAsync(StemPath, ...)` was called exactly
once.

Assertions. The resulting model row is a resolved suggestion row (`IsSuggestion` true, so not the
single-segment scored fallback), its `Chain.Count` is greater than 1, and the chain is in
root-to-leaf order: `Chain[0].DisplayName` is `Inbox`, `Chain[1].DisplayName` is `Projects`, and the
last segment's `DisplayName` is `Apollo`.

Test run summary reported by vstest: `Test Run Successful. Total tests: 1, Passed: 1`.

Satisfies AC-11.
