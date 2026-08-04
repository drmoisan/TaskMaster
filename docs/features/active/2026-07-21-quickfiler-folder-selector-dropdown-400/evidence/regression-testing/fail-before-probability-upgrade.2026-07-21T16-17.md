# Fail-before probability and hierarchy upgrade

Timestamp: 2026-07-21T16-17Z

Build Command: `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`

Build EXIT_CODE: 0

Filtered Test Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbRenderProjectionSelectorTests|FullyQualifiedName~FolderBreadcrumbBridgeRouterEdgeTests|FullyQualifiedName~FolderBreadcrumbBridgeRouterTests"`

Filtered Test EXIT_CODE: 1

- Total: 34
- Passed: 25
- Failed: 9
- Skipped: 0

Intended fail-before signatures:

- `Project_ScoredFallback_UsesFallbackTextAndUnchangedFormatterOutput`: scored fallback projected as a non-suggestion and dropped its percentage.
- `ProjectCollapsed_ReturnsExactlyCommittedSelectedDataRow` and `ProjectCollapsed_NoSelectionOrNonSelectableSelection_ReturnsNoDataRow`: missing dedicated one-row collapsed projection.
- `SetSuggestions_ResolvedKeyButEmptyChain_FallsBackToPlainPathRow`: fallback lost the scored-fallback discriminator and score.
- `SetSuggestionFallbacks_SynchronouslyRetainsIdentityPathAndProbability`: missing synchronous scored-fallback population method.
- `SetSuggestions_ResolvedHierarchy_RetainsFallbackIdentityAndProbability`: resolved hierarchy replaced the stable path identity with a provider-key identity.
- `SetSuggestions_ProviderFailure_PreservesScoredFallback`: provider failure escaped and left no scored fallback.
- `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives`: upgraded selection did not retain the stable path identity.
- `SetSuggestionsAsync_OlderCompletionCannotOverwriteNewerGeneration`: an older completion overwrote the newer result.

Compatibility Correction: `Platform=AnyCPU` is the root-authorized mechanical equivalent for the invalid direct-project `Platform='Any CPU'` token. No behavioral scope changed.

Output Summary: The test project compiled, 34 tests were discovered, and nine failed only on the intended probability-drop, fallback-conversion, collapsed-projection, stable-selection, and stale-generation defects. There was no compile, discovery, tool-resolution, UI, or display failure.
