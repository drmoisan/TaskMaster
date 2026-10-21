# Bootstrap: NuGet restore (issue 942)

Timestamp: 2026-09-30T07-25
Task: P0-T6
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $env:MSBUILDDISABLENODEREUSE = "1"; & .\scripts\vscode\Invoke-Restore.ps1; "RESTORE_EXIT=$LASTEXITCODE"; "PACKAGE_DIRS=..."; foreach ($proj in @("TaskMaster\TaskMaster.csproj", "TaskMaster.Test\TaskMaster.Test.csproj")) { ... "ANALYZER_MISSING $proj = $missing" }'
EXIT_CODE: 0

Output Summary:
- RESTORE_EXIT=0
- PACKAGE_DIRS=172
- ANALYZER_MISSING TaskMaster\TaskMaster.csproj = 0
- ANALYZER_MISSING TaskMaster.Test\TaskMaster.Test.csproj = 0
- Every analyzer Include of the two Write Set projects resolves relative to its own project directory; no ANALYZER PATH SKEW.
- Execution note: the restore script's console output was redirected to an ignored log under the repository coverage directory (it carries absolute host paths); the payload's gate lines above are unchanged.
