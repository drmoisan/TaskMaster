# Baseline: dotnet tool restore — issue #877

Timestamp: 2026-09-13T10-44
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; Write-Host "CWD=$((Get-Location).Path)"; dotnet tool restore; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0
Output Summary: `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` followed by `Restore was successful.` The manifest-pinned CSharpier version 1.2.6 is available in this worktree. Run under an acquired build lock, released immediately after the command returned.

## Working-directory verification

The payload emitted `CWD=<repo-root>` resolving to the 877 worktree root, confirming the mandatory `Set-Location` prefix took effect and that the restore applied to this checkout rather than to the coordinator session worktree. Host paths in the `Command:` row are retained only where they name the worktree the prefix is required to select; the emitted `CWD` value is redacted to repository-relative form here.

## Lock discipline

- Acquire: `pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "<parallel-build-lock>/acquire.txt"))) 877'` — `ACQUIRED 877`.
- Release: `pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "<parallel-build-lock>/release.txt"))) 877'` — `RELEASED by 877`.
