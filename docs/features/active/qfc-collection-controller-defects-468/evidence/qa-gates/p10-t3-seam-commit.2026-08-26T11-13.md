# [P10-T3] Commit of the issue #471 `ShrinkByRows` seam, in isolation

Timestamp: 2026-08-26T11-13

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs
git commit -m "refactor(471): extract the shared panel-height arithmetic behind ShrinkByRows"
git show --name-only --format='%H %s' HEAD
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Commit `6cac5a822883d70da3e2cede927185435398f66d` —
`refactor(471): extract the shared panel-height arithmetic behind ShrinkByRows`.

`git show --name-only HEAD` path list, verbatim and complete:

```
QuickFiler/Controllers/QfcCollectionController.cs
```

Exactly one path, and it is `<CTRL>`. The task's acceptance is met.

## Why the seam is committed separately (D15)

D15 requires each of the AC-20 seams to be committed apart from the defect fix that lands on top of
it. The separation is what makes the neutrality claim reviewable: the diff of this commit can be
read on its own and shown to preserve the pre-existing inverted sign, and the P10-T2 suite run
(958 passed, identical to P9-T4) is attached to precisely this tree state. Folding the seam and the
sign correction into one commit would leave no tree at which the neutrality assertion could be
evaluated.

## Staging hygiene

The `git add` used an explicit pathspec. `.claude/agent-memory/**` and `.claude/state/**` are
dirty in this worktree, are not owned by this feature, and were not staged. `git status --porcelain`
after the commit still shows them as modified/untracked, together with the plan checkbox edit and the
Phase 10 evidence artifacts, which the Phase 10 fix commit (P10-T12) absorbs.
