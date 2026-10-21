# Command Channel Probe (P0-T3)

Timestamp: 2026-09-29T08-51
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; Write-Output "PROBE-OK"; Write-Output ("PSVERSION=" + $PSVersionTable.PSVersion.ToString()); Write-Output ("TS=" + (Get-Date).ToString("yyyy-MM-ddTHH-mm"))'
EXIT_CODE: 0
Output Summary:
- PROBE-OK
- PSVERSION=7.6.6 (begins with 7; System.IO.Path.GetRelativePath is available to the runner scripts)
- CHANNEL: COMMAND
- Note: the payload is the plan's probe prefixed by Set-Location to the worktree root (per E5, every command runs with the current directory at the worktree root) and suffixed by a timestamp read used for this artifact.
