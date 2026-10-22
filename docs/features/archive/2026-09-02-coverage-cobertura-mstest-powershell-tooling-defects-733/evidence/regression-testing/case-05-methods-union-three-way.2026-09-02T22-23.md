# Case 05 — Three-member methods union merge (P1-T5, expect-fail)

Timestamp: 2026-09-02T22-23

Task: [P1-T5] [expect-fail]

## Change Made

Added a new, isolated three-member merge fixture to
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 as the single It
"unions the methods of every group member into the merged class", inside a new self-contained
`Describe 'Merge-CoberturaClassesByFilename'` block appended at the end of the file.

The fixture holds three classes that share the filename Ns\Foo.cs:

- `Ns.Foo`, the declaring class, contributing method 'M';
- `Ns.Foo.<>c`, a stateless-lambda closure class, contributing method 'N';
- `Ns.Foo.<>c__DisplayClass1_0`, a distinct capturing closure class, contributing method 'O'.

The It asserts the merged class's `<methods>` node carries exactly 3 method elements and that
their names, in document order, are 'M,N,O' — that is, all three names with no duplication.
The comment records spec.md's Assumptions spot check: distinct group members never legitimately
share an identical method name, which is why no deduplication key is introduced.

The new Describe block is placed at the end of the file deliberately. P1-T14's acceptance text
prescribes extracting the most recently added self-contained block into a sibling file if the
file exceeds the 500-line ceiling; the P0-T4 baseline measured this file at 498 lines, so that
extraction is expected and this placement makes it a clean whole-block move.

## Command

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script: `Import-Module Pester -MinimumVersion 5.0`, `New-PesterConfiguration` with
`Run.Path` = tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 (absolute path
within the item worktree), `Run.PassThru = $true`, `Output.Verbosity = "Detailed"`,
`Filter.FullName` = "*unions the methods of every group member*", then the explicit trailing
branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 1

ExpectedExitCode: 1

## Observed Failure (as predicted)

```
Describing Merge-CoberturaClassesByFilename
  [-] unions the methods of every group member into the merged class 173ms (154ms|19ms)
   at $methodNames.Count | Should -Be 3,
      tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:535
   Expected 3, but got 1.
```

Counts: Passed 0, Failed 1, Skipped 0 (25 NotRun, excluded by the name filter).

## Output Summary

The predicted pre-fix failure was observed exactly: exactly one method survives the merge under
today's clone-primary-only behavior, and that single method is 'M' (the primary class's own),
against the asserted 3. The count assertion is the one that fails first and is the
discriminating one. P1-T11 adds the union-append loop that makes this pass. Absolute host paths
in the captured Pester output were replaced with their repository-relative equivalents.
