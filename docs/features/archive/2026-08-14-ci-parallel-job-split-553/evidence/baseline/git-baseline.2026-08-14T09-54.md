# Git Baseline — Issue #553

- Timestamp: 2026-08-14T09-54 (local) / 2026-08-14T13:54:53Z (UTC)
- Task: [P0-T2]

Command:

```
git rev-parse HEAD; git rev-parse --abbrev-ref HEAD; git status --porcelain
```

EXIT_CODE: 0

## Output Summary

- BASELINE_SHA: `e246688b87a436567b6951729a74d106328ae04c`
- BRANCH: `feature/ci-parallel-job-split-553` (matches the expected value recorded
  in the plan's Conventions section; this value is what [P3-T2] and [P3-T4]
  reference, never a hard-coded literal)
- Merge base with `origin/main`: `2073f717bbfac30053f3d6a4e652d99af3ae5c9c`
  (branch is 1 commit ahead: `e246688b docs(553): promote CI parallel job split,
  capture baseline, research, spec, and user story`)
- Remote branch state: `feature/ci-parallel-job-split-553` does not yet exist on
  `origin`, so `git push -u origin <BRANCH>` in [P3-T2] is the correct form.

Verbatim `git status --porcelain` output at baseline:

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M docs/features/active/2026-08-14-ci-parallel-job-split-553/plan.2026-08-14T09-05.md
?? .claude/agent-memory/atomic-executor/project_pwsh_git_gh_cli_gotchas.md
?? .claude/agent-memory/atomic-planner/project_553_ci_parallel_split_plan_seams.md
?? docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/other/
?? docs/features/potential/promoted/2026-08-14-orchestrator-hooks-reference-absent-python-validators.md
?? docs/features/potential/promoted/2026-08-14-potential-to-issue-promoted-copy-not-written.md
```

Classification of the pre-existing dirty entries (expected and permitted by the
task text; noted, not gated on):

| Path | Class |
| --- | --- |
| `.claude/agent-memory/atomic-executor/MEMORY.md` + `project_pwsh_git_gh_cli_gotchas.md` | agent memory written during preflight validation |
| `.claude/agent-memory/atomic-planner/MEMORY.md` + `project_553_ci_parallel_split_plan_seams.md` | agent memory written during planning |
| `docs/features/.../plan.2026-08-14T09-05.md` | the plan of record, revised in place across three preflight passes |
| `docs/features/.../evidence/other/` | the [P0-T1] artifact written moments before this capture |
| `docs/features/potential/promoted/*.md` | two unrelated promoted-potential documents, pre-existing on this branch |

**No entry under `.github/` and no `*.cs`, `*.csproj`, `*.props`, `*.targets`, or
`packages.config` entry is present.** This confirms the [P0-T1] acceptance clause
that no code file had been modified at the start of execution.

## Acceptance ([P0-T2])

- Artifact exists; `BASELINE_SHA` and `BRANCH` are recorded for use by later tasks.
- This is a record of the starting state, not an invariant that `HEAD` remains at
  this SHA. Later diff-scoped verifications reference `BASELINE_SHA`; [P5-T3] uses
  `git merge-base origin/main HEAD` rather than this literal so it remains correct
  after the Phase 3–4 commits.
