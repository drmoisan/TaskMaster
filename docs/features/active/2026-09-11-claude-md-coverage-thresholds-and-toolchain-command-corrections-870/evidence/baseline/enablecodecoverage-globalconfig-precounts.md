# Phase 0 — Pre-change Occurrence Counts: coverage switch and globalconfig

Timestamp: 2026-09-14T08-12

Execution note: both commands were run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running each command from the worktree root.

## Search 1

Command: `git grep -n -E -- "[/]EnableCodeCoverage" CLAUDE.md`

EXIT_CODE: 0

Output Summary: exactly two matching lines, recorded verbatim as observed.

```
CLAUDE.md:390:4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /ResultsDirectory:coverage\test-results /Logger:trx;LogFileName=mstest-run.trx`
CLAUDE.md:408:4. **Test**: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /ResultsDirectory:coverage\test-results /Logger:trx;LogFileName=mstest-run.trx`
```

Matching line numbers observed: 390, 408.

## Search 2

Command: `git grep -n -F -- ".globalconfig" CLAUDE.md`

EXIT_CODE: 0

Output Summary: exactly two matching lines, recorded verbatim as observed.

```
CLAUDE.md:197:   - C# code must pass Roslyn/.NET analyzer diagnostics configured by `.editorconfig`, `.globalconfig`, and project properties.
CLAUDE.md:273:- Prefer built-in .NET SDK analyzers and configuration through `.editorconfig` / `.globalconfig`.
```

Matching line numbers observed: 197, 273.
