# P2-T2 — Namespace Tests

Timestamp: 2026-09-13T05-48
Task: [P2-T2]

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1 with Run.PassThru, Output.Verbosity Detailed, and an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

## Verbatim result

```
Pester v5.6.1
Starting discovery in 1 files.
Discovery found 2 tests in 103ms.
Running tests.
Describing Get-TrxRunSummary namespace handling
  [+] reads the counters correctly from a namespaced test-result document 78ms (59ms|19ms)
  [+] an unprefixed XPath over the same fixture selects zero nodes 4ms (2ms|1ms)
Tests completed in 458ms
Tests Passed: 2, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
PESTER_COUNTS passed=2 failed=0 skipped=0
```

PASSED: 2
FAILED: 0
SKIPPED: 0

## Acceptance mapping

Both named tests are recorded as passed:
`reads the counters correctly from a namespaced test-result document`, and
`an unprefixed XPath over the same fixture selects zero nodes`.

COMPANION_TEST_ASSERTED_NODE_COUNT: 0

The companion test's asserted node count is exactly zero:

```
Get-UnprefixedResultSummaryNodeCount -TrxContent $script:namespacedTrxFixture |
    Should -Be 0
```

Both tests read the identical fixture, `$script:namespacedTrxFixture`, declared once in the
`BeforeAll` block as a here-string. Its root element declares the default TeamTest namespace. The
companion is what makes the pair meaningful: without it the first test would also pass for a reader
that only works on a document with no namespace, and a namespace regression would then surface as a
run reporting zero of everything rather than as a failure.

## Structural facts about the test file

- `BeforeAll` dot-sources `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` once.
- The single test-only helper, `Get-UnprefixedResultSummaryNodeCount`, is declared inside that block.
- Every fixture is a here-string assigned to a script-scoped variable. No fixture is loaded from a
  path, and no test creates, writes or deletes a file.
- The file measures 69 content lines at this point and carries a UTF-8 byte-order mark.

## Output Summary

EXIT_CODE: 0. Two passed, zero failed, zero skipped. The companion test's asserted node count is 0.
