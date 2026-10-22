# Node Suites: Order Isolation and FolderBreadcrumb.js Coverage (#1002)

Timestamp: 2026-10-22T10-14
Command: node --test --test-isolation=none --test-concurrency=1 --experimental-test-coverage --test-coverage-include=QuickFiler\Resources\FolderBreadcrumb.js --test-reporter=spec <three files in the order below>, run three times in one node process per order (launcher SCRATCH/issue-1002-node-orders.ps1)
EXIT_CODE: 0
Output Summary:
- Every order: tests 80, pass 80, fail 0 (78 from the issue-997 suites plus the 2 new issue-1002 tests).
- `FolderBreadcrumb.js` coverage in every order: line 100.00 percent, branch 100.00 percent, funcs 100.00 percent; P0-T9 baseline was line 100.00 percent, so coverage is unchanged.

| Run | File order | Seed | Exit | Tests / pass / fail |
|---|---|---|---|---|
| normal | issue-997-common, issue-997-recursive, issue-1002-nesting | n/a | 0 | 80 / 80 / 0 |
| reversed | issue-1002-nesting, issue-997-recursive, issue-997-common | n/a | 0 | 80 / 80 / 0 |
| shuffled | issue-1002-nesting, issue-997-recursive, issue-997-common | 1002 | 0 | 80 / 80 / 0 |

The shuffle is a Fisher-Yates permutation driven by `System.Random(1002)` over the normal order. For seed 1002 the permutation coincides with the reversed order; it is recorded as produced, not re-drawn.
