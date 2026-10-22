Timestamp: 2026-08-28T22-48
Command: rg -a -i -F -c -- '<literal>' <path>, for each of the three D1 literal classes across the four
sanitized files (excluding p2-t3.trx, per Phase 1 scope)
EXIT_CODE: 0
Output Summary: 4x3 hit-count matrix (all zero):

| File | reporoot_hits | user_hits | host_hits |
|---|---|---|---|
| evidence/remediation-baseline/p0-t6/p0-t6.trx | 0 | 0 | 0 |
| evidence/remediation-baseline/p0-t7/p0-t7.trx | 0 | 0 | 0 |
| evidence/regression-testing/p1-t3/p1-t3.trx | 0 | 0 | 0 |
| evidence/qa-gates/p4-t4/p4-t4.trx | 0 | 0 | 0 |

Total hit count across all twelve sub-checks: 0. Zero files produced a read error. Quoted against
P0-T3's nonzero matrix for the same four files (71/73/37, 2473/2475/1238, 71/73/37, 2475/2477/1239
respectively), confirming the remediation removed every hit that the before-sweep detected.
