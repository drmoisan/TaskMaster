# Node Suites in Five File Orders (Issue #1007, P18-T6)

Timestamp: 2026-10-25T22-50
Command: node --test --test-concurrency=1 --test-reporter=spec [--experimental-test-coverage --test-coverage-include=QuickFiler/Resources/FolderBreadcrumb.js on the normal run] <the four tests/quickfiler-breadcrumb suites in the order below>; the shuffled orders come from a helper under SCRATCH that applies a Fisher-Yates shuffle driven by System.Random(seed), so a seed always yields the same order
EXIT_CODE: 0
Output Summary: five orders (normal, reversed, and shuffles with seeds 1007, 20261025 and 424242). Each run: tests 90, pass 90, fail 0, node exit 0. `FolderBreadcrumb.js` line coverage on the normal run: 100.00 percent (branch 100.00, functions 100.00), against 100.00 percent at P0-T7 (baseline-node-suites.2026-10-23T13-41.md), so coverage is unchanged while the script grew from 350 to 393 lines. Summary derived from the node spec output and its coverage table.

| Order | Seed | File order (execution order, concurrency 1) | Tests | Pass | Fail | Exit |
|---|---|---|---|---|---|---|
| normal (coverage) | - | issue-997-common, issue-997-recursive, issue-1002-nesting, issue-1007-keys | 90 | 90 | 0 | 0 |
| reversed | - | issue-1007-keys, issue-1002-nesting, issue-997-recursive, issue-997-common | 90 | 90 | 0 | 0 |
| shuffle | 1007 | issue-1002-nesting, issue-997-recursive, issue-997-common, issue-1007-keys | 90 | 90 | 0 | 0 |
| shuffle | 20261025 | issue-1007-keys, issue-997-recursive, issue-1002-nesting, issue-997-common | 90 | 90 | 0 | 0 |
| shuffle | 424242 | issue-1002-nesting, issue-1007-keys, issue-997-common, issue-997-recursive | 90 | 90 | 0 | 0 |

| Coverage of `QuickFiler/Resources/FolderBreadcrumb.js` | Line % | Branch % | Functions % |
|---|---|---|---|
| P0-T7 baseline (3 suites) | 100.00 | 100.00 | 100.00 |
| P18-T6 normal run (4 suites) | 100.00 | 100.00 | 100.00 |
