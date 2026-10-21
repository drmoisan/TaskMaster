# Terminal residual commit (issue #826, [P8-T21])

Timestamp: 2026-09-09T19-50

Every field in this artifact is an observation made **before** the artifact was written. This task is
ordered write-then-commit, not commit-then-write, and that ordering is what makes its acceptance
reachable: the artifact and the check-off are both authored first, so the terminal `git add` sweeps them
along with the residual and no path this plan authors remains uncommitted.

Command: `git status --porcelain --untracked-files=all -- . ":(exclude).claude"`

Run on its own as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard.

EXIT_CODE: 0

TerminalCommitCommand: the three lines of the block this task runs after the artifact and the check-off
are written, in this order:

```
git add -- . ":(exclude).claude"
git commit -m "docs(826): record the committed write-set gate and close the plan checklist"
git status --porcelain --untracked-files=all -- . ":(exclude).claude"
```

`git add -A` is not used, for the reason given in [P8-T19]: it would sweep a queued sibling promotion's
untracked file onto this branch. The [P8-T19] commit is not amended.

Commit subject this task will use:

```
docs(826): record the committed write-set gate and close the plan checklist
```

## Porcelain span the observed invocation printed

```
 M docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/plan.2026-09-08T23-52.md
?? docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/evidence/qa-gates/p8-t19-final-git-state.md
?? docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/evidence/qa-gates/p8-t20-committed-write-set.md
```

Output Summary: three paths reported, all inside the [P8-T1] allow-list. They are the terminal residual
[P8-T19] and [P8-T20] left behind:

- `plan.2026-09-08T23-52.md` — carries the [P8-T19] and [P8-T20] check-offs, and will additionally carry
  this task's own check-off by the time the terminal `git add` runs.
- `evidence/qa-gates/p8-t19-final-git-state.md` — [P8-T19]'s artifact, written after that task's own
  post-commit observation.
- `evidence/qa-gates/p8-t20-committed-write-set.md` — [P8-T20]'s artifact, written after that task's span
  was observed.

This artifact itself is not in the list above, because the list was observed before it was written. It
and this task's check-off are swept by the same terminal `git add`.

The terminal commit's own exit code and its post-commit porcelain span are deliberately **not** recorded
here. Writing them back would create a fresh uncommitted file and the task would not terminate. They are
reported to the orchestrator in the completion message instead, which is the scoping clause the plan's
fail-closed evidence rule states for exactly this task.
