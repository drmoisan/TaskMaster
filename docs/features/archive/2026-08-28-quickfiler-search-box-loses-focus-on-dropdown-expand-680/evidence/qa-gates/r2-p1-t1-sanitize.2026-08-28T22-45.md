Timestamp: 2026-08-28T22-45
Command: D1 `Set-TrxSanitized` routine applied via one pwsh invocation looping over the four target
paths, failing on first exception
EXIT_CODE: 0
Output Summary: Routine completed for all four files with no exception. Before/after byte-count table
(bytes, not literal values):

| File | BytesBefore | BytesAfter |
|---|---|---|
| evidence/remediation-baseline/p0-t6/p0-t6.trx | 48988 | 46375 |
| evidence/remediation-baseline/p0-t7/p0-t7.trx | 1716479 | 1624992 |
| evidence/regression-testing/p1-t3/p1-t3.trx | 48988 | 46375 |
| evidence/qa-gates/p4-t4/p4-t4.trx | 1719403 | 1627842 |

For every one of the four files, BytesAfter is strictly less than BytesBefore. A follow-up sweep for the
escaped placeholder `&lt;repo-root&gt;` confirms nonzero hits landed in all four files (71, 2473, 71,
2475 respectively), and `git status --porcelain` shows all four files as modified.
