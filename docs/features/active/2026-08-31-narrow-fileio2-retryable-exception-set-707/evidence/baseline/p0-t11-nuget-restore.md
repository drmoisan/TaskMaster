Timestamp: 2026-09-03T11-59
Command: Test-Path packages ; pwsh -File scripts/vscode/Invoke-Restore.ps1
EXIT_CODE: 0

OBSERVED_PACKAGES_PRESENT (before): False
OBSERVED_PACKAGES_PRESENT (after): True

Output Summary: MSBuild restore succeeded, 172 package(s) restored to packages.config projects, 0 Warning(s), 0 Error(s). `Test-Path packages` returns True after the restore.
