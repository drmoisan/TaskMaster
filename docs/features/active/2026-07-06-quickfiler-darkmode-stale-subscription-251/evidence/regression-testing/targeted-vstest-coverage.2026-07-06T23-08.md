# Targeted Post-Fix Regression Run — Issue #251

Timestamp: 2026-07-06T23-56

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerDarkModeTests"

(Executed as: `vstest.console.exe "QuickFiler.Test/bin/Debug/QuickFiler.Test.dll" /EnableCodeCoverage /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerDarkModeTests"` in this Git Bash environment; `/InIsolation` is required per environment notes for Moq-based assemblies. `~` substring filter operator used per prior confirmed vstest 18.7.0 behavior in this repo.)

EXIT_CODE: 0

Output Summary: Total tests: 2. Passed: 2. Failed: 0.
- `Cleanup_ThenDarkModePropertyChanged_DoesNotThrow` — Passed [349 ms].
- `CleanupAsync_ThenDarkModePropertyChanged_DoesNotThrow` — Passed [3 ms].

Both tests include, as part of their Assert step, `Mock<IQfcItemController>.Verify(c => c.SetThemeDark(It.IsAny<bool>()), Times.Never)` and the equivalent `SetThemeLight` verification against a mocked `IQfcItemController` injected into the controller's private `_itemGroups` field after cleanup. Because both tests passed (no `MockException` raised by the `Verify` calls), this confirms `SetThemeDark`/`SetThemeLight` were never invoked when `PropertyChanged("DarkMode")` was raised after `Cleanup()`/`CleanupAsync()`. This satisfies AC1 (pass-after half) and AC5.

Coverage attachment: `TestResults/e82d0f58-abc7-409c-8f08-829cdecea047/DanMoisan_MEGALODON4_2026-07-06.23_33_57.coverage` (not separately converted here; full-suite post-change coverage conversion is captured in Phase 2, P2-T4/P2-T5).
