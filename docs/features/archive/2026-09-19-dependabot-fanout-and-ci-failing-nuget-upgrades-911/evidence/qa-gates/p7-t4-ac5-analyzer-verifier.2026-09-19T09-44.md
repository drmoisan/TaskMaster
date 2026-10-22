# P7-T4 — AC5: every analyzer item agrees with its manifest (#898)

Timestamp: 2026-09-20T01-24

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $r = & "<execution-worktree-root>\scripts\dependencies\Repair-PackageManifestConsistency.ps1" -WhatIf; foreach ($p in $r.Verification) { "PROJECT $($p.ProjectName) analyzer=$($p.ExaminedAnalyzerCount) disagree=$($p.DisagreementCount) absent=$($p.Report.AbsentFromManifestCount) segment=$($p.Report.MissingRoslynSegmentCount)" }'
```

EXIT_CODE: 0

The verifier is `scripts/dependencies/ConsistencyVerifier.psm1`; the command above runs it over the
working tree through the composition root, which supplies the restore-directory delegates the
detectors consume. `-WhatIf` is used so the report is produced without the run being able to write.

## Output Summary

```
TOTALS analyzerItems=162 analyzerProjects=17 projects=18 disagreements=0 absent=2 segment=0 orphan=7 repairs=0 success=True
```

## Acceptance conditions

| Condition | Observed | Result |
|---|---|---|
| Analyzer-item version disagreements | 0 | PASS |
| `<Analyzer Include>` items examined | 162 | PASS |
| Project files carrying analyzer items | 17 | PASS |
| Absent-from-manifest instances for `QuickFiler.Test/QuickFiler.Test.csproj` | 2 | PASS |
| Missing-Roslyn-segment instances | 0, over 162 examined | PASS |

The examined counts are the non-vacuity guard: a detector that matched nothing would report zero
disagreements and zero examined, and would fail this criterion. The disagreement figure is zero
over a population of 162 items across 17 files, and `disagreements=0` is the total across every
element kind the detector examines, of which the analyzer kind is one.

## Per-project report

| Project | Analyzer items | Disagreements | Absent from manifest | Missing segment | Orphaned hint paths |
|---|---|---|---|---|---|
| QuickFiler.Test.csproj | 11 | 0 | 2 | 0 | 4 |
| QuickFiler.csproj | 9 | 0 | 0 | 0 | 0 |
| SVGControl.Test.csproj | 2 | 0 | 0 | 0 | 0 |
| SVGControl.csproj | 0 | 0 | 0 | 0 | 0 |
| Tags.Test.csproj | 11 | 0 | 0 | 0 | 0 |
| Tags.csproj | 9 | 0 | 0 | 0 | 0 |
| TaskMaster.Test.csproj | 11 | 0 | 0 | 0 | 0 |
| TaskMaster.csproj | 9 | 0 | 0 | 0 | 0 |
| TaskTree.Test.csproj | 11 | 0 | 0 | 0 | 1 |
| TaskTree.csproj | 9 | 0 | 0 | 0 | 0 |
| TaskVisualization.Test.csproj | 11 | 0 | 0 | 0 | 0 |
| TaskVisualization.csproj | 9 | 0 | 0 | 0 | 0 |
| ToDoModel.Test.csproj | 11 | 0 | 0 | 0 | 0 |
| ToDoModel.csproj | 9 | 0 | 0 | 0 | 0 |
| UtilitiesCS.Test.csproj | 11 | 0 | 0 | 0 | 2 |
| UtilitiesCS.csproj | 9 | 0 | 0 | 0 | 0 |
| VBFunctions.Test.csproj | 11 | 0 | 0 | 0 | 0 |
| VBFunctions.csproj | 9 | 0 | 0 | 0 | 0 |
| **Total** | **162** | **0** | **2** | **0** | **7** |

Eighteen project files were examined and seventeen of them carry an analyzer item; `SVGControl`
carries none, which is the shape the plan's Measured Tree Facts record.

## The two non-fatal classes, both recorded and neither affecting this criterion

**Absent from manifest, exactly 2 instances, both in one file:**

```
ABSENT QuickFiler.Test.csproj kind=Import line=8   folder=altcover.8.6.45
ABSENT QuickFiler.Test.csproj kind=Import line=514 folder=altcover.8.6.45
```

Both are `Exists()`-guarded `<Import>` elements naming a package no manifest declares and that no
restore produces, so the build is unaffected. No exception is hard-coded for the identifier: the
class is derived from the manifest, and the import is reported because nothing declares it. The
class is counted and named, and produces no failure result. It is tracked as issue #912.

The count is exactly 2 rather than 6 because an orphaned `<HintPath>` is reported by its own class.
A `HintPath` whose package no manifest declares is necessarily orphaned as well, so the
absent-from-manifest class covers the remaining element kinds and each instance is counted once.
`QuickFiler.Test` carries 4 orphaned hint paths alongside the 2 imports; `TaskTree.Test` carries 1
and `UtilitiesCS.Test` 2, and none of those three files contributes to the absent-from-manifest
figure.

**Missing Roslyn segment, exactly 0 instances over 162 examined.** The zero is a measurement, not
an assumption. Each of the 162 items was compared against the path set the preserve rule derives
for that item's own package from the restored listing, and all 162 were found in their own
package's derived set. `packages/Meziantou.Analyzer.3.0.235` ships `roslyn5.0` and
`packages/Roslynator.Analyzers.5.0.0` ships `roslyn4.7`, which are the segments the committed items
name, so every preserved segment resolves.

This task checks off **AC5** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
