# Pester Coverage After Phase 3 — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-01-40
- Task: [P3-T11]
- Findings: R3, R6, R7, R8
- Command: CMD-PESTER-ALL with `<OUTPATH>` = `coverage/p3-t11-pester-coverage.xml`
- EXIT_CODE: 0

## Counts Line, Verbatim

```
PESTER Passed=318 Failed=0 Skipped=0 Total=318
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Failed` | 0 | **0** | PASS |
| `Total` | exactly the [P2-T7] `Total` plus 6, so 312 + 6 = **318** | **318** | PASS |

The six added tests are the four [P3-T1] workflow assertions, [P3-T3]'s write-set assertion and
[P3-T5]'s idempotence assertion.

## Report-Level LINE Counter

| Measurement | [P2-T7] | [P3-T11] |
|---|---|---|
| Covered | 1611 | **1611** |
| Missed | 95 | **95** |
| Instrumented | 1706 | **1706** |
| Aggregate percent | 94.43 | **94.43** |

94.43 is at least 80. The figures are unchanged because Phase 3 edited no measured PowerShell
production file: its production edits are all in
`.github/workflows/dependabot-repair.yml`, which is YAML and is not in the coverage scope.

## Per-File LINE Counters and the Baseline Comparison

| Source file | [P2-T7] covered | [P3-T11] covered | Not below | Percent | At least 90 required |
|---|---|---|---|---|---|
| `dependencies/AnalyzerItemRepair.psm1` | 106 | **106** | yes | 100.00 | PASS |
| `dependencies/ConsistencyVerifier.psm1` | 158 | **158** | yes | 98.75 | PASS |
| `dependencies/PackageCompatibility.psm1` | 33 | **33** | yes | 100.00 | PASS |
| `dependencies/PackageGraph.psm1` | 164 | **164** | yes | 100.00 | PASS |
| `dependencies/ProjectConsistency.psm1` | 103 | **103** | yes | 100.00 | PASS |
| `dependencies/Repair-PackageManifestConsistency.ps1` | 212 | **212** | yes | 93.81 | PASS |
| `vscode/Install-RepoDotNetSdk.ps1` | 13 | 13 | yes | 39.39 | n/a |
| `vscode/Invoke-MSTest.ps1` | 49 | 49 | yes | 87.50 | n/a |
| `vscode/Invoke-MSTest.TrxSummary.ps1` | 40 | 40 | yes | 95.24 | n/a |
| `vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | 93 | yes | 100.00 | n/a |
| `vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 32 | yes | 96.97 | n/a |
| `vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 204 | 204 | yes | 96.23 | n/a |
| `vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | 18 | yes | 100.00 | n/a |
| `vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 39 | 39 | yes | 97.50 | n/a |
| `vscode/Invoke-MSTestWithCoverage.ps1` | 113 | 113 | yes | 89.68 | n/a |
| `vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 33 | 33 | yes | 100.00 | n/a |
| `vscode/Invoke-Restore.ps1` | 22 | 22 | yes | 95.65 | n/a |
| `vscode/Invoke-VSBuild.ps1` | 46 | 46 | yes | 93.88 | n/a |
| `vscode/Sync-PackageReferences.ps1` | 104 | **104** | yes | 81.89 | n/a |
| `vscode/TestProcessCleanup.ps1` | 29 | 29 | yes | 100.00 | n/a |

Twenty of twenty at or above the [P2-T7] value. All six `scripts/dependencies/` files at or
above 90, the lowest being 93.81.

## Why the Coverage Figures Did Not Move

Phase 3 added six tests and changed no measured production line. Five of the six tests are
**text assertions over the workflow file**; the sixth, [P3-T3]'s write-set assertion, drives the
composition root over a fixture whose code paths the existing suite already covered.

That is the expected result and is recorded rather than passed over: a phase whose production
change is entirely YAML cannot move a PowerShell line-coverage figure. The evidence that Phase 3
did something is in [P3-T8]'s four red-to-green pairs and in `Total` rising by exactly 6, not
here.

## Standing-In Statement, Gate Rule 12

The three permitted evidence forms for a coverage claim are defined against the C# Cobertura
pipeline. Pester emits JaCoCo and there is no Cobertura stage on the PowerShell route, so the
figures recorded in this artifact **stand in for** a permitted evidence form that does not exist
for that route. The collector document `coverage/p3-t11-pester-coverage.xml` is gitignored at
`.gitignore:144` and is deliberately not committed.

## Output Summary

318 passed, 0 failed, exit 0; `Total` is exactly the [P2-T7] 312 plus the 6 tests this phase
added. Aggregate line coverage 94.43 percent, unchanged. Every measured file at or above its
[P2-T7] covered count, and all six `scripts/dependencies/` files at or above 90.
