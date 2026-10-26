# Baseline Node Suites (Issue #1007)

Timestamp: 2026-10-23T13-41
Command: node --test --test-concurrency=1 --test-reporter=spec --experimental-test-coverage --test-coverage-include=QuickFiler/Resources/FolderBreadcrumb.js tests/quickfiler-breadcrumb/issue-997-common.test.mjs tests/quickfiler-breadcrumb/issue-997-recursive.test.mjs tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs
EXIT_CODE: 0
Output Summary:
- tests 80, pass 80, fail 0, cancelled 0, skipped 0.
- `FolderBreadcrumb.js` line coverage 100.00 percent (branch 100.00, functions 100.00), as reported by the node experimental coverage table.
- File order equals execution order (test concurrency 1): issue-997-common, issue-997-recursive, issue-1002-nesting.
