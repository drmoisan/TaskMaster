# P3-T6 — Results-Directory Switch and Default Tests

Timestamp: 2026-09-13T06-03
Task: [P3-T6]

`tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1` was created with three
tests. The file carries a UTF-8 byte-order mark, opens with `Set-StrictMode -Version Latest`, uses
advanced functions with `[CmdletBinding()]` for its two test-only readers, and uses singular nouns.

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 with Run.PassThru and Detailed output, printing the passed, failed and skipped counts and ending with an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

```
Describing Get-DotnetCoverageArgumentList results directory and log file name
  [+] includes the explicit results directory and trx log file name in the coverage argument list
  [+] places both new switches after the argument separator

Describing Invoke-MSTestWithCoverageMain results-directory default
  [+] defaults the coverage entry-point results directory beneath the repository coverage directory
PESTER_COUNTS passed=3 failed=0 skipped=0
```

PASSED: 3
FAILED: 0
SKIPPED: 0

## Acceptance mapping

All three named tests are recorded as passed:

| Test name | Recorded |
|---|---|
| `includes the explicit results directory and trx log file name in the coverage argument list` | passed |
| `places both new switches after the argument separator` | passed |
| `defaults the coverage entry-point results directory beneath the repository coverage directory` | passed |

- The first test calls the builder directly and asserts the returned array contains
  `/ResultsDirectory:` carrying the supplied directory and `/Logger:trx;LogFileName=` carrying the
  supplied explicit log file name.
- The second asserts by index comparison that the index of each is strictly greater than the index of
  the argument separator element, rather than merely asserting presence.
- The third reads the results-directory parameter default from `Invoke-MSTestWithCoverageMain`'s
  abstract syntax tree and asserts the default text is exactly `'coverage\test-results'`, including
  the single quotes, rather than invoking the entry point.

## Output Summary

EXIT_CODE: 0. Three named tests, all passed, zero failed, zero skipped.
