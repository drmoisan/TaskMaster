Timestamp: 2026-08-25T14-50

### Acceptance Criteria Status
- Source: `spec.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0

Final reconciliation: all final C# QA and coverage gates passed.

1. PASS: hierarchy lookup regression evidence remains intact.
2. PASS: direct selection regression evidence remains intact.
3. PASS: segment-activation regression evidence remains intact.
4. PASS: single-prefix EmailFilerConfig regression evidence remains intact.
5. PASS: existing behavior coverage remains intact.
6. PASS: no `@` parsing or `Store.FilePath` change.
7. PASS: fail-before evidence (`evidence/regression-testing/issue-609-case-variant-fail-before.2026-08-25T14-29.md`), focused case-variant and exact-case pass (`evidence/regression-testing/issue-609-case-variant-post-fix.2026-08-25T14-29.md`), and scope conformance (`evidence/other/issue-609-case-variant-scope-conformance.2026-08-25T14-29.md`) verify the constrained correction and aligned score projection.
8. PASS: final C# format, analyzer, nullable, coverage, comparison, and diff-check passed. Evidence: `evidence/qa-gates/csharpier-format-final.2026-08-25T14-29.md`, `csharpier-check-final.2026-08-25T14-29.md`, `csharp-analyzers-final.2026-08-25T14-29.md`, `csharp-nullable-final.2026-08-25T14-29.md`, `csharp-tests-coverage-final.2026-08-25T14-29.md`, `issue-609-case-variant-coverage-comparison.2026-08-25T14-29.md`, and `issue-609-case-variant-diff-check.2026-08-25T14-29.md`.
