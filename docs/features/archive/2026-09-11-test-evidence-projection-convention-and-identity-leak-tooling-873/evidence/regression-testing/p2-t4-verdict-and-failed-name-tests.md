# P2-T4 — Verdict, Empty-Collection and Negative-Path Tests

Timestamp: 2026-09-13T05-52
Task: [P2-T4]

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1 with Run.PassThru, Output.Verbosity Detailed, and an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

## Verbatim result

```
Discovery found 7 tests in 120ms.
Describing Get-TrxRunSummary namespace handling
  [+] reads the counters correctly from a namespaced test-result document 98ms
  [+] an unprefixed XPath over the same fixture selects zero nodes 4ms

Describing Get-TrxRunSummary skipped derivation
  [+] reports skipped as total minus executed and preserves the verbatim platform figures 12ms
  [+] states the skipped derivation in the formatted output 13ms

Describing Get-TrxRunSummary verdict and failed names
  [+] reports the run verdict and the names of the failed results 6ms
  [+] returns an empty failed-name collection for a fixture with no result elements 8ms
  [+] throws a specific message for a document with no result-summary node 22ms
Tests completed in 625ms
Tests Passed: 7, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
PESTER_COUNTS passed=7 failed=0 skipped=0
```

PASSED: 7
FAILED: 0
SKIPPED: 0

## Acceptance mapping

All three tests this task adds are recorded as passed:
`reports the run verdict and the names of the failed results`,
`returns an empty failed-name collection for a fixture with no result elements`, and
`throws a specific message for a document with no result-summary node`.

The verdict fixture carries four results of four different outcomes, two of them Failed, so the
assertion that the returned names are exactly `Contoso.Gamma.FailsFirst` and
`Contoso.Gamma.FailsSecond` in document order rules out both an implementation that returned every
result name and one that returned only the first failure. The verdict itself is asserted to equal the
result-summary outcome attribute value `Failed`.

The empty-collection test asserts three separate things without piping the value into `Should`:
that the value is not null, that it is an array, and that its element count is 0. Piping is avoided
deliberately, because piping an empty collection sends no input at all and would make an empty result
indistinguishable from a null one.

The negative-path test supplies a fixture that parses as XML and carries a result element, so the only
thing absent is the result-summary node, and asserts the thrown message matches
`*has no <ResultSummary> node.*`.

The test file measures 193 content lines after this task.

## Output Summary

EXIT_CODE: 0. Seven passed, zero failed, zero skipped.
