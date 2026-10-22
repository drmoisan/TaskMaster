# P3-T10 — Phase 3 Post-Phase Line Counts

Timestamp: 2026-09-13T06-03
Task: [P3-T10]

Measured after the phase's format step, so the figures are the ones the repository's 500-line ceiling
applies to.

Command: pwsh -NoProfile -Command '<count the lines of each of the three files and print the repository-relative path followed by the bare integer>'
EXIT_CODE: 0

```
scripts/vscode/Invoke-MSTestWithCoverage.ps1 438
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 491
tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 106
```

## Acceptance mapping

The artifact carries three lines of the form path followed by a bare integer, and every integer is at
most 500: 438, 491 and 106.

`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` is the constrained file. It measured 496
lines at the Phase 0 post-format baseline, leaving four lines of headroom, and it had to absorb two
extra arguments at ten call sites plus a reader mock, two fixture replacements and a two-parameter
mock-body extension. Splatting from four argument sets declared in the file's `BeforeAll` block
removed five continuation lines at each of the ten converted sites, which is why the file now measures
491 rather than exceeding the ceiling. No test was removed to achieve this: Pester discovery reports
the same 28 tests in that file as before the change.

## Output Summary

EXIT_CODE: 0. Three figures recorded, all at most 500: entry point 438, repaired shared call-site test
file 491, assembly-discovery test file 106.
