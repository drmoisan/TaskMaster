# P0-T11 — NuGet Package Restore

Timestamp: 2026-08-31T18-51

OBSERVED_PACKAGES_PRESENT: False

Command: pwsh -File scripts/vscode/Invoke-Restore.ps1
EXIT_CODE: 0

PACKAGES_PRESENT_AFTER: True

Output Summary: `Test-Path packages` returned False before the restore. The restore was run unconditionally as the task requires, because restore is idempotent and a present-but-incomplete `packages` directory would defeat a presence-only precondition check. MSBuild's Restore target reported `Installed: 172 package(s) to packages.config projects`, then `Build succeeded. 0 Warning(s) 0 Error(s)`. `Test-Path packages` returns True after the restore, so the analyzer HintPath targets every first-party project references are now materialized.
