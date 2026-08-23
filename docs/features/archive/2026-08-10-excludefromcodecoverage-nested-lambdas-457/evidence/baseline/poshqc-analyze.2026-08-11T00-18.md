# [P0-T7] PoshQC analyze baseline

Timestamp: 2026-08-11T00-18
Command: `mcp__drm-copilot__run_poshqc_analyze` with
`workspace_root = C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a` and
`scan_folders = ["scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1", "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"]`,
paired with
`pwsh -NoProfile -Command 'Invoke-ScriptAnalyzer -Path "<file>"'` for each file in the scan set
EXIT_CODE: 1 (MCP `ok:false`, `Command exited with code 1`); paired direct runs each exited 0

MCP Result (verbatim):

```json
{
  "ok": false,
  "tool": "run_poshqc_analyze",
  "workspace_root": "C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a",
  "summary": "Command exited with code 1.",
  "stderr_excerpt": "Exception: PSScriptAnalyzer reported 1 issue(s)."
}
```

## Scan set

- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` is excluded for the reason recorded in `[P0-T6]`.

## Diagnostic count by severity

| Severity | Count |
|---|---|
| Error | 0 |
| Warning | 1 |
| Information | 0 |
| **Total** | **1** |

The MCP payload reports only the count (`PSScriptAnalyzer reported 1 issue(s)`). The verbatim list
below is obtained from the paired direct `Invoke-ScriptAnalyzer` runs.

## Full diagnostic list (verbatim, from the paired direct runs)

Command: `pwsh -NoProfile -Command 'Invoke-ScriptAnalyzer -Path "scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1" | Format-List RuleName,Severity,ScriptName,Line,Message'`
EXIT_CODE: 0

```
RuleName   : PSUseSingularNouns
Severity   : Warning
ScriptName : Invoke-MSTestWithCoverage.Helpers.ps1
Line       : 140
Message    : The cmdlet 'Get-CoberturaLineConditionCoverageParts' uses a plural noun. A singular noun should be used
             instead.
```

Command: `pwsh -NoProfile -Command 'Invoke-ScriptAnalyzer -Path "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1" | Format-List RuleName,Severity,ScriptName,Line,Message'`
EXIT_CODE: 0

```
(no output — zero diagnostics)
```

## BASELINE DIAGNOSTIC SET (the set `[P3-T3]` compares against)

| # | Rule | Severity | File | Line |
|---|---|---|---|---|
| 1 | `PSUseSingularNouns` | Warning | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 140 |

Baseline diagnostics on `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`: none.

This confirms the plan's preflight-recorded fact verbatim: the helpers module carries a pre-existing
`PSUseSingularNouns` Warning on `Get-CoberturaLineConditionCoverageParts`, and
`run_poshqc_analyze` exits 1 on a Warning. Renaming that function is out of scope: it would exceed the
two permitted edits fixed by `[P2-T8]`, `[P2-T9]` and spec AC 13.

`[P3-T3]` passes when the post-change diagnostic set is identical to this one-item set: zero
diagnostics on `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`, zero on either test file,
and no diagnostic on `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` other than row 1 above.
`EXIT_CODE: 1` from the MCP surface remains acceptable at `[P3-T3]` under exactly that condition.

## Output Summary

One diagnostic repository-wide across the scan set: `PSUseSingularNouns` (Warning) on
`Get-CoberturaLineConditionCoverageParts` at `Invoke-MSTestWithCoverage.Helpers.ps1:140`. Zero errors.
Zero diagnostics on the test file. MCP exits 1 because PSScriptAnalyzer reported one issue; that is
the expected pre-existing baseline state, not a regression.
