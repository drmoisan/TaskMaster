# Case 04 — Methods union-merge, existing test reversal (P1-T4, expect-fail)

Timestamp: 2026-09-02T22-21

Task: [P1-T4] [expect-fail]

## Change Made

Updated the existing It "preserves the primary class methods subtree and every hits value when
merging" in tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:

- The assertion `$methodNodes.Count | Should -Be 1` became `Should -Be 2`.
- The assertion `$methodNodes[0].name | Should -Be 'M'` became a join over every retained
  method name, asserted equal to 'M,N', so both the declaring class's method and the closure
  class's method are named individually.
- The test's own comment was changed from "Locks the decision not to merge or strip
  <methods>." to "Locks the union-merge decision for <methods> (issue #733, finding 2)."
- The `hitsByLine` assertion is unchanged, so the line-merge behavior this test also pins is
  still asserted verbatim.

This is a deliberate, spec-approved reversal of the test's prior assertion per spec.md's
Risks & Mitigations section, not an unintended regression. The prior assertion recorded the
clone-primary-only behavior that issue #733 finding 2 identifies as the defect.

## Command

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with
`Run.Path` = tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 (absolute path
within the item worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`,
`Filter.FullName` = "*preserves the primary class methods subtree*", then the explicit trailing
branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 1

ExpectedExitCode: 1

## Observed Failure (as predicted)

```
Describing ConvertTo-KoverageCoberturaXml
  [-] preserves the primary class methods subtree and every hits value when merging 171ms
   at $methodNodes.Count | Should -Be 2,
      tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:350
   Expected 2, but got 1.
```

Counts: Passed 0, Failed 1, Skipped 0 (24 NotRun, excluded by the name filter).

## Output Summary

The predicted pre-fix failure was observed exactly: the merged class carries a single method
node, containing only 'M', against the updated assertion of 2. The failure surfaces on the
count assertion first, which is the discriminating one. P1-T11 adds the union-append loop that
makes this pass. Absolute host paths in the captured Pester output were replaced with their
repository-relative equivalents.
