# P4-T1 — Outlook-closed precondition, re-confirmed before the Phase 4 rebuilds

Timestamp: 2026-09-13T23-31

Command: `Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue`

EXIT_CODE: 0

Output Summary: the command returned no process object. The pipeline result was `$null`, so the
guard printed `NO_OUTLOOK_PROCESS`. Microsoft Outlook was CLOSED when this check ran, and remained
so through the Phase 4 rebuilds in P4-T4 and P4-T5.

Outlook was not running, so nothing had to be closed and nothing was killed. Had it been running,
the correct action would have been to close it through its own user interface; killing it is
prohibited because a killed process can leave the add-in build output locked, which is the very
failure this precondition exists to prevent.

`Get-Process` is a PowerShell cmdlet rather than a native executable, so `$LASTEXITCODE` is not
populated by it. The recorded exit code of 0 records that the cmdlet completed with no terminating
error.

This artifact is the evidence AC7 cites for the clause that the measurement required no live
Outlook process.
