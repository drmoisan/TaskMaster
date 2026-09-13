# P2-T3 — Skipped-Derivation Tests

Timestamp: 2026-09-13T05-50
Task: [P2-T3]

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1 with Run.PassThru, Output.Verbosity Detailed, and an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

## Verbatim result

```
Discovery found 4 tests in 102ms.
Describing Get-TrxRunSummary namespace handling
  [+] reads the counters correctly from a namespaced test-result document 78ms
  [+] an unprefixed XPath over the same fixture selects zero nodes 3ms

Describing Get-TrxRunSummary skipped derivation
  [+] reports skipped as total minus executed and preserves the verbatim platform figures 11ms
  [+] states the skipped derivation in the formatted output 12ms
Tests completed in 478ms
Tests Passed: 4, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
PESTER_COUNTS passed=4 failed=0 skipped=0
```

PASSED: 4
FAILED: 0
SKIPPED: 0

## Acceptance mapping

Both tests this task adds are recorded as passed:
`reports skipped as total minus executed and preserves the verbatim platform figures`, and
`states the skipped derivation in the formatted output`.

The derivation fixture declares total 12 against executed 7, so total exceeds executed and the
derived skipped figure is 5. Its notExecuted figure is 4, deliberately different from 5, so an
implementation that copied the platform's notExecuted count instead of subtracting would fail this
test rather than coincide with it. The not-executed and inconclusive figures, 4 and 2, are asserted to
be carried through unchanged, alongside error 1, timeout 0, aborted 0, passed 5 and failed 2.

The formatter test asserts the rendered string matches `derived as total minus executed`, `Skipped 5`,
`notExecuted 4` and `inconclusive 2`, so the derivation is stated in words in the output and no
reported figure is lost.

The test file measures 120 content lines at this point.

## Output Summary

EXIT_CODE: 0. Four passed, zero failed, zero skipped.
