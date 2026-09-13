# P1-T5 — Reconciliation Tests

Timestamp: 2026-09-13T05-30
Task: [P1-T5]

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1 with Run.PassThru, Output.Verbosity Detailed, and an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

## Verbatim result

```
Describing ConvertTo-JacocoPackageProjection
  [+] emits the exact projection document for a multi-package fixture 100ms
  [+] derives missed as valid minus covered for lines and branches independently 39ms
  [+] emits zero line counters for a package with no class elements 3ms
  [+] emits a zero BRANCH counter rather than omitting it when no branch data is present 12ms
  [+] throws the existing missing-packages wording without introducing a second wording 11ms

Describing Assert-JacocoProjectionReconciliation
  [+] returns without throwing when the projection totals equal the source root attributes 14ms
  [+] throws naming the expected and the observed totals when the projection disagrees 7ms

Describing Invoke-MSTestWithCoverage.Projection.ps1 counting-rule delegation
  [+] delegates the counting rule to the per-package helper and re-derives nothing 42ms
Tests completed in 672ms
Tests Passed: 8, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
COUNTS passed=8 failed=0 skipped=0
```

PASSED: 8
FAILED: 0
SKIPPED: 0

Both tests this task adds are recorded as passed:
`returns without throwing when the projection totals equal the source root attributes`, and
`throws naming the expected and the observed totals when the projection disagrees`.

The negative test rewrites the first package's LINE counter from two covered to one covered, so the
summed projection covered total becomes 3 against the source root's declared 4, and asserts the
thrown message matches `expected 4\b` and `observed 3\b` as two separate numeric substrings. It also
asserts the rewritten projection differs from the original, so a substitution that matched nothing
cannot let the test pass for the wrong reason.

## Output Summary

EXIT_CODE: 0. Eight passed, zero failed, zero skipped.
