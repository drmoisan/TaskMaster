# PoshQC Analyze After Phase 2 — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-54-32
- Task: [P2-T9]
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

### Owned-File Counts, Each Enumerated

The five files Phase 2 modified:

| # | File | Findings | Required |
|---|---|---|---|
| 1 | `scripts/dependencies/ProjectConsistency.psm1` | **0** | exactly 0 |
| 2 | `scripts/dependencies/ConsistencyVerifier.psm1` | **0** | exactly 0 |
| 3 | `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | **0** | exactly 0 |
| 4 | `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | **0** | exactly 0 |
| 5 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | **0** | exactly 0 |

The fifth is the file [P2-T6] added the R9c assertion to.

A total of 0 would be a failure unless `N` is 0, which [P0-T7] forbids. The total is 13.

## Full Finding List — 13 Tuples

| # | File path | Rule name | Line | In the [P0-T7] set |
|---|---|---|---|---|
| 1 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | 26 | yes |
| 2 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | 36 | yes |
| 3 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | 39 | yes |
| 4 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | 59 | yes |
| 5 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | 79 | yes |
| 6 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | 106 | yes |
| 7 | `scripts/vscode/Invoke-MSTest.ps1` | `PSAvoidUsingWriteHost` | 210 | yes |
| 8 | `scripts/vscode/Invoke-MSTest.ps1` | `PSAvoidUsingWriteHost` | 211 | yes |
| 9 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | `PSUseSingularNouns` | 139 | yes |
| 10 | `scripts/vscode/Invoke-Restore.ps1` | `PSAvoidUsingWriteHost` | 101 | yes |
| 11 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSUseSingularNouns` | 52 | yes |
| 12 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSUseSingularNouns` | 87 | yes |
| 13 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSAvoidUsingWriteHost` | 245 | yes |

Every row matches a [P0-T7] row element by element. All 13 lie in `scripts/vscode/` files this
cycle does not touch.

## A Note on the Write-Verbose Addition

[P2-T6] added a `Write-Verbose` call to `$script:DefaultFileLister`. That raised no
`PSAvoidUsingWriteHost` finding, because `Write-Verbose` is the approved stream for this purpose
and the rule targets `Write-Host` only. The 4 `PSAvoidUsingWriteHost` findings in
`Install-RepoDotNetSdk.ps1` and the 4 elsewhere are all pre-existing rows 4 through 8, 10 and 13.

## Output Summary

13 findings, equal to the [P0-T7] baseline. Zero in each of the five files Phase 2 modified,
enumerated individually. All 13 remaining findings are members of the baseline tuple set.
`ok:false` is the expected tool state and is not asserted.
