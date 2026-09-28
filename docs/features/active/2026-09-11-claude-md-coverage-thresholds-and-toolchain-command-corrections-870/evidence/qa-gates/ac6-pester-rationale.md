# QA Gate — AC6: Pester branch-coverage rationale stated

Timestamp: 2026-09-14T08-17

Execution note: the command was run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running it from the worktree root.

Command: `git grep -n -F -- "Pester does not measure branch coverage" CLAUDE.md`

EXIT_CODE: 0

Output Summary: one matching line. The absence of a PowerShell branch-coverage floor is stated together with its reason, on the same line as the PowerShell line-coverage figure.

```
CLAUDE.md:304:  - PowerShell line coverage must remain `>= 80%`; Pester does not measure branch coverage, so no PowerShell branch coverage floor is stated.
```

Result: PASS.
