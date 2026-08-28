Timestamp: 2026-08-28T22-33
Command: rg -a -i -F -c -- '<literal>' <path> (run once per D1 literal class — worktree-root prefix,
$env:USERNAME value, $env:COMPUTERNAME value — against each of the five named files; literal values
resolved fresh per D1, never hardcoded)
EXIT_CODE: 0
Output Summary: R2_BEFORE_MATRIX (hit counts only, no literal values reproduced):

| File | reporoot_hits | user_hits | host_hits |
|---|---|---|---|
| evidence/remediation-baseline/p0-t6/p0-t6.trx | 71 | 73 | 37 |
| evidence/remediation-baseline/p0-t7/p0-t7.trx | 2473 | 2475 | 1238 |
| evidence/regression-testing/p1-t3/p1-t3.trx | 71 | 73 | 37 |
| evidence/regression-testing/p2-t3/p2-t3.trx | 73 | 75 | 38 |
| evidence/qa-gates/p4-t4/p4-t4.trx | 2475 | 2477 | 1239 |

Every one of the five files returns a nonzero hit count for all three literal classes (not merely "at
least one" as the acceptance condition requires) — the leak is real and the search mechanism detects it.
This is the D8 positive control for Phase 1/2's later zero-hit claims.
