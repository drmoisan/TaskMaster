# Phase 0 — Pre-change Occurrence Counts: dotnet-coverage and editorconfig

Timestamp: 2026-09-14T08-12

ExpectedExitCode: 1

Expectation note: the `ExpectedExitCode` field above applies to Search 1, the "dotnet-coverage" search, which is expected to exit 1 because `git grep` exits 1 when no line matches and no line is expected to match before the fix. Search 2 is expected to exit 0 and did.

Execution note: both commands were run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running each command from the worktree root.

## Search 1

Command: `git grep -n -F -- "dotnet-coverage" CLAUDE.md`

EXIT_CODE: 1

Output Summary: zero matching lines; the command produced no output. This is the pre-change state of the positive-control literal that the Phase 1 fix will introduce.

## Search 2

Command: `git grep -n -F -- ".editorconfig" CLAUDE.md`

EXIT_CODE: 0

Output Summary: exactly two matching lines, recorded verbatim as observed.

```
CLAUDE.md:197:   - C# code must pass Roslyn/.NET analyzer diagnostics configured by `.editorconfig`, `.globalconfig`, and project properties.
CLAUDE.md:273:- Prefer built-in .NET SDK analyzers and configuration through `.editorconfig` / `.globalconfig`.
```

Matching line numbers observed: 197, 273.
