---
name: cobertura-substitution-must-happen-precommit
description: Converting committed raw Cobertura to JaCoCo does NOT keep the blobs out of history — this run's PRs are merged with merge commits, so the pre-conversion commit stays reachable from main. Intercept before the executor's commit.
metadata:
  type: project
---

Deleting a raw Cobertura report in a later commit does **not** keep it out of `main`. Verified end to
end on #648, 2026-09-01.

**What happened.** The atomic-executor committed two ~10.6 MB `.cobertura.xml` reports in `8d933975`.
I converted them to package-level JaCoCo projections and removed them in `08868ba0`, per
[[jacoco-not-cobertura-for-evidence]]. Feature review correctly flagged that the blobs were still
reachable and recommended a squash merge. **The PR was merged with a merge commit** (two parents:
`git log -1 --format=%P` on the merge showed both the base and the branch tip), so `8d933975` became
an ancestor of `origin/main` and 21,191,113 bytes are now permanent.

**The tell, before you rely on a squash:** check how the run's earlier siblings were merged.
`git log --merges --oneline origin/main` showed `Merge pull request #716/#717/#718` — merge commits,
every one. A squash produces no such subject line. On a parallel run the merge method is the parent
orchestrator's, not yours, and asking for a squash in the PR body does not bind it.

**How to apply.** The conversion must happen **before** the executor's commit, not after it. Two
workable interceptions:

1. Tell the executor in the delegation prompt to write the coverage evidence as a JaCoCo projection
   directly, and to keep the raw Cobertura only in the gitignored `coverage/` directory.
2. Failing that, run the substitution while the raw files are still *untracked or staged* — i.e.
   between the executor finishing and its own `git add`. A `git reset --soft` after the fact is the
   only remaining fix and it is usually unavailable, because the three feature-review audits cite the
   pre-conversion SHA as their own evidence for the finding, so rewriting history destroys the audit
   trail that documents it.

**Do not over-weight this at merge time.** `origin/main` already carries **281** `.cobertura.xml`
files *present in the tree*, which is strictly worse than a blob reachable only through history. A
converted branch is an improvement over the prevailing pattern even when the merge method defeats the
history goal. Report it as merge-method-dependent with that context, and do not treat it as blocking.

Related: [[jacoco-not-cobertura-for-evidence]], [[csharp-coverage-denominator-two-figures]],
[[external-actor-can-merge-your-child-pr-midrun]].
