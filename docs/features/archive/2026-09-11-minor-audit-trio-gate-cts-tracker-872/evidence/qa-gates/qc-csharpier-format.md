# Phase 2 — CSharpier Format Stage

Timestamp: 2026-09-13T15-29
Task: [P2-T1]

Command: dotnet tool run csharpier format .
EXIT_CODE: 0

FormattedFileCount: 1625
ChangedFileCount: 0

Output Summary: the completing pass of the formatter printed the single summary line
`Formatted 1625 files in 1779ms.` and exited 0. The anchored numstat taken immediately before the run
and the anchored numstat taken immediately after it are byte-identical across all 27 enumerated paths,
so `ChangedFileCount:` is 0 and no restart is triggered by this task on this pass. The falsifiable
observation is that before-and-after comparison rather than the exit code, because the format
subcommand exits 0 both when it changed nothing and when it repaired drift. The processed count of
1625 counts files visited, not files repaired, so it is recorded but is not the restart trigger.

## Spans Run, In Order

```
$b = ((Select-String -Path 'docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872/evidence/baseline/base-commit.md' -Pattern 'BaseCommit: ' -SimpleMatch | Select-Object -First 1).Line -split ' ')[-1]
git add --intent-to-add -- . ":(exclude).claude" ":(exclude)docs/features/potential"
git diff --numstat $b -- . ":(exclude).claude" ":(exclude)docs/features/potential"
dotnet tool run csharpier format .
git add --intent-to-add -- . ":(exclude).claude" ":(exclude)docs/features/potential"
git diff --numstat $b -- . ":(exclude).claude" ":(exclude)docs/features/potential"
```

The base anchor read from the `BaseCommit:` line of the P0-T2 artifact is
`430e2a11db0fa7069d02d42e18df46d21f7db7b5`. Both numstat spans carry it as their ref operand; an
unanchored diff would compare the worktree against the index and would pass vacuously after a commit.
Each numstat is preceded by its own intent-to-add span because a numstat enumerates tracked changes
only, so an uncommitted new file would otherwise be invisible to both runs. All four spans carry the
same two exclusions: the agent-memory tree, which the executor writes to during the run, and the
potential features directory, in which a parallel run's other items queue their own untracked
promotion files that must not be placed in this worktree's index.

## Completing Pass — Numstat Comparison

Both spans enumerated the same 27 paths with the same insertion and deletion figures. The six Write
Set paths that survive the delivery and the two deleted ones read as follows in both spans:

| Path | Insertions | Deletions |
|---|---|---|
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs | 103 | 0 |
| UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs | 100 | 0 |
| UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs | 0 | 231 |
| UtilitiesCS.Test/UtilitiesCS.Test.csproj | 0 | 1 |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs | 1 | 1 |
| UtilitiesCS/Threading/ProgressPackage.cs | 38 | 1 |
| UtilitiesCS/Threading/ProgressTrackerAsync.cs | 0 | 109 |
| UtilitiesCS/UtilitiesCS.csproj | 0 | 1 |

The remaining 19 paths are this plan's own bookkeeping and evidence under the feature folder.

## Superseded Pass 1 — The Restart It Triggered

Pass 1 ran the same six spans at 2026-09-13T15-28. The formatter printed
`Formatted 1625 files in 5509ms.` and exited 0, and its two numstat spans disagreed on exactly one
path: `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs` read 96 insertions and 0 deletions before
the run and 100 insertions and 0 deletions after it. Every other path was identical in both spans.
`ChangedFileCount:` on pass 1 was therefore 1, which is the stated restart trigger, and the Phase 2
loop restarted from this task. The formatter had repaired drift in the file that P1-T6 through P1-T8
extended, adding four lines to it; its line count moved from 216 to 220.

Pass 1 is recorded here rather than in a separate artifact so that the restart is auditable from the
same file that records the completing pass. Neither the formatter's exit code nor its processed file
count distinguished pass 1 from pass 2: both printed 1625 files and both exited 0. Only the numstat
comparison did.

## Restart Accounting For This Task

- Pass 1, 2026-09-13T15-28: `ChangedFileCount: 1`, restart triggered.
- Pass 2, 2026-09-13T15-29: `ChangedFileCount: 0`, this task passes and the loop advances to P2-T2.

The build lock was acquired immediately before each formatter invocation and released immediately
after it returned. No lock was held across any read, edit or wait.
