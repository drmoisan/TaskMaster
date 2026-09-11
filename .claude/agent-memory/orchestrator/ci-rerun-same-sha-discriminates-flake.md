---
name: ci-rerun-same-sha-discriminates-flake
description: Before opening a remediation cycle on a red CI check, check run_attempt history — a re-run of the identical head SHA is the decisive flake-vs-real-defect experiment and costs nothing
metadata:
  type: feedback
---

On a CI-failure remediation pass, query `gh api repos/<o>/<r>/actions/runs/<id>` for `run_attempt` and
`gh api .../runs/<id>/attempts/<n>/jobs` BEFORE diagnosing. A re-run of the same head SHA with no
intervening commit is the cleanest discriminator that exists between a flake and a real defect.

**Why:** On issue #799 the caller supplied a strong, specific hypothesis — order-dependent static-seam
pollution exposed by my branch's added tests. Attempt 2 of the same run, on the identical SHA, passed
7108/7108. Identical code, identical test set, identical ordering inputs; only runner scheduling
differed. That single fact refuted the hypothesis outright and made any test-ordering analysis moot.
A re-run had already been triggered and was in flight while I was reading source. See
[[my-own-negative-claims-need-a-scoped-search]] for the converse discipline.

**How to apply:**
- Poll the run first. If `run_attempt > 1`, compare attempt conclusions before touching any file.
- If a re-run is in flight, wait for it. Do NOT commit anything meanwhile — a commit moves the head
  and cancels/supersedes a run that may already be green ([[feedback_commit_before_ci_gate]] is about
  landing commits *before* the gate, not during it).
- Pair the re-run result with a mechanism derivation from the source; the re-run says *whether* it is
  non-deterministic, the source says *why*. Report both.
- Per-test durations in the job log quantify the margin. Grep the test name in the green run and the
  red run: `99 ms` vs `389 ms` against a 250 ms budget settled it numerically.
- A flake in sibling-owned code is not yours to fix. Report it; do not widen your blast radius.
