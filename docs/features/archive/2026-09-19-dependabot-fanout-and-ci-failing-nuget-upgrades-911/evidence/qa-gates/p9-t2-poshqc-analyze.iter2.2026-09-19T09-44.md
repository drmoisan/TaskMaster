# P9-T2 — PowerShell QA step 2, PoshQC analyze (iteration 2)

Timestamp: 2026-09-20T09-44

Command: CMD-POSHQC-ANALYZE — MCP tool `mcp__drm-copilot__run_poshqc_analyze`, `workspace_root`
passed as `<execution-worktree-root>`.

Exact `scan_folders` argument value passed:

```
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

The value is supplied explicitly because the tool otherwise resolves its scan set from
`config/poshqc-scan.json`, which does not exist in this repository, and an omitted argument would
measure nothing.

EXIT_CODE: 1

MCP payload, verbatim:

```
ok: false
tool: run_poshqc_analyze
workspace_root: <execution-worktree-root>
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 13 issue(s).
```

`MCP Result: ok:true` is not asserted and is expected to be false while the residual baseline
findings remain in files outside the Write Set. Exit 1 is the tool response to a non-empty diagnostic
set, not a failure of this gate.

Output Summary: 13 findings, the same set P4-T2 and P6-T2 recorded and a subset of the 16-tuple
P0-T17 baseline. Zero findings in the fifteen files this change created or modified.

## Integer total finding count

**13.**

## How the tuple set was obtained

The MCP tool reports a count only, so the tuple set was reconciled against a direct run of the same
analyzer over the same four folders, which is the method P4-T2 established:

```
Invoke-ScriptAnalyzer -Path "scripts/dependencies" -Recurse
Invoke-ScriptAnalyzer -Path "scripts/vscode" -Recurse
Invoke-ScriptAnalyzer -Path "tests/scripts/dependencies" -Recurse
Invoke-ScriptAnalyzer -Path "tests/scripts/vscode" -Recurse
```

The direct run totals **13**, equal to the reported 13. The two also agreed at 18 on iteration 1,
before the owned findings were corrected, so the agreement is not an artefact of the final state.

## Full finding list — 13 file, rule, severity, line tuples

| # | File path | Rule name | Severity | Line | P0-T17 baseline row |
|---|---|---|---|---|---|
| 1 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 26 | 1 |
| 2 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 36 | 2 |
| 3 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSUseOutputTypeCorrectly | Information | 39 | 3 |
| 4 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 59 | 4 |
| 5 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 79 | 5 |
| 6 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | PSAvoidUsingWriteHost | Warning | 106 | 6 |
| 7 | `scripts/vscode/Invoke-MSTest.ps1` | PSAvoidUsingWriteHost | Warning | 210 | 7 |
| 8 | `scripts/vscode/Invoke-MSTest.ps1` | PSAvoidUsingWriteHost | Warning | 211 | 8 |
| 9 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | PSUseSingularNouns | Warning | 139 | 9 |
| 10 | `scripts/vscode/Invoke-Restore.ps1` | PSAvoidUsingWriteHost | Warning | 101 | 10 |
| 11 | `scripts/vscode/Invoke-VSBuild.ps1` | PSUseSingularNouns | Warning | 52 | 11 |
| 12 | `scripts/vscode/Invoke-VSBuild.ps1` | PSUseSingularNouns | Warning | 87 | 12 |
| 13 | `scripts/vscode/Invoke-VSBuild.ps1` | PSAvoidUsingWriteHost | Warning | 245 | 13 |

Every row matches a P0-T17 baseline row element by element on file path, rule name, severity and
line. The three baseline rows absent from this set are 14, 15 and 16, the three PSAvoidUsingWriteHost
findings in `scripts/vscode/Sync-PackageReferences.ps1`, a file this change rewrote and whose
findings it resolved.

## Finding count for the fifteen files this change created or modified

| # | File | Findings |
|---|---|---|
| 1 | `scripts/dependencies/PackageGraph.psm1` | 0 |
| 2 | `scripts/dependencies/PackageCompatibility.psm1` | 0 |
| 3 | `scripts/dependencies/AnalyzerItemRepair.psm1` | 0 |
| 4 | `scripts/dependencies/ProjectConsistency.psm1` | 0 |
| 5 | `scripts/dependencies/ConsistencyVerifier.psm1` | 0 |
| 6 | `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | 0 |
| 7 | `scripts/vscode/Sync-PackageReferences.ps1` | 0 |
| 8 | `tests/scripts/dependencies/PackageGraph.Tests.ps1` | 0 |
| 9 | `tests/scripts/dependencies/PackageCompatibility.Tests.ps1` | 0 |
| 10 | `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1` | 0 |
| 11 | `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` | 0 |
| 12 | `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | 0 |
| 13 | `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | 0 |
| 14 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | 0 |
| 15 | `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | 0 |

Total over the fifteen owned files: **0.**

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| scan_folders argument value recorded exactly | yes | recorded above | PASS |
| Integer total finding count recorded | yes | 13 | PASS |
| Full finding tuple list recorded | yes | 13 rows | PASS |
| Total is exactly 13, the set P4-T2 and P6-T2 recorded | 13 | 13 | PASS |
| Finding count over the fifteen owned files | exactly 0 | 0 | PASS |
| Every finding a member of the 16-tuple P0-T17 baseline, element by element | yes | rows 1 to 13 | PASS |

The exact-13 total is the non-vacuity guard. A run that resolved no file at all would report a total
of 0, an owned count of 0 and a vacuously true subset relation over the empty set, so **a total of 0
is a failure, not a clean result.** It reported 13.
