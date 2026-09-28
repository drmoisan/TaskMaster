# In-Place Corrections Cycle — PoshQC Analyze

- Timestamp: 2026-09-20T09-54-02
- Cycle: 2026-09-20T09-42 in-place corrections (R-C2-1 through R-C2-5)
- Command: `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders` supplied explicitly
- EXIT_CODE: 1
- ExpectedExitCode: 1

## The Exact `scan_folders` Argument

```json
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

## A Prior Unscoped Invocation Is Recorded Here and Discarded

The first invocation in this cycle omitted `scan_folders` and reported **48** issues. That
figure is not comparable to the baseline and is not the gate result. `config/poshqc-scan.json`
does not exist in this repository, so an omitted `scan_folders` measures the whole workspace
rather than the four folders the baseline measured. The invocation was repeated with the
argument supplied and that repeated invocation is the one recorded below. The unscoped figure is
stated rather than dropped because a reader comparing run logs would otherwise find an
unexplained 48.

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

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| Total equals the P0-T17 / P0-T7 baseline `N` | 13 | **13** | PASS |
| Every finding a member of the baseline tuple set | yes | 13 of 13, element by element | PASS |
| Findings in files this cycle modified | exactly 0 | **0** | PASS |
| Findings outside `scripts/vscode/` | 0 | **0** | PASS |

### The Five PowerShell Files This Cycle Modified, Each Enumerated

| # | File | Findings | Required |
|---|---|---|---|
| 1 | `scripts/dependencies/ProjectConsistency.psm1` | **0** | exactly 0 |
| 2 | `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | **0** | exactly 0 |
| 3 | `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` | **0** | exactly 0 |
| 4 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | **0** | exactly 0 |
| 5 | `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | **0** | exactly 0 |
| | **Total over the five** | **0** | |

The two remaining files this cycle edited, `.github/workflows/README.md` and the
`p3-t10-workflow-footprint` evidence artifact, are Markdown and are not analyzer inputs.

## Full Finding List — 13 Tuples

Produced by a direct `Invoke-ScriptAnalyzer -Recurse` over the same four folders, which also
totals 13. Identical, element by element, to the P0-T7 baseline list.

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

A `PSAvoidUsingWriteHost` finding was a plausible outcome of correction R-C2-3, which added a
`Write-Information` call. It did not occur: `Write-Information` is not a host-writing cmdlet and
carries no such rule, and the table above shows zero findings in the file that now calls it.

## Why `ok:false` Is Not a Failure of This Gate

`MCP Result: ok:true` is not an acceptance condition. The 13 pre-existing findings all sit in
`scripts/vscode/` production scripts outside this cycle's write set, and the tool exits 1
whenever the diagnostic set is non-empty. The gate is the equality with the baseline and the
per-file owned counts, and both hold.

## Output Summary

13 findings against the four scoped folders, equal to and element-by-element identical with the
baseline. Zero findings in each of the five PowerShell files this cycle modified, enumerated
individually, and zero outside `scripts/vscode/` altogether. Step 2 of the loop passes.
