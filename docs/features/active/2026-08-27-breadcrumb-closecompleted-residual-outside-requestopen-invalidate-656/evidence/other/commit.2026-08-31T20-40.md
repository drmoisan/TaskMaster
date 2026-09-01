# Commit Record (Issue #656)

Timestamp: 2026-09-01T14-54
Task: [P4-T10]

Command:
```
git add QuickFiler QuickFiler.Test docs/features/active/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656
git commit -m "fix(breadcrumb): stop suppressing a close while the host reports open (#656)"
git rev-parse HEAD
git status --porcelain -- QuickFiler QuickFiler.Test
```

EXIT_CODE: 0

Resulting commit id: `d3dd9fe00180c449c80b2771a4befcc474f512bb`

Post-commit `git status --porcelain -- QuickFiler QuickFiler.Test` produced **no output**, which is
the acceptance condition: both production trees are clean and everything this change touched under
them is committed.

## Staged-set verification before the commit

`git diff --cached --name-only` listed 29 paths and no others:

- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` (the sole production file)
- `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` (the sole test file)
- 26 evidence artifacts under this feature folder's `evidence/baseline/`,
  `evidence/regression-testing/`, `evidence/qa-gates/` and `evidence/other/`
- this feature folder's `plan.2026-08-31T20-10.md`, carrying the task check-offs

No path under `.claude/agent-memory/` and no path under `artifacts/orchestration/` appears in the
staged set.

## Why the pathspec is scoped

The `git add` names three explicit pathspecs rather than using `-A`, `.` or `-u`. Two categories of
tracked file in this repository would otherwise be swept into this commit and corrupt the change
footprint: `.claude/agent-memory/**`, which is tracked and is written concurrently by other
processes during a run, and `artifacts/orchestration/orchestrator-state.json`, which is tracked and
carries an unrelated item's checkpoint. Neither is part of this item's change set. No
`git update-index` command was run by this task or by any other task in this plan.

## Purpose

This commit exists so that the anchored three-dot diff assertions in P4-T11 through P4-T14 are
non-vacuous. A name-listing diff against a base ref reports committed changes only, so running those
assertions before this commit would have returned an empty list regardless of what the change
actually touched.

Output Summary: Commit `d3dd9fe00180c449c80b2771a4befcc474f512bb` created from an explicitly scoped
staged set of 29 paths. The scoped production-tree status is clean after the commit.
