# P3-T4 — The Entry-Point Reported Text

Timestamp: 2026-09-09T11-10
Task: [P3-T4]
Command: no command; this artifact records the asserted string and its producer
EXIT_CODE: 0

## The asserted string

Test T-F, `renders the four counts and both percentages on one line`, in
`tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1`, asserts that the rendered
text is exactly:

```
First-party coverage: lines 3/4 (75.00%), branches 4/8 (50.00%)
```

Test T-G, `renders the report line from a Cobertura string without touching the filesystem`,
asserts the identical string through the composition function. Both tests pass; see
`evidence/regression-testing/p3-t1-pass-after.md`.

The string carries all four counts (`3`, `4`, `4`, `8`) and both derived percentages
(`75.00%`, `50.00%`), which is what AC8 requires.

## The producing function

| Role | Function | File |
| --- | --- | --- |
| Producer of the string | `Format-CoberturaFirstPartyCoverageSummary` | `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` |
| Single-line entry-point call site | `Get-CoberturaFirstPartyCoverageReport` | `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` |
| Consumer | `Invoke-MSTestWithCoverageMain` | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` |

`Format-CoberturaFirstPartyCoverageSummary` is pure: it takes the aggregation result and returns a
string, and it performs no filesystem, network or process I/O.
`Get-CoberturaFirstPartyCoverageReport` composes the aggregation with the formatter and likewise
performs no I/O; it exists so that the wiring added to `Invoke-MSTestWithCoverageMain` is exactly
one line.

## The assertion runs without invoking a coverage run

Both T-F and T-G build their input from an in-memory here-string Cobertura literal and call the pure
functions directly. Neither starts `vstest.console.exe`, `dotnet-coverage` or
`Invoke-MSTestWithCoverageMain`, neither reads or writes any file, and neither creates a temporary
file. That separation is exactly why the formatting function is factored out of the entry point.

## The existing output line is retained

The wiring line added to `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is placed **immediately
before** the existing `Write-Output "Done. Coverage artifact: $resolvedOutputPath"` line, not in
place of it, so any consumer matching on that line continues to work. P2-T3 verified this with
`git grep -c -F -e 'Done. Coverage artifact:'`, which returned one line with count 1, and P4-T6 and
P4-T5 re-verify the surrounding file state.

Output Summary: The string `First-party coverage: lines 3/4 (75.00%), branches 4/8 (50.00%)` is
asserted by two passing tests. Its producer is the pure function
`Format-CoberturaFirstPartyCoverageSummary`, reached from the entry point through the single-line
call site `Get-CoberturaFirstPartyCoverageReport`. The assertion runs without invoking a coverage
run, and the pre-existing `Done. Coverage artifact:` output line is retained.
