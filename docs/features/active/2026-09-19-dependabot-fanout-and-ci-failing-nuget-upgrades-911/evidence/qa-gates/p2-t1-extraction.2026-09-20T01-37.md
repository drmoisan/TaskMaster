# R5 Step 1 — `Resolve-ReferenceAssemblyVersion` Extracted Into the Reconciliation Module

- Timestamp: 2026-09-20T08-48-39
- Task: [P2-T1]
- Finding: R5, decision D1
- EXIT_CODE: 0

## What Was Done

`Resolve-ReferenceAssemblyVersion` was moved **verbatim** out of
`scripts/dependencies/Repair-PackageManifestConsistency.ps1`, where it was a private function of a
498-line script R9d declares at capacity, and into `scripts/dependencies/ProjectConsistency.psm1`,
immediately ahead of `Invoke-VersionReconciliation`.

The function body, its parameter block and its comment-based help are byte-identical to the
originals. Four supporting edits accompany the move:

1. `Import-Module (Join-Path $PSScriptRoot 'PackageCompatibility.psm1')` added beside the module's
   two existing imports, with a comment naming `Select-CompatibleAssetFolder` as the reason.
   `PackageCompatibility.psm1` imports nothing, so no cycle is created.
2. `Resolve-ReferenceAssemblyVersion` added to the module's `Export-ModuleMember` list.
3. `Resolve-ReferenceAssemblyVersion` added to the header `Exported functions:` list.
4. The function and its preceding blank line removed from the composition root.

The edits were made with the `Edit` tool. **`sed` through the Bash tool was not used**, per
**gate rule 15**.

## Presence Check

```
Select-String -SimpleMatch 'function Resolve-ReferenceAssemblyVersion'
```

| File | Required | Measured | Result |
|---|---|---|---|
| `scripts/dependencies/ProjectConsistency.psm1` | exactly 1 | **1** | PASS |
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | exactly 0 | **0** | PASS |

The pair is the check. A function present in both files, or in neither, fails: the first is a
copy rather than a move, the second is a deletion.

## Exported-Name List

Read from the loaded module rather than from the source text, so the assertion reads what
PowerShell actually exports:

```
Import-Module scripts/dependencies/ProjectConsistency.psm1 -Force
(Get-Module ProjectConsistency).ExportedFunctions.Keys
```

```
Invoke-BindingRedirectReconciliation
Invoke-VersionReconciliation
Resolve-ReferenceAssemblyVersion
```

| Clause | Required | Measured | Result |
|---|---|---|---|
| Exported-name count | exactly 3 | **3** | PASS |

## Line Counts

| File | [P0-T5] baseline | After [P2-T1] | Ceiling | Result |
|---|---|---|---|---|
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | 498 | **462** | at most 498 minus 30, so 468 | PASS |
| `scripts/dependencies/ProjectConsistency.psm1` | 331 | **373** | at most 380 | PASS |

The composition root moves **down** 36 lines, from 2 lines of headroom under the 500-line cap to
38. That is the "extract rather than append" R9d asks for, applied to the file R9d named.

## Anchored Numstat

```
git diff --numstat HEAD -- scripts/dependencies/ProjectConsistency.psm1 scripts/dependencies/Repair-PackageManifestConsistency.ps1
```

```
42	0	scripts/dependencies/ProjectConsistency.psm1
0	36	scripts/dependencies/Repair-PackageManifestConsistency.ps1
```

| Clause | Required | Measured | Result |
|---|---|---|---|
| Composition root deletions | at least 30 | **36** | PASS |
| Module additions | at least 30 | **42** | PASS |

The diff is anchored to `HEAD`. An unanchored `git diff` compares the worktree against the index
and passes vacuously once anything is staged, so it could not fail for an executor who staged
before running it.

The module gains 42 lines against the root's 36 because the move carries the four supporting
edits: the 4-line import block with its comment, the 1-line addition to `Export-ModuleMember`,
and the 1-line addition to the header list.

## Why This Discharge and Not the Other

The review offered two routes for R5. Decision **D1** selected this one and the rejected route's
cost is recorded in the plan: `Invoke-ProjectConsistencyRepair` is called by four test sites, and
three of them carry criteria — `AC16-` twice and `AC21-` once, the last being the #908 regression
fixture. Retargeting those three onto the composition root would re-base delivered criterion
evidence for what is a parameter-threading error, and leaving the function exported while
retargeting its tests away from it would leave 99 uncovered lines in the module.

## Output Summary

The function exists once in `ProjectConsistency.psm1` and zero times in the composition root. The
module exports exactly 3 names. The composition root falls from 498 to 462 lines and the module
rises from 331 to 373, both inside their ceilings. Anchored numstat records 36 deletions from the
root and 42 additions to the module.
