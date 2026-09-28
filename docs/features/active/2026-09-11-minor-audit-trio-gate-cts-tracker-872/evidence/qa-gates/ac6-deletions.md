# AC6 — Deletion Of The Dormant Tracker And Its Test

Timestamp: 2026-09-13T15-42
Task: [P2-T11]

Verdict: PASS

Command: git add --intent-to-add -- . ":(exclude).claude" ":(exclude)docs/features/potential"
Command: git status --porcelain --untracked-files=all -- UtilitiesCS/Threading/ProgressTrackerAsync.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
Command: git diff --name-status $b -- UtilitiesCS/Threading/ProgressTrackerAsync.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
EXIT_CODE: 0

## On-Disk Absence

Neither path exists on disk. A `Test-Path` query against each returned False:

```
TrackerSrcExists: False
TrackerTestExists: False
```

## Anchored Name-Status Diff

The diff is anchored to the base commit `430e2a11db0fa7069d02d42e18df46d21f7db7b5` recorded by P0-T2.
It printed exactly two lines and each begins with the deletion status letter D:

```
D	UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
D	UtilitiesCS/Threading/ProgressTrackerAsync.cs
```

## Porcelain Span, Verbatim

```
```

The fenced block is empty because the command printed nothing. That is the expected state and not a
failure of the check: the Phase 1 commit already recorded both deletions, and porcelain status reports
the difference between the worktree and the index, which is empty once a change is committed. The
anchored diff is the mechanism that still carries the evidence at this point, which is why the two
spans accompany each other. They are complementary rather than redundant, and each alone is wrong in
one state: a name-listing diff enumerates tracked changes only and is blind to an untracked path, while
porcelain goes empty after a commit.

The intent-to-add span excludes the potential features directory because intent-to-add is a staging
operation, and in a parallel run other items queue their own untracked promotion files there that must
not be placed in this worktree's index. It also excludes the agent-memory tree, which the executor
writes to during the run.

## Corroboration

The analyzer rebuild in P2-T3 and the nullable rebuild in P2-T4 both succeeded with zero errors. Since
this repository's projects are not SDK-style and name every compiled source by an explicit Compile
item, a build could not have succeeded had either deleted file still been named by its owning project
file. P2-T10 records the matching Compile-item removals and a `TrackerReferences:` count of 0.
