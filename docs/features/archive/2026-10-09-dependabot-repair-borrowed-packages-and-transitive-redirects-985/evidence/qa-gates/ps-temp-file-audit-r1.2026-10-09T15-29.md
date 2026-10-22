# PowerShell Temporary-file Audit (R1, issue #985)

ITERATION: 1
Timestamp: 2026-10-09T15-29
Command: Grep tool, pattern `New-TemporaryFile|GetTempPath|GetTempFileName|TestDrive|WriteAllText|Set-Content|Out-File|New-Item`, count mode
EXIT_CODE: 0
Output Summary:
- tests/scripts/dependencies/BindingRedirectSync.Tests.ps1: 0
- tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1: 0
- Positive control, scripts/dependencies/Repair-PackageManifestConsistency.ps1: 1 (line 373, the default writer's `WriteAllText`), so the matcher is live.
- Result: PASS; no SWEEP-BLIND condition.
