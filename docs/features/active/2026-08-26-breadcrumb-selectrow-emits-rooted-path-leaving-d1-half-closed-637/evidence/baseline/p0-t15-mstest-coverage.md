Timestamp: 2026-08-31T10:07:03-04:00
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\\p0-t15-baseline.cobertura.xml
EXIT_CODE: unavailable (the wrapper launcher exited while its coverage and test descendants continued; both verified descendants were terminated after they stopped progressing).
Output Summary: The wrapper discovered 9 test assemblies. It did not produce the required Cobertura document or coverage attributes. Before termination, the captured run reported at least eight 60-second test timeouts; total/passed/failed totals are unavailable because the run did not finish.

BASELINE_EXECUTION_BLOCKED:
- Required output absent: coverage/p0-t15-baseline.cobertura.xml.
- The output-adjacent effective coverage configuration was cleaned up after the verified process-tree termination.
- Observed timed-out tests:
  - InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState
  - CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController
  - EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose
  - InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme
  - CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing
  - EnsureDispatcher_ScopeDisposedTwice_IsIdempotent
  - InitializeBool_ThroughThePumpHost_CompletesAndInitializesState
  - Transaction_SecondCallerCannotInstallUntilTheFirstRestores
- The process command lines were verified as targeting this worktree before only their two dotnet-coverage process trees were terminated.
- P0-T15 remains unchecked. P0-T16 and P0-T17 cannot run without the required Cobertura document.
