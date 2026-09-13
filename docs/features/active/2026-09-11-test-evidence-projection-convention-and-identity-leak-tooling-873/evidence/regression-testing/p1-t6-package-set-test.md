# P1-T6 — Package-Set Document-Order Test

Timestamp: 2026-09-13T05-32
Task: [P1-T6]

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1 with Run.PassThru, Output.Verbosity Detailed, and an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

## Verbatim result

```
Describing ConvertTo-JacocoPackageProjection
  [+] emits the exact projection document for a multi-package fixture 88ms
  [+] derives missed as valid minus covered for lines and branches independently 40ms
  [+] emits zero line counters for a package with no class elements 3ms
  [+] emits a zero BRANCH counter rather than omitting it when no branch data is present 11ms
  [+] emits one package element per source package in document order 6ms
  [+] throws the existing missing-packages wording without introducing a second wording 11ms

Describing Assert-JacocoProjectionReconciliation
  [+] returns without throwing when the projection totals equal the source root attributes 12ms
  [+] throws naming the expected and the observed totals when the projection disagrees 7ms

Describing Invoke-MSTestWithCoverage.Projection.ps1 counting-rule delegation
  [+] delegates the counting rule to the per-package helper and re-derives nothing 35ms
Tests completed in 632ms
Tests Passed: 9, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
COUNTS passed=9 failed=0 skipped=0
```

PASSED: 9
FAILED: 0
SKIPPED: 0

## Acceptance mapping

The named test `emits one package element per source package in document order` is recorded as
passed. Its asserted package-element count is exactly 3:

```
$packageElements.Count | Should -Be 3
```

The three fixture package names are `Zeta.First`, `Alpha.Second` and `Mid.Third`, asserted by index
in that order. They are deliberately not in alphabetical order, so an implementation that sorted its
output would fail rather than pass by coincidence.

The fifteen-package set of the committed reference instance is not reproduced, because that instance
was converted from a raw collector document rather than from a post-processed one.

## Output Summary

EXIT_CODE: 0. Nine passed, zero failed, zero skipped. The document-order test asserts exactly three
package elements.
