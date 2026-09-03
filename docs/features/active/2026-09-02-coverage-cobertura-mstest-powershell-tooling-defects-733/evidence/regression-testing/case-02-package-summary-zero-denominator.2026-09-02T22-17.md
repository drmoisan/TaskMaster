# Case 02 — Get-CoberturaPackageLineSummary zero-denominator fallback (P1-T2, expect-fail)

Timestamp: 2026-09-02T22-17

Task: [P1-T2] [expect-fail]

## Change Made

Added a second `It` to the `Describe 'Get-CoberturaPackageLineSummary'` block in
tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1:
"falls back to a zero rate when no class in the package carries any lines".

The fixture is a `<package>` whose two classes carry no `<lines>` element (and no `<methods>`
element), which is valid input per the Get-CoberturaClassLineSummary contract. The package's
own `line-rate` and `branch-rate` attributes, and both classes' attributes, are deliberately
set to non-zero stale values ('0.5' and '0.25') so a returned '0' cannot be produced by copying
the input. The It asserts LineRate and BranchRate both equal the string '0', matching the
zero-denominator fallback convention Get-CoberturaCoverageSummary already uses, and additionally
asserts LinesValid and BranchesValid are '0'.

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
  [-] accumulates line and branch totals across every class in the package 54ms (35ms|19ms)
   at <ScriptBlock>, tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1:38
   CommandNotFoundException: The term 'Get-CoberturaPackageLineSummary' is not recognized as a
   name of a cmdlet, function, script file, or executable program.
  [-] falls back to a zero rate when no class in the package carries any lines 20ms (20ms|1ms)
   at <ScriptBlock>, tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1:63
   CommandNotFoundException: The term 'Get-CoberturaPackageLineSummary' is not recognized as a
   name of a cmdlet, function, script file, or executable program.
```

Counts: Passed 0, Failed 2, Skipped 0.

## Output Summary

The predicted pre-fix failure for the new It was observed exactly: a CommandNotFoundException
on `Get-CoberturaPackageLineSummary`. The P1-T1 It continues to fail with the same exception,
as expected at this point in the phase. Pester version 5.6.1. Absolute host paths in the
captured Pester output were replaced with their repository-relative equivalents.
