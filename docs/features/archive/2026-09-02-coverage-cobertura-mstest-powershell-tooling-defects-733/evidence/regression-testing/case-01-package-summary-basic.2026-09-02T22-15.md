# Case 01 — Get-CoberturaPackageLineSummary basic accumulation (P1-T1, expect-fail)

Timestamp: 2026-09-02T22-15

Task: [P1-T1] [expect-fail]

## Change Made

Created tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 with:

- `Set-StrictMode -Version Latest` at file scope.
- A `BeforeAll` that resolves the repository root from `$PSScriptRoot` and dot-sources
  scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, mirroring the `BeforeAll` pattern in
  tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1.
- `Describe 'Get-CoberturaPackageLineSummary'` containing one `It`:
  "accumulates line and branch totals across every class in the package".

The It builds a two-class `<package>` fixture. Class `Ns.A` carries lines 10 (hits 1) and 11
(hits 0). Class `Ns.B` carries lines 20 (hits 1) and 21 (hits 1, branch True,
condition-coverage "50% (1/2)"). Hand-computed package totals: LinesValid 4, LinesCovered 3,
LineRate '0.75', BranchesValid 2, BranchesCovered 1, BranchRate '0.5'. All six returned values
are asserted.

## Command

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with
`Run.Path` = tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 (absolute
path within the item worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`, then
the explicit trailing branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 1

ExpectedExitCode: 1

## Observed Failure (as predicted)

```
Describing Get-CoberturaPackageLineSummary
  [-] accumulates line and branch totals across every class in the package 54ms (34ms|20ms)
   at <ScriptBlock>, tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1:38
   CommandNotFoundException: The term 'Get-CoberturaPackageLineSummary' is not recognized as a
   name of a cmdlet, function, script file, or executable program.
```

Counts: Passed 0, Failed 1, Skipped 0.

## Output Summary

The predicted pre-fix failure was observed exactly: a CommandNotFoundException on
`Get-CoberturaPackageLineSummary`, because that function does not exist yet. The production
function is created by P1-T8 and made reachable from Helpers.ps1 by P1-T9. Pester version
5.6.1. Absolute host paths in the captured Pester output were replaced with their
repository-relative equivalents.
