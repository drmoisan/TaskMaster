# Baseline — Diff Anchor (P0-T2, re-recorded under SD23)

SUPERSEDED BASELINE RE-RECORDED: SD23

RE-ANCHORED BASE: 736c2cf2

Timestamp: 2026-09-05T21-54

## Why the earlier record is superseded

An external actor rebased the feature branch from `a007f72e` onto `origin/main` at `77c6d314`
during execution. Every prior commit received a new SHA. The base commit the superseded record
named, `b95a5252`, is orphaned and is no longer an ancestor of HEAD, so every figure and every
ancestry claim taken against it describes a commit that is no longer on this branch.

The `pre-782-base` tag has been re-anchored to `736c2cf2`, the last documentation-only commit
before the implementation commit. This task verifies that anchor rather than creating it.
`git tag -f pre-782-base HEAD` — the command the superseded form of this task carried — is
prohibited: it would move the anchor to HEAD, and every `pre-782-base`-anchored gate in the plan
would then compare a tree against itself and pass vacuously.

## Measurement method and measuring party

The four Phase 0 gate baselines re-recorded by P0-T3 through P0-T7 were measured by the
**orchestrator, not the executor**, at the re-anchored base commit `736c2cf2`, by the
temporary-restore method: the orchestrator restored the six Write Set source files Phase 1 has
changed so far — `UtilitiesCS/Threading/UiThread.cs`,
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, `UtilitiesCS/Threading/ProgressTracker.cs`,
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`,
and `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — to their `pre-782-base` content with
`git checkout pre-782-base -- <those six paths>`, ran the four gates, restored those files to HEAD
in a `finally` block, and left the worktree clean and at HEAD afterwards.

This task, P0-T2, is one of the two Phase 0 tasks that do run their own commands, because it reads
properties of the repository that do not depend on the Phase 1 working tree. **The five commands
recorded below were run by the executor**, not by the orchestrator, and their exit codes and output
are the executor's own observations.

Command:

```text
git rev-parse pre-782-base
git merge-base --is-ancestor pre-782-base HEAD
git rev-parse origin/main
git merge-base --is-ancestor origin/main HEAD
git status --porcelain --untracked-files=all
```

EXIT_CODE: 0

Output Summary:

`git rev-parse pre-782-base` exited 0 and printed the 40-character SHA:

```text
736c2cf234cdd71b604c908f348b6aa89b256b53
```

`git merge-base --is-ancestor pre-782-base HEAD` exited **0**, so the re-anchored tag is an
ancestor of HEAD.

`git rev-parse origin/main` exited 0 and printed the 40-character SHA:

```text
77c6d31404e2bc2291aec7eb9561e393c20cdcae
```

`git merge-base --is-ancestor origin/main HEAD` exited **0**, so the branch is correctly based on
`origin/main` and no further rebase is required.

`git status --porcelain --untracked-files=all` exited 0 and printed **no lines**. The porcelain
image is recorded verbatim below and is empty:

```text
```

The empty image is expected. Unlike the superseded record, `evidence/baseline/phase0-instructions-read.md`
does not appear here as an untracked entry: it is now a committed tracked file, having been committed
by the external history rewrite, and P0-T1 is not re-run. The plan file does not appear here either,
because this task's commands ran before any check-off was written to it in this execution.

`spec.md` and `user-story.md` are absent from this porcelain output, so the worktree was clean at
the point this record was taken.

## Superseded record, retained for audit and not carried forward as current

The superseded revision of this artifact named the base commit `b95a5252` and recorded a two-line
porcelain image:

```text
 M docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/plan.2026-09-05T15-47.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/baseline/phase0-instructions-read.md
```

Both the commit and that two-line image are superseded. Neither is carried forward as though it
were current. P7-T9, P8-T19, and P8-T20 subtract pre-existing entries against the **empty** image
recorded above, not against the two-line one.
