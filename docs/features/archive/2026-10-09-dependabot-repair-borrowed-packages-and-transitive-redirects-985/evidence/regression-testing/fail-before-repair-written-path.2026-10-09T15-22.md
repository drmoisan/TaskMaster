# Fail-before: Repair entry-point redirect-sync tests (R1, CR-1 and CR-2, issue #985)

Timestamp: 2026-10-09T15-22
Command: pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-pester.ps1 -WorkspaceRoot WORKSPACE-ROOT -TestPath tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- Right reason: PESTER Passed=7 Failed=2 Skipped=0 NotRun=0 Total=9.
- Failed set is exactly N7 (`states the direction of the synchronised redirect in the body`) and N9 (`lists the application configuration once in the written paths`).
- N9 message: `Expected 1, but got 2.` (the application configuration is listed twice in WrittenPath).
- N8 (`applies both rewrites to the application configuration`) passed, so both passes wrote the fixture.

P2-T3 counts (`tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1`):
- `^\s+It '`: 9; `^\s+Context '`: 3.
- `New-TemporaryFile|GetTempPath|GetTempFileName|TestDrive|WriteAllText|Set-Content|Out-File|New-Item`: 0.
- Carriage returns (`\r$`): 0 (LF kept).

Failed tests (verbatim):
```
FAILED-TEST: Repair-PackageManifestConsistency redirect synchronisation (issue 985).A transitive redirect left stale by an upgrade elsewhere, with no candidate upgrade supplied.states the direction of the synchronised redirect in the body
FAILED-MESSAGE: Expected regular expression 'log4net 3\.4\.0\.0 to 3\.5\.0\.0 \(HighestDeployed, Upgrade\)' to match '## Repairs applied No repairs were applied.  ## Binding redirects synchronised - Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)', but it did not match.
FAILED-TEST: Repair-PackageManifestConsistency redirect synchronisation (issue 985).An application configuration rewritten by both the redirect sync and the normalisation pass.lists the application configuration once in the written paths
FAILED-MESSAGE: Expected 1, but got 2.
```
