---
name: preflight-without-build-access-cannot-clear-a-plan
description: Reading-only preflight rounds cleared a plan six times and still missed a blocker that halts execution on a false diagnosis; the first round run with a working build found it immediately. Give at least one preflight round real toolchain access.
metadata:
  type: feedback
---

At least one preflight round on a plan that runs build/test commands MUST be performed by a reviewer
that can actually run them. Reading-only rounds do not substitute, however many you run.

**Why.** On issue #736 the plan cleared `PREFLIGHT: ALL CLEAR` after six rounds, then survived a
post-merge repair and another round — seven rounds, all by reviewers without build access. The first
round run with a working toolchain (2026-09-04) found three defects, one of them a blocker no reading
could have surfaced: the plan had **no NuGet restore step**. The 18 legacy non-SDK projects each carry
a `packages.config`, and a fresh worktree has no `packages` directory, so the plan's own first msbuild
invocation fails with a missing-packages error in 17 projects plus a CS0246 cascade. The damage is not
the stall — it is that P0-T4's failure branch reads any non-zero msbuild exit as *"the analyzer gate is
already red at the merge base"* and instructs the executor to record the diagnostics and stop. The plan
would have halted at its second build task on a diagnosis that was false.

This is exactly the class `atomic-plan-contract` describes as undetectable by reading and covered by
"observe a command's success-case output before asserting over that output, and by nothing else." The
gate rules G1-G9 do not catch it. A reviewer cannot delegate this to the validator.

**How to apply.**

- Before S5, run one preflight round whose prompt explicitly instructs the reviewer to RUN the plan's
  commands and report the observed values, not to reason about them. Name the specific unobserved
  values you want back. On #736 those were the discovered-assembly count, the leaf assembly names, a
  `Compare-Object` numeral, the coverage status string, and the repository-wide rates — all five turned
  out correct, which is itself worth knowing, and the blocker sat somewhere nobody had thought to look.
- Budget it: that round took ~16 minutes because it builds and runs the suite. Then scope the
  CONFIRMING round to a text-and-citation diff review and tell it explicitly not to rebuild, or you pay
  the 16 minutes twice for nothing.
- The round has a side benefit worth planning around: it warms the worktree (SDK bootstrapped, packages
  restored, solution built), so execution starts from a hot tree. It also creates a hazard — record in
  the checkpoint that the reviewer performed the restore, or a later reader mistakes a plan defect for
  a fixed one. The #736 reviewer flagged this against itself, correctly: "the plan would otherwise
  depend on an unrecorded preflight side effect, and would fail in any fresh worktree."

Related: [[convergence-signal-is-systematically-optimistic]] — the reading-only rounds kept returning
`CONVERGENCE: NO FURTHER ROUNDS EXPECTED` while a blocker was still sitting in Phase 0.
