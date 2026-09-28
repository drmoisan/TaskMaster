# [P7-T1] Human checkpoint — Outlook closed before the final rebuilds

- Issue: #792
- Timestamp: 2026-09-17T21-02
- Command: `$outlook = Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue; if ($outlook) { Write-Output 'HALT: Outlook is running. Close Outlook through File > Exit (never end the process), then re-run this task.'; exit 2 }; Write-Output 'OUTLOOK-CLOSED: true'` (CMD-OUTLOOK, run from `coverage/plan792-helper.ps1 -Step outlook` under `pwsh -NoProfile` with the item worktree as the working directory; the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`)
- EXIT_CODE: 0
- Output Summary: `OUTLOOK-CLOSED: true`. No `HALT:` line was printed. No process named `OUTLOOK` was found.

## Positive control

The same `Get-Process -Name <name> -ErrorAction SilentlyContinue` form applied to `pwsh` returned 9 processes in the same invocation, so a null result for `OUTLOOK` is a true absence rather than a cmdlet that reports nothing.

## Human confirmation

The maintainer confirmed in the Phase 7 delegation that Outlook was verified not running immediately before this phase began. No process was ended by this task or by any other task in this plan; `Stop-Process` was not called. Because the loop in this phase can restart and every build task ([P7-T4], [P7-T5]) re-runs CMD-OUTLOOK, a running Outlook at any later check HALTS that task for a human rather than being closed by the executor.
