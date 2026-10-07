# P1-T1 test literals emptied (issue #973; section 9 T1)

Timestamp: 2026-10-03T10-58
Command: Edit tool on tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 (two replacements: the 17-line $expectedDebt literal at lines 293-309 to `        $expectedDebt = @()`; line 310 to `        $expectedUnverifiable = @('netstandard')`); then Grep tool `^\s*\$expectedDebt = @\(\)` count, `'Azure.Core\|1.62.0.0'` count, `^\s*\$expectedUnverifiable = @\('netstandard'\)` count, `Clients.ActiveDirectory` count, CMD-LINECOUNT, CMD-CRCOUNT
EXIT_CODE: 0
Output Summary: both literals replaced; every gate count as stated; file 319 lines with 319 carriage returns (CRLF preserved).

`^\s*\$expectedDebt = @\(\)`: 1
`'Azure.Core\|1.62.0.0'`: 0
`^\s*\$expectedUnverifiable = @\('netstandard'\)`: 1
`Clients.ActiveDirectory`: 0
LINECOUNT: 319
CRCOUNT: 319
