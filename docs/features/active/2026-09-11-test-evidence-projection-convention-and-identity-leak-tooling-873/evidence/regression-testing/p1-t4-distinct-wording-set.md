# P1-T4 — Distinct Missing-Packages Wording Set, Before and After

Timestamp: 2026-09-13T05-28
Task: [P1-T4]

Command: pwsh -NoProfile -Command '<case-sensitive fixed-string Select-String for the phrase "does not contain a" over every *.ps1 in scripts/vscode, printing the occurrence count, each hit as file leaf name and line number, then the trimmed distinct set and its cardinality>'
EXIT_CODE: 0

## BEFORE this task

```
OCCURRENCE_COUNT=4
HIT=Invoke-MSTestWithCoverage.FirstParty.ps1:60
HIT=Invoke-MSTestWithCoverage.Helpers.ps1:118
HIT=Invoke-MSTestWithCoverage.Helpers.ps1:428
HIT=Invoke-MSTestWithCoverage.Projection.ps1:55
DISTINCT_CARDINALITY=1
DISTINCT=throw 'Cobertura XML does not contain a <packages> node.'
```

BEFORE_DISTINCT_CARDINALITY: 1

## AFTER this task

```
OCCURRENCE_COUNT=4
HIT=Invoke-MSTestWithCoverage.FirstParty.ps1:60
HIT=Invoke-MSTestWithCoverage.Helpers.ps1:118
HIT=Invoke-MSTestWithCoverage.Helpers.ps1:428
HIT=Invoke-MSTestWithCoverage.Projection.ps1:55
DISTINCT_CARDINALITY=1
DISTINCT=throw 'Cobertura XML does not contain a <packages> node.'
```

AFTER_DISTINCT_CARDINALITY: 1

The cardinality is one in both states, which is the assertion. The occurrence count is deliberately
not asserted: the phrase already occurred three times across two pre-existing part files before this
delivery began, and the fourth occurrence is the projection part file's reuse of the same literal, so
an occurrence count measures the number of call sites rather than the number of wordings.

## Paired Pester run

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1 with Run.PassThru and an explicit exit>'
EXIT_CODE: 0

```
Describing ConvertTo-JacocoPackageProjection
  [+] emits the exact projection document for a multi-package fixture 83ms
  [+] derives missed as valid minus covered for lines and branches independently 38ms
  [+] emits zero line counters for a package with no class elements 3ms
  [+] emits a zero BRANCH counter rather than omitting it when no branch data is present 10ms
  [+] throws the existing missing-packages wording without introducing a second wording 11ms

Describing Invoke-MSTestWithCoverage.Projection.ps1 counting-rule delegation
  [+] delegates the counting rule to the per-package helper and re-derives nothing 28ms
Tests completed in 570ms
Tests Passed: 6, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
COUNTS passed=6 failed=0 skipped=0
```

PASSED: 6
FAILED: 0
SKIPPED: 0

Both tests this task adds are recorded as passed:
`delegates the counting rule to the per-package helper and re-derives nothing`, and
`throws the existing missing-packages wording without introducing a second wording`.

The wording test obtains its expected message by invoking
`Get-CoberturaFirstPartyCoverageSummary` on the same no-packages fixture and capturing what it
throws, so the comparison is against the wording the repository actually uses rather than against a
third copy of the literal.

## Output Summary

EXIT_CODE: 0 for the search and for the Pester run. Distinct-wording-set cardinality is 1 before and
1 after. Six passed, zero failed, zero skipped.
