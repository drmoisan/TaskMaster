# [P4-T9] CSharpier Format — The Two Viewer Files After the Layout Decision

Timestamp: 2026-09-08T10-06
Command: `dotnet tool run csharpier format QuickFiler/Viewers/BreadcrumbDropDownHost.cs QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` (the [P4-T6] command)
EXIT_CODE: 0
Output Summary: The formatter ran over both viewer files after the [P4-T8] branch-B relocation and rewrote neither. Both observation spans are byte-identical before and after the invocation, so the relocated members carry CSharpier's own layout at the new indentation, which is unchanged because both parts declare the members at the same nesting depth.

Verbatim printed `Formatted` line:

```
Formatted 2 files in 2066ms.
```

PATH_SETS_IDENTICAL: True
DIFFSTAT_IDENTICAL: True

## The observation pair

The same two spans [P4-T6] used were captured immediately before and immediately after this invocation: `git status --porcelain --untracked-files=all` and `git diff --stat origin/main`, both unscoped.

`git status --porcelain --untracked-files=all` reported 30 lines before and 30 lines after, byte-identical line for line and in the same order. `PATH_SETS_IDENTICAL: True` records that comparison. The set is the 11 modified paths and 19 untracked evidence artifacts listed in full in the [P4-T6] artifact, extended by the artifacts written since; no path entered or left the set across this invocation.

`git diff --stat origin/main` was likewise byte-identical before and after. `DIFFSTAT_IDENTICAL: True` records that comparison. The two rows this task could have moved, and the summary row, read after the invocation:

```
 QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs  |   55 +-
 QuickFiler/Viewers/BreadcrumbDropDownHost.cs       |   37 -
 40 files changed, 3326 insertions(+), 279 deletions(-)
```

Both rows carried the same counts before the invocation. The diffstat span is the discriminating half of the pair: both viewer files were already reported as `M` by porcelain status before the invocation and would still be reported as `M` after a rewrite, whereas a rewrite would move the per-file insertion and deletion counts.

## Why the exit code is not the observation

`format` rewrites tracked source and still exits 0 after rewriting, and prints a `Formatted <count> files in <duration>ms.` line whether or not it changed anything, so neither distinguishes a clean run from a repairing one.

## Procedural note

This task's command was first run with a path-scoped diffstat, `git diff --stat origin/main -- QuickFiler QuickFiler.Test`, rather than the unscoped span [P4-T6] used. That run also reported both spans identical and the formatter changing nothing. The command was then re-run with the exact unscoped observation pair the task specifies, and the values recorded above are from that second run. Running the formatter twice is inert here because it changed nothing on either pass, which the identical spans of both passes establish.

## Interpretation

The formatter changed nothing, so no toolchain-loop restart is triggered by this step, and the counts [P4-T10] audits are stable.
