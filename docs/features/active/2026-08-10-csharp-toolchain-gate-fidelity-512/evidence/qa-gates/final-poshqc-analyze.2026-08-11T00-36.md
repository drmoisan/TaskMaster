# Final QC step 2 (PowerShell) — PoshQC analyze ([P6-T2])

Timestamp: 2026-08-11T00-36
Command: `mcp__drm-copilot__run_poshqc_analyze` with `workspace_root = "C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb"` and `scan_folders = ["scripts/vscode", "tests/scripts/vscode"]`
EXIT_CODE: 1

**`EXIT_CODE: 0` is not the acceptance condition for this task and is not asserted.** PoshQC analyze
is RED at the merge base with 16 pre-existing findings. Acceptance is **zero new findings relative to
the [P0-T15] baseline multiset**.

## Return payload (verbatim)

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

| Run | Count |
|---|---|
| [P0-T15] merge-base baseline | 16 |
| [P2-T8] post-fix | 16 |
| [P6-T2] final (this run) | **16** |
| **Delta vs baseline** | **0** |

## Full final table, with the complete line-number history

| # | Severity | Rule | File | [P0-T15] | [P2-T8] | [P6-T2] |
|---|---|---|---|---|---|---|
| 1 | Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 26 | 26 | 26 |
| 2 | Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 36 | 36 | 36 |
| 3 | Information | PSUseOutputTypeCorrectly | Install-RepoDotNetSdk.ps1 | 39 | 39 | 39 |
| 4 | Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 59 | 59 | 59 |
| 5 | Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 79 | 79 | 79 |
| 6 | Warning | PSAvoidUsingWriteHost | Install-RepoDotNetSdk.ps1 | 106 | 106 | 106 |
| 7 | Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 119 | 119 | 119 |
| 8 | Warning | PSAvoidUsingWriteHost | Invoke-MSTest.ps1 | 120 | 120 | 120 |
| 9 | Warning | PSUseSingularNouns | Invoke-MSTestWithCoverage.Helpers.ps1 | 146 | 146 | 146 |
| 10 | Warning | PSAvoidUsingWriteHost | Invoke-Restore.ps1 | 32 | 32 | 32 |
| 11 | Warning | PSUseSingularNouns | **Invoke-VSBuild.ps1** | 47 | **52** | **52** |
| 12 | Warning | PSUseSingularNouns | **Invoke-VSBuild.ps1** | 78 | **87** | **87** |
| 13 | Warning | PSAvoidUsingWriteHost | **Invoke-VSBuild.ps1** | 137 | **147** | **147** |
| 14 | Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 150 | 150 | 150 |
| 15 | Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 154 | 154 | 154 |
| 16 | Warning | PSAvoidUsingWriteHost | Sync-PackageReferences.ps1 | 157 | 157 | 157 |

**Every line number in a file this feature does not edit is unchanged across all three runs.**

## `(Severity, Rule, File)` multiset — identical to the baseline

| Severity | Rule | File | Baseline | Final |
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

**Zero new findings.** Specifically **no new PSUseSingularNouns** (no function renamed, no new
plural-noun function introduced) and **no new PSAvoidUsingWriteHost** (the call added by [P2-T3] is
`Write-Warning`).

## Per-finding line-number reconciliation, with both delta terms

| Finding | Baseline line | [P2-T1]-[P2-T4] insertion delta | [P6-T1] formatter delta | Expected final | Observed final | Reconciled |
|---|---|---|---|---|---|---|
| PSUseSingularNouns, `Get-MSBuildBuildArguments` | 47 | **+5** | **0** | 52 | **52** | YES |
| PSUseSingularNouns, `Get-RequestedMSBuildProperties` | 78 | **+9** | **0** | 87 | **87** | YES |
| PSAvoidUsingWriteHost, `Write-Host "Using MSBuild: ..."` | 137 | **+10** | **0** | 147 | **147** | YES |

The `[P2-T1]`-`[P2-T4]` insertion counts are itemized in
`FEATURE/evidence/qa-gates/poshqc-analyze-postfix.2026-08-10T23-35.md`.

The **formatter delta is 0** for all three findings. `FEATURE/evidence/qa-gates/final-poshqc-format.2026-08-11T00-34.md`
records that [P6-T1] left `scripts/vscode/Invoke-VSBuild.ps1` modified relative to the merge base
(which [P6-T1] expressly permits, because it is one of this feature's two edited PowerShell files),
but introduced **no change of its own**: the per-file diff counts are unchanged before and after the
format run and the diff hunks contain only the [P2-T1]-[P2-T4] edits.

**The post-fix line numbers recorded by [P2-T8] (52, 87, 147) are therefore unchanged by Phases 3-6**,
and every shift from the baseline is fully accounted for by the sum of the two recorded deltas. No
unaccounted shift exists, and no line number changed in any file this feature does not edit.

## Output Summary

PoshQC analyze returned `EXIT_CODE: 1` with **16** findings — identical in count and in
`(Severity, Rule, File)` multiset to the [P0-T15] merge-base baseline, i.e. **zero new findings**,
and specifically no new PSUseSingularNouns and no new PSAvoidUsingWriteHost. The three
`Invoke-VSBuild.ps1` findings sit at 52 / 87 / 147, unchanged from [P2-T8] and fully reconciled
against a +5 / +9 / +10 insertion delta plus a **zero** formatter delta.
