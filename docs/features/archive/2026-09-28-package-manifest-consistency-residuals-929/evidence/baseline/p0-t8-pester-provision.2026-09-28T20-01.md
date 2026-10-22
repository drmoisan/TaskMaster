# P0-T8 — Pester provisioning (N/A, read-only listing)

Timestamp: 2026-09-30T09-18
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Get-Module Pester -ListAvailable | Select-Object Name,Version | Format-Table -AutoSize | Out-String'
EXIT_CODE: 0
Output Summary:
```
Name   Version
----   -------
Pester 5.6.1
Pester 3.4.0
```
N/A: no raw Pester invocation in this plan; the MCP test route supplies its own Pester

GATE-SUBSTITUTION: Pester provisioning replaced by the PoshQC MCP test route

No Install-Module was run. The listing carries no expectation and is recorded as measured.
