# P0-T6 — Outlook-closed precondition (baseline)

Timestamp: 2026-09-13T22-58

Command: `Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue`

EXIT_CODE: 0

Output Summary: the command returned no process object. The pipeline result was `$null`, so the
guard printed `NO_OUTLOOK_PROCESS`. Microsoft Outlook is CLOSED. No process was killed and none
needed to be; the precondition was already satisfied when the check ran.

`Get-Process` is a PowerShell cmdlet rather than a native executable, so `$LASTEXITCODE` is not
populated by it. The recorded exit code of 0 records that the cmdlet completed with no terminating
error, which is the observation this gate needs.

Why this precondition exists: a running Outlook holds a lock on the add-in build output, and a
rebuild then fails to write its assemblies. Every rebuild in this plan (P0-T13, P0-T14, P1-T7,
P2-T4, P4-T4, P4-T5) depends on this check. It is re-confirmed before the Phase 4 rebuilds by
P4-T1.
