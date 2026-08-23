# Preserved Contract Correction Fail-Before

Timestamp: 2026-07-22T22-52

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; & $vstest 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation /Tests:SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives,MalformedInboundMessage_PostsRouterErrorResponse,SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection /Logger:'console;Verbosity=normal'`

EXIT_CODE: 1

Output Summary: VSTest 18.8.0 discovered exactly 3 tests. All 3 failed for the intended preserved-contract reasons, with no missing or unrelated failure.

- `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives` expected selected index 1 and observed -1 after plain rows were replaced by source-qualified suggestion rows.
- `MalformedInboundMessage_PostsRouterErrorResponse` was rejected by `BreadcrumbUiDispatcher.CaptureCurrent` because its host-neutral test harness constructed the public coordinator without an owning `SynchronizationContext`.
- `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection` was rejected at the same public-constructor UI-context boundary.

Reconciliation: This exact three-test run confirms the three deterministic failures first recorded in `preserved-breadcrumb-contracts-fail.2026-07-23T02-32.md`. The earlier artifact used the exact 23-test preserved filter and reported 20 passing and these same 3 failing tests. No P7 batch-C correction edit had been made before this run.
