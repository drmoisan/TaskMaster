# P0-T17 — PowerShell Analyzer Baseline

Timestamp: 2026-09-19T23-06

Command: CMD-POSHQC-ANALYZE-BASELINE — MCP tool `mcp__drm-copilot__run_poshqc_analyze`.

Exact `scan_folders` argument value passed:

```
["scripts/vscode", "tests/scripts/vscode"]
```

`workspace_root` passed as the execution worktree root.

EXIT_CODE: 1

MCP payload, verbatim:

```
ok: false
tool: run_poshqc_analyze
workspace_root: <repo-root>
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 16 issue(s).
```

`MCP Result: ok:true` is not asserted and is expected to be `false` while the pre-existing findings
remain. Exit 1 is the tool's response to a non-empty diagnostic set.

## Integer total finding count

**16.**

## How the tuple set was obtained

The MCP tool reports a count only — no rule name, no file, no line — so the tuple set the plan
requires cannot be read from it. The count was reconciled against a direct run of the same analyzer
over the same two folders:

```
Invoke-ScriptAnalyzer -Path "scripts/vscode" -Recurse
Invoke-ScriptAnalyzer -Path "tests/scripts/vscode" -Recurse
```

EXIT_CODE: 0 (the direct cmdlet returns diagnostics as objects and does not signal on a non-empty
set).

The direct run totals **16**, equal to the MCP tool's reported 16, which is what establishes that
the direct invocation reproduces the MCP tool's effective rule set and that the tuples below are the
tool's own finding set rather than a different analyzer's.

## Full finding list — 16 `(file path, rule name, line)` tuples

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
| 14 | `scripts/vscode/Sync-PackageReferences.ps1` | PSAvoidUsingWriteHost | Warning | 150 |
| 15 | `scripts/vscode/Sync-PackageReferences.ps1` | PSAvoidUsingWriteHost | Warning | 154 |
| 16 | `scripts/vscode/Sync-PackageReferences.ps1` | PSAvoidUsingWriteHost | Warning | 157 |

By severity: 13 Warning, 3 Information, 16 total.

`tests/scripts/vscode` contributes **0** findings across its 18 test files.

## Split against the spec `## Write Set`

| Partition | Count | Files |
|---|---|---|
| Outside the Write Set | **13** | `Install-RepoDotNetSdk.ps1` (6), `Invoke-MSTest.ps1` (2), `Invoke-MSTestWithCoverage.Helpers.ps1` (1), `Invoke-Restore.ps1` (1), `Invoke-VSBuild.ps1` (3) — **5 files** |
| Inside the Write Set | 3 | `scripts/vscode/Sync-PackageReferences.ps1` (3) |

This matches the figure the plan records: 16 total with 13 in five files outside the Write Set. No
total differing from 16 was observed, so nothing is reported for absorption.

The three inside the Write Set are the `PSAvoidUsingWriteHost` findings at lines 150, 154 and 157 of
`scripts/vscode/Sync-PackageReferences.ps1`, exactly as the plan's Measured Tree Facts row records
them. The P3-T4 rewrite removes all three, which is why P4-T2 expects the total to fall from 16 to
13.

## Status of this set

**This 16-member tuple set is the baseline every later analyzer task compares against.** P2-T2
asserts a total of exactly 16 with every finding a member of this set and zero findings in the files
Batch A creates. P4-T2 expects 13 after the `Sync-PackageReferences.ps1` rewrite.

## Acceptance evaluation

- The exact `scan_folders` argument value passed is recorded. PASS.
- The integer total finding count is recorded — **16**. PASS.
- The full finding list is recorded as an enumerated set of `(file path, rule name, line)` tuples —
  16 rows, each carrying all three fields plus severity. PASS.
- `MCP Result: ok:true` is not asserted; `ok:false` was observed and recorded. Per instruction.
- A total differing from 16 would be recorded and reported rather than absorbed. None was observed.

Output Summary: CMD-POSHQC-ANALYZE-BASELINE over
`["scripts/vscode", "tests/scripts/vscode"]` returned EXIT_CODE 1 with
`ok:false` and `PSScriptAnalyzer reported 16 issue(s).` The total is **16** — 13 Warning and 3
Information — reconciled against a direct `Invoke-ScriptAnalyzer -Recurse` run over the same two
folders that returns the same 16 and supplies the per-finding detail the MCP tool does not. All 16
tuples are enumerated above. 13 findings across 5 files sit outside the spec `## Write Set`; the
remaining 3 are the `PSAvoidUsingWriteHost` findings at lines 150, 154 and 157 of
`scripts/vscode/Sync-PackageReferences.ps1`, which P3-T4 removes. `tests/scripts/vscode` is clean at
0 findings.
