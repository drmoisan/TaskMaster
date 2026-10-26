# Node Suites, Normal and Reversed Order (Issue #1007, remediation cycle 1, P5-T8)

Timestamp: 2026-10-26T00-05
Command: node --test --test-concurrency=1 <the four tests/quickfiler-breadcrumb suites in the order below>, run once per order from the item worktree
EXIT_CODE: 0
Output Summary: Both orders had tests 90, pass 90, fail 0, node exit 0. The count equals the 90 recorded in `node-suites-run.2026-10-25T22-50.md`, so the stylesheet edit left the page-script suites unchanged. Summary derived from the node reporter totals.

| Order | File order (execution order, concurrency 1) | Tests | Pass | Fail | Exit |
|---|---|---|---|---|---|
| normal | issue-997-common, issue-997-recursive, issue-1002-nesting, issue-1007-keys | 90 | 90 | 0 | 0 |
| reversed | issue-1007-keys, issue-1002-nesting, issue-997-recursive, issue-997-common | 90 | 90 | 0 | 0 |

Reference count: `node-suites-run.2026-10-25T22-50.md` records 90 tests per order. Both runs match it.
