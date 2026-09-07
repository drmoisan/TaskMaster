# P9-T6 — Final coverage collection for the QuickFiler.Test assembly

Timestamp: 2026-09-07T16-00
Task: [P9-T6]
Issue: #796
Channel used: A

Command, the P0-T11 command form with the output path
`coverage\p9-t6-final.cobertura.xml`:

```
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput coverage\p9-t6-final.cobertura.xml
```

EXIT_CODE: 1

## Why a non-zero exit code is accepted here

The recorded stderr contains the literal `is below the required 80`. The exact message
was:

```
Cobertura line coverage 24.1857% is below the required 80% threshold.
```

That is the single-line message scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
line 54 throws when the document-level line rate is under the runner's own 80 percent
floor, and it is thrown from that file at line 54 column 9 as the stack line in the
console output confirms. The throw occurs after the Cobertura post-processing, so the
numbers were still written and are readable. The run itself reported
`Test Run Successful. Total tests: 1380, Passed: 1380`. This is exactly the condition
P0-T11 recorded and the same carve-out applies. Any other non-zero exit code would have
failed this task.

The 24.1857 percent figure is a whole-solution document-level rate produced by a run
scoped to a single test assembly. It is recorded as the post-change datum, not as a
policy verdict, on the same terms as the 24.1387 percent the baseline recorded.

## The output file exists

coverage/p9-t6-final.cobertura.xml was written and is readable; the six attributes below
were read back out of it. It sits at a gitignored path (`.gitignore` line 144 ignores
everything under coverage/) and is therefore never committed.

## Output Summary — document-level attributes

```
line-rate=0.241857
lines-covered=14925
lines-valid=61710
branch-rate=0.230082
branches-covered=3685
branches-valid=16016
```

All six numeric attributes are recorded, read with the P0-T11 extraction command form
against the final document.

Side-by-side with the baseline recorded in
evidence/baseline/p0-t11-coverage-baseline.md:

| Attribute | P0-T11 baseline | P9-T6 final |
|---|---|---|
| line-rate | 0.241387 | 0.241857 |
| lines-covered | 14867 | 14925 |
| lines-valid | 61590 | 61710 |
| branch-rate | 0.229747 | 0.230082 |
| branches-covered | 3664 | 3685 |
| branches-valid | 15948 | 16016 |

The no-regression comparison of the two ratios is computed and recorded in P9-T7, which
is the task the plan assigns it to; the figures are reproduced here only so that both
sides of that comparison are readable from one artifact.

## Per-file rows for the five named files

Rows were obtained with the P0-T11 grouping command form: every `//class` node grouped
by its `filename` attribute, summing per group the count of `lines/line` child nodes and
the count of `lines/line[@hits>0]` child nodes. Class nodes are grouped by `filename`
because a C# async state machine is emitted as a separate class node and would otherwise
split one source file's denominator across several nodes. The relative child axis is
used rather than a descendant axis, because a descendant axis double-counts on nested
nodes.

Filenames are reproduced verbatim as the tool emitted them, which uses backslash
separators. The forward-slash spellings the plan uses name the same files.

| Filename as emitted | lines-covered | lines-valid | P0-T11 lines-covered | P0-T11 lines-valid |
|---|---|---|---|---|
| `QuickFiler\Controllers\QfcFormController.Deactivate.cs` | 48 | 48 | 25 | 25 |
| `QuickFiler\Viewers\BreadcrumbDropDownHost.cs` | 287 | 289 | 291 | 293 |
| `QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs` | 24 | 24 | 23 | 23 |
| `QuickFiler\Controllers\QfcItemController.EventHandlers.cs` | 97 | 118 | 89 | 108 |
| `QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs` | 234 | 238 | 234 | 238 |

None of the five is ABSENT. Every one carries at least one class node whose `filename`
attribute names it, so no row records `ABSENT: no class node carries this filename`.
Because no file is recorded ABSENT at both P0-T11 and P9-T6, task P9-T7 has no file to
exclude from the changed-code denominator on that basis and its NOT MEASURABLE list
carries no entry from this cause.

The BreadcrumbDropDownHost.cs denominator fell from 293 to 289 while its covered count
fell from 291 to 287. That is the expected consequence of executed task P1-T2 relocating
`OnDropDownClosed` out of the main part; the relocated lines reappear in the row below.

## The new diagnostics part

A sixth row exists in the final document that has no baseline counterpart, because the
file did not exist at P0-T11:

| Filename as emitted | lines-covered | lines-valid |
|---|---|---|
| `QuickFiler\Viewers\BreadcrumbDropDownHost.Diagnostics.cs` | 28 | 28 |

It is recorded here for completeness. Task P9-T7 reports it on its own line and excludes
it from the behavioural changed-code figure, so that logging-only coverage cannot
inflate the figure for the behavioural changes.

## Test population

The coverage run executed 1380 tests with 1380 Passed, the same population and the same
result as the P9-T5 full-assembly run. The runner applies the `TestCategory!=LiveOutlook`
filter at scripts/vscode/Invoke-MSTestWithCoverage.ps1 line 76, which is the same filter
P9-T5 passes explicitly, so the two runs are comparable and neither starts an external
Outlook process.

Output Summary: the final Cobertura document was written to
coverage/p9-t6-final.cobertura.xml and all six document-level attributes were read back
from it: line-rate 0.241857, lines-covered 14925, lines-valid 61710, branch-rate
0.230082, branches-covered 3685, branches-valid 16016. All five per-file rows carry
figures and none is ABSENT. EXIT_CODE 1 is accepted because the recorded stderr carries
the literal `is below the required 80`, exactly as at P0-T11.
