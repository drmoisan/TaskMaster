# P0-T2 — Branch Baseline

Timestamp: 2026-09-09T10-40
Task: [P0-T2]
EXIT_CODE: 0

## Command 1

Command: `git rev-parse --abbrev-ref HEAD`
EXIT_CODE: 0

```
bug/coverage-aggregation-double-counts-method-rows-815-exec
```

## Command 2

Command: `git merge-base HEAD epic/review-residuals-2026-09-08-integration`
EXIT_CODE: 0

```
48cd004b57aea56a5ce118a2c0690889186d3cf1
```

The printed object name is 40 characters.

## Command 3

Command: `git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 0

```
(no output)
```

Output Summary: The worktree is on branch `bug/coverage-aggregation-double-counts-method-rows-815-exec`.
The merge base against `epic/review-residuals-2026-09-08-integration` resolves to the
40-character object name `48cd004b57aea56a5ce118a2c0690889186d3cf1`, which matches the base recorded
for this execution. The scoped porcelain command over `scripts/vscode` and `tests/scripts/vscode`
printed nothing, so both folders are clean before any change is made. All three acceptance
conditions hold.
