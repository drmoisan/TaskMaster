# [P7-T2] CSharpier Check (Read-Only)

Timestamp: 2026-09-08T10-19
Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0
Output Summary: The read-only check exited 0 and reported no drifting file, so the tree is formatter-clean against the manifest-pinned CSharpier 1.2.6 after every source change this plan makes. The checked-file count rose by exactly 2 from the [P0-T8] baseline, which is exactly the two new `.cs` files this plan creates.

Verbatim printed line:

```
Checked 1615 files in 7085ms.
```

FINAL-CSHARPIER-CHECKED-FILES: 1615
CHECKED-FILES-DELTA: 2

The delta is `FINAL-CSHARPIER-CHECKED-FILES` 1615 minus `BASELINE-CSHARPIER-CHECKED-FILES` 1613 from [P0-T8]. The task requires it to be at least 2 and it is exactly 2.

## Why exit 0 is the observation here

`check` is read-only and exits non-zero on drift, so unlike `format` its exit code does distinguish a clean tree from a drifting one. Exit 0 together with the single `Checked` line is the clean-tree observation.

## DELTA-COMPOSITION

Every path this plan creates, and whether CSharpier could count it:

| Path | In the counted set? | Why |
| --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` | INSIDE | A new `.cs` file under no ignore rule. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` | INSIDE | A new `.cs` file under no ignore rule. |
| `coverage/810-effective-coverage.config` | OUTSIDE | Written by [P0-T12] after the [P0-T8] baseline, so it was a candidate residual. It is not counted: CSharpier 1.2.6 recognises `packages.config` by filename rather than the `.config` extension generally, and this file is named `810-effective-coverage.config`. The file was confirmed present on disk, so its absence from the count is a matter of what CSharpier accepts, not of the file not existing. |
| The 30 evidence artifacts under `.../evidence/` | OUTSIDE | `.csharpierignore` excludes `**/evidence/**`, and they are Markdown, which CSharpier does not process. |
| `docs/.../plan.2026-09-07T21-59.md`, `spec.md`, `issue.md` | OUTSIDE | Markdown. |
| `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj` | OUTSIDE | Modified rather than created, so they add no new file to any count, and `.csharpierignore` excludes `*.csproj` regardless. |

The two INSIDE entries account for a delta of 2, and the observed delta is 2.

DELTA-ATTRIBUTION: COMPLETE

The composition block accounts for the whole delta with no unattributed remainder, so no `RESIDUAL:` figure is recorded. The lower bound of 2 is the gate: a delta of 1 or fewer would have meant at least one of the two new source files did not enter the counted set, which would be a real defect in the project registration or in the file location. No upper bound is gated, because the checked-file count is not confined to the files this plan creates; on the immediately preceding feature in this code area the same repo-wide command moved by 7 while that branch contributed only three countable files.
