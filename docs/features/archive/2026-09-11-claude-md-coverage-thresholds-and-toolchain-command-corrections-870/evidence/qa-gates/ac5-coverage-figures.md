# QA Gate — AC5 (part 1 of 2): the four settled coverage figures

Timestamp: 2026-09-14T08-17

Execution note: all four commands were run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running each command from the worktree root.

## Search 1

Command: `git grep -n -F -- "C# line coverage must remain" CLAUDE.md`

EXIT_CODE: 0

Output Summary: one matching line.

```
CLAUDE.md:303:  - C# line coverage must remain `>= 80%`, and C# branch coverage must remain `>= 75%`.
```

## Search 2

Command: `git grep -n -F -- "C# branch coverage must remain" CLAUDE.md`

EXIT_CODE: 0

Output Summary: one matching line, the same line 303, which states both C# figures in one sentence.

```
CLAUDE.md:303:  - C# line coverage must remain `>= 80%`, and C# branch coverage must remain `>= 75%`.
```

## Search 3

Command: `git grep -n -F -- "PowerShell line coverage must remain" CLAUDE.md`

EXIT_CODE: 0

Output Summary: one matching line.

```
CLAUDE.md:304:  - PowerShell line coverage must remain `>= 80%`; Pester does not measure branch coverage, so no PowerShell branch coverage floor is stated.
```

## Search 4

Command: `git grep -n -F -- "90%" CLAUDE.md`

EXIT_CODE: 0

Output Summary: one matching line, the pre-existing new-code target, which this change left unedited.

```
CLAUDE.md:312:  - Any new modules, classes, or methods added must target `>= 90%` coverage.
```

Result: PASS. All four searches report at least one matching line each.
