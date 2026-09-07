# [P13-T3] Commit of the move-readiness seam, in isolation

Timestamp: 2026-08-26T11-41

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs
git commit -m "refactor(474): split the move-readiness evaluation from its notification"
git show --name-only --format='%H %s' HEAD
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Commit `4938779a7a4092da1de24e7b62a0c05c5272831e` —
`refactor(474): split the move-readiness evaluation from its notification`.

`git show --name-only HEAD` path list, verbatim and complete:

```
QuickFiler/Controllers/QfcCollectionController.cs
```

Exactly one path, and it is `<CTRL>`. The task's acceptance is met.

## Why the seam is committed separately (D15)

This is the third and last of the three AC-20 seams. Committing it alone gives a named tree at which
the seam's behaviour-neutrality was measured: P13-T2 recorded 964 passed, identical to P12-T4, with
no test added. The two readiness tests land on top of it in the Phase 13 fix commit.

Unlike the `ShrinkByRows` and `DrainBackgroundLoadingTasksAsync` seams, this one is not followed by
a behavioural correction — the seam *is* the fix for issue #474 defect 2, whose defect is that
readiness cannot be inspected without presenting a modal dialog. What follows the seam is the two
tests that were impossible before it, not a further production edit.

## Staging hygiene

The `git add` used an explicit pathspec. `.claude/agent-memory/**` and `.claude/state/**` remain
unstaged. The Phase 12 evidence artifacts, the P11-T8 artifact, the AC-7 check-off in `spec.md` and
the Phase 12 and 13 plan checkbox edits remain uncommitted and are absorbed by the Phase 13 fix
commit (P13-T8), per the plan's commit cadence.
