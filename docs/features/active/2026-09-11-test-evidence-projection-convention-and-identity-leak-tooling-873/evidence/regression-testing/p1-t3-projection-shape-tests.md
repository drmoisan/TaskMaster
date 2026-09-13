# P1-T3 — Projection Shape, Arithmetic and Boundary Tests

Timestamp: 2026-09-13T05-26
Task: [P1-T3]

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1 with Run.PassThru, Output.Verbosity Detailed, and an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

## Verbatim result

```
Pester v5.6.1
Starting discovery in 1 files.
Discovery found 4 tests in 108ms.
Running tests.
Describing ConvertTo-JacocoPackageProjection
  [+] emits the exact projection document for a multi-package fixture 93ms (72ms|20ms)
  [+] derives missed as valid minus covered for lines and branches independently 41ms (40ms|1ms)
  [+] emits zero line counters for a package with no class elements 4ms (3ms|1ms)
  [+] emits a zero BRANCH counter rather than omitting it when no branch data is present 12ms (11ms|1ms)
Tests completed in 557ms
Tests Passed: 4, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
COUNTS passed=4 failed=0 skipped=0
```

PASSED: 4
FAILED: 0
SKIPPED: 0

## Named tests and the criteria they carry

| Test name | Criterion |
|---|---|
| `emits the exact projection document for a multi-package fixture` | AC1 |
| `derives missed as valid minus covered for lines and branches independently` | AC2 |
| `emits a zero BRANCH counter rather than omitting it when no branch data is present` | AC7 |
| `emits zero line counters for a package with no class elements` | AC8 |

## Structural facts about the test file

- `BeforeAll` dot-sources `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` once, and both
  test-only helper functions, `Get-NormalizedProjectionText` and `Get-ProjectionCounterElement`, are
  declared inside that block.
- Every fixture is a here-string assigned to a script-scoped variable cast to an XML document inside
  the `It` block that uses it. No fixture is loaded from a path.
- The file creates, writes and deletes no file. It measures 221 content lines and carries a UTF-8
  byte-order mark.

The exact-shape assertion compares the returned string to a here-string literal after reducing both
sides to line-feed line endings, and nothing else. The two-space indentation per level, the space
before each self-closing slash, the absence of an XML declaration and of a DOCTYPE, the single `name`
attribute on the root `report` element carrying the literal `TaskMaster`, the single `name` attribute
on each `package` child, and the two `counter` children per package in the order LINE then BRANCH are
therefore all compared character for character.

## Output Summary

EXIT_CODE: 0. Four passed, zero failed, zero skipped, for exactly the four named tests this task
specifies.
