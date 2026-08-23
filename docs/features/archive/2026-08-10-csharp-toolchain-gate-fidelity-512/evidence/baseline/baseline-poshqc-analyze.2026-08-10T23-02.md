# Baseline — PoshQC analyze at this HEAD ([P0-T15])

Timestamp: 2026-08-10T23-02
Command: `mcp__drm-copilot__run_poshqc_analyze` with `workspace_root = "C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb"` and `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]`
EXIT_CODE: 1

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

**`EXIT_CODE: 1` is the value the tool returned and is recorded as returned.** PoshQC analyze is
**RED at the merge base**. `EXIT_CODE: 0` is therefore **not** an acceptance condition anywhere in
this plan, and is not asserted by [P2-T8] or [P6-T2]. Acceptance for those tasks is "no new finding
relative to this baseline multiset".

## Finding count

**16** — matching the count recorded in the pre-existing artifact
`baseline-powershell-toolchain.2026-08-10T15-40.md`. No divergence to record.

## Full rule / file / line table at this HEAD

Enumerated with `Invoke-ScriptAnalyzer -Path './scripts/vscode' -Recurse` plus
`Invoke-ScriptAnalyzer -Path './tests/scripts/vscode' -Recurse`
(`pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-psscriptanalyzer.ps1`, EXIT_CODE 0;
the direct analyzer channel is used because the MCP function returns only a count, not a table).

| # | Severity | Rule | File | Line |
|---|---|---|---|---|
| 1 | Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 26 |
| 2 | Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 36 |
| 3 | Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 39 |
| 4 | Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 59 |
| 5 | Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 79 |
| 6 | Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 106 |
| 7 | Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 119 |
| 8 | Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 120 |
| 9 | Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 146 |
| 10 | Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 32 |
| 11 | **Warning** | **PSUseSingularNouns** | **Invoke-VSBuild.ps1** | **47** |
| 12 | **Warning** | **PSUseSingularNouns** | **Invoke-VSBuild.ps1** | **78** |
| 13 | **Warning** | **PSAvoidUsingWriteHost** | **Invoke-VSBuild.ps1** | **137** |
| 14 | Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 150 |
| 15 | Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 154 |
| 16 | Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 157 |

Zero findings originate in `tests/scripts/vscode`.

## The `(Severity, Rule, File)` comparison multiset

This is the exact basis [P2-T8] and [P6-T2] compare against. Line numbers are excluded from the
multiset and reconciled separately.

| Severity | Rule | File | Multiplicity |
|---|---|---|---|
| Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 3 |
| Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 3 |
| Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 2 |
| Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 1 |
| Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 1 |
| Warning | PSUseSingularNouns | Invoke-VSBuild.ps1 | 2 |
| Warning | PSAvoidUsingWriteHost | Invoke-VSBuild.ps1 | 1 |
| Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 3 |
| **Total** | | | **16** |

## Line numbers expected to shift

Only the three findings in `scripts/vscode/Invoke-VSBuild.ps1` (baseline lines **47**, **78**,
**137**) may shift, and only by the net number of lines [P2-T1] through [P2-T4] insert above each.
Any line-number change in a file this feature does not edit is a failure.

## Constraints this baseline imposes on Phase 2

- Do **not** rename `Get-MSBuildBuildArguments` (line 47) or `Get-RequestedMSBuildProperties`
  (line 78) to satisfy PSUseSingularNouns — both are referenced by the Pester test file and renaming
  is out of scope for all four issues.
- Do **not** add a new plural-noun function (would produce a seventeenth finding).
- Do **not** add a new `Write-Host`. The `Write-Warning` introduced by spec executable-carrier row 23
  is a different rule and is permitted.

## Output Summary

PoshQC analyze returns `EXIT_CODE: 1` with **16** PSScriptAnalyzer findings at this HEAD, three of
them inside `scripts/vscode/Invoke-VSBuild.ps1` (the file this feature edits). The full
rule/file/line table and the `(Severity, Rule, File)` multiset are recorded above as the sole
comparison basis for [P2-T8] and [P6-T2]. The measured count equals the expected 16, so no divergence
needs recording.
