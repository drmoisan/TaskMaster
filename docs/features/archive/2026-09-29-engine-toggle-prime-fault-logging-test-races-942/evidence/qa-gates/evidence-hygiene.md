# Evidence hygiene sweep (issue 942)

Timestamp: 2026-09-30T07-53
Task: P3-T13
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $acct = Split-Path -Leaf $env:USERPROFILE; $machine = $env:COMPUTERNAME; $files = @(Get-ChildItem -LiteralPath "docs\features\active\2026-09-29-engine-toggle-prime-fault-logging-test-races-942" -Recurse -File -Filter "*.md"); ... "FILES_SCANNED=... ACCOUNT_HITS=... MACHINE_HITS=... DRIVE_USERS_HITS=..."' (the P3-T13 payload, run verbatim)
EXIT_CODE: 0

Output Summary:
- FILES_SCANNED=36 ACCOUNT_HITS=0 MACHINE_HITS=0 DRIVE_USERS_HITS=0
- The account and machine tokens are derived at run time; neither value is written into this artifact.
- The drive-path count normalises backslashes to forward slashes and applies the CI hygiene guard's user-profile pattern case-insensitively with separator runs; no occurrence was found, so no REDACTED-PATH repair was needed.
- Scope: every Markdown file in the feature folder at the time of the sweep, including the plan. The two artifacts written later (evidence/other/ac-status-summary.md and evidence/other/reduced-audit-handoff.md) carry no host path by construction.
