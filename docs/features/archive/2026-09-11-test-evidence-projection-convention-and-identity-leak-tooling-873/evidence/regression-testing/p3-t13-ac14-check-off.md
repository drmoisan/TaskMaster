# P3-T13 — AC14 Checked Off

Timestamp: 2026-09-13T06-03
Task: [P3-T13]

Exactly one criterion is checked off by this task: AC14 in
`docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/spec.md`.

Command: pwsh -NoProfile -Command '<Invoke-Pester over tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 with Run.PassThru and Detailed output, printing the passed, failed and skipped counts>'
EXIT_CODE: 0

```
Describing Test-RawCoverageDocumentRetained
  [+] retains the raw document when the output directory is the repository coverage directory
  [+] discards the raw document for any other output directory
  [+] discards the raw document for a subdirectory of the repository coverage directory

Describing Invoke-MSTestWithCoverageMain discard ordering
  [+] discards only after the threshold assertion, the projection write and the reconciliation assertion
PESTER_COUNTS passed=9 failed=0 skipped=0
```

## Acceptance mapping

- AC14's checkbox is marked `[x]` in `spec.md`.
- All four tests from P3-T7 are recorded as passed:

  | Test name | Recorded |
  |---|---|
  | `retains the raw document when the output directory is the repository coverage directory` | passed |
  | `discards the raw document for any other output directory` | passed |
  | `discards the raw document for a subdirectory of the repository coverage directory` | passed |
  | `discards only after the threshold assertion, the projection write and the reconciliation assertion` | passed |

- The Phase 3 toolchain artifact
  `docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p3-t9-phase3-toolchain.md`
  records, under the heading `Files created, written or deleted by the tests`, that no test in this
  phase created, wrote or deleted a file, and supports that with the post-test porcelain status across
  the whole worktree, which lists no coverage document, no test-result document, no summary and no
  projection.

## Output Summary

EXIT_CODE: 0. AC14 checked off. All four named tests recorded as passed; 9 passed, 0 failed, 0 skipped
over the file; the Phase 3 toolchain artifact records that no test in the phase touched the
filesystem.
