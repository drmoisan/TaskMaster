# Case 03 — Stale package-level line-rate after a filename merge (P1-T3, expect-fail)

Timestamp: 2026-09-02T22-19

Task: [P1-T3] [expect-fail]

## Change Made

Extended the existing It "computes the merged per-file line-rate from the merged rollup alone"
in tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 with two assertions on the
surviving `<package>` node, read via `SelectSingleNode('//package')`:

- `line-rate` must equal '0.6' (the merged package's 3 covered of 5 valid lines).
- `branch-rate` must equal '0' (the fixture carries no branch lines).

A comment citing issue #733 finding 1 records why the assertion exists. No other assertion in
the test was altered.

## Command

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with
`Run.Path` = tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 (absolute path
within the item worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`,
`Filter.FullName` = "*computes the merged per-file line-rate*", then the explicit trailing
branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 1

ExpectedExitCode: 1

## Observed Failure (as predicted)

```
Describing ConvertTo-KoverageCoberturaXml
  [-] computes the merged per-file line-rate from the merged rollup alone 323ms (297ms|26ms)
   at $resultXml.SelectSingleNode('//package').'line-rate' | Should -Be '0.6',
      tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:273
   Expected strings to be the same, but they were different.
   Expected: '0.6'
   But was:  '0'
```

Counts: Passed 0, Failed 1, Skipped 0 (24 NotRun, excluded by the name filter).

## Output Summary

The predicted pre-fix failure was observed exactly: the package node's `line-rate` attribute
remains at the fixture's stale input value of '0' because no code path currently writes it
after a merge. The `branch-rate` assertion is not the discriminating one here (a correct
implementation and the stale input both yield '0' for this branch-free fixture); the
`line-rate` assertion is what fails and what P1-T12 makes pass. Absolute host paths in the
captured Pester output were replaced with their repository-relative equivalents.
