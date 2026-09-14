# P3-T7 — Retention Predicate and Discard-Ordering Tests

Timestamp: 2026-09-13T06-03
Task: [P3-T7]

Four tests were added to
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`: three that exercise
`Test-RawCoverageDocumentRetained` directly, and one that captures call order through the named
wrapper seams.

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 with Run.PassThru and Detailed output, printing the passed, failed and skipped counts and ending with an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

```
Describing Test-RawCoverageDocumentRetained
  [+] retains the raw document when the output directory is the repository coverage directory
  [+] discards the raw document for any other output directory
  [+] discards the raw document for a subdirectory of the repository coverage directory

Describing Invoke-MSTestWithCoverageMain discard ordering
  [+] discards only after the threshold assertion, the projection write and the reconciliation assertion
PESTER_COUNTS passed=7 failed=0 skipped=0
```

PASSED: 7 over the whole file, of which the four named tests of this task are all recorded as passed.
FAILED: 0
SKIPPED: 0

## Acceptance mapping

| Test name | Recorded |
|---|---|
| `retains the raw document when the output directory is the repository coverage directory` | passed |
| `discards the raw document for any other output directory` | passed |
| `discards the raw document for a subdirectory of the repository coverage directory` | passed |
| `discards only after the threshold assertion, the projection write and the reconciliation assertion` | passed |

The subdirectory case is not redundant with the unrelated-directory case. The predicate is specified
as an equality between the output path's full parent directory and the repository root joined with
`coverage`, and a containment implementation would return true for a subdirectory of that tree while
still returning false for the unrelated directory the second test supplies, so without this case a
containment implementation would pass both other tests. The subdirectory the test uses is the
repository root joined with `coverage\tm873-external-output`, which is the same directory P6-T3 runs
against, so a containment implementation would additionally suppress the discard there.

The ordering test captures call order into the script-scoped collection `$script:callOrder` through
mocks of `Assert-CoberturaLineCoverageThreshold`, the projection write (a `Set-Content` mock filtered
to the projection path), `Assert-JacocoProjectionReconciliation` and `Remove-Item`, then asserts the
discard entry's index is greater than the index of each of the other three. It also asserts each of
the other three indices is greater than -1, so the comparison cannot pass vacuously against an absent
entry. The entry point is invoked with a coverage output beneath
`coverage\tm873-external-output`, which the predicate reports as not retained, so the discard is
actually reached on that path.

## No test creates, writes or deletes a file

Command: pwsh -NoProfile -Command '<search the test file for path-based fixture loads and filesystem writes, and print the byte-order-mark state and the line count>'
EXIT_CODE: 0

```
PATH_FIXTURE_LOADS=0
BOM=True
LINES=268
```

The search covered `Get-Content -LiteralPath`, `Get-Content -Path`, `Import-Clixml`,
`[System.IO.File]`, `[System.IO.Directory]`, `New-Item`, `Out-File`, `Set-Content -LiteralPath`,
`Set-Content -Path` and `Remove-Item -LiteralPath`/`-Path`, and returned zero matches. Every fixture
in the file is a here-string assigned to a script-scoped variable in the `BeforeAll` block, and every
filesystem command the entry point would reach is mocked, so the file contains no path-based fixture
load and no test touches the filesystem.

## Output Summary

EXIT_CODE: 0. All four named tests passed; 7 passed, 0 failed, 0 skipped over the file; zero
path-based fixture loads; the discard was reached and observed to follow all three of the threshold
assertion, the projection write and the reconciliation assertion.
