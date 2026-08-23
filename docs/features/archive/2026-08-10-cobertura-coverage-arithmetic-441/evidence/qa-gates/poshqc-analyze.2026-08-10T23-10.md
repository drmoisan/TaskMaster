# Final QA — PoshQC Analyze (P4-T2)

Timestamp: 2026-08-10T23-10

Toolchain step 2 of 3. Unconditional command task; `EXIT_CODE: SKIPPED` is not a valid outcome.

Command:

```
# (1) MCP call
mcp__drm-copilot__run_poshqc_analyze
    workspace_root = 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a'
    scan_folders   = ['scripts/vscode', 'tests/scripts/vscode']

# (2) per-file breakdown (the actual gate)
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
foreach ($p in @(
    'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1',
    'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1')) {
    Invoke-ScriptAnalyzer -Path (Join-Path $root $p) |
        Select-Object ScriptName, RuleName, Severity, Line, Message | Format-List
}
```

EXIT_CODE: 1 (MCP call)

MCP payload: `ok`: `false`; `summary`: `Command exited with code 1.`;
`stderr_excerpt`: `Exception: PSScriptAnalyzer reported 16 issue(s).`

**This non-zero exit is expected at every invocation and does not fail this gate.** Sixteen
pre-existing findings exist under `scripts/vscode` across six files (`Install-RepoDotNetSdk.ps1`,
`Invoke-MSTest.ps1`, `Invoke-MSTestWithCoverage.Helpers.ps1`, `Invoke-Restore.ps1`,
`Invoke-VSBuild.ps1`, `Sync-PackageReferences.ps1`), of which exactly one is in an in-scope file.
The count is unchanged from the P0-T15 baseline (16 -> 16). Only a finding absent from the P0-T15
per-file baseline fails this gate.

Output Summary:

```
===== scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1 =====
count=1

ScriptName : Invoke-MSTestWithCoverage.Helpers.ps1
RuleName   : PSUseSingularNouns
Severity   : Warning
Line       : 140
Message    : The cmdlet 'Get-CoberturaLineConditionCoverageParts' uses a plural noun. A singular noun should be used
             instead.

===== tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1 =====
count=0
```

## Baseline diff — keyed on `(ScriptName, RuleName, Severity, Message)`, excluding `Line`

### `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`

| Key | In P0-T15 baseline | Post-change | Verdict |
| --- | --- | --- | --- |
| (`Invoke-MSTestWithCoverage.Helpers.ps1`, `PSUseSingularNouns`, `Warning`, `The cmdlet 'Get-CoberturaLineConditionCoverageParts' uses a plural noun. A singular noun should be used instead.`) | present | present | **pre-existing, expected to persist** |

- Post-change finding set: **1**. Baseline finding set: **1**.
- Post-change set **is a subset of** the baseline set. **NEW findings: 0.**
- `Line` moved 146 -> **140**. This is the anticipated shift: P2-T2 replaced an 11-line inner loop
  with a 5-line helper call, moving every later declaration up by 6 lines. `146 - 6 = 140`, exactly.
  A line-number move on an otherwise-identical finding is **not** a new finding, which is why `Line`
  is excluded from the comparison key.
- The `PSUseSingularNouns` finding is **not fixed**, deliberately. Clearing it requires renaming the
  exported function `Get-CoberturaLineConditionCoverageParts`, which `spec.md` § Implementation
  strategy lists as Unmodified and § Technical specifications forbids. A zero-findings acceptance is
  deliberately not used here because it is unsatisfiable within scope; AC-15 is a no-new-findings
  gate.
- The new function `Get-CoberturaClassLineSummary` introduced **no** finding of its own: its noun
  (`CoberturaClassLineSummary`) is singular and its verb (`Get`) is approved.

### `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`

Baseline finding set: **0**. Post-change finding set: **0**. **NEW findings: 0.** The 167 added
lines introduced no analyzer debt.

## Gate verdict

**PASS — zero new findings on either changed file relative to the P0-T15 baseline.**
