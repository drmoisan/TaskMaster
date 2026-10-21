# Case 06 — max(hits) second-seen-strictly-higher merge branch (P1-T6)

Timestamp: 2026-09-02T22-25

Task: [P1-T6] (deliberately NOT tagged expect-fail)

## Change Made

Added a new, minimal, focused fixture to
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 as the It
"takes the higher hits value when the second class seen for a filename is strictly higher",
inside the `Describe 'Merge-CoberturaClassesByFilename'` block introduced by P1-T5.

The fixture isolates the max(hits) merge branch:

- exactly two classes (`Ns.Bar` and `Ns.BarNested`) share the filename Ns\Bar.cs;
- they overlap on exactly one line number (42);
- only the hits value varies (1 in the first-seen class, 9 in the second-seen class);
- the second-seen class is strictly higher, so a first-seen-wins implementation and a
  last-seen-wins implementation are both distinguishable from max().

The It asserts the merged class carries exactly one line and that its hits attribute equals the
higher value, '9'.

## Command

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with
`Run.Path` = tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 (absolute path
within the item worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`,
`Filter.FullName` = "*takes the higher hits value*", then the explicit trailing branch
`if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 0

## Observed Result

```
Describing Merge-CoberturaClassesByFilename
  [+] takes the higher hits value when the second class seen for a filename is strictly higher
      210ms (186ms|24ms)
```

Counts: Passed 1, Failed 0, Skipped 0 (26 NotRun, excluded by the name filter).

## Output Summary

The test passes against unmodified production code, exactly as the task requires. This is not an
expect-fail case: the max(hits) branch in Merge-CoberturaClassesByFilename already behaves
correctly, and issue #733 finding 4 is a test-coverage gap rather than a defect, per spec.md's
corrected scope. No production code was changed by this task. Absolute host paths in the
captured Pester output were replaced with their repository-relative equivalents.
