# P0-T11 — Coverage baseline for the QuickFiler.Test assembly

Timestamp: 2026-09-07T14-14
Task: [P0-T11]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput coverage\p0-t11-baseline.cobertura.xml
```

EXIT_CODE: 1

## Why a non-zero exit code is accepted here

The recorded stderr contains the literal `is below the required 80`. The exact
message was:

```
Cobertura line coverage 24.1387% is below the required 80% threshold.
```

That is the single-line message scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
line 54 throws when the document-level line rate is under the runner's own 80 percent
floor. The throw occurs after the Cobertura post-processing at line 342, so the
numbers were still written and are readable. The run itself reported
`Test Run Successful. Total tests: 1370, Passed: 1370`. Any other non-zero exit code
would have failed this task.

The 24.1387 percent figure is a whole-solution document-level rate produced by a run
scoped to a single test assembly; it is recorded as the baseline datum, not as a
policy verdict.

## Output Summary — document-level attributes

```
line-rate=0.241387
lines-covered=14867
lines-valid=61590
branch-rate=0.229747
branches-covered=3664
branches-valid=15948
```

All six numeric attributes above are recorded.

## Per-file rows for the five named files

Rows were obtained by grouping every `//class` node by its `filename` attribute and
summing, per group, the count of `lines/line` child nodes and the count of
`lines/line[@hits>0]` child nodes. Class nodes are grouped by `filename` because a
C# async state machine is emitted as a separate class node and would otherwise split
one source file's denominator across several nodes. The relative child axis is used
rather than a descendant axis, because a descendant axis double-counts on nested
nodes.

Filenames are reproduced verbatim as the tool emitted them, which uses backslash
separators. The forward-slash spellings in the plan name the same five files.

| Filename as emitted | lines-covered | lines-valid |
|---|---|---|
| `QuickFiler\Controllers\QfcFormController.Deactivate.cs` | 25 | 25 |
| `QuickFiler\Viewers\BreadcrumbDropDownHost.cs` | 291 | 293 |
| `QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs` | 23 | 23 |
| `QuickFiler\Controllers\QfcItemController.EventHandlers.cs` | 89 | 108 |
| `QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs` | 234 | 238 |

None of the five is ABSENT. Every one of the five carries a class node whose
`filename` attribute names it, so no row records
`ABSENT: no class node carries this filename`, and task P9-T7 has no file to exclude
from the changed-code denominator on the strength of this baseline.

## Raw output

The Cobertura XML is at the gitignored path coverage/p0-t11-baseline.cobertura.xml
(`.gitignore` line 144 ignores everything under coverage/).
