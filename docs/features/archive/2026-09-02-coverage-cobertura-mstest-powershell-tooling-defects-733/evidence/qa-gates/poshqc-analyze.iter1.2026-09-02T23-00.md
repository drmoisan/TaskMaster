# P5-T2 — PoshQC analyze gate (Final QA Loop, iteration 1)

Timestamp: 2026-09-02T23-00

## Command 1 — MCP analyze run

Command: `mcp__drm-copilot__run_poshqc_analyze` with
`workspace_root` = the item worktree repository root and
`scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`.

EXIT_CODE: 1

MCP payload:

```
ok: false
tool: run_poshqc_analyze
summary: Command exited with code 1.
stderr_excerpt: Exception: PSScriptAnalyzer reported 17 issue(s).
```

The exit code of 1 is this tool's response to any non-empty diagnostic set at any severity,
including Information. The P0-T6 baseline also exited 1, with 16 issues. The exit code is
therefore not the discriminator between baseline and post-change state; the per-file diagnostic
list below is.

## Command 2 — Direct per-file Invoke-ScriptAnalyzer over all 13 write-set files

Command: `pwsh -NoProfile -Command` with a single-quoted outer wrapper and a double-quoted inner
script, calling `Invoke-ScriptAnalyzer -Path` once per file over the 6 production files and
7 test files in this plan's Phase 5 write set, then `exit 0`.

EXIT_CODE: 0

Verbatim output:

```
FILE: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | diagnostics=1
    PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | line 137
FILE: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTest.ps1 | diagnostics=3
    PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | line 145
    PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | line 146
    PSUseOutputTypeCorrectly | Information | scripts/vscode/Invoke-MSTest.ps1 | line 100
FILE: scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | diagnostics=0
FILE: scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1 | diagnostics=0
FILE: tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | diagnostics=0
TOTAL IN-SCOPE DIAGNOSTICS: 4
```

## Comparison against the P0-T6 baseline set

P0-T6 baseline set (3 diagnostics, all Warning, all pre-existing):

| Rule | Severity | File | Baseline line | This run |
|---|---|---|---|---|
| PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 141 | still present, now line 137 |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 119 | still present, now line 145 |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 120 | still present, now line 146 |

The three baseline diagnostics all survive with identical rule, severity, and file. Their line
numbers moved because this plan's edits inserted and removed lines above them: the Helpers.ps1
`PSUseSingularNouns` target (`Get-CoberturaLineConditionCoverageParts`) moved up 4 lines when
P1-T10's refactor replaced the inline per-class accumulation loop with a call to
`Get-CoberturaPackageLineSummary`, and the two `Invoke-MSTest.ps1` `Write-Host` calls moved down
26 lines when P4-T4 added the `Get-MSTestAssemblyPathList` function above them. No baseline
diagnostic was newly introduced or newly resolved.

Newly introduced by this plan, not present at baseline:

| Rule | Severity | File | Line |
|---|---|---|---|
| PSUseOutputTypeCorrectly | Information | scripts/vscode/Invoke-MSTest.ps1 | 100 |

Six files in the Phase 5 write set did not exist at the P0-T6 baseline
(`scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1`,
`scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1`). All six report zero
diagnostics, so none of them contributes to the count difference.

The MCP folder-scan count rose from 16 to 17, which is exactly the one newly introduced
in-scope diagnostic above. The 13 out-of-scope diagnostics recorded at baseline in
`Install-RepoDotNetSdk.ps1`, `Invoke-Restore.ps1`, `Invoke-VSBuild.ps1`, and
`Sync-PackageReferences.ps1` are unchanged.

## Remediation applied on this iteration

`PSUseOutputTypeCorrectly` at `scripts/vscode/Invoke-MSTest.ps1` line 100 was raised against the
`[OutputType([System.Array])]` attribute that P4-T4 placed on `Get-MSTestAssemblyPathList`: the
analyzer's AST output-type inference for `return , @(...)` does not resolve to `System.Array`.
`.claude/rules/powershell.md` line 94 lists "Creating PSScriptAnalyzer debt and deferring
cleanup" as prohibited, so this newly introduced diagnostic was resolved rather than recorded and
deferred.

Candidate attributes were measured directly with
`Invoke-ScriptAnalyzer -ScriptDefinition <file text> -IncludeRule PSUseOutputTypeCorrectly`:

| Declared attribute | PSUseOutputTypeCorrectly diagnostics |
|---|---|
| `[OutputType([System.Array])]` (as landed by P4-T4) | 1 |
| `[OutputType([string[]])]` | 1 |
| `[OutputType([psobject[]])]` | 1 |
| `[OutputType([System.Collections.IEnumerable])]` | 1 |
| `[OutputType([object])]` | 1 |
| `[OutputType([System.Object[]])]` | 0 |

`[OutputType([System.Object[]])]` was applied. It is an array-typed output attribute, so P4-T4's
acceptance ("`[OutputType([System.Array])]` or an equivalent array-typed output attribute") is
still satisfied, and it is the runtime-accurate declaration: `@(...)` materializes a
`System.Object[]`. No other change was made to the function.

## Output Summary

- MCP analyze: `ok` false, EXIT_CODE 1, 17 issues across both scan folders (baseline: 16).
- Direct per-file scan over the 13 write-set files: 4 diagnostics — 3 Warning, 1 Information.
- Three of the four are the pre-existing P0-T6 baseline diagnostics, unchanged in rule,
  severity, and file, with line numbers shifted by this plan's insertions.
- One diagnostic (`PSUseOutputTypeCorrectly`, Information, `Invoke-MSTest.ps1` line 100) was
  newly introduced by this plan and was fixed on this iteration.
- Because this task changed a tracked file, the Final QA Loop restarts from P5-T1 at iteration 2.
  This iteration-1 artifact is not the final analyze record; see the iteration-2 artifact.
