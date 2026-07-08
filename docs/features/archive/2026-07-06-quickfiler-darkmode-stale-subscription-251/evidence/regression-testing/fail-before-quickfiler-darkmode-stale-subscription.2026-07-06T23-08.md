# Fail-Before Evidence — Issue #251 (QuickFiler Dark-Mode Stale Subscription)

Timestamp: 2026-07-06T23-47

Command: vstest.console.exe "QuickFiler.Test/bin/Debug/QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerDarkModeTests"

EXIT_CODE: 1

Output Summary: Total tests: 2. Failed: 2 (`Cleanup_ThenDarkModePropertyChanged_DoesNotThrow`, `CleanupAsync_ThenDarkModePropertyChanged_DoesNotThrow`). Both tests reproduce the reported defect: raising `PropertyChanged("DarkMode")` on the mocked `IOlObjects` after `Cleanup()`/`CleanupAsync()` throws `System.NullReferenceException: Object reference not set to an instance of an object.` at `QuickFiler.Controllers.QfcCollectionController.DarkMode_CheckedChanged(Object sender, EventArgs e)` in `QfcCollectionController.cs:line 2121` — the exact source location and exception type documented in `issue.md`'s Actual Behavior section. This confirms the regression tests reproduce the pre-fix defect (AC1, fail-before half) prior to any production code change.

Both tests were added to the pre-fix codebase (production `QfcCollectionController.cs` unmodified at the time of this run) and run against the build described in `implementation-scope.2026-07-06T23-08.md`. This evidence entry covers both P1-T2 (`Cleanup_ThenDarkModePropertyChanged_DoesNotThrow`) and P1-T3 (`CleanupAsync_ThenDarkModePropertyChanged_DoesNotThrow`).
