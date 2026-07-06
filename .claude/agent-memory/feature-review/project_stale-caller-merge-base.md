---
name: stale-caller-merge-base
description: A caller-supplied merge-base SHA can be stale when main advanced (another PR merged) between when it was computed and when feature-review runs; always recompute via git merge-base
metadata:
  type: project
---

On the issue #244 cycle-2 re-audit (2026-07-06), the delegating prompt supplied merge-base `961a768e0b093ec468c8180c9dc53996e1e6421a`. `git merge-base HEAD origin/main` resolved to `b5f279624377cc82b884bb24ff81c46c899f3e6d`, one full merged PR (#245, `bug/app-events-readiness-comexception-242`) ahead of the supplied SHA. Using the stale SHA as the diff base would have pulled `TaskMaster.Test/AppGlobals/HookReadinessCoordinatorTests.cs` and `UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs` (issue #242's already-merged, unrelated changes) into the #244 audit's scope — a scope-widening error caused by a stale input, not a narrowing attempt.

**Why:** `main` moves between when an orchestrator/planner computes a merge-base and when feature-review actually runs, especially across multi-cycle remediation loops on a long-lived worktree. Trusting the caller-supplied SHA verbatim risks silently auditing the wrong diff (either widened with already-merged unrelated commits, or narrowed if `main` had moved the other direction).

**How to apply:** Always independently recompute the merge-base via `git merge-base HEAD origin/main` (after `git fetch origin main`) regardless of what SHA the delegating prompt supplies, per the Scope Invariant's authoritative-source list. If the recomputed value differs from the supplied value, do not treat this as caller "scope narrowing" to reject — it is a staleness correction. Record the discrepancy and the verification method (e.g., `git merge-base --is-ancestor <supplied> <resolved>` plus the intervening `git log --oneline <supplied>..<resolved>`) in the policy audit for transparency, then proceed with the diff computed from the correctly resolved merge-base. Cross-check the resulting file list against `artifacts/pr_context.appendix.txt`'s independently-generated diffstat as a second confirmation.
