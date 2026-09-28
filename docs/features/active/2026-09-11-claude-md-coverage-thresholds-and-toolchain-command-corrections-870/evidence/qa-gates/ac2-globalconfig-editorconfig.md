# QA Gate — AC2: nonexistent analyzer-configuration file name removed

Timestamp: 2026-09-14T08-17

ExpectedExitCode: 1

Expectation note: the `ExpectedExitCode` field above applies to Search 1, the ".globalconfig" search, for which zero matches is the required post-change result and `git grep` therefore exits 1. Search 2 is the positive control and is expected to exit 0.

Execution note: both commands were run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running each command from the worktree root.

## Search 1

Command: `git grep -n -F -- ".globalconfig" CLAUDE.md`

EXIT_CODE: 1

Output Summary: zero matching lines; the command produced no output. The nonexistent file name no longer occurs anywhere in CLAUDE.md.

## Search 2 (positive control)

Command: `git grep -n -F -- ".editorconfig" CLAUDE.md`

EXIT_CODE: 0

Output Summary: two matching lines, 197 and 273, which is at least the two the gate requires. Both are the sites the correction touched, and each now names the real, existing analyzer-configuration file as the sole source.

```
CLAUDE.md:197:   - C# code must pass Roslyn/.NET analyzer diagnostics configured by `.editorconfig` and project properties.
CLAUDE.md:273:- Prefer built-in .NET SDK analyzers and configuration through `.editorconfig`.
```

Result: PASS.
