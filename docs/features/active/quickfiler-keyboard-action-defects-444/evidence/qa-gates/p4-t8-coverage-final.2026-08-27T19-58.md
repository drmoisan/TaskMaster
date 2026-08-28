# [P4-T8] Final coverage capture

Timestamp: 2026-08-27T19-58
Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.final.xml`
EXIT_CODE: 0
Output Summary: `Test Run Successful.` `Total tests: 6713`, `Passed: 6713`. Root Cobertura
`line-rate` 0.851295 and `branch-rate` 0.7921, giving `FinalLineCoveragePercent = 85.13` and
`FinalBranchCoveragePercent = 79.21`. Per-file: `KbdActions.cs` line-rate 0.9897959183673469,
`QfcItemController.Navigation.cs` line-rate 0.92126.

The raw Cobertura document stays in the gitignored `coverage` directory
(`coverage\coverage.cobertura.final.xml`). Only the extracted figures below are committed.

## Root `<coverage>` element (verbatim attribute values)

```xml
<coverage line-rate="0.851295" branch-rate="0.7921" complexity="25254" version="1.9"
          timestamp="1787860633" lines-covered="54402" lines-valid="63905"
          branches-covered="12935" branches-valid="16330">
```

```
FinalLineCoveragePercent   = 85.13
FinalBranchCoveragePercent = 79.21
FinalMeasurableLines       = 63905   (lines-valid)
```

Both figures are numeric. This is the **unfiltered whole-run denominator** the repository's own
wrapper produces: `dotnet-coverage` instruments every assembly loaded at run time, so `lines-valid`
counts vendored and test-support code as well as first-party production code. It is the same
denominator and the same command as the `[P0-T20]` baseline, which is what makes the two directly
comparable; `[P4-T11]` performs that comparison.

## Per-file line rates for the two changed files

| File | `<class>` `line-rate` | `<class>` `branch-rate` |
| --- | --- | --- |
| `QuickFiler\Controllers\KbdActions.cs` | 0.9897959183673469 | 1 |
| `QuickFiler\Controllers\QfcItemController.Navigation.cs` | 0.92126 | 0.875 |

No `<class>` element exists whose `filename` is `QuickFiler\Controllers\QfcCollectionController.cs`.
That class carries `[ExcludeFromCodeCoverage]` at its declaration, so its lines are outside every
coverage denominator (decision D-P4). Its absence from the document was verified by an XPath query
over `//class` filtered on that filename, which returned no node.

## Threshold branch

**Not taken.** `Assert-CoberturaLineCoverageThreshold` inside the wrapper throws when the
repository-wide Cobertura `line-rate` falls below 80 percent. The observed rate is 85.13 percent, so
the assertion passed, the post-processing step ran (`Post-processing coverage XML for Koverage
compatibility... Done.`) and the document was written back normally. `COVERAGE-THRESHOLD-THROW` is
not recorded.

## Discovery-contamination branch

**Not taken.** The wrapper reports `Discovered 9 test assemblies.` It discovers by absolute
`FullName` and filters only on `\bin\<Configuration>\`, `\obj\`, and `\ref\`, applying no
agent-worktree exclusion. This execution worktree itself sits beneath a `.claude` segment, so every
absolute `FullName` contains `\.claude\`; the condition is therefore evaluated against the path
**expressed relative to `WS`**, and none of the nine relative paths contains a `.claude` segment.
The nine, relative to `WS`, are identical to the set `[P4-T6]` enumerated and to the `[P0-T20]`
baseline set:

```
.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
.\SVGControl.Test\bin\Debug\SVGControl.Test.dll
.\Tags.Test\bin\Debug\Tags.Test.dll
.\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
.\TaskTree.Test\bin\Debug\TaskTree.Test.dll
.\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
.\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
.\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
.\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

No nested worktree exists under `WS` (`.claude/worktrees` does not exist inside this worktree), so
no sibling checkout's stale binaries could be swept in. `CONTAMINATED-DISCOVERY:` is not recorded.

## Acceptance

- Both figures are numeric — met (`85.13`, `79.21`).
- No discovered assembly path expressed relative to `WS` contains a `.claude` segment — met.
