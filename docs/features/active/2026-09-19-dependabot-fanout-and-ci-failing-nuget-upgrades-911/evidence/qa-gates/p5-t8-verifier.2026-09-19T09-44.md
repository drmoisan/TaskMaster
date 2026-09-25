# P5-T8 — Verifier implemented in ConsistencyVerifier.psm1

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module "<execution-worktree-root>\scripts\dependencies\ConsistencyVerifier.psm1" -Force -ErrorAction Stop; Get-Command -Module ConsistencyVerifier; line counts of the three Batch C modules'
```

EXIT_CODE: 0

## Output Summary

```
IMPORT=ok
LINES=493
Find-OrphanedHintPath, Find-PackageAbsentFromManifest, Find-VersionDisagreement,
Get-ConsistencyFailureResult, Get-ConsistencyRepairsReport, Get-ExaminedElementCount,
Get-MissingRoslynSegmentFinding, Invoke-ProjectConsistencyRepair, Test-ReferenceCompleteness
OTHER ProjectConsistency.psm1 LINES=365
OTHER AnalyzerItemRepair.psm1 LINES=260
```

A behavioural smoke run of the acceptance suite at this point reported
`Passed=12 Failed=1 Total=13`. The single failure is the AC21 case, whose post-repair half
depends on `Invoke-AnalyzerItemRepair`, still a declared pass-through until P5-T12. Its
pre-repair half — the separate guard and analyzer disagreements — now passes.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| Module imports without error | yes | `IMPORT=ok`, run with `-ErrorAction Stop` |
| Exports the verifier | `Invoke-ProjectConsistencyRepair` | present |
| Exports the disagreement surface | `Find-VersionDisagreement` | present |
| Exports the orphaned `<HintPath>` surface | `Find-OrphanedHintPath` | present |
| Exports the reference-completeness surface | `Test-ReferenceCompleteness` | present |
| Exports the absent-from-manifest surface | `Find-PackageAbsentFromManifest` | present |
| Exports the missing-Roslyn-segment aggregator | `Get-MissingRoslynSegmentFinding` | present |
| Exports the report function | `Get-ConsistencyRepairsReport` | present |
| At most 500 lines | <= 500 | 493 |

All five detector or aggregator surfaces are present, plus the report function, the
failure-result constructor, the examined-count accessor and the entry point: nine exports.

## The 500-line ceiling was reached and resolved without dropping behaviour

The first implementation measured **644 lines** and the first compaction pass **598**. Two
changes brought it to 493, and neither removed behaviour or a test:

1. **The restore-path vocabulary moved to `AnalyzerItemRepair.psm1`.**
   `Get-RestorePackageFolder`, `Get-FolderPackageIdentity` and `Get-FolderBearingElement`,
   with the separator patterns they use, now live in the module whose entire subject is
   restore paths, and are exported from it. `ConsistencyVerifier.psm1` already imported
   that module for the analyzer repair, so no new dependency edge was created and no
   fourth production file was added — Batch C is at its cap of three and a new file would
   have breached it. `AnalyzerItemRepair.psm1` stands at 260 lines with the vocabulary
   included and before its own P5-T12 implementation.
2. **Comment-based help was shortened, and the two private helpers carry a leading comment
   rather than a help block.** Public function contracts remain documented with
   `.SYNOPSIS` and a `.PARAMETER` entry per parameter; the longer rationale that had been
   in the source is recorded here instead.

P5-T22's sanctioned remedy for an overrun is to move work to Batch D rather than reset the
batch state file. That remedy was not needed: no work moved out of the batch, and no task
deleted or reset `.claude/state/powershell-batch-budget.<session-id>.json`.

## Behaviour implemented

- **Disagreement detection.** Every dependent element carrying a package folder segment —
  `<Import>`, `<Error>`, `<HintPath>`, `<Analyzer Include>` — is compared against the
  manifest version for its package. Each finding carries its element kind, so the guard
  disagreement and the analyzer-item disagreement of the #908 three-way divergence are
  reported separately. The result carries `ExaminedCount` and `ExaminedAnalyzerCount`.
  `<Reference>` is excluded: its Include carries an assembly version, which is not
  required to track the package version, so a difference is not evidence of anything.
- **Orphaned `<HintPath>` detection**, with the examined hint-path count.
- **Reference completeness.** For each manifest package, for each library asset the
  injected asset provider resolves, a `<HintPath>` naming that asset under that package's
  folder and a `<Reference>` whose simple name matches the asset must both exist.
- **Absent-from-manifest detection**, the first non-fatal class. Reported for every
  dependent element kind, counted and named, never a failure. No package identifier is
  special-cased, so the live altcover instance in `QuickFiler.Test` is reported by the
  general rule rather than by an exception.
- **Missing-Roslyn-segment aggregation**, the second non-fatal class. The records are
  produced by `AnalyzerItemRepair.psm1`; this module aggregates, counts and names them and
  builds no derivation of its own. `-ExaminedItemCount` accompanies the records so a clean
  aggregation is distinguishable from an aggregation over nothing.
- **Per-project repairs report** and **failure-result constructor**.
- **Entry point.** Repairs freely and fails only on residual inconsistency, returning a
  failure result naming the condition — `MissingReference` or
  `ResidualVersionDisagreement` — and the project. Analyzer-item regeneration is applied
  only where a disagreement is detected for that package in that project. A disagreement on
  an item the repair deliberately left alone, because its preserved segment is absent, is
  excused rather than escalated: that class is non-fatal and guessing a path is prohibited.

## Nested imports are taken without -Force, and the reason is measured

The three modules import one another without `-Force`. With `-Force`, a nested
`Import-Module` removes the target module from the **whole session** before re-importing it
into the importing module's scope. Measured here: the acceptance suite imported
`ProjectConsistency.psm1` and then `ConsistencyVerifier.psm1`, and all six AC11 and AC14
cases then failed with `The term 'Invoke-VersionReconciliation' is not recognized`, because
loading the verifier had stripped the reconciliation module out of the caller's session.
Removing `-Force` from the three intra-module imports took the suite from `Failed=7` to
`Failed=1`. The remaining failure is the AC21 case awaiting P5-T12.
