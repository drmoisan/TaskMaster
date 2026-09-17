# [P0-T2] Human checkpoint — Outlook closed before any build

- Issue: #792
- Timestamp: 2026-09-17T18-34
- Command: `$outlook = Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue; if ($outlook) { Write-Output 'HALT: Outlook is running. Close Outlook through File > Exit (never end the process), then re-run this task.'; exit 2 }; Write-Output 'OUTLOOK-CLOSED: true'` (CMD-OUTLOOK, run under `pwsh -NoProfile` with the item worktree as the working directory)
- EXIT_CODE: 0
- Output Summary: `OUTLOOK-CLOSED: true`. No `HALT:` line was printed. No process named `OUTLOOK` was found.

## Human confirmation

Outlook was closed by a person through its normal exit path before this check ran; the maintainer confirmed in the Phase 0 delegation that Outlook was verified not running immediately before execution began. No process was ended by this task or by any other task in this plan; `Stop-Process` was not called.
