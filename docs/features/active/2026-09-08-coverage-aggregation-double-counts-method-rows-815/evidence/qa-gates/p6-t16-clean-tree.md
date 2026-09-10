# P6-T16 — Tree State After The Phase 6 Commit

Timestamp: 2026-09-09T11-48
Task: [P6-T16]
EXIT_CODE: 0

## Command 1 — staging

Command: `git add -A -- docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815`
EXIT_CODE: 0

## Command 2 — commit

Command: `git commit -F <message file>`
EXIT_CODE: 0

```
[bug/coverage-aggregation-double-counts-method-rows-815-exec 983bed16] docs(815): check off AC1 through AC13 and record AC14 as PARTIAL
 5 files changed, 174 insertions(+), 28 deletions(-)
```

## Command 3 — the recorded listing

Command: `git status --porcelain --untracked-files=all`
EXIT_CODE: 0

```
(no output)
```

The listing was captured **after** the commit and **before** this artifact was written, because this
artifact and this task's own plan check-off both land inside this feature's folder and would
otherwise appear in the listing the task asserts is empty of that folder.

## Acceptance

| Assertion | Result |
| --- | --- |
| No path under `scripts/vscode/` | **holds**; the listing is empty |
| No path under `tests/scripts/vscode/` | **holds** |
| No path under this feature's folder | **holds** |

Paths under `.claude/agent-memory/` would have been permitted here by the plan decision D6 carve-out
rule. None appeared.

## Commits produced by this execution

| Order | SHA | Subject |
| --- | --- | --- |
| 1 | `0649538d` | `fix(coverage): expose a deduplicated first-party Cobertura aggregation (#815)` |
| 2 | `e3a3a2f8` | `fix(coverage): make the mocked post-processed Cobertura stub structurally valid (#815)` |
| 3 | `143fd5ae` | `docs(815): record the Phase 4 scope gates and the Phase 5 QA loop evidence` |
| 4 | `983bed16` | `docs(815): check off AC1 through AC13 and record AC14 as PARTIAL` |

The plan schedules commits at P4-T1, P5-T10 and P6-T16, which are commits 1, 3 and 4. Commit 2 is the
loop-restart commit the Phase 5 preamble and P5-T2 authorize when a stage changes a file; it carries
the repair recorded in `evidence/qa-gates/p5-t6-test.md` and was made before the loop restarted so
that P5-T2's tree observation stayed satisfiable.

Nothing was pushed, no pull request was opened, and no merge was performed.

Output Summary: `git status --porcelain --untracked-files=all` printed nothing after the Phase 6
commit `983bed16`. The worktree is clean: nothing remains uncommitted under `scripts/vscode/`,
`tests/scripts/vscode/` or this feature's folder. Four commits were produced on
`bug/coverage-aggregation-double-counts-method-rows-815-exec`.
