# P7-T1 — Composition root created and proven inert under -WhatIf

Timestamp: 2026-09-20T00-48

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $before = @(git status --porcelain --untracked-files=all) -join "`n"; $r = & "<execution-worktree-root>\scripts\dependencies\Repair-PackageManifestConsistency.ps1" -WhatIf; $after = @(git status --porcelain --untracked-files=all) -join "`n"; "IDENTICAL: $($before -ceq $after)"'
```

EXIT_CODE: 0

## Output Summary

The composition root `scripts/dependencies/Repair-PackageManifestConsistency.ps1` was created with
the `Write` tool and ran to completion under `-WhatIf` against the working tree. The porcelain
capture taken immediately before the run is byte-identical to the one taken immediately after it, so
the run modified no file. `WrittenPath` on the returned record is empty, which is the script's own
statement of the same fact.

## Acceptance conditions

| Condition | Observed | Result |
|---|---|---|
| Declares `[CmdletBinding(SupportsShouldProcess = $true)]` | line 56, verbatim `[CmdletBinding(SupportsShouldProcess = $true)]` | PASS |
| Imports all five modules from `scripts/dependencies/` | `PackageGraph.psm1`, `PackageCompatibility.psm1`, `AnalyzerItemRepair.psm1`, `ProjectConsistency.psm1`, `ConsistencyVerifier.psm1` | PASS |
| At most 500 lines | 493 | PASS |
| `-WhatIf` run completes | `EXIT_CODE: 0`, `IsSuccess: True` | PASS |
| Porcelain captures byte-identical | `IDENTICAL: True` | PASS |

The empty post-run capture is deliberately not asserted: the tree carries uncommitted evidence and
the plan's own check-off edits at this point, so it is non-empty in both captures.

## Import declarations, verbatim

```
[CmdletBinding(SupportsShouldProcess = $true)]
Import-Module (Join-Path $PSScriptRoot 'PackageGraph.psm1')
Import-Module (Join-Path $PSScriptRoot 'PackageCompatibility.psm1')
Import-Module (Join-Path $PSScriptRoot 'AnalyzerItemRepair.psm1')
Import-Module (Join-Path $PSScriptRoot 'ProjectConsistency.psm1')
Import-Module (Join-Path $PSScriptRoot 'ConsistencyVerifier.psm1')
```

## Porcelain capture, identical before and after

```
 M docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/other/p6-t7-batch-c-boundary.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p6-t6-commit.2026-09-19T09-44.md
?? scripts/dependencies/Repair-PackageManifestConsistency.ps1
```

## Figures the `-WhatIf` run reported

| Figure | Value |
|---|---|
| `ExaminedProjectCount` | 18 |
| `ExaminedElementCount` | 2559 |
| `ExaminedAnalyzerItemCount` | 162 |
| `AnalyzerProjectCount` | 17 |
| `RepairCount` | 0 |
| `VersionDisagreementCount` | 0 |
| `AbsentFromManifestCount` | 2 |
| `MissingRoslynSegmentCount` | 0 |
| `OrphanedHintPathCount` | 7 |
| `ExaminedManifestCount` | 18 |
| `ExaminedAppConfigCount` | 17 |
| `WrittenPath` count | 0 |

`Body` carried the `## Repairs applied` block reading `No repairs were applied.` and carried no
`## Packages skipped` block, the run having recorded no skip.

## Design decisions taken inside this task, with the measurement behind each

1. **The composition root wires the module functions directly rather than calling
   `Invoke-ProjectConsistencyRepair`.** That entry point calls `Invoke-VersionReconciliation`
   without `-AssemblyVersion`, whose documented fallback is the manifest version. Measured over the
   working tree, that fallback rewrites a `<Reference>` assembly version to the package version
   wherever the Include's simple name equals the package identifier: 51 such rewrites in
   `QuickFiler.csproj` alone, for example
   `Apache.Arrow, Version=23.0.0.0` to `Version=23.0.0` and
   `Microsoft.Data.Analysis, Version=1.0.0.0` to `Version=0.23.0`. An assembly version is not
   required to track its package version, so the module's own help states that the caller supplies
   the resolved value. Resolving it is filesystem work and therefore belongs in the composition
   root.

2. **A declared `<Reference>` assembly version is confirmed, not selected.** For each package, the
   restored directory is enumerated for an assembly named for the package and the versions those
   assemblies declare are collected. A declared version present in that set is preserved; a version
   absent from it is rewritten to the version in the asset folder `Select-CompatibleAssetFolder`
   picks. Measured over the working tree: 796 of 796 declared Reference versions are confirmed by
   some assembly their package ships, so the rule is a no-op here and remains falsifiable — a
   Reference naming a version its package ships nowhere is still rewritten. A first draft that
   selected the compatible folder's assembly outright disagreed with 9 declared versions and would
   have rewritten them.

3. **The missing-Roslyn-segment class is measured over every analyzer item, not only over items the
   repair pass touched.** Each of the 162 items is compared against the path set
   `Get-AnalyzerAssemblyPath` derives for its own package from the restored listing. All 162 are
   confirmed, so the class is 0 over an examined population of 162 rather than 0 over 0.

4. **The absent-from-manifest class excludes the `HintPath` kind.** An orphaned `<HintPath>` is
   reported by its own class; a `HintPath` whose package no manifest declares is necessarily
   orphaned as well, so counting it in both classes inflates both figures. With the exclusion the
   class reports exactly the 2 `Exists()`-guarded altcover `<Import>` elements in
   `QuickFiler.Test/QuickFiler.Test.csproj`, and the orphaned-hint-path class reports the 7
   `<HintPath>` entries separately.

5. **Binding-redirect reconciliation is scoped to the packages the run upgraded.** See the finding
   recorded in `p7-t5-ac15-repair-idempotence.2026-09-19T09-44.md`.

6. **The identity delegate searches the package's library directory and memoises its answer.** An
   unrestricted search also matched analyzer and tooling copies of a same-named assembly, which
   offer versions a Reference never resolves. Re-measured after the restriction: 796 of 796
   declared versions still confirmed, so the restriction narrows the evidence without losing a
   confirmation. The figures in the table above were re-taken after this amendment and the run
   remained inert, `IDENTICAL: True`.
