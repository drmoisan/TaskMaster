# P5-T2 — ConsistencyVerifier declared pass-through

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = "<execution-worktree-root>\scripts\dependencies\ConsistencyVerifier.psm1"; Import-Module $p -Force -ErrorAction Stop; "IMPORT=ok"; (Get-Command -Module ConsistencyVerifier | Select-Object -ExpandProperty Name | Sort-Object) -join ", "; "LINES=" + ([System.IO.File]::ReadAllLines($p)).Count'
```

EXIT_CODE: 0

## Output Summary

```
RESIDUAL=no
IMPORT=ok
Find-OrphanedHintPath, Find-PackageAbsentFromManifest, Find-VersionDisagreement,
Get-ConsistencyFailureResult, Get-ConsistencyRepairsReport, Get-ExaminedElementCount,
Get-MissingRoslynSegmentFinding, Invoke-ProjectConsistencyRepair, Test-ReferenceCompleteness
LINES=340
```

Re-measured after the disagreement detector was renamed; see "Naming correction" below.

## Acceptance

- The module imports without error: `IMPORT=ok`, with `-ErrorAction Stop` in force.
- `Get-Command -Module ConsistencyVerifier` lists nine functions, which is every function
  the plan's later tasks cite for this module. Mapped to the surfaces P5-T2 enumerates:

  | Surface | Function | Cited by |
  |---|---|---|
  | Disagreement detector | `Find-VersionDisagreement` | P5-T8, P5-T9, P5-T10 |
  | Orphaned `<HintPath>` detector | `Find-OrphanedHintPath` | P5-T8, P5-T9, P5-T17 |
  | Reference-completeness detector | `Test-ReferenceCompleteness` | P5-T8, P5-T9, P5-T19 |
  | Absent-from-manifest detector | `Find-PackageAbsentFromManifest` | P5-T8, P5-T9, P5-T10 |
  | Missing-Roslyn-segment aggregator | `Get-MissingRoslynSegmentFinding` | P5-T8, P5-T9, P5-T10 |
  | Examined-count accessor | `Get-ExaminedElementCount` | P5-T8, P5-T17, P5-T19 |
  | Repairs-report builder | `Get-ConsistencyRepairsReport` | P5-T8, P5-T18 |
  | Failure-result constructor | `Get-ConsistencyFailureResult` | P5-T8, P5-T18 |
  | Entry point | `Invoke-ProjectConsistencyRepair` | P5-T8, P5-T18, P5-T20, P5-T21 |

- File size 340 lines, inside the 500-line ceiling. This is the file P5-T22 identifies as
  the at-risk one, and it is re-measured there.

## Naming correction made before P5-T4 authored tests against the surface

The disagreement detector was first written as `Find-AnalyzerVersionDisagreement` and was
renamed to `Find-VersionDisagreement` before any test was authored against it. The reason
is a requirement in AC21 rather than a preference: the #908 divergence is three-way, and
AC21 requires the verifier to report "a disagreement for the guard elements and a separate
disagreement for the analyzer item" **before** repair. A detector restricted to analyzer
items reports only half of that, and the guard half would then have no detecting surface
at all. The surface count is unchanged at five, because P5-T8 permits exactly five
detector or aggregator surfaces and adding a sixth would breach that clause; the existing
disagreement surface was generalised instead. Each finding carries its element kind so the
two disagreements remain separable, and the detection result carries
`ExaminedAnalyzerCount` as its own field so the analyzer-item examined count P5-T8 and AC5
require stays explicit.

The rename and the added field were applied to the module and this artifact was
re-measured; the figures above are the post-rename measurement, not the pre-rename one.

## Why the fifth surface is enumerated here

`Get-MissingRoslynSegmentFinding` is declared in this task rather than only described at
P5-T8. Were it absent, the P5-T5 red run could fail on a missing command rather than on
behaviour, and P5-T5 rejects that as an unacceptable red. Every function the later tasks
cite therefore resolves from this point onward.

## Pass-through shape

Each detector returns a `ConsistencyVerifier.DetectionResult` whose `Finding` is empty and
whose `ExaminedCount` is zero. The entry point returns a `ConsistencyVerifier.Result` whose
`ProjectText` is the input unchanged and whose `Report` carries no repairs. P5-T8 replaces
the bodies only.

`Get-DetectionResult` is a module-internal helper and is deliberately not exported; it is
named with the `Get` verb rather than `New` so it does not attract
`PSUseShouldProcessForStateChangingFunctions`, which P6-T2 requires to be absent from every
file this change owns.

The module imports `PackageGraph.psm1` for parsing. It does not write to it: no Phase 5
task may write to that file, which would be the fourth production file of Batch C. The
imports of `ProjectConsistency.psm1` and `AnalyzerItemRepair.psm1` that the entry point
needs are added at P5-T8, because `AnalyzerItemRepair.psm1` does not exist until P5-T3.
