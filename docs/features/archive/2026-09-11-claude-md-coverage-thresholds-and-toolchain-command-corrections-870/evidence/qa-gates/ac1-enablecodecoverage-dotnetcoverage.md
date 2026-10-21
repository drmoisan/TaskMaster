# QA Gate — AC1: coverage collector switch removed, coverage route named

Timestamp: 2026-09-14T08-17

ExpectedExitCode: 1

Expectation note: the `ExpectedExitCode` field above applies to Search 1, the "/EnableCodeCoverage" search, for which zero matches is the required post-change result and `git grep` therefore exits 1. Search 2 is the positive control and is expected to exit 0.

Execution note: both commands were run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running each command from the worktree root.

## Search 1

Command: `git grep -n -E -- "[/]EnableCodeCoverage" CLAUDE.md`

EXIT_CODE: 1

Output Summary: zero matching lines; the command produced no output. The literal no longer occurs anywhere in CLAUDE.md, which is what AC1 requires.

## Search 2 (positive control)

Command: `git grep -n -F -- "dotnet-coverage" CLAUDE.md`

EXIT_CODE: 0

Output Summary: two matching lines, 392 and 410, which is at least the two the gate requires. The search mechanism is therefore working and the zero-match result of Search 1 is a genuine absence rather than a broken search.

```
CLAUDE.md:392:4. Run the `test: MSTest with Coverage (Koverage)` VS Code task (or invoke `Invoke-MSTestWithCoverage.ps1` under `scripts/vscode` directly), which wraps an inner `vstest.console.exe` invocation inside an outer `dotnet-coverage` collect process; the built-in Code Coverage data collector is deliberately withheld from the inner vstest invocation because it conflicts with the outer `dotnet-coverage` instrumentation.
CLAUDE.md:410:4. **Test**: Run the `test: MSTest with Coverage (Koverage)` VS Code task (or invoke `Invoke-MSTestWithCoverage.ps1` under `scripts/vscode` directly), which wraps an inner `vstest.console.exe` invocation inside an outer `dotnet-coverage` collect process; the built-in Code Coverage data collector is deliberately withheld from the inner vstest invocation because it conflicts with the outer `dotnet-coverage` instrumentation.
```

Result: PASS.
