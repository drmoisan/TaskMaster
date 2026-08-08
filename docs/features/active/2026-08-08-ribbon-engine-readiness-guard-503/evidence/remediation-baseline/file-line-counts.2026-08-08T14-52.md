# Phase 0 — Pre-Remediation File Line Counts (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P0-T6]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; 'TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs','TaskMaster\Ribbon\RibbonExplorer.xml' | ForEach-Object { '{0}={1}' -f $_, (Get-Content $_ | Measure-Object -Line).Lines }; (Select-String -Path 'TaskMaster\Ribbon\RibbonExplorer.xml' -Pattern 'getEnabled=' -AllMatches | Measure-Object).Count"`
EXIT_CODE: 0

Corroborating command: `wc -l "TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs" "TaskMaster/Ribbon/RibbonExplorer.xml"` plus `grep -c '^[[:space:]]*$'` on each path.

## Output Summary

Verbatim output of the plan's stated command:

```text
TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs=279
TaskMaster\Ribbon\RibbonExplorer.xml=539
8
```

Corroborating physical line counts:

```text
  309 TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs
  539 TaskMaster/Ribbon/RibbonExplorer.xml
```

Blank-line counts: `RibbonExplorerXmlTests.cs` = 30; `RibbonExplorer.xml` = 0.

| Path | Plan expectation | `Measure-Object -Line` | Physical (`wc -l`) | Reconciled |
|---|---|---|---|---|
| `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` | 309 | 279 | **309** | Yes — matches the plan expectation on the physical count |
| `TaskMaster\Ribbon\RibbonExplorer.xml` | 539 | 539 | **539** | Yes — both methods agree |
| `getEnabled=` occurrences in `RibbonExplorer.xml` | 8 | 8 | — | Yes |

## Recorded measurement-method deviation (not a tree deviation)

The plan's stated measurement, `(Get-Content $path | Measure-Object -Line).Lines`, reports **279** for `RibbonExplorerXmlTests.cs` against the plan's stated expectation of **309**. This is a **measurement-method artifact, not a difference in the file**: `Measure-Object -Line` counts lines within each input string and contributes zero for an empty string, so the 30 blank lines in that file are not counted. `wc -l` reports the physical count of 309, which is the figure the plan expected and the figure the 500-line cap is expressed against.

`RibbonExplorer.xml` contains zero blank lines, so the two methods agree at 539 for that path and every later gate on the XML (the 527 ceiling in P2-T2 and P3-T3) is unaffected by the discrepancy regardless of which method is used.

Consequence recorded for later tasks: where a `.cs` path is measured against the 500-line cap (P1-T1 and P3-T3), the physical count is the binding figure. `Measure-Object -Line` under-reports it and must not be used alone to certify a `.cs` file as under the cap. This observation is recorded, not acted on beyond the two files in this cycle's scope lock; it is not a defect in the source tree and no file is modified in response to it.

Binary outcome satisfied on the reconciled physical counts: `RibbonExplorerXmlTests.cs` = 309, `RibbonExplorer.xml` = 539, and 8 `getEnabled` occurrences. The `Measure-Object -Line` figure of 279 is recorded verbatim above rather than overwritten.
