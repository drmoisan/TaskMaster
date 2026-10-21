# P3-T11 — AC4 Checked Off

Timestamp: 2026-09-13T06-03
Task: [P3-T11]

Exactly one criterion is checked off by this task: AC4 in
`docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/spec.md`.

Command: pwsh -NoProfile -Command '<Invoke-Pester over the projection test file and the results-directory test file with Run.PassThru, printing the recorded result of each named test this check-off depends on, then the passed, failed and skipped counts>'
EXIT_CODE: 0

```
NAMED| Passed | emits one package element per source package in document order
NAMED| Passed | returns without throwing when the projection totals equal the source root attributes
NAMED| Passed | throws naming the expected and the observed totals when the projection disagrees
NAMED| Passed | builds the projection from the post-processed content rather than the raw collector string
NAMED| Passed | invokes the reconciliation assertion on the coverage path
PESTER_COUNTS passed=18 failed=0 skipped=0
```

## Acceptance mapping

- AC4's checkbox is marked `[x]` in `spec.md`.
- The three-package document-order test from P1-T6,
  `emits one package element per source package in document order`, is recorded as passed.
- The post-processed-source abstract-syntax-tree test from P3-T8,
  `builds the projection from the post-processed content rather than the raw collector string`, is
  recorded as passed.

The remaining three named results in the run above belong to AC5 and are recorded here only because
the same run produced them; they are the evidence P3-T12 cites, not this task's.

## Output Summary

EXIT_CODE: 0. AC4 checked off. Both tests its criterion names are recorded as passed; 18 passed, 0
failed, 0 skipped across the two files the run covered.
