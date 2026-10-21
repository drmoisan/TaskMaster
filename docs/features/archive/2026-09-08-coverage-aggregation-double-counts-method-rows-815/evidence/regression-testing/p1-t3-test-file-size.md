# P1-T3 — New Test File Physical Line Count

Timestamp: 2026-09-09T10-57
Task: [P1-T3]
Command: `pwsh -NoProfile -Command '(Get-Content -LiteralPath "tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1").Count'`
EXIT_CODE: 0

```
269
```

## Appended at 2026-09-09T11-03 — post-restructure re-measurement

Command: `pwsh -NoProfile -Command '(Get-Content -LiteralPath "tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1").Count'`
EXIT_CODE: 0

```
271
```

The file grew by 2 lines during P3-T1, when the private differential helper was moved from file
scope into the `BeforeAll` block. The reason is recorded in
`evidence/regression-testing/p3-t1-pass-after.md`. The count is still at or below the 500-line
ceiling with 229 lines of headroom, so the acceptance condition of this task continues to hold. The
Phase 1 measurement below is retained unaltered as the record of what was measured at that point.

Output Summary: `tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1` is 269
physical lines, which is at or below the 500-line ceiling in
`.claude/rules/general-code-change.md` with 231 lines of headroom. No split is required and the
ceiling was not raised. This count is recorded before Phase 2 so the file-size gate at P4-T5 has a
Phase 1 reference for the new test file.
