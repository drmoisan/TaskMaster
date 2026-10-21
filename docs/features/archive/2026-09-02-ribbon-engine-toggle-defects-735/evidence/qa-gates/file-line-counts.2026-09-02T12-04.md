# Phase 4 — Post-Format Line-Count Audit (P4-T2)

Timestamp: 2026-09-03T02-58
Task: [P4-T2]
Command: `@(Get-Content -LiteralPath <path>).Count` for each file, measured AFTER the P4-T1 format pass.
EXIT_CODE: 0

This is the authoritative ceiling audit. The P0-T10 counts were advisory, because the formatter
reflows to its print width and can push a hand-written file past the ceiling.

Counting idiom: `@(Get-Content ...).Count`, which counts every physical line.
`Get-Content | Measure-Object -Line` is NOT used; it silently drops blank lines and under-reports
(it read 360 for a 389-line file at baseline).

## Measured counts against the 500-line ceiling

| File | Lines | At or below 500 |
|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | **515** | **NO** |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | 444 | Yes |
| `TaskMaster/Ribbon/SpamManagerResetGate.cs` | 141 | Yes |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 496 | Yes |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 459 | Yes |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | 277 | Yes |
| `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` | 326 | Yes |

Six of the seven measured files are under the ceiling. One is over it.

## The CustomUI document is deliberately not measured against this ceiling

`TaskMaster/Ribbon/RibbonExplorer.xml` is 544 lines. It is not in the table above because it is a
resource document rather than production code, test code or a reusable script; it already exceeded
500 lines before this change, at 545 lines in the P0-T10 baseline; and this change only REMOVES a
line from it, taking it from 545 to 544. Measuring it against the ceiling would fail a file this
change improved.

## Movement from the advisory baseline

| File | P0-T10 baseline | This audit | Delta |
|---|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | 389 | 515 | +126 |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | 412 | 444 | +32 |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 323 | 496 | +173 |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 459 | 459 | 0 |
| `TaskMaster/Ribbon/SpamManagerResetGate.cs` | did not exist | 141 | new |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | did not exist | 277 | new |
| `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` | did not exist | 326 | new |

The coordinator's +126 exceeds the research record's projection of roughly 455 to 465 lines. The
projection was an estimate made before the code and its documentation were written; the plan
required this task to measure rather than assume, and the measurement is 515.

## Consequence

`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` at 515 lines is above the ceiling, so the P4-T3
contingency resolves on **branch B**: the versioned cache is extracted into its own class. The plan
is explicit that documentation must not be trimmed to fit, so reducing the file by deleting the
XML-doc paragraphs that explain the compare-and-swap rationale is not an available option.

## Pass 2 — the branch B re-measurement

P4-T3 took branch B and extracted the versioned cache, then re-ran P4-T1 and this audit. The
re-measured counts are the FINAL, authoritative figures:

| File | Lines | At or below 500 |
|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | **415** | **Yes** |
| `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` | 157 | Yes |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | 444 | Yes |
| `TaskMaster/Ribbon/SpamManagerResetGate.cs` | 141 | Yes |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 496 | Yes |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 459 | Yes |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | 277 | Yes |
| `TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs` | 213 | Yes |
| `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` | 326 | Yes |

**All nine measured files are at or below the 500-line ceiling.** The coordinator moved from 515 to
415, a reduction of 100 lines, by moving five members into the new cache class rather than by
trimming documentation.

`TaskMaster/Ribbon/RibbonExplorer.xml` remains outside this audit for the reason stated above, and
is unchanged at 544 lines.

Pass 1 result: Seven files measured after the format pass. Six are at or below the 500-line ceiling
(444, 141, 496, 459, 277, 326). `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` is 515 lines and
exceeds it, so P4-T3 takes branch B. The CustomUI resource document, at 544 lines, is deliberately
outside this audit and moved from 545 to 544.

Output Summary: On pass 1, seven files were measured after the format pass and six were at or below
the 500-line ceiling, with `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` at 515 lines, which
triggered the P4-T3 branch B contingency. After branch B extracted the versioned cache and P4-T1 and
P4-T2 were re-run, nine files were measured and every one is at or below the ceiling, with the
coordinator at 415 lines. The CustomUI resource document is deliberately outside this audit and moved
from 545 to 544 lines.
