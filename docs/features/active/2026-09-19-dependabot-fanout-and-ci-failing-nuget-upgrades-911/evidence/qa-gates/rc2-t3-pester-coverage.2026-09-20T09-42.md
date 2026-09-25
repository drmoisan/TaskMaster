# In-Place Corrections Cycle — Pester With Coverage

- Timestamp: 2026-09-20T09-55-40
- Cycle: 2026-09-20T09-42 in-place corrections (R-C2-1 through R-C2-5)
- Command: CMD-PESTER-ALL with `<OUTPATH>` = `coverage/p-c2-pester-coverage.iter1.xml`, then
  CMD-JACOCO-PERFILE with `<LEAF>` = `Sync-PackageReferences.ps1`
- EXIT_CODE: 0
- ExpectedExitCode: 0

## Counts Line, Verbatim

```
PESTER Passed=320 Failed=0 Skipped=0 Total=320
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | **0** | PASS |
| `Failed` | 0 | **0** | PASS |
| `Skipped` | 0 | **0** | PASS |
| `Total` | the [P5-T3] `Total` of 318 plus the 2 tests this cycle adds | **320** | PASS |

The two added tests are named in the "Tests Added" section below. The run is unfiltered, so
`Total` and the executed population coincide.

## Report-Level LINE Counter

| Measurement | Value | Previous ([P5-T3]) | Required |
|---|---|---|---|
| Covered | **1613** | 1611 | — |
| Missed | **94** | 95 | — |
| Instrumented | 1707 | 1706 | — |
| **Aggregate line coverage** | **94.49 percent** | 94.43 | at or above 94.43 |

Instrumented rose by one because correction R-C2-3 replaced a single `Write-Verbose` statement
with a two-statement form: a string assignment and a `Write-Information` call.

## Per-File LINE Counters

| Source file | Covered | Missed | Percent | Previous covered | Required |
|---|---|---|---|---|---|
| `dependencies/AnalyzerItemRepair.psm1` | 106 | 0 | **100.00** | 106 | at least 90 |
| `dependencies/ConsistencyVerifier.psm1` | 158 | 2 | **98.75** | 158 | at least 90 |
| `dependencies/PackageCompatibility.psm1` | 33 | 0 | **100.00** | 33 | at least 90 |
| `dependencies/PackageGraph.psm1` | 164 | 0 | **100.00** | 164 | at least 90 |
| `dependencies/ProjectConsistency.psm1` | 103 | 0 | **100.00** | 103 | at least 90 |
| `dependencies/Repair-PackageManifestConsistency.ps1` | 213 | 14 | **93.83** | 212 | at least 90 |
| `vscode/Install-RepoDotNetSdk.ps1` | 13 | 20 | 39.39 | 13 | — |
| `vscode/Invoke-MSTest.ps1` | 49 | 7 | 87.50 | 49 | — |
| `vscode/Invoke-MSTest.TrxSummary.ps1` | 40 | 2 | 95.24 | 40 | — |
| `vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | 0 | 100.00 | 93 | — |
| `vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 1 | 96.97 | 32 | — |
| `vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 204 | 8 | 96.23 | 204 | — |
| `vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | 0 | 100.00 | 18 | — |
| `vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 39 | 1 | 97.50 | 39 | — |
| `vscode/Invoke-MSTestWithCoverage.ps1` | 113 | 13 | 89.68 | 113 | — |
| `vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 33 | 0 | 100.00 | 33 | — |
| `vscode/Invoke-Restore.ps1` | 22 | 1 | 95.65 | 22 | — |
| `vscode/Invoke-VSBuild.ps1` | 46 | 3 | 93.88 | 46 | — |
| **`vscode/Sync-PackageReferences.ps1`** | **105** | 22 | **82.68** | 104 | at or above 81.89 |
| `vscode/TestProcessCleanup.ps1` | 29 | 0 | 100.00 | 29 | — |

No file's covered count fell. All six `scripts/dependencies/` files remain at or above 90, the
lowest being 93.83.

## The Sync File

| Clause | Required | Measured | Result |
|---|---|---|---|
| Percent | at or above 81.89 | **82.68** | PASS |
| Covered count | at least 104 | **105** | PASS |
| Covered plus missed | 127, unchanged | **127** | PASS |

`UNCOVERED=` verbatim:

```
UNCOVERED=60,61,63,64,66,68,69,71,73,74,76,78,80,82,84,86,88,90,91,387,390,422
```

Line **410 is absent**, where the [P5-T3] list contained it. The 22 that remain are the 19
delegate-table lines 60 through 91 and the 3 top-level invocation lines 387, 390 and 422 — the
two classes decision **D5** identified as unreachable from a unit test. 82.68 percent is
therefore the ceiling under the existing seam, not merely an improvement on 81.89.

The file still sits below the 85 percent floor in `.claude/rules/general-unit-test.md` and above
the 80 percent floor in `CLAUDE.md`. That conflict is open issue #668 and is unchanged by this
cycle; only the distance to the stricter reading narrowed.

## Tests Added

| # | File | Test name | Drives |
|---|---|---|---|
| 1 | `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` | `R5- preserves the declared version for an Include whose case differs from the manifest identifier` | `Resolve-ReferenceAssemblyVersion` then `Invoke-VersionReconciliation` |
| 2 | `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | `R2- reports the up-to-date outcome for the whole run when every hint path already resolves` | `Invoke-PackageReferenceSync`, line 410 |

Test 1 was observed **failing before** correction R-C2-2 was applied, on the assertion
`$assemblyVersion | Should -BeExactly '1.0.2'`, with the actual value the empty string — the
exact mechanism the finding predicts. It passes after.

## Correction to a Statement in the Review

The code review and the remediation inputs both describe line 410 as "the non-zero-fix summary
branch". It is not. Line 410 is the `else` arm, `Write-Information 'Sync-PackageReferences: All
HintPaths are up to date'`. The non-zero arm is line 407, and it was **already covered** before
this cycle by the end-to-end repair test, which asserts a `FixedCount` of 1; neither 406 nor 407
appears in the [P5-T3] `UNCOVERED=` list. Had the recommendation been followed as written — "add
one test that supplies a seam producing at least one hint-path repair" — the new test would have
duplicated an existing one and line 410 would have stayed uncovered at 104 of 127.

The test written instead drives the zero-fix aggregate outcome, which is what line 410 is, and
it reaches the coverage figure the review predicted. The line number in the finding was correct;
its description of that line was not.

## Standing-In Statement, Gate Rule 12

The three permitted evidence forms for a coverage claim are defined against the C# Cobertura
pipeline. Pester emits JaCoCo and there is no Cobertura stage on the PowerShell route, so the
figures recorded in this artifact **stand in for** a permitted evidence form that does not exist
for that route. The collector document `coverage/p-c2-pester-coverage.iter1.xml` is gitignored at
`.gitignore:144` and is deliberately not committed.

## Output Summary

320 passed, 0 failed, 0 skipped, exit 0. Aggregate line coverage 94.49 percent, above the
previous 94.43. `Sync-PackageReferences.ps1` at 82.68 percent with 105 of 127 covered, above the
previous 81.89, with line 410 now covered and the remaining 22 confirmed unreachable from a unit
test. No measured file's covered count fell. Step 3 of the loop passes.
