# Final QA Step 2 — PoshQC Analyze, Iteration 1

- Timestamp: 2026-09-20T09-08-05
- Task: [P5-T2]
- Command: CMD-POSHQC-ANALYZE
- EXIT_CODE: 1
- ExpectedExitCode: 1

## The Exact `scan_folders` Argument

```json
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

## Integer Finding Total

**13.**

MCP payload, verbatim:

```
ok: false
tool: run_poshqc_analyze
workspace_root: <execution-worktree-root>
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 13 issue(s).
```

A direct `Invoke-ScriptAnalyzer` run over the same four folders also totals 13.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| Total equals the `N` [P0-T7] recorded | 13 | **13** | PASS |
| Every remaining finding a member of the [P0-T7] tuple set | yes | 13 of 13 | PASS |

### The Seven PowerShell Files This Cycle Modified, Each Enumerated

| # | File | Findings | Required |
|---|---|---|---|
| 1 | `scripts/dependencies/ProjectConsistency.psm1` | **0** | exactly 0 |
| 2 | `scripts/dependencies/ConsistencyVerifier.psm1` | **0** | exactly 0 |
| 3 | `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | **0** | exactly 0 |
| 4 | `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | **0** | exactly 0 |
| 5 | `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | **0** | exactly 0 |
| 6 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | **0** | exactly 0 |
| 7 | `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | **0** | exactly 0 |
| | **Total over the seven** | **0** | |

**Findings outside `scripts/vscode/`: 0.** Every one of the 13 sits in a `scripts/vscode/`
production script this cycle does not touch, which is the strongest available form of the
element-by-element subset check: the seven owned files contribute nothing, and no file outside
`scripts/vscode/` contributes anything either.

A total of 0 would be a failure unless `N` is 0, which [P0-T7] forbids. The total is 13.

## Full Finding List — 13 Tuples

Identical, element by element, to the [P0-T7] baseline:

| # | File path | Rule name | Severity | Line |
|---|---|---|---|---|
| 1 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | Information | 26 |
| 2 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | Information | 36 |
| 3 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | Information | 39 |
| 4 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | Warning | 59 |
| 5 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | Warning | 79 |
| 6 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | Warning | 106 |
| 7 | `scripts/vscode/Invoke-MSTest.ps1` | `PSAvoidUsingWriteHost` | Warning | 210 |
| 8 | `scripts/vscode/Invoke-MSTest.ps1` | `PSAvoidUsingWriteHost` | Warning | 211 |
| 9 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | `PSUseSingularNouns` | Warning | 139 |
| 10 | `scripts/vscode/Invoke-Restore.ps1` | `PSAvoidUsingWriteHost` | Warning | 101 |
| 11 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSUseSingularNouns` | Warning | 52 |
| 12 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSUseSingularNouns` | Warning | 87 |
| 13 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSAvoidUsingWriteHost` | Warning | 245 |

## Why `ok:false` Is Not a Failure of This Gate

`MCP Result: ok:true` is **not** an acceptance condition and is expected to be false while the
13 pre-existing findings remain. Exit 1 is the tool's response to a non-empty diagnostic set.
The gate is the equality with `N` and the per-file owned counts, and both hold.

## Output Summary

13 findings, equal to the [P0-T7] baseline. Zero in each of the seven PowerShell files this
cycle modified, enumerated individually, and zero outside `scripts/vscode/` altogether. Step 2
of the final loop passes.
