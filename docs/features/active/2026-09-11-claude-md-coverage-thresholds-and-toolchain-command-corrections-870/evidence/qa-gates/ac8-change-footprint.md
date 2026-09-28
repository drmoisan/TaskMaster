# QA Gate — AC8: change footprint

Timestamp: 2026-09-14T08-17

Execution note: both commands were run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running each command from the worktree root. BASE_COMMIT is the 40-character hash recorded in `docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/evidence/baseline/merge-base-commit.md`.

## Command 1

Command: `git diff --name-only e4a337505af5ce0c53641d3a89343f20c1e2c6c1..HEAD`

EXIT_CODE: 0

Output Summary: nine changed paths.

```
CLAUDE.md
docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/evidence/other/preflight-round-1-delta.md
docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/evidence/other/spec-amendment-ac8.2026-09-12T11-10.md
docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/issue.md
docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/plan.2026-09-12T10-25.md
docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/research/2026-09-12T10-45-claude-md-coverage-toolchain-corrections-research.md
docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/spec.md
docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/user-story.md
docs/features/potential/promoted/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md
```

## Command 2

Command: `git status --porcelain`

EXIT_CODE: 0

Output Summary: three entries, all inside this item's feature folder. One is the plan file carrying this run's task check-offs; the other two are the untracked evidence directories this phase and Phase 0 wrote.

```
 M docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/plan.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/evidence/baseline/
?? docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/evidence/qa-gates/
```

## Exclusion set (three entries)

1. Every path falling under `docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/`.
2. `docs/features/potential/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md` — the promotion lifecycle record committed by the preparation step that precedes execution. It does not appear in either command's output, because the promotion was recorded as a rename and only the destination path is listed; the entry is retained in the exclusion set as declared.
3. `docs/features/potential/promoted/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md` — the destination of that same promotion rename, likewise committed by the preparation step and not by this phase.

## Remaining paths after the exclusions

Remaining path count: 1

Remaining path list:

```
CLAUDE.md
```

Result: PASS. Exactly one path remains after the three exclusions, and that path is CLAUDE.md.
