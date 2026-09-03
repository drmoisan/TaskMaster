---
name: repo-wide-cobertura-line-rate-is-nondeterministic
description: The root coverage/@line-rate this repo's dotnet-coverage merge produces is not reproducible across runs of an identical tree — never gate on it unbranched; branch on lines-valid comparability
metadata:
  type: feedback
---

Never write an unbranched direction bar ("post-change repository line-rate >= baseline") against the root `coverage` element's `line-rate` in `coverage\coverage.cobertura.xml`. `dotnet-coverage`'s cross-assembly merge is order- and parallelism-sensitive, so the figure moves across runs of an identical tree.

**Why:** two documented instances on this harness. Same pre-change tree, two runs: 47.16% at `lines-valid=180246` versus 81.02% at `lines-valid=97933`. A baseline/final-QC pair whose diff added roughly 150 production lines: 70.32% at `lines-valid=82070` versus 85.25% at `lines-valid=64124`. The swing is the merged denominator changing by a factor of nearly two, not coverage changing. Any plan whose change adds tens of lines is gating a quantity dominated by merge noise, so the gate fails for reasons unrelated to whether the change is correct.

**How to apply:** capture `lines-valid` alongside `line-rate` in the Phase 0 baseline task, then make the final-QC comparison two-branch:

- **Branch A, comparable denominators** — the two `lines-valid` figures differ by at most 1 percent of the baseline figure: gate the rate, allowing a small tolerance (0.5 percentage points) rather than strict non-decrease.
- **Branch B, incomparable denominators** — they differ by more than 1 percent: the two rates were computed over different instrumented denominators. Record the comparison but do not gate it, and require the artifact to say so in one sentence.

The plan must require exactly one branch to hold and to be named in the artifact, so the branch is auditable rather than an escape hatch. Per-class and per-file figures are unaffected by the merge non-determinism and stay hard gates — put the real no-regression weight on the per-changed-line comparison ([[deletion-adjusted-coverage-no-regression-gate]]), which needs a Phase 0 copy of the baseline per-line hits because the final run overwrites the XML in place.

Related: [[project-441-cobertura-arithmetic-plan-seams]], [[async-state-machine-coverage-aggregation]], [[project-494-threshold-reconciliation-plan-seams]].
