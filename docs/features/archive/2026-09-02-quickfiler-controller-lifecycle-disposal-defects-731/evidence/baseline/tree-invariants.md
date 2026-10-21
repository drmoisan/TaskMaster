# Phase 0 — Tree invariants baseline

Timestamp: 2026-09-03T13-21

Task: [P0-T2]
Issue: #731

Command:
1. `git rev-parse --abbrev-ref HEAD`
2. `git rev-parse HEAD`
3. `git rev-parse origin/main`
4. `git merge-base HEAD origin/main`
5. `git status --porcelain`
6. `git status --porcelain --untracked-files=all`
7. `git diff --name-status 35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e`

EXIT_CODE:
- `git rev-parse --abbrev-ref HEAD` = 0
- `git rev-parse HEAD` = 0
- `git rev-parse origin/main` = 0
- `git merge-base HEAD origin/main` = 0
- `git status --porcelain` = 0
- `git status --porcelain --untracked-files=all` = 0
- `git diff --name-status 35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e` = 0

Diff base: 35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e

## Output Summary

### 1. `git rev-parse --abbrev-ref HEAD`

```
bug/quickfiler-controller-lifecycle-disposal-defects-731
```

### 2. `git rev-parse HEAD`

```
9436c81b391852398662c32f141bb1635a311564
```

### 3. `git rev-parse origin/main`

```
35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e
```

### 4. `git merge-base HEAD origin/main`

```
35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e
```

### 5. `git status --porcelain` (collapsed form, full output)

```
 M docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/plan.2026-09-02T12-02.md
?? docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/evidence/
```

### 6. `git status --porcelain --untracked-files=all` (full output)

```
 M docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/plan.2026-09-02T12-02.md
?? docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/evidence/baseline/phase0-instructions-read.md
```

### 7. `git diff --name-status 35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e` (full output)

```
M	.claude/agent-memory/atomic-executor/MEMORY.md
M	.claude/agent-memory/atomic-executor/project_agent_memory_tracked_breaks_unscoped_git_gates.md
M	.claude/agent-memory/atomic-executor/project_dotnet_coverage_denominator_nondeterminism.md
A	.claude/agent-memory/atomic-executor/project_scope_gate_cannot_list_artifacts_written_after_it.md
M	.claude/agent-memory/atomic-planner/MEMORY.md
M	.claude/agent-memory/atomic-planner/agent-memory-is-tracked-scope-git-gates.md
A	.claude/agent-memory/atomic-planner/porcelain-collapses-untracked-directories.md
A	.claude/agent-memory/atomic-planner/project_731_lifecycle_disposal_plan_seams.md
A	.claude/agent-memory/atomic-planner/repo-wide-cobertura-line-rate-is-nondeterministic.md
A	.claude/agent-memory/atomic-planner/self-referential-evidence-enumeration.md
M	.claude/agent-memory/task-researcher/MEMORY.md
A	.claude/agent-memory/task-researcher/project_qfc_lifecycle_disposal_731.md
A	docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/issue.md
A	docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/plan.2026-09-02T12-02.md
A	docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/research/2026-09-02T13-10-controller-lifecycle-disposal-fix-design-research.md
A	docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/spec.md
```

### Summary

Branch is `bug/quickfiler-controller-lifecycle-disposal-defects-731`, as expected. The merge base with `origin/main` equals `origin/main` itself at capture time, because `HEAD` is a merge commit whose second parent is that commit. The working tree is not clean: one tracked-modified path and one untracked directory in the collapsed porcelain form. The anchored name-status diff lists 16 tracked paths already changed on this branch before any Phase 1 edit: 12 under `.claude/agent-memory/` and 4 in this feature folder.

## Pre-existing out-of-scope paths

Every path listed by either porcelain form that is not in the PLAN WRITE SET.

| Path | Tag |
|---|---|
| `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/plan.2026-09-02T12-02.md` | tracked-modified |
| `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/evidence/` | untracked (collapsed form directory entry) |
| `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/evidence/baseline/phase0-instructions-read.md` | untracked (`--untracked-files=all` form) |

This table is the DISCLOSED BASELINE SET consumed by [P5-T1].

## Pre-existing tracked diff paths

Every path listed by the anchored `git diff --name-status 35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e`, with its status letter. This is the PRE-EXISTING TRACKED DIFF SET subtracted by [P5-T9].

| Status | Path |
|---|---|
| M | `.claude/agent-memory/atomic-executor/MEMORY.md` |
| M | `.claude/agent-memory/atomic-executor/project_agent_memory_tracked_breaks_unscoped_git_gates.md` |
| M | `.claude/agent-memory/atomic-executor/project_dotnet_coverage_denominator_nondeterminism.md` |
| A | `.claude/agent-memory/atomic-executor/project_scope_gate_cannot_list_artifacts_written_after_it.md` |
| M | `.claude/agent-memory/atomic-planner/MEMORY.md` |
| M | `.claude/agent-memory/atomic-planner/agent-memory-is-tracked-scope-git-gates.md` |
| A | `.claude/agent-memory/atomic-planner/porcelain-collapses-untracked-directories.md` |
| A | `.claude/agent-memory/atomic-planner/project_731_lifecycle_disposal_plan_seams.md` |
| A | `.claude/agent-memory/atomic-planner/repo-wide-cobertura-line-rate-is-nondeterministic.md` |
| A | `.claude/agent-memory/atomic-planner/self-referential-evidence-enumeration.md` |
| M | `.claude/agent-memory/task-researcher/MEMORY.md` |
| A | `.claude/agent-memory/task-researcher/project_qfc_lifecycle_disposal_731.md` |
| A | `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/issue.md` |
| A | `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/plan.2026-09-02T12-02.md` |
| A | `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/research/2026-09-02T13-10-controller-lifecycle-disposal-fix-design-research.md` |
| A | `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/spec.md` |

Count: 16 paths — 12 under `.claude/agent-memory/` and 4 tracked feature-folder paths.

## Baseline line counts

Task: [P0-T3]
Timestamp: 2026-09-03T13-22
Command: `(Get-Content -LiteralPath '<path>').Count` for each of the seven paths below.
EXIT_CODE: 0

```
QuickFiler/Controllers/QfcCollectionController.cs = 2327
QuickFiler/Controllers/QfcQueue.cs = 505
QuickFiler/Controllers/QfcDatamodel.cs = 480
QuickFiler/Controllers/QfcFormController.SetupDisposal.cs = 234
QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs = 496
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs = 498
QuickFiler.Test/Controllers/QfcDatamodelTests.cs = 401
```

Output Summary: Seven `path = integer` rows recorded. These seven integers are the authoritative baseline for the Phase 5 size gates.

## Issue #683 baseline

Task: [P0-T4]
Timestamp: 2026-09-03T13-22
Command: read line 33 of `docs/features/potential/promoted/2026-08-28-qfcformcontroller-setupdisposal-coverage-debt.md`
EXIT_CODE: 0

Line 33, quoted verbatim:

```
The file's whole-file line coverage is 70.70%, with 46 lines uncovered. All 46 uncovered lines were verified pre-existing at the issue #677 merge-base baseline (not introduced or touched by that fix); the two lines #677 added to this file are both 100% covered.
```

Output Summary: The issue-#683 baseline for `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` is 70.70 percent whole-file line coverage with 46 uncovered lines. [P5-T8] compares its post-change re-measurement against these two numerals.
