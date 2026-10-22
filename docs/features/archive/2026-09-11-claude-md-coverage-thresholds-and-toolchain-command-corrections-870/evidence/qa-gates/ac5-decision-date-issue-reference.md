# QA Gate — AC5 (part 2 of 2): decision date and issue reference

Timestamp: 2026-09-14T08-17

Execution note: both commands were run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running each command from the worktree root.

## Search 1

Command: `git grep -n -F -- "2026-09-11" CLAUDE.md`

EXIT_CODE: 0

Output Summary: one matching line, carrying the date on which the project maintainer settled the coverage figures.

```
CLAUDE.md:305:  - These coverage figures were settled by the project maintainer on 2026-09-11 (issue #563).
```

## Search 2

Command: `git grep -n -F -- "#563" CLAUDE.md`

EXIT_CODE: 0

Output Summary: one matching line, the same line 305, which carries the issue reference alongside the decision date.

```
CLAUDE.md:305:  - These coverage figures were settled by the project maintainer on 2026-09-11 (issue #563).
```

Result: PASS. Both searches report at least one matching line each.
