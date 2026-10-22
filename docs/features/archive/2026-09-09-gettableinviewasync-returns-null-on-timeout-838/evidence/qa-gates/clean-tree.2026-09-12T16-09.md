# P4-T39 — Clean-worktree assertion

Timestamp: 2026-09-13T03-28

Command: `git -C . status --porcelain --untracked-files=all -- . ":(exclude).claude/agent-memory" ":(exclude)docs/features/potential"`

EXIT_CODE: 0

Output Summary: the captured listing, verbatim:

```
 M docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md
```

The listing contains no path other than this plan file, which is the acceptance clause for this step. The plan file appears because the executor has been checking off completed tasks as it goes and P4-T38's check-off is the most recent edit to it.

The agent-memory path under `.claude` is excluded because it is tracked and the executor may have written to it during the run. `docs/features/potential` is excluded because a sibling item's queued promotion file must never be swept onto this branch. The all-untracked-files option is used because the default collapses an untracked directory to a single entry, which would hide a stray file inside it.

This artifact is written before the commit this task performs, so its `EXIT_CODE:` is scoped to the status command observed above. The commit's own exit code and the resulting head identifier are reported to the orchestrator in the executor's completion message rather than recorded here, which is the one scoping clause the plan's fail-closed evidence rule states.
