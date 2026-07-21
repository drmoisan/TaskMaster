# Phase 1 — Pass-After Regression Evidence (P1-T8)

Timestamp: 2026-07-20T22-14

Command: `vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Tests:SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection,SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount,SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives`
(Full solution rebuilt at Configuration=Debug with the atomic-swap fix applied before the run.)

EXIT_CODE: 0

Output Summary:
- Total tests: 3. Passed: 3. Failed: 0.
  - SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection (AC-1): mid-upgrade `SelectRow(1)` no longer throws; selection applied and survives the swap.
  - SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount (AC-2): observable row count stays at the pre-upgrade count (2) throughout the gated in-flight rebuild.
  - SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives (AC-3): FolderContains / GetSelectedFolder / SelectRow return pre-upgrade-consistent results in flight, and the host-selected index (1 -> path "\\Inbox\\Projects\\Zephyr") survives the atomic swap.
- The same test that failed pre-fix (fail-before.2026-07-20T21-41.md) now passes, completing AC-1's fail-before / pass-after pair.
- Fix: FolderBreadcrumbBridgeRouter.SetSuggestionsAsync builds the upgraded rows into a local list (no _model mutation while awaiting) and applies them via the new BreadcrumbStateModel.ReplaceRows atomic backing-list swap, which preserves a valid current selection.
