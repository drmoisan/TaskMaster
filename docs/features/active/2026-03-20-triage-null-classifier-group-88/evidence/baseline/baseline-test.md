# Baseline — Test Run

- **Timestamp:** 2026-03-20T09-49
- **Command:** `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug` (wraps vstest.console.exe over discovered *.Test.dll assemblies)
- **EXIT_CODE:** 1
- **Output Summary:** Test run aborted. Test host process crashed with StackOverflowException (pre-existing). Results before crash: 447 passed, 2 skipped, total time ~7.9s. The StackOverflowException is a pre-existing baseline condition unrelated to this change.
