# Pass-after probability and hierarchy upgrade

Timestamp: 2026-07-21T16-19Z

Build Command: `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`

Build EXIT_CODE: 0

Primary Test Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbRenderProjectionSelectorTests|FullyQualifiedName~FolderBreadcrumbBridgeRouterEdgeTests|FullyQualifiedName~FolderBreadcrumbBridgeRouterTests"`

Primary Test EXIT_CODE: 0

- Total: 34
- Passed: 34
- Failed: 0
- Skipped: 0
- Elapsed: 1.5030 seconds

Issue #398 Test Command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"Name=ReplaceRows_PreservesSelectionWhenIndexStillValid|Name=SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount|Name=SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives"`

Issue #398 Test EXIT_CODE: 0

- Total: 3
- Passed: 3
- Failed: 0
- Skipped: 0
- Discovered: `ReplaceRows_PreservesSelectionWhenIndexStillValid`
- Discovered: `SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount`
- Discovered: `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives`

Compatibility Correction: `Platform=AnyCPU` is the root-authorized mechanical equivalent for the invalid direct-project `Platform='Any CPU'` token.

Acceptance mapping:

- AC-1: collapsed projection contains the selected row and its unchanged formatter output.
- AC-10: synchronous, resolved, unresolved, empty-chain, and provider-failure paths retain score and stable identity.
- AC-11: off-model build, atomic replacement, in-flight selection, readback consistency, and stale-generation rejection pass.
- AC-19: existing router and issue #398 regression tests in the requested filters pass.

Output Summary: Both filtered runs passed with zero failures/skips, including all three named issue #398 tests.
