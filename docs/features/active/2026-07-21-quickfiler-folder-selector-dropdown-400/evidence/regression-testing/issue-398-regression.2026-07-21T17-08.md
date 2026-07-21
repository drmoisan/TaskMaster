# Issue #398 Regression

Timestamp: 2026-07-21T17:08:00Z

Command:

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"Name=ReplaceRows_PreservesSelectionWhenIndexStillValid|Name=ReplaceRows_ClearsSelectionWhenIndexBeyondNewCount|Name=SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount|Name=SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives|Name=SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection"
```

EXIT_CODE: 0

Passed: 5

Failed: 0

Skipped: 0

Discovered and passed:

- `ReplaceRows_PreservesSelectionWhenIndexStillValid`
- `ReplaceRows_ClearsSelectionWhenIndexBeyondNewCount`
- `SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount`
- `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives`
- `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection`
