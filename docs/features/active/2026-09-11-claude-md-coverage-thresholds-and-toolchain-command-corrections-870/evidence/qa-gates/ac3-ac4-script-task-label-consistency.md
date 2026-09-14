# QA Gate — AC3 and AC4: script path, VS Code task label, withheld-collector note

Timestamp: 2026-09-14T08-17

Execution note: all three commands were run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running each command from the worktree root.

## Search 1

Command: `git grep -n -F -- "Invoke-MSTestWithCoverage.ps1" CLAUDE.md`

EXIT_CODE: 0

Output Summary: three matching lines, 392, 410 and 426. Lines 392 and 410 are the two corrected step 4 toolchain entries; line 426 is the Committed Test Evidence Format paragraph re-attributed by P1-T5.

## Search 2

Command: `git grep -n -F -- "deliberately withheld" CLAUDE.md`

EXIT_CODE: 0

Output Summary: exactly two matching lines, 392 and 410. This is the count the gate requires: one occurrence per corrected step 4 entry, confirming the two restatements carry the same withheld-collector note and no third site states it. The two line numbers are the pre-change 390 and 408 shifted down by two, which is the line-count delta introduced by replacing one UT2 coverage line with three.

```
CLAUDE.md:392:4. Run the `test: MSTest with Coverage (Koverage)` VS Code task (or invoke `Invoke-MSTestWithCoverage.ps1` under `scripts/vscode` directly), which wraps an inner `vstest.console.exe` invocation inside an outer `dotnet-coverage` collect process; the built-in Code Coverage data collector is deliberately withheld from the inner vstest invocation because it conflicts with the outer `dotnet-coverage` instrumentation.
CLAUDE.md:410:4. **Test**: Run the `test: MSTest with Coverage (Koverage)` VS Code task (or invoke `Invoke-MSTestWithCoverage.ps1` under `scripts/vscode` directly), which wraps an inner `vstest.console.exe` invocation inside an outer `dotnet-coverage` collect process; the built-in Code Coverage data collector is deliberately withheld from the inner vstest invocation because it conflicts with the outer `dotnet-coverage` instrumentation.
```

## Search 3

Command: `git grep -n -F -- "test: MSTest with Coverage" CLAUDE.md`

EXIT_CODE: 0

Output Summary: two matching lines, 392 and 410. The VS Code task label is named at both step 4 sites. The label as written in the file is the full `test: MSTest with Coverage (Koverage)`, matching the label declared for that task in the workspace task configuration.

Result: PASS. Each of the three searches reports at least one matching line, and the "deliberately withheld" search reports exactly two.
