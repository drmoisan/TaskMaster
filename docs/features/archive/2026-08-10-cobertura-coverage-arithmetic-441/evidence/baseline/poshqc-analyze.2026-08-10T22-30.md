# Baseline PoshQC Analyze (P0-T15)

Timestamp: 2026-08-10T22-30

Command:

```
# (1) MCP call
mcp__drm-copilot__run_poshqc_analyze
    workspace_root = 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cc35d4011888c2a'
    scan_folders   = ['scripts/vscode', 'tests/scripts/vscode']

# (2) per-file breakdown the MCP summary does not provide
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
foreach ($p in @(
    'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1',
    'tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1')) {
    Invoke-ScriptAnalyzer -Path (Join-Path $root $p) |
        Select-Object ScriptName, RuleName, Severity, Line, Message | Format-List
}
```

EXIT_CODE: 1 (MCP call). **This non-zero exit is expected at every invocation and is not on its own
a failure.** The MCP payload was `ok:false` with
`summary`: `Command exited with code 1.` and
`stderr_excerpt`: `Exception: PSScriptAnalyzer reported 16 issue(s).`
The 16 findings are pre-existing and span six files under `scripts/vscode`; exactly one of them is
in a file this feature touches. The MCP call therefore cannot itself serve as the gate at any scope.
The gate is the per-file breakdown below.

Output Summary:

```
===== scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1 =====
count=1

ScriptName : Invoke-MSTestWithCoverage.Helpers.ps1
RuleName   : PSUseSingularNouns
Severity   : Warning
Line       : 146
Message    : The cmdlet 'Get-CoberturaLineConditionCoverageParts' uses a plural noun. A singular noun should be used
             instead.

===== tests\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.Tests.ps1 =====
count=0
```

## Per-file baseline finding list (the set P4-T2 diffs against)

**The baseline is keyed on `(ScriptName, RuleName, Severity, Message)`. `Line` is recorded as an
observation only and is deliberately excluded from the key.** P2-T2 shortens
`Get-CoberturaCoverageSummary`'s inner loop and P2-T1 inserts a new function, both of which shift
every later declaration in the file, so this finding will not still be on line 146 after the change.
A line-number move on an otherwise-identical finding is **not** a new finding.

### `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — 1 finding

| Key field | Value |
| --- | --- |
| ScriptName | `Invoke-MSTestWithCoverage.Helpers.ps1` |
| RuleName | `PSUseSingularNouns` |
| Severity | `Warning` |
| Message | `The cmdlet 'Get-CoberturaLineConditionCoverageParts' uses a plural noun. A singular noun should be used instead.` |
| *Line (observation only, not part of the key)* | *146* |

### `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` — 0 findings

The baseline finding set for this file is empty.

## Scope note

The single `PSUseSingularNouns` finding **must not be fixed.** Clearing it requires renaming the
exported function `Get-CoberturaLineConditionCoverageParts`, which `spec.md` § Implementation
strategy lists as **Unmodified** and which § Technical specifications forbids ("No exported function
signature changes"). AC-15 is a no-new-findings gate against this recorded baseline, not a
zero-findings gate. Its persistence after the change is expected and is not a failure.

Both figures match the plan's § Verified Environment Facts exactly (1 and 0). No halt condition is
triggered.
