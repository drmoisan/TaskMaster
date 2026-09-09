# Phase 6 — Toolchain step 1: CSharpier format

Timestamp: 2026-09-09T14-37

Task: [P6-T1]

Command: `dotnet tool run csharpier format .`

`format` rewrites tracked source and still exits 0 after rewriting, and prints one line of the form
`Formatted <count> files in <duration>ms.` in both cases, so the exit code alone cannot distinguish
a clean run from a repairing one and the printed count is a count of files processed rather than of
files changed. `git status --porcelain --untracked-files=all` and
`git diff --stat d636b0f28f548181685260d929de6d7d2940d1da` were therefore captured immediately
before and immediately after each invocation and each pair compared. The 40-character SHA is
transcribed from the `BASE-SHA:` field of `evidence/baseline/p0-t2-branch-and-base.md` per D2.

EXIT_CODE: 0

## Pass 1

Verbatim printed line:

```
Formatted 1622 files in 4545ms.
```

Exit code 0. The porcelain path set before the invocation was the three lines

```
 M docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/plan.2026-09-08T23-50.md
?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p5-t4-r5-comment-only-diff.md
?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p5-t5-r2-decision-fence.md
```

and the same three lines after it. The anchored diffstat summary line was
` 46 files changed, 1608 insertions(+), 91 deletions(-)` on both sides.

## Pass 2 — the mechanical comparison

Pass 1's before and after snapshots were taken as separate commands, so pass 2 re-ran the same
invocation with both snapshots captured and compared inside one process, making the equality a
computed value rather than a reading of two printed blocks.

Verbatim printed line:

```
Formatted 1622 files in 1570ms.
```

Exit code 0.

PATH_SETS_IDENTICAL: True
DIFFSTAT_IDENTICAL: True

Supporting observations from the same comparison: the porcelain path count was 3 before and 3
after, and the anchored diffstat summary line was
` 46 files changed, 1608 insertions(+), 91 deletions(-)` before and after.

Neither pair differed, so this step changed no file and the loop is not restarted from [P6-T1].
The three porcelain rows are this plan's own in-flight documents: the plan file carries the task
check-offs, and the two untracked artifacts are the [P5-T4] and [P5-T5] records that [P6-T46]
commits.

Output Summary: Two formatter passes, both at exit 0, both printing `Formatted 1622 files`. The
final pass carries `PATH_SETS_IDENTICAL: True` and `DIFFSTAT_IDENTICAL: True`, so the formatter
rewrote nothing and no loop restart is required.
