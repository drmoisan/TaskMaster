# PoshQC Analyzer Baseline — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-30-54
- Task: [P0-T7]
- Command: CMD-POSHQC-ANALYZE, the MCP tool `mcp__drm-copilot__run_poshqc_analyze`
- EXIT_CODE: 1
- ExpectedExitCode: 1

## The Exact `scan_folders` Argument

```json
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

Supplied explicitly. `config/poshqc-scan.json` does not exist here, so an omitted argument would
measure nothing. `workspace_root` was the absolute execution-worktree path, recorded here in
placeholder-normalised form as `<execution-worktree-root>`.

## MCP Payload, Verbatim

```
ok: false
tool: run_poshqc_analyze
workspace_root: <execution-worktree-root>
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 13 issue(s).
```

`MCP Result: ok:true` is **not** an acceptance condition and is expected to be false while the
pre-existing findings remain on this tree. Exit 1 is the tool's response to a non-empty diagnostic
set, not a failure of this gate.

## Integer Finding Total

**N = 13.**

A total of 0 would be a failure and not a clean result: it is what a run that resolved no files
reports. Every later analyze task in this cycle asserts equality with this `N`, and that equality
is the non-vacuity guard for each of them.

## How the Tuple Set Was Obtained

The MCP tool reports a count only. The tuple set was measured by a direct run of the same analyzer
over the same four folders, which is the method the predecessor cycle established:

```
Invoke-ScriptAnalyzer -Path scripts/dependencies       -Recurse
Invoke-ScriptAnalyzer -Path scripts/vscode             -Recurse
Invoke-ScriptAnalyzer -Path tests/scripts/dependencies -Recurse
Invoke-ScriptAnalyzer -Path tests/scripts/vscode       -Recurse
```

The direct run totals **13**, equal to the MCP-reported 13. The two independent measurements agree,
which is what licenses reading the tuple list from the direct run.

**Observation recorded rather than suppressed.** The direct run emitted one non-terminating
`Invoke-ScriptAnalyzer: Object reference not set to an instance of an object.` line. It did not
change the total, which still agrees with the MCP count of 13, and it produced no diagnostic
record. It is recorded here so a later reader who sees the same line does not read it as a
divergence.

## Full Finding List — 13 Tuples

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

Ordered by file path then line, as the direct run emitted them.

Every finding lies in `scripts/vscode/`, in a file no phase of this cycle edits. Zero findings sit
in `scripts/dependencies/`, in `tests/scripts/dependencies/` or in `tests/scripts/vscode/`, which
is why the per-file owned counts asserted in Phases 1, 2, 3 and 5 all read 0.

The set is identical, element by element, to the 13 the predecessor cycle recorded at its P9-T2, so
this cycle inherits a stable baseline. It is also the baseline the merge of `origin/main` did not
disturb.

## Output Summary

`N = 13`, from two independent measurements that agree: the MCP tool's `13 issue(s)` and a direct
`Invoke-ScriptAnalyzer` run over the same four folders. All 13 tuples enumerated. All 13 lie in
`scripts/vscode/` files this cycle does not touch. `ok:false` is the expected tool state and is not
asserted.
