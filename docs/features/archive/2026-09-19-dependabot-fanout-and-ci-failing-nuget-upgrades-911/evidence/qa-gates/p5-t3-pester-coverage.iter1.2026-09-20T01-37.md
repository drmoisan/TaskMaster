# Final QA Step 3 — Pester With Coverage, Iteration 1

- Timestamp: 2026-09-20T09-09-38
- Task: [P5-T3]
- Finding: R2
- Command: CMD-PESTER-ALL with `<OUTPATH>` = `coverage/p5-t3-pester-coverage.iter1.xml`, then
  CMD-JACOCO-PERFILE with `<LEAF>` = `Sync-PackageReferences.ps1`
- EXIT_CODE: 0

## Counts Line, Verbatim

```
PESTER Passed=318 Failed=0 Skipped=0 Total=318
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | **0** | PASS |
| `Failed` | 0 | **0** | PASS |
| `Skipped` | 0 | **0** | PASS |
| `Total` | equals the [P3-T11] `Total` of **318** | **318** | PASS |

`Total` is unchanged from [P3-T11] because Phase 4 added no test; it sanitised markdown only.

## Report-Level LINE Counter

| Measurement | Value | Required |
|---|---|---|
| Covered | **1611** | — |
| Missed | **95** | — |
| Instrumented | 1706 | — |
| **Aggregate line coverage** | **94.43 percent** | at least 80 |

## Per-File LINE Counters

| Source file | Covered | Missed | Percent | Required |
|---|---|---|---|---|
| `dependencies/AnalyzerItemRepair.psm1` | 106 | 0 | **100.00** | at least 90 |
| `dependencies/ConsistencyVerifier.psm1` | 158 | 2 | **98.75** | at least 90 |
| `dependencies/PackageCompatibility.psm1` | 33 | 0 | **100.00** | at least 90 |
| `dependencies/PackageGraph.psm1` | 164 | 0 | **100.00** | at least 90 |
| `dependencies/ProjectConsistency.psm1` | 103 | 0 | **100.00** | at least 90 |
| `dependencies/Repair-PackageManifestConsistency.ps1` | 212 | 14 | **93.81** | at least 90 |
| `vscode/Install-RepoDotNetSdk.ps1` | 13 | 20 | 39.39 | — |
| `vscode/Invoke-MSTest.ps1` | 49 | 7 | 87.50 | — |
| `vscode/Invoke-MSTest.TrxSummary.ps1` | 40 | 2 | 95.24 | — |
| `vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | 0 | 100.00 | — |
| `vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 1 | 96.97 | — |
| `vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 204 | 8 | 96.23 | — |
| `vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | 0 | 100.00 | — |
| `vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 39 | 1 | 97.50 | — |
| `vscode/Invoke-MSTestWithCoverage.ps1` | 113 | 13 | 89.68 | — |
| `vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 33 | 0 | 100.00 | — |
| `vscode/Invoke-Restore.ps1` | 22 | 1 | 95.65 | — |
| `vscode/Invoke-VSBuild.ps1` | 46 | 3 | 93.88 | — |
| **`vscode/Sync-PackageReferences.ps1`** | **104** | 23 | **81.89** | at least 80, covered at least 104 |
| `vscode/TestProcessCleanup.ps1` | 29 | 0 | 100.00 | — |

All **six** `scripts/dependencies/` files are at or above 90, the lowest being 93.81.

## The Sync File

| Clause | Required | Measured | Result |
|---|---|---|---|
| Percent | at least 80 | **81.89** | PASS |
| Covered count | at least 104 | **104** | PASS |

## The Nine Lines Are Still Covered

`UNCOVERED=` verbatim:

```
UNCOVERED=60,61,63,64,66,68,69,71,73,74,76,78,80,82,84,86,88,90,91,387,390,410,422
```

| Line | Present in `UNCOVERED=`? | Verdict |
|---|---|---|
| 151 | no | **covered** |
| 180 | no | **covered** |
| 248 | no | **covered** |
| 290 | no | **covered** |
| 293 | no | **covered** |
| 330 | no | **covered** |
| 336 | no | **covered** |
| 337 | no | **covered** |
| 345 | no | **covered** |

**None of the nine is present.** The 23 that remain are the 19 delegate-table lines 60 through
91 and the 4 top-level invocation lines 387, 390, 410 and 422, the two classes decision **D5**
identified as unreachable from a unit test.

This is the final confirmation of the R2 discharge, taken after every later phase's edits, and
it matches [P1-T10] exactly.

## Standing-In Statement, Gate Rule 12

The three permitted evidence forms for a coverage claim are defined against the C# Cobertura
pipeline. Pester emits JaCoCo and there is no Cobertura stage on the PowerShell route, so the
figures recorded in this artifact **stand in for** a permitted evidence form that does not exist
for that route. The collector document `coverage/p5-t3-pester-coverage.iter1.xml` is gitignored
at `.gitignore:144` and is deliberately not committed.

## Output Summary

318 passed, 0 failed, 0 skipped, exit 0. Aggregate line coverage 94.43 percent. All six
`scripts/dependencies/` files at or above 90. `Sync-PackageReferences.ps1` at 81.89 percent with
104 covered, and all nine target logic lines still covered. Step 3 of the final loop passes.
