# PoshQC analyze after the executable-carrier fix ([P2-T8])

Timestamp: 2026-08-10T23-35
Command: `mcp__drm-copilot__run_poshqc_analyze` with `workspace_root = "C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb"` and `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]`
EXIT_CODE: 1

**`EXIT_CODE: 0` is not the acceptance condition for this task and is not asserted.** PoshQC analyze
is RED at the merge base with 16 pre-existing findings
(`FEATURE/evidence/baseline/baseline-poshqc-analyze.2026-08-10T23-02.md`). Acceptance is **zero new
findings relative to that baseline multiset**.

## MCP return payload (verbatim)

```json
{
  "ok": false,
  "tool": "run_poshqc_analyze",
  "workspace_root": "C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-ac1a08c3569adb7eb",
  "summary": "Command exited with code 1.",
  "stderr_excerpt": "Exception: PSScriptAnalyzer reported 16 issue(s)."
}
```

## Finding count

| | Count |
|---|---|
| [P0-T15] baseline | 16 |
| Post-fix (this run) | **16** |
| **Delta** | **0** |

## Full post-fix table

Enumerated via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-psscriptanalyzer.ps1`
(EXIT_CODE 0), the same direct channel [P0-T15] used.

| # | Severity | Rule | File | Baseline line | Post-fix line |
|---|---|---|---|---|---|
| 1 | Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 26 | 26 |
| 2 | Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 36 | 36 |
| 3 | Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 39 | 39 |
| 4 | Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 59 | 59 |
| 5 | Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 79 | 79 |
| 6 | Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 106 | 106 |
| 7 | Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 119 | 119 |
| 8 | Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 120 | 120 |
| 9 | Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 146 | 146 |
| 10 | Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 32 | 32 |
| 11 | Warning | PSUseSingularNouns | **Invoke-VSBuild.ps1** | 47 | **52** |
| 12 | Warning | PSUseSingularNouns | **Invoke-VSBuild.ps1** | 78 | **87** |
| 13 | Warning | PSAvoidUsingWriteHost | **Invoke-VSBuild.ps1** | 137 | **147** |
| 14 | Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 150 | 150 |
| 15 | Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 154 | 154 |
| 16 | Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 157 | 157 |

**Every line number in a file this feature does not edit is unchanged.** Only the three findings in
`scripts/vscode/Invoke-VSBuild.ps1` moved, which is exactly what the baseline predicted.

## `(Severity, Rule, File)` multiset — identical

| Severity | Rule | File | Baseline | Post-fix |
|---|---|---|---|---|
| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 3 | 3 |
| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 3 | 3 |
| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 2 | 2 |
| Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 1 | 1 |
| Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 1 | 1 |
| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 2 | 2 |
| Warning | PSAvoidUsingWriteHost | Invoke-VSBuild.ps1 | 1 | 1 |
| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 3 | 3 |
| **Total** | | | **16** | **16** |

**Zero new findings.** Specifically: **no new PSUseSingularNouns** (no function was renamed and no
new plural-noun function was introduced — `-Target` is a parameter, not a function) and **no new
PSAvoidUsingWriteHost** (the added call is `Write-Warning`, a different rule that PSScriptAnalyzer
does not flag under this settings set).

## Per-finding line-number reconciliation

Net line insertions above each finding, from [P2-T1] through [P2-T4]:

| Edit | Lines inserted | Location |
|---|---|---|
| [P2-T1] `-Target` in the script `param(...)` block | **+4** (`[Parameter(...)]`, `[ValidateSet(...)]`, `[string]$Target = 'Build',`, one blank separator) | above all three findings |
| [P2-T3] deprecation comment above the script-level `[switch]$EnableNullable` (spec row 20) | **+1** | above all three findings |
| [P2-T2] `-Target` in the `Get-MSBuildBuildArguments` `param(...)` block | **+4** (same four lines) | above findings 12 and 13 only |
| [P2-T3] deprecation comment above the function-level `[switch]$EnableNullable` (spec row 20) | **+1** | above finding 13 only |
| [P2-T2] `'/t:Build',` -> `"/t:$Target",` | **0** (one-for-one line replacement) | — |
| [P2-T3] `$properties += 'Nullable=enable'` -> `Write-Warning '...'` | **0** (one-for-one line replacement) | — |
| [P2-T4] `-Target $Target` added to the call site | **0** (same-line edit) | below all three findings |

| Finding | Baseline line | Net insertions above | Expected | Observed | Reconciled |
|---|---|---|---|---|---|
| PSUseSingularNouns, `Get-MSBuildBuildArguments` | 47 | 4 + 1 = **5** | 52 | **52** | YES |
| PSUseSingularNouns, `Get-RequestedMSBuildProperties` | 78 | 4 + 1 + 4 = **9** | 87 | **87** | YES |
| PSAvoidUsingWriteHost, `Write-Host "Using MSBuild: ..."` | 137 | 4 + 1 + 4 + 1 = **10** | 147 | **147** | YES |

Confirmed independently by direct grep of the post-fix file:

```
52:function Get-MSBuildBuildArguments {
87:function Get-RequestedMSBuildProperties {
147:Write-Host "Using MSBuild: $msbuildPath"
```

Every shift is fully accounted for by the recorded insertion counts. No unaccounted shift exists.

## Output Summary

PoshQC analyze returned `EXIT_CODE: 1` with **16** findings — identical in count and in
`(Severity, Rule, File)` multiset to the [P0-T15] merge-base baseline, i.e. **zero new findings**.
The only line-number movement is the three findings inside `scripts/vscode/Invoke-VSBuild.ps1`
(47 -> 52, 78 -> 87, 137 -> 147), each reconciled exactly against the +5 / +9 / +10 net insertions
made by [P2-T1] through [P2-T4]. No line number changed in any file this feature does not edit.
