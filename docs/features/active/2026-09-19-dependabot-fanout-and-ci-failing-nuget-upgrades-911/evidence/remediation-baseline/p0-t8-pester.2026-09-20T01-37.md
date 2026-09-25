# Pester Baseline with Coverage — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-32-43
- Task: [P0-T8]
- Finding: R2. **This artifact is the fail-before evidence for R2.**
- Command: CMD-PESTER-ALL with `<OUTPATH>` = `coverage/p0-t8-pester-coverage.xml`, then
  CMD-JACOCO-PERFILE with `<DOC>` = that path and `<LEAF>` = `Sync-PackageReferences.ps1`
- EXIT_CODE: 0

## Counts Line, Verbatim

```
PESTER Passed=302 Failed=0 Skipped=0 Total=302
```

`Failed=0`. `Total = 302`, at or above the required 302. This run is **unfiltered**, so `Total` and
the executed population coincide and no `NotRun` caveat applies.

## Report-Level LINE Counter

| Measurement | Value |
|---|---|
| Covered | **1598** |
| Missed | **104** |
| Instrumented | 1702 |
| Aggregate line coverage | **93.89 percent** |

93.89 clears the authoritative floor of 80 and also clears the superseded 85.

## Per-File LINE Counters

The seven files the review measured per file are marked. All 20 measured files are listed, because
[P1-T10], [P2-T7], [P3-T11] and [P5-T3] each assert that no per-file covered count fell below the
value recorded here, and that assertion needs every file's baseline, not only the seven.

| Source file | Covered | Missed | Instrumented | Percent | Review's seven |
|---|---|---|---|---|---|
| `dependencies/AnalyzerItemRepair.psm1` | 106 | 0 | 106 | 100.00 | yes |
| `dependencies/ConsistencyVerifier.psm1` | 157 | 2 | 159 | 98.74 | yes |
| `dependencies/PackageCompatibility.psm1` | 33 | 0 | 33 | 100.00 | yes |
| `dependencies/PackageGraph.psm1` | 164 | 0 | 164 | 100.00 | yes |
| `dependencies/ProjectConsistency.psm1` | 88 | 0 | 88 | 100.00 | yes |
| `dependencies/Repair-PackageManifestConsistency.ps1` | 224 | 14 | 238 | 94.12 | yes |
| `vscode/Sync-PackageReferences.ps1` | **95** | **32** | **127** | **74.80** | yes |
| `vscode/Install-RepoDotNetSdk.ps1` | 13 | 20 | 33 | 39.39 | no |
| `vscode/Invoke-MSTest.ps1` | 49 | 7 | 56 | 87.50 | no |
| `vscode/Invoke-MSTest.TrxSummary.ps1` | 40 | 2 | 42 | 95.24 | no |
| `vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | 0 | 93 | 100.00 | no |
| `vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 1 | 33 | 96.97 | no |
| `vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 204 | 8 | 212 | 96.23 | no |
| `vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | 0 | 18 | 100.00 | no |
| `vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 39 | 1 | 40 | 97.50 | no |
| `vscode/Invoke-MSTestWithCoverage.ps1` | 113 | 13 | 126 | 89.68 | no |
| `vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 33 | 0 | 33 | 100.00 | no |
| `vscode/Invoke-Restore.ps1` | 22 | 1 | 23 | 95.65 | no |
| `vscode/Invoke-VSBuild.ps1` | 46 | 3 | 49 | 93.88 | no |
| `vscode/TestProcessCleanup.ps1` | 29 | 0 | 29 | 100.00 | no |

The Sync file's covered plus missed is **127**, the expected instrumented count. That figure is the
invariant Phase 1 preserves: Phase 1 edits no production file, so a later run reporting a different
instrumented count would mean the production file moved and every line citation in this phase would
be void.

## `UNCOVERED=` for `scripts/vscode/Sync-PackageReferences.ps1`, Verbatim

```
UNCOVERED=60,61,63,64,66,68,69,71,73,74,76,78,80,82,84,86,88,90,91,151,180,248,290,293,330,336,337,345,387,390,410,422
```

Thirty-two members, which is the missed count. They partition exactly as decision **D5** states:

| Class | Lines | Count |
|---|---|---|
| `Get-PackageSyncSeam` delegate table | 60, 61, 63, 64, 66, 68, 69, 71, 73, 74, 76, 78, 80, 82, 84, 86, 88, 90, 91 | 19 |
| **Pure logic, every one a negative or error path** | **151, 180, 248, 290, 293, 330, 336, 337, 345** | **9** |
| Top-level invocation | 387, 390, 410, 422 | 4 |
| Total | | 32 |

## Fail-Before Containment Check for R2

Each of the nine checked individually against the list above.

| Line | Owning function | Behaviour | Present in `UNCOVERED=`? |
|---|---|---|---|
| 151 | `Resolve-ManifestPackageId` | `return ''` when no manifest key prefixes the folder | **yes** |
| 180 | `Resolve-PackageAssetFolder` | `return ''` when the library directory is absent | **yes** |
| 248 | `Get-HintPathRepair` | the #902 rejection warning | **yes** |
| 290 | `Repair-ProjectReferenceVersion` | early return, no matching Include | **yes** |
| 293 | `Repair-ProjectReferenceVersion` | early return, version already agrees | **yes** |
| 330 | `Invoke-ProjectReferenceSync` | no project file beside the manifest | **yes** |
| 336 | `Invoke-ProjectReferenceSync` | conflict-marker warning | **yes** |
| 337 | `Invoke-ProjectReferenceSync` | the corresponding skip return | **yes** |
| 345 | `Invoke-ProjectReferenceSync` | empty repair set | **yes** |

**Nine of nine present.** This is the fail-before evidence for R2: it is true today, and every one
of the nine must be **absent** from the same list at [P1-T10].

The owning function names above are the **real** ones, read from the file. The review named
`Get-PackageIdentifier` for line 151 and `Set-ReferenceAssemblyVersion` for lines 290 and 293;
neither identifier exists in the file. The line numbers the review gave are correct and are
confirmed by this run.

## Standing-In Statement, Gate Rule 12

The three permitted evidence forms for a coverage claim are all defined against the C# Cobertura
pipeline: Pester emits JaCoCo and there is no Cobertura stage on the PowerShell route. The figures
recorded in this artifact therefore **stand in for** a permitted evidence form that does not exist
for the Pester route. The collector document itself is `coverage/p0-t8-pester-coverage.xml`, which
`.gitignore:144` ignores and which is deliberately not committed; the figures this task asserts are
projected into this `.md` artifact instead.

## Output Summary

302 passed, 0 failed, 0 skipped, exit 0. Aggregate line coverage 93.89 percent, 1598 of 1702.
`scripts/vscode/Sync-PackageReferences.ps1` at 74.80 percent, 95 of 127, with all nine target logic
lines uncovered. The nine-member containment holds, which establishes the fail-before state for R2.
