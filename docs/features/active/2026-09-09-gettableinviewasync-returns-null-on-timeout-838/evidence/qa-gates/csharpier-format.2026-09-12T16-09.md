# P4-T1 — Formatter write-mode pass over the worktree root

Timestamp: 2026-09-13T03-03

Canonical CLAUDE.md command text: `dotnet tool run csharpier format .`

Command: a single pwsh payload that first runs `git -C . add -N` on the two created files so they are visible to the diff, captures `git -C . diff --numstat 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- . ":(exclude).claude/agent-memory" ":(exclude)docs/features/potential"` as the before-listing, runs the formatter in write mode, records its exit code, captures the same numstat command as the after-listing, and compares the two listings row by row keyed on path.

This task ran once. The single recorded run is the final run and it has `FORMAT_CHANGED_TREE=False`, so the toolchain loop does not restart at this task.

## Run 1 (final)

EXIT_CODE: 0

```
FORMAT_EXIT=0
FORMAT_CHANGED_TREE=False
DIFFERING_ROW_COUNT=0
```

The formatter printed `Formatted 1626 files in 6928ms.`

The before-listing and the after-listing are identical. Both carry the same 37 rows: the six code and project rows

```
1	1	UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs
301	0	UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncFailureContractTests.cs
1	0	UtilitiesCS.Test/UtilitiesCS.Test.csproj
33	0	UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.Failures.cs
29	8	UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
1	0	UtilitiesCS/UtilitiesCS.csproj
```

and 31 Markdown rows under the feature folder, comprising the five feature documents and the 26 evidence artifacts written up to this point.

Output Summary: the write-mode formatter pass exited 0 and changed no file in the tree. `DIFFERING_ROW_COUNT=0` means no path's numstat row differs between the before-listing and the after-listing, so the acceptance clause that every differing path be one of the four C# files in the Write Set holds vacuously and no path outside the Write Set was rewritten. This is the observation the task requires beyond the exit code: a write-mode command rewrites tracked source and still exits 0 after rewriting, so the exit code alone cannot distinguish a clean run from a repairing one. The numstat comparison is used rather than porcelain status because every file the formatter could rewrite here is already listed by status, so a membership comparison would read 0 whatever was rewritten. That no file needed repair is consistent with P0-T16, which established that the tree carried no pre-existing format drift, and shows that the four C# files this change touches were authored in the formatter's own output shape.
