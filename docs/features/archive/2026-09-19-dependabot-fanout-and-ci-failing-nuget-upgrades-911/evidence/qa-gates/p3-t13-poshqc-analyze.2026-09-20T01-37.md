# PoshQC Analyze After Phase 3 — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-03-39
- Task: [P3-T13]
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
| `tests/scripts/dependencies/DependabotConfig.Tests.ps1` findings | exactly 0 | **0** | PASS |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` findings | exactly 0 | **0** | PASS |
| Every remaining finding a member of the [P0-T7] tuple set | yes | 13 of 13 | PASS |

The two files named are the only PowerShell files Phase 3 modified.

**Findings outside `scripts/vscode/`: 0.** That single figure is the strongest form of the
subset check available here: every one of the 13 lies in a `scripts/vscode/` file, and this cycle
touches no file in that folder, so all 13 are necessarily the pre-existing rows.

A total of 0 would be a failure unless `N` is 0, which [P0-T7] forbids. The total is 13.

## Full Finding List — 13 Tuples

Identical, element by element, to the [P0-T7] baseline and to the [P1-T13] and [P2-T9] lists:

| # | File path | Rule name | Line |
|---|---|---|---|
| 1 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | 26 |
| 2 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | 36 |
| 3 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSUseOutputTypeCorrectly` | 39 |
| 4 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | 59 |
| 5 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | 79 |
| 6 | `scripts/vscode/Install-RepoDotNetSdk.ps1` | `PSAvoidUsingWriteHost` | 106 |
| 7 | `scripts/vscode/Invoke-MSTest.ps1` | `PSAvoidUsingWriteHost` | 210 |
| 8 | `scripts/vscode/Invoke-MSTest.ps1` | `PSAvoidUsingWriteHost` | 211 |
| 9 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | `PSUseSingularNouns` | 139 |
| 10 | `scripts/vscode/Invoke-Restore.ps1` | `PSAvoidUsingWriteHost` | 101 |
| 11 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSUseSingularNouns` | 52 |
| 12 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSUseSingularNouns` | 87 |
| 13 | `scripts/vscode/Invoke-VSBuild.ps1` | `PSAvoidUsingWriteHost` | 245 |

## The `PSReviewUnusedParameter` Class Did Not Recur

Phase 1 added nine `PSReviewUnusedParameter` findings and had to correct them. Phase 3's
`Get-WorkflowStepBlock` helper declares two parameters and reads both — `$Line` in the `foreach`
and `$StepName` in the name comparison — so the class did not recur. `[AllowEmptyString()]` was
added to `$Line` for a different reason, recorded at [P3-T1].

## Output Summary

13 findings, equal to the [P0-T7] baseline. Zero in each of the two files Phase 3 modified, and
zero outside `scripts/vscode/` altogether. `ok:false` is the expected tool state and is not
asserted.
