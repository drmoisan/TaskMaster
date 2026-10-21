# P4-T5 — Plain Results-Directory Tests

Timestamp: 2026-09-13T06-18
Task: [P4-T5]

Command: pwsh -NoProfile -Command '<Invoke-Pester limited to tests/scripts/vscode/Invoke-MSTest.ResultsDirectory.Tests.ps1 with Run.PassThru, printing the passed, failed and skipped counts, ending with an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

```
Discovery found 3 tests in 112ms.
Describing Get-VsTestArgumentList results directory and log file name
  [+] includes the explicit results directory and trx log file name in the vstest argument list
Describing Invoke-MSTestMain results-directory default
  [+] defaults the entry-point results directory beneath the repository coverage directory
Describing Invoke-MSTestMain non-fatal summary path
  [+] warns without writing a summary when the test-result document cannot be read
Tests completed in 993ms
Tests Passed: 3, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
COUNTS passed=3 failed=0 skipped=0
```

PASSED: 3
FAILED: 0
SKIPPED: 0

All three named tests passed, which is this task's acceptance.

## What each test asserts

`includes the explicit results directory and trx log file name in the vstest argument list` calls the
builder directly through a splatted hashtable and asserts membership of both switches by index
comparison: the results-directory switch index is greater than -1 and the logger switch index is greater
than the results-directory index, so the assertion pins the order as well as the presence.

`defaults the entry-point results directory beneath the repository coverage directory` reads the
`ResultsDirectory` parameter of `Invoke-MSTestMain` from the parsed abstract syntax tree, asserts exactly
one such parameter exists, and asserts its default value text is byte-for-byte the single-quoted literal
`'coverage\test-results'`. The entry point is not invoked, so the assertion depends on the declared
default and not on any mock arrangement.

`warns without writing a summary when the test-result document cannot be read` mocks the content reader
to throw, which is what a missing test-result document does, then runs the entry point to completion and
asserts `Set-Content` and `Remove-Item` are each invoked exactly zero times and that exactly one warning
was emitted whose text begins `Test-result summary was not written:`. The run itself succeeds, which is
the point: an unreadable test-result document must neither fail the run, nor leave a summary behind, nor
discard the document whose summary was never written.

## File conventions and hygiene

The file carries a UTF-8 byte-order mark, opens with `Set-StrictMode -Version Latest`, declares its one
test-only helper `Get-PlainEntryPointAst` inside the `BeforeAll` block as an advanced function with
`[CmdletBinding()]` and a singular noun, and uses carriage-return line-feed endings. It measures 119
content lines, against the repository ceiling of 500.

NO_TEST_WRITES_TO_DISK: true. No test creates, writes or deletes a file, and no fixture is loaded from a
path: every value is an in-memory literal or the parsed tree of a production file that is read and never
written. `Set-Content` and `Remove-Item` are both mocked and both asserted at zero invocations.

Output Summary: 3 passed, 0 failed, 0 skipped. The membership test, the parameter-default test and the
non-fatal-path test all pass, and no test in the file touches the filesystem.
