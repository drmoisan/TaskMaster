---
name: poshqc-analyze-exit1-on-warning
description: run_poshqc_analyze exits 1 on Warning-severity findings, so "EXIT_CODE 0 with zero error-severity diagnostics" is self-contradictory; Remove-* verbs need SupportsShouldProcess
metadata:
  type: project
---

`mcp__drm-copilot__run_poshqc_analyze` exits **1 for any PSScriptAnalyzer finding, including Warning severity**. A plan acceptance clause phrased "`EXIT_CODE: 0` with zero error-severity diagnostics" is therefore self-contradictory whenever a Warning exists, and is a preflight defect.

Two concrete traps in `scripts/vscode/`:

1. **Pre-existing Warning in the module under change.** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` carries `PSUseSingularNouns` on `Get-CoberturaLineConditionCoverageParts`. Any plan that gates on `EXIT_CODE: 0` over that file is unsatisfiable unless the function is renamed — which usually blows a "exactly N edits to this file" scope lock. The correct gate is *diagnostic-set equality against the Phase 0 baseline*, not exit 0.

2. **`Remove-` verb needs `SupportsShouldProcess`.** A function named `Remove-*` declared with a bare `[CmdletBinding()]` raises `PSUseShouldProcessForStateChangingFunctions` (Warning). Verified fix that yields zero diagnostics: `[CmdletBinding(SupportsShouldProcess = $true)]` plus a single `if ($PSCmdlet.ShouldProcess('<target>', '<action>')) { ... }` guard around the mutation. The state-changing verb list is New/Set/Remove/Start/Stop/Restart/Resume/Suspend; `Merge-`, `Get-`, `Test-`, `ConvertTo-` do not trigger it.

**Why:** both were found at preflight on the #457 closure-filter plan, where P3-T3 demanded `EXIT_CODE: 0` and P2-T5 specified a bare `[CmdletBinding()]` on `Remove-CoberturaExemptClosureCoverage`.

**How to apply:** when validating or executing a PowerShell plan, run `Invoke-ScriptAnalyzer -Path <file>` directly to enumerate the baseline diagnostic set before accepting any exit-code gate, and check every planned `Remove-`/`Set-`/`New-` function name against the ShouldProcess rule. See [[poshqc-pester-mcp-no-numeric-detail]].
