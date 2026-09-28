# Pester Coverage After Phase 2 — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-53-05
- Task: [P2-T7]
- Finding: R5
- Command: CMD-PESTER-ALL with `<OUTPATH>` = `coverage/p2-t7-pester-coverage.xml`
- EXIT_CODE: 0

## Counts Line, Verbatim

```
PESTER Passed=312 Failed=0 Skipped=0 Total=312
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Failed` | 0 | **0** | PASS |
| `Total` | exactly the [P1-T10] `Total` plus 2, so 310 + 2 = **312** | **312** | PASS |

The two added tests are [P2-T2]'s `R5- preserves a Reference assembly version the package version
does not track` and [P2-T6]'s `R9c- records the enumerated directory count in the default manifest
lister`.

## Report-Level LINE Counter

| Measurement | [P1-T10] | [P2-T7] |
|---|---|---|
| Covered | 1607 | **1611** |
| Missed | 95 | **95** |
| Instrumented | 1702 | **1706** |
| Aggregate percent | 94.42 | **94.43** |

94.43 is at least 80. The instrumented total rose by 4 because Phase 2 added executable lines: one
resolver call in `ConsistencyVerifier.psm1` and the verbose record in the composition root, net of
the function that moved between two measured files.

## Per-File LINE Counters

| Source file | Covered | Missed | Percent | At least 90 required | Result |
|---|---|---|---|---|---|
| `dependencies/AnalyzerItemRepair.psm1` | 106 | 0 | **100.00** | yes | PASS |
| `dependencies/ConsistencyVerifier.psm1` | 158 | 2 | **98.75** | yes | PASS |
| `dependencies/PackageCompatibility.psm1` | 33 | 0 | **100.00** | yes | PASS |
| `dependencies/PackageGraph.psm1` | 164 | 0 | **100.00** | yes | PASS |
| `dependencies/ProjectConsistency.psm1` | 103 | 0 | **100.00** | yes | PASS |
| `dependencies/Repair-PackageManifestConsistency.ps1` | 212 | 14 | **93.81** | yes | PASS |
| `vscode/Install-RepoDotNetSdk.ps1` | 13 | 20 | 39.39 | no | — |
| `vscode/Invoke-MSTest.ps1` | 49 | 7 | 87.50 | no | — |
| `vscode/Invoke-MSTest.TrxSummary.ps1` | 40 | 2 | 95.24 | no | — |
| `vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | 0 | 100.00 | no | — |
| `vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 1 | 96.97 | no | — |
| `vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 204 | 8 | 96.23 | no | — |
| `vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | 0 | 100.00 | no | — |
| `vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 39 | 1 | 97.50 | no | — |
| `vscode/Invoke-MSTestWithCoverage.ps1` | 113 | 13 | 89.68 | no | — |
| `vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 33 | 0 | 100.00 | no | — |
| `vscode/Invoke-Restore.ps1` | 22 | 1 | 95.65 | no | — |
| `vscode/Invoke-VSBuild.ps1` | 46 | 3 | 93.88 | no | — |
| `vscode/Sync-PackageReferences.ps1` | **104** | 23 | **81.89** | no | — |
| `vscode/TestProcessCleanup.ps1` | 29 | 0 | 100.00 | no | — |

All **six** files under `scripts/dependencies/` are at or above 90, which is this change's own
stricter local requirement under AC24 and is independent of the 80 floor. The lowest is
`Repair-PackageManifestConsistency.ps1` at 93.81.

| Clause | Required | Measured | Result |
|---|---|---|---|
| `vscode/Sync-PackageReferences.ps1` covered | at least the [P1-T10] value of 104 | **104** | PASS |

## The Function Move Did Not Drop Either File Below 90

This is the failure mode the acceptance is written against: moving a 35-line function between two
measured files could have dropped either below 90.

| File | [P1-T10] covered / instrumented | [P2-T7] covered / instrumented | Percent |
|---|---|---|---|
| `ProjectConsistency.psm1` | 88 / 88 | **103 / 103** | 100.00, unchanged |
| `Repair-PackageManifestConsistency.ps1` | 224 / 238 | **212 / 226** | 94.12 to **93.81** |
| `ConsistencyVerifier.psm1` | 157 / 159 | **158 / 160** | 98.74 to **98.75** |

The function arrived in `ProjectConsistency.psm1` fully covered, so that file stays at 100 percent
on a larger denominator. It left `Repair-PackageManifestConsistency.ps1` fully covered too, so
that file's percentage moves down by 0.31 points: its 14 uncovered lines are now a slightly larger
share of a 12-line-smaller denominator. 93.81 clears 90 with 3.81 points to spare.
`ConsistencyVerifier.psm1` gained one covered line, the new resolver call.

## Standing-In Statement, Gate Rule 12

The three permitted evidence forms for a coverage claim are defined against the C# Cobertura
pipeline. Pester emits JaCoCo and there is no Cobertura stage on the PowerShell route, so the
figures recorded in this artifact **stand in for** a permitted evidence form that does not exist
for that route. The collector document `coverage/p2-t7-pester-coverage.xml` is gitignored at
`.gitignore:144` and is deliberately not committed.

## Output Summary

312 passed, 0 failed, exit 0; `Total` is exactly the [P1-T10] 310 plus the 2 tests this phase
added. Aggregate line coverage 94.43 percent. All six `scripts/dependencies/` files at or above
90, the lowest being 93.81. `Sync-PackageReferences.ps1` holds at 104 covered.
