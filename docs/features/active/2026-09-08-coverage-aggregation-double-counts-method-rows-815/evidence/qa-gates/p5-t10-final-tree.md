# P5-T10 — Tree State After The Phase 4 And Phase 5 Evidence Commit

Timestamp: 2026-09-09T11-40
Task: [P5-T10]
EXIT_CODE: 0

## Command 1 — staging

Command: `git add -A -- docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815`
EXIT_CODE: 0

## Command 2 — commit

Command: `git commit -F <message file>`
EXIT_CODE: 0

```
[bug/coverage-aggregation-double-counts-method-rows-815-exec 143fd5ae] docs(815): record the Phase 4 scope gates and the Phase 5 QA loop evidence
 18 files changed, 2297 insertions(+), 17 deletions(-)
```

## Command 3 — the recorded listing

Command: `git status --porcelain --untracked-files=all`
EXIT_CODE: 0

```
(no output)
```

The listing was captured **after** the commit and **before** this artifact was written. That ordering
is required: this artifact and this task's own plan check-off both land inside this feature's folder,
so capturing the listing afterwards would show them and contradict the assertion the task makes.

## Acceptance

| Assertion | Result |
| --- | --- |
| No path outside the five permitted prefixes of plan decision D6 | **holds**; the listing is empty |
| No path under `scripts/vscode/` | **holds** |
| No path under `tests/scripts/vscode/` | **holds** |
| No path under this feature's folder | **holds** |

Paths under `.claude/agent-memory/` would have been permitted here by the D6 carve-out rule, because
that directory is tracked and agents write to it during a run. None appeared.

Output Summary: `git status --porcelain --untracked-files=all` printed nothing after the Phase 4 and
Phase 5 evidence commit `143fd5ae`. The worktree is clean: nothing remains uncommitted under
`scripts/vscode/`, `tests/scripts/vscode/` or this feature's folder, and no path outside the five
permitted prefixes appears anywhere in the tree.
