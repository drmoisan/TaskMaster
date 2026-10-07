---
name: project-964-cycle1-test-only-coverage-gap-remediation-seams
description: #964 remediation cycle 1 plan seams - test-only coverage-gap plan needs a dossier not a fail-before run, data-row trx naming, commit-task check-off circularity, no-Bash planning limits
metadata:
  type: project
---

Remediation cycle 1 for #964 (R-1 null/empty engine-key test, R-2 symmetric assertions) was planned test-only in one SinkGuard partial.

- A test-only coverage-gap remediation cannot have a failing run; plan a fail-before exception dossier whose alternative proof is a before/after pair (class-node branch-rate 0.5 then 1, new test name 0 hits then 1, fixture total 43 then 45).
- A `[DataTestMethod]` reports one trx counter per row and no parent counter, so totals rise by the row count. Rows are named `<Method> (<arg>)`; the trx RESULT reader must prefix-match, not equal-match.
- Read the class node attribute in the raw Cobertura document with Grep on an explicit file path using `\x5C` for the backslash in `filename=`; use a distinct runner stage label so the pre-change document survives as the false-before control.
- The final commit task cannot contain its own check-off: stage after the last evidence file, then admit the plan file in the post-commit porcelain with a numstat bound equal to the number of late check-offs.
- No Bash tool in the planning session: git state came from the worktree gitdir `HEAD` and `logs/HEAD` files read with Read; the cycle base SHA came from the reflog.

**Why:** the plan was written without Bash and without a validator tool, so every figure had to be re-derived by Read/Grep.
**How to apply:** reuse for any later test-only remediation cycle of a coordinator-style item.
