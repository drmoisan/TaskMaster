# P6-T2 — PowerShell analyzer, Batch C close-out

Timestamp: 2026-09-19T09-44

Command: CMD-POSHQC-ANALYZE — MCP tool `mcp__drm-copilot__run_poshqc_analyze`,
`workspace_root` passed as the execution worktree root.

Exact `scan_folders` argument value passed:

```
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

EXIT_CODE: 1

MCP payload of the accepted run, verbatim:

```
ok: false
tool: run_poshqc_analyze
workspace_root: <repo-root>
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 13 issue(s).
```

`MCP Result: ok:true` is not asserted and is expected to be `false` while the residual
baseline findings stand. Exit 1 is the tool's response to a non-empty diagnostic set.

## Integer total finding count

**13.**

## How the tuple set was obtained

The MCP tool reports a count only, so the tuple set was reconciled against a direct run of
the same analyzer over the same four folders, which is the method P4-T2 established:

```
Invoke-ScriptAnalyzer -Path "scripts/dependencies" -Recurse
Invoke-ScriptAnalyzer -Path "scripts/vscode" -Recurse
Invoke-ScriptAnalyzer -Path "tests/scripts/dependencies" -Recurse
Invoke-ScriptAnalyzer -Path "tests/scripts/vscode" -Recurse
```

The direct run totals **13**, equal to the MCP tool's reported 13. The two agreed at 25 as
well, before the owned findings were fixed, so the agreement is not an artefact of the
final state.

## Full finding list — 13 `(file path, rule name, line)` tuples

| # | File path | Rule name | Severity | Line |
|---|---|---|---|---|
| 1 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 26 |
| 2 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 36 |
| 3 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 39 |
| 4 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 59 |
| 5 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 79 |
| 6 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 106 |
| 7 | `scripts/vscode/Invoke-MSTest.ps1` | PSAvoidUsingWriteHost | Warning | 210 |
| 8 | `scripts/vscode/Invoke-MSTest.ps1` | PSAvoidUsingWriteHost | Warning | 211 |
| 9 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | PSUseSingularNouns | Warning | 139 |
| 10 | `scripts/vscode/Invoke-Restore.ps1` | PSAvoidUsingWriteHost | Warning | 101 |
| 11 | `scripts/vscode/Invoke-VSBuild.ps1` | PSUseSingularNouns | Warning | 52 |
| 12 | `scripts/vscode/Invoke-VSBuild.ps1` | PSUseSingularNouns | Warning | 87 |
| 13 | `scripts/vscode/Invoke-VSBuild.ps1` | PSAvoidUsingWriteHost | Warning | 245 |

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `scan_folders` argument value recorded exactly | yes | above |
| Integer total finding count recorded | yes | 13 |
| Full finding tuple list recorded | yes | 13 rows above |
| Total is exactly 13 | 13 | 13 |
| Finding count for files this change has created or modified | exactly 0 | 0 |
| Every finding is a member of the 16-tuple P0-T17 baseline | yes | see below |

The exact-13 total is the non-vacuity guard. A run that resolved no files at all reports a
total of 0, an owned count of 0 and a vacuously true subset relation over the empty set, so
**a total of 0 would be a failure, not a clean result.** It reported 13.

### Owned-file count

The thirteen files this change has created or modified as of this task are the seven P4-T2
enumerated — `scripts/dependencies/PackageGraph.psm1`,
`scripts/dependencies/PackageCompatibility.psm1`,
`scripts/vscode/Sync-PackageReferences.ps1`,
`tests/scripts/dependencies/PackageGraph.Tests.ps1`,
`tests/scripts/dependencies/PackageCompatibility.Tests.ps1`,
`tests/scripts/dependencies/DependabotConfig.Tests.ps1`,
`tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` — plus the six Batch C files:
`scripts/dependencies/AnalyzerItemRepair.psm1`,
`scripts/dependencies/ProjectConsistency.psm1`,
`scripts/dependencies/ConsistencyVerifier.psm1`,
`tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1`,
`tests/scripts/dependencies/ProjectConsistency.Tests.ps1` and
`tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1`.

**None of the thirteen appears in the finding list above.** Every finding sits in one of
five files outside the spec `## Write Set`: `Install-RepoDotNetSdk.ps1`,
`Invoke-MSTest.ps1`, `Invoke-MSTestWithCoverage.Helpers.ps1`, `Invoke-Restore.ps1` and
`Invoke-VSBuild.ps1`.

### Subset relation against the P0-T17 baseline

All 13 tuples are members of the 16-tuple baseline set P0-T17 recorded. The three baseline
tuples not present are the `PSAvoidUsingWriteHost` findings at
`scripts/vscode/Sync-PackageReferences.ps1` lines 150, 154 and 157, which the P3-T4 rewrite
removed; that is the same 16-to-13 reduction P4-T2 recorded, and the set is element-for-
element identical to P4-T2's. No finding lies outside the baseline subset, so no unrelated
file was perturbed.

The tuples remain comparable by line number because no file carrying a baseline finding was
rewritten by any P6-T1 format pass; P6-T1 records the hash comparison that establishes it,
which is the condition gate rule 14 attaches to this comparison.

## Twelve owned findings were introduced and fixed, not waived

The first invocation of this task reported **25**. The twelve above the baseline were all
in Batch C files, and each was fixed in place rather than suppressed. The zero-owned-
findings clause did the work it was written for, for the second time in this run.

| Finding | File | Cause | Fix |
|---|---|---|---|
| PSUseBOMForUnicodeEncodedFile | `ConsistencyVerifier.psm1` | two em dashes (U+2014) in a comment, in a file with no byte-order mark | replaced with ASCII parentheses; the repository's other PowerShell files are pure ASCII with no mark, and adding one would have diverged from them |
| PSUseShouldProcessForStateChangingFunctions | `AnalyzerItemRepair.psm1`, `ProjectConsistency.psm1` | the `Update-` verb is on the rule's state-changing list, but both functions are pure string transforms | renamed to `Get-RewrittenPackageFolderLine` and `Get-RewrittenReferenceVersionLine`, which describe what they return |
| PSReviewUnusedParameter x4 | `AnalyzerItemRepair.psm1`, `ProjectConsistency.psm1` | a parameter referenced only inside a nested scriptblock reads as unused to static analysis | bound to a local at statement level before the closure captures it, with a comment giving the reason |
| PSReviewUnusedParameter x4 | the two test suites | fixture delegates declare the caller's two-parameter contract but consult only one, or neither | added an explicit `$null = ...` discard and a comment recording that the signature is the contract |
| PSUseOutputTypeCorrectly | `AnalyzerItemRepair.psm1` | `Get-FolderBearingElement` declared `[OutputType([pscustomobject])]` and returned an array | declared `[OutputType([pscustomobject[]])]` and cast both return expressions to that type |

## The phase restarted twice

A non-zero rewrite count restarts the phase from P6-T1, and changing files to fix analyzer
findings restarts the toolchain loop from the format step. Both happened:

1. P6-T1 pass 1 rewrote 3 files; pass 2 rewrote 0.
2. This task reported 25; the twelve owned findings were fixed; P6-T1 ran a third time and
   rewrote 0, confirming the fixes are formatter-stable; this task was re-run and reported
   13.

After the fixes, the full Pester population over `tests/scripts/dependencies` and
`tests/scripts/vscode` reported `Passed=264 Failed=0 Total=264`, so the renames and the
local bindings changed no behaviour.

3. P6-T3 then measured `ProjectConsistency.psm1` below its per-module coverage clause, four
   test cases were added, P6-T1 ran a fourth time and rewrote 0, and **this task was
   re-invoked and again reported exactly 13** with 0 in owned files. That re-invocation is
   the one recorded at the top of this artifact, so the result above describes the tree as
   it stands at the Batch C commit rather than an earlier state.

