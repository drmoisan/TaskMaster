# P11-T8 Invoke-MSTestWithCoverage strict fail-before result

Timestamp: 2026-08-04T11-09

Command: `pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1' -Output Normal -PassThru; exit ([int]($result.FailedCount -gt 0))"`

EXIT_CODE: 1

Output Summary: Pester v5.6.1 discovered 17 tests. Fifteen passed and two failed. The new strict wrapper tests failed because neither `Invoke-MSTestWithCoverageMain` nor `Invoke-VsWhereExe` existed in the production script. No external executable, temporary file, mutable environment state, or coverage collection was invoked by the added tests.

Failure details:

- `Invoke-MSTestWithCoverage main wrapper seam.exposes a callable main entrypoint for isolated mocked execution`: expected a command but received no command.
- `Invoke-MSTestWithCoverage main wrapper seam.exposes a callable vswhere wrapper for executable-free tests`: expected a command but received no command.

The five error branches and `-NoExecute` command-shape behavior cannot be individually invoked before extraction because the production script has no callable main boundary. P11-T10 adds their fully mocked, in-process assertions after P11-T9 provides that boundary.
