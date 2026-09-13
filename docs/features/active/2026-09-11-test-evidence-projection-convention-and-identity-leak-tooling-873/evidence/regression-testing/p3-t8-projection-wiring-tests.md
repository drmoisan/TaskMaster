# P3-T8 — Entry-Point Abstract-Syntax-Tree Tests

Timestamp: 2026-09-13T06-03
Task: [P3-T8]

Two tests were added to
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`.

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 with Run.PassThru and Detailed output, printing the passed, failed and skipped counts and ending with an explicit exit returning 1 when the failed count exceeds zero>'
EXIT_CODE: 0

```
Describing Invoke-MSTestWithCoverageMain projection wiring
  [+] builds the projection from the post-processed content rather than the raw collector string
  [+] invokes the reconciliation assertion on the coverage path
PESTER_COUNTS passed=9 failed=0 skipped=0
```

PASSED: 9 over the whole file, of which both named tests of this task are recorded as passed.
FAILED: 0
SKIPPED: 0

## Acceptance mapping

| Test name | Recorded |
|---|---|
| `builds the projection from the post-processed content rather than the raw collector string` | passed |
| `invokes the reconciliation assertion on the coverage path` | passed |

The first test parses the entry point's abstract syntax tree, locates the command element invoking
`ConvertTo-JacocoPackageProjection`, reads the element bound to its `XmlDocument` parameter, and
asserts that element is a variable expression. It then locates the assignment statement whose
right-hand side invokes `ConvertTo-KoverageCoberturaXml` and asserts the two variable names are
identical, so the argument is the variable that receives the post-processor's return value rather than
the raw collector string. Both halves are required: the first alone would accept any variable, and the
second alone would not connect that variable to the projection call.

The second test asserts a command element invoking `Assert-JacocoProjectionReconciliation` is present
inside the entry point definition, which is the coverage path.

## Output Summary

EXIT_CODE: 0. Both named tests passed; 9 passed, 0 failed, 0 skipped over the file.
