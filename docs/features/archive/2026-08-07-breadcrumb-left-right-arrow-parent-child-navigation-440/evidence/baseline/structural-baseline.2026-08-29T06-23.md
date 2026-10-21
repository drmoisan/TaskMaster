# Phase 0 — Structural Baseline (issue #440, plan task P0-T9)

Timestamp: 2026-08-29T06-23

Command (single `pwsh -NoProfile -Command` payload, run from the repository root):

```
$a = "UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs"
$b = "UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs"
$c = "UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs"
(Get-Content -LiteralPath $a).Count
(Get-Content -LiteralPath $b).Count
(Get-Content -LiteralPath $c).Count
@(Select-String -LiteralPath $a -SimpleMatch -Pattern "activeIndex.Value == row.Chain.Count - 1").Count
@(Select-String -LiteralPath $a -SimpleMatch -Pattern "row.ActivateSegment(activeIndex.Value - 1)").Count
git merge-base HEAD b56400ab663a85b6039139d4548f408821e957ce
```

EXIT_CODE: 0

## Output Summary

### (a) Line counts, each measured as `(Get-Content -LiteralPath <path>).Count`

| File | Observed | Plan expectation | Match |
| --- | --- | --- | --- |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 248 | 248 | yes |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` | 235 | 235 | yes |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | 495 | 495 | yes |

### (b) Occurrences of the literal `activeIndex.Value == row.Chain.Count - 1`

In `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`, counted with
`Select-String -SimpleMatch`: **1**. Plan expectation: 1. Match.

### (c) Occurrences of the literal `row.ActivateSegment(activeIndex.Value - 1)`

In the same file, counted the same way: **1**. Plan expectation: 1. Match.

### (d) Merge-base confirmation

`git merge-base HEAD b56400ab663a85b6039139d4548f408821e957ce` printed:

```
b56400ab663a85b6039139d4548f408821e957ce
```

`BASE` is therefore an ancestor of the working branch, as the plan asserts.

## Drift

No divergence from the plan's recorded values. No later gate requires re-derivation
on drift grounds.
