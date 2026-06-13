# Fail-Before Evidence — Issue #193

Timestamp: 2026-06-13T01-56
Command: git stash push -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; Invoke-Pester -Path ./tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 -Output Detailed; git stash pop
EXIT_CODE: 0 (Pester reported 2 failing tests as expected)

## Purpose

Demonstrate that the new regression tests fail before the production fix to
`Get-KoverageProjectAllowlist`. The production file was temporarily stashed so
that the original (unfixed) helper module was loaded, while the new tests were
present.

## Output Summary

Pester v5.6.1 — Tests Passed: 3, Failed: 2, Skipped: 0.

Failing (as designed, without the fix):

- `ConvertTo-KoverageCoberturaXml` > `excludes .Test packages from the report and
  from the aggregate covered/valid line totals`
  - Expected `UtilitiesCS.Test` to not be found in collection
    `@('UtilitiesCS', 'UtilitiesCS.Test')`, but it was found.
- `Get-KoverageProjectAllowlist` > `excludes projects that resolve to a .Test
  assembly name`
  - Expected `$null` or empty, but got
    `@('QuickFiler.Test', 'SVGControl.Test', 'Swordfish.NET.Test', 'Tags.Test',
    'TaskMaster.Test', 'TaskVisualization.Test', 'ToDoModel.Test',
    'UtilitiesCS.Test', 'VBFunctions.Test')`.

Passing (unaffected by the stash):

- `ConvertTo-KoverageCoberturaXml` > `preserves backslash separators...`
- `ConvertTo-KoverageCoberturaXml` > `merges duplicate class entries...`
- `Get-KoverageProjectAllowlist` > `retains non-test production projects...`

The production fix was restored via `git stash pop` after capture.
