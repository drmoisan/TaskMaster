# Pester Coverage After Phase 1 — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-46-19
- Task: [P1-T10]
- Finding: R2. **This is the pass-after half of the [P0-T8] fail-before.**
- Command: CMD-PESTER-ALL with `<OUTPATH>` = `coverage/p1-t10-pester-coverage.xml`, then
  CMD-JACOCO-PERFILE with `<LEAF>` = `Sync-PackageReferences.ps1`
- EXIT_CODE: 0

## Toolchain Re-Run Note

This gate was run twice, and the figures below are from the **second** run, against the tree as it
now stands.

The first run produced the same figures. [P1-T13] then reported 22 analyzer findings against the
baseline of 13: nine `PSReviewUnusedParameter` warnings in the seam delegates this phase added to
`tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, each for a `param()` the delegate
declared and never referenced. Each delegate was corrected to answer by its parameter — recording
the path it was asked for, or returning a different result for a different input — rather than by
deleting the parameter, so every assertion in the eight tests is unchanged and the fixtures became
more discriminating rather than less. Per the toolchain rule in `CLAUDE.md`, format, this coverage
gate and the analyzer were then re-run in order over the corrected file.

The second run reports `Passed=310 Failed=0`, report LINE `covered=1607 missed=95`, the Sync file
at `covered=104 missed=23`, and the identical `UNCOVERED=` list. Every figure below is therefore
the same in both runs.

## Counts Line, Verbatim

```
PESTER Passed=310 Failed=0 Skipped=0 Total=310
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Failed` | 0 | **0** | PASS |
| `Total` | exactly the [P0-T8] `Total` plus 8, so 302 + 8 = **310** | **310** | PASS |

The run is unfiltered, so `Total` and the executed population coincide and the `NotRun` caveat of
**gate rule 2** does not apply.

## The Nine Lines, Checked Individually

`UNCOVERED=` for `scripts/vscode/Sync-PackageReferences.ps1`, verbatim:

```
UNCOVERED=60,61,63,64,66,68,69,71,73,74,76,78,80,82,84,86,88,90,91,387,390,410,422
```

| Line | Discharging task | Present in `UNCOVERED=`? | Verdict |
|---|---|---|---|
| 151 | [P1-T2] | no | **covered** |
| 180 | [P1-T3] | no | **covered** |
| 248 | [P1-T4] | no | **covered** |
| 290 | [P1-T5] | no | **covered** |
| 293 | [P1-T6] | no | **covered** |
| 330 | [P1-T7] | no | **covered** |
| 336 | [P1-T8] | no | **covered** |
| 337 | [P1-T8] | no | **covered** |
| 345 | [P1-T9] | no | **covered** |

**Nine of nine covered.** Every one of the nine that [P0-T8] recorded as uncovered is absent from
this list. The 23 that remain are the 19 delegate-table lines, 60 through 91, and the 4 top-level
invocation lines 387, 390, 410 and 422 — exactly the two classes decision **D5** identified as
not reachable from a unit test.

## The File

| Measurement | Required | [P0-T8] | [P1-T10] | Result |
|---|---|---|---|---|
| Covered | at least 104 | 95 | **104** | PASS |
| Missed | — | 32 | 23 | — |
| Covered plus missed | still **127** | 127 | **127** | PASS |
| Percent | — | 74.80 | **81.89** | — |

The instrumented count is unchanged at 127, which is the invariance **gate rule 14** requires:
Phase 1 edited no production file, so the nine line citations of [P0-T8] and of the fail-before
dossier remain valid against the same file. A different instrumented count would have meant the
production file moved and would have invalidated every line citation in this phase.

## Per-File Comparison Against the [P0-T8] Baseline

| Source file | [P0-T8] covered | [P1-T10] covered | Not below baseline |
|---|---|---|---|
| `dependencies/AnalyzerItemRepair.psm1` | 106 | 106 | yes |
| `dependencies/ConsistencyVerifier.psm1` | 157 | 157 | yes |
| `dependencies/PackageCompatibility.psm1` | 33 | 33 | yes |
| `dependencies/PackageGraph.psm1` | 164 | 164 | yes |
| `dependencies/ProjectConsistency.psm1` | 88 | 88 | yes |
| `dependencies/Repair-PackageManifestConsistency.ps1` | 224 | 224 | yes |
| `vscode/Install-RepoDotNetSdk.ps1` | 13 | 13 | yes |
| `vscode/Invoke-MSTest.ps1` | 49 | 49 | yes |
| `vscode/Invoke-MSTest.TrxSummary.ps1` | 40 | 40 | yes |
| `vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | 93 | yes |
| `vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 32 | yes |
| `vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 204 | 204 | yes |
| `vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | 18 | yes |
| `vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 39 | 39 | yes |
| `vscode/Invoke-MSTestWithCoverage.ps1` | 113 | 113 | yes |
| `vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 33 | 33 | yes |
| `vscode/Invoke-Restore.ps1` | 22 | 22 | yes |
| `vscode/Invoke-VSBuild.ps1` | 46 | 46 | yes |
| `vscode/Sync-PackageReferences.ps1` | 95 | **104** | yes, `+9` |
| `vscode/TestProcessCleanup.ps1` | 29 | 29 | yes |

Twenty of twenty at or above baseline. Exactly one file moved, and it moved by exactly the nine
lines this phase targeted.

## Aggregate

| Measurement | [P0-T8] | [P1-T10] |
|---|---|---|
| Report LINE covered | 1598 | **1607** |
| Report LINE missed | 104 | **95** |
| Instrumented | 1702 | 1702 |
| Aggregate percent | 93.89 | **94.42** |

94.42 is at least 80, the authoritative floor, and also clears the superseded 85.

## Standing-In Statement, Gate Rule 12

The three permitted evidence forms for a coverage claim are defined against the C# Cobertura
pipeline. Pester emits JaCoCo and there is no Cobertura stage on the PowerShell route, so the
figures recorded in this artifact **stand in for** a permitted evidence form that does not exist
for that route. The collector document `coverage/p1-t10-pester-coverage.xml` is gitignored at
`.gitignore:144` and is deliberately not committed.

## Output Summary

310 passed, 0 failed, exit 0; `Total` is exactly the baseline 302 plus the 8 tests this phase
added. All nine target lines are now covered, each checked individually. The Sync file moves from
95 of 127 to 104 of 127, 74.80 to 81.89 percent, on an unchanged instrumented count. No other
measured file lost coverage. Aggregate line coverage rises from 93.89 to 94.42 percent.
