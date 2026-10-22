# Remediation Cycle 1 Sanitize Final (Issue 930)

Timestamp: 2026-09-29T10-23

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath WORKTREE-ROOT; $acct = [regex]::Escape((Split-Path -Leaf $env:USERPROFILE)); $machine = [regex]::Escape($env:COMPUTERNAME); $root = [regex]::Escape((Resolve-Path .).Path); $bs = [regex]::Escape([string][char]92); $drive = "[A-Za-z]:[" + $bs + "/]Users[" + $bs + "/]"; $files = @(Get-ChildItem -Recurse -File -LiteralPath docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930); "FEATURE_FILES=$($files.Count)"; "ACCOUNT_HITS=$(@($files | Select-String -Pattern "(?i)$acct").Count)"; "HOST_HITS=$(@($files | Select-String -Pattern "(?i)$machine").Count)"; "ROOT_HITS=$(@($files | Select-String -Pattern "(?i)$root").Count)"; "DRIVE_USERS_HITS=$(@($files | Select-String -Pattern $drive).Count)"; "RAW_TOOL_DOCS=$(@($files | Where-Object { $_.Name -match "[.](trx|coverage|coveragexml)$" -or $_.Name -match "[.]cobertura[.]xml$" -or $_.Name -match "[.]log$" }).Count)"; "CONTROL_ACCOUNT_HITS=$(@(Get-Content -LiteralPath coverage/930-final-coverage.log | Select-String -Pattern "(?i)$acct").Count)"; "CONTROL_ROOT_HITS=$(@(Get-Content -LiteralPath coverage/930-final-coverage.log | Select-String -Pattern "(?i)$root").Count)"; "CONTROL_HOST_HITS=$(@(Get-Content -LiteralPath coverage/test-results/930-final/930-final.trx | Select-String -Pattern "(?i)$machine").Count)"; "CONTROL_DRIVE_HITS=$(@(Get-Content -LiteralPath coverage/930-final-coverage.log | Select-String -Pattern $drive).Count)"'

EXIT_CODE: 0

Output Summary (counts only; no matched value is recorded):

- FEATURE_FILES=64
- ACCOUNT_HITS=0
- HOST_HITS=0
- ROOT_HITS=0
- DRIVE_USERS_HITS=0
- RAW_TOOL_DOCS=0
- CONTROL_ACCOUNT_HITS=15
- CONTROL_ROOT_HITS=15
- CONTROL_HOST_HITS=7325
- CONTROL_DRIVE_HITS=15

The five identity counts are zero and the four controls are each at least 1.
