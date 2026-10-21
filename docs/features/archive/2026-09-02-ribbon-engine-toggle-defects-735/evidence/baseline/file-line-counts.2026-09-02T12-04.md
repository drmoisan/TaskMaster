# Phase 0 — Pre-Change File Line Counts (P0-T10)

Timestamp: 2026-09-03T01-27
Task: [P0-T10]
Command: `@(Get-Content -LiteralPath <path>).Count` for each file, with headroom computed as `500 - <count>`.
EXIT_CODE: 0

## Counting-idiom note

The first attempt used `Get-Content -LiteralPath <path> | Measure-Object -Line`. That idiom
under-counts: `Measure-Object -Line` does not count empty strings, so every blank line in the file is
dropped. It reported 360 for `EngineToggleStateCoordinator.cs` against a true 389. The counts below
use `@(Get-Content -LiteralPath <path>).Count`, which counts every physical line. All five source
figures reproduce the plan's cited lengths exactly.

## Measured counts

| File | Lines | Headroom to 500 |
|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | 389 | 111 |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | 412 | 88 |
| `TaskMaster/Ribbon/RibbonExplorer.xml` | 545 | -45 |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 323 | 177 |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 459 | 41 |
| `TaskMaster.Test/TaskMaster.Test.csproj` | 393 | 107 |

## Observations carried forward

- `TaskMaster/Ribbon/RibbonExplorer.xml` is already 45 lines above 500 before this change. It is a
  CustomUI resource document, not production or test code, and this change only removes a line from
  it. P4-T2 records that it is deliberately not measured against the 500-line ceiling.
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` has only 41 lines of headroom. This
  is exactly the constraint that drives the plan's partial-class decision: the six new race tests
  cannot fit in this file, so P3-T1 adds the `partial` keyword and P3-T2 puts them in a second file.
- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` at 389 with 111 lines of headroom is the file
  the P4-T3 contingency is written for. Research projects roughly 455 to 465 lines after formatting,
  which would leave it under the ceiling, but P4-T3 measures rather than assumes.
- `TaskMaster/Ribbon/RibbonController.Intelligence.cs` at 412 with 88 lines of headroom; the
  research projects roughly 440 after the Finding 2 edit.

These counts are advisory. The authoritative audit is P4-T2, taken after the final format pass,
because the formatter reflows to its print width and can push a hand-written file past the ceiling.

Output Summary: Six files measured. Five C#/XML sources: 389, 412, 545, 323, 459 lines; the test
project file 393 lines. Only `RibbonExplorer.xml` exceeds 500, at 545, and it is a resource document
carved out of the ceiling by P4-T2.
