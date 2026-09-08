# [P0-T8] CSharpier Formatting Baseline

Timestamp: 2026-09-08T09-18
Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0
Output Summary: The read-only check exited 0 and reported no drifting file, so the tree is clean against the manifest-pinned CSharpier 1.2.6 before any task of this plan edits a source file. The pre-existing drift set is empty.

Verbatim printed line:

```
Checked 1613 files in 6541ms.
```

BASELINE-CSHARPIER-CHECKED-FILES: 1613

## Why exit 0 is the observation here

`check` is read-only and exits non-zero on drift, so unlike `format` its exit code does distinguish a clean tree from a drifting one. Exit 0 together with the single `Checked` line is therefore the clean-tree observation, and no before-and-after tree comparison is required for this task.

PRE-EXISTING-DRIFT-SET: none
