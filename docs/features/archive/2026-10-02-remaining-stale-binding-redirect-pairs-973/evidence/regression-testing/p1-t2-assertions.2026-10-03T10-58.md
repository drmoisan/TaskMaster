# P1-T2 assertions replaced (issue #973; section 9 T2)

Timestamp: 2026-10-03T10-58
Command: Edit tool on tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 (the collection comparison and the unverifiable assertion, formerly lines 331-332 and 315-316 after T1, replaced by the three section 9 T2 lines); then Grep tool counts listed below, CMD-LINECOUNT, CMD-CRCOUNT
EXIT_CODE: 0
Output Summary: the three T2 lines are present; the old known-debt message is gone; the examined-count guard (line 314) and the Fizzler/Unsafe exclusion (line 318) are intact; 320 lines, 320 carriage returns.

`@\(\$expectedDebt\)\.Count \| Should -Be 0`: 1
`\$actualDebt\.Count \| Should -Be 0 -Because`: 1
`only the deliberate netstandard redirect may be unverifiable`: 1
`the stale redirect set must equal the recorded known debt`: 0
`Should -Be \$redirectElement`: 1 (line 314)
`Fizzler\|\*`: 1 (line 318)
LINECOUNT: 320
CRCOUNT: 320
