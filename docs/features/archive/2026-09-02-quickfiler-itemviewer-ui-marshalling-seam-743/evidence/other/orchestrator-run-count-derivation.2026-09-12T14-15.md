# Orchestrator Derivation: Sufficient Run Count (Issue #743, Acceptance Criterion 3)

Timestamp: 2026-09-12T14-15
Collected by: orchestrator (preparation mode)
Method: closed-form arithmetic over the base rate recorded in issues #592, #511 and #571
EXIT_CODE: 0

## The recorded figures

- Pre-fix run-level failure rate: approximately 1 in 21, that is approximately 4.8 percent.
- The issue record asserts that thirty consecutive clean runs has probability approximately `0.952^30`,
  approximately 0.23, under the null hypothesis of no effect.

## Verification of the recorded figure

Let `q` be the per-run pass probability under the null, `q = 1 - 0.048 = 0.952`.

`ln(0.952) = -0.0491903`
`30 * (-0.0491903) = -1.4757`
`exp(-1.4757) = 0.2286`

So `0.952^30 = 0.229`, which rounds to the 0.23 the issue record states. The recorded figure is CONFIRMED.
Thirty clean runs leaves an approximately one-in-four chance of arising with no fix at all, so it is not
sufficient evidence of efficacy.

## Derivation of a sufficient run count

The test is: observe `N` consecutive clean runs and reject the null of no effect when the probability of
that observation under the null falls below a significance level `alpha`.

Required: `q^N <= alpha`, therefore `N >= ln(alpha) / ln(q)`.

With `q = 0.952`:

| alpha | `ln(alpha) / ln(q)` | Smallest integer N |
|---|---|---|
| 0.05 | 2.995732 / 0.0491903 = 60.90 | 61 |
| 0.01 | 4.605170 / 0.0491903 = 93.62 | 94 |

With the unrounded base rate `1/21`, so `q = 20/21 = 0.952381` and `ln(q) = -0.0487902`:

| alpha | `ln(alpha) / ln(q)` | Smallest integer N |
|---|---|---|
| 0.05 | 61.40 | 62 |
| 0.01 | 94.39 | 95 |

**Recommended acceptance figure: 62 consecutive clean runs for `alpha = 0.05`.** Choosing the unrounded
base rate is the conservative option of the two, and 62 is therefore defensible whichever rounding a
reviewer applies. If a reviewer demands `alpha = 0.01`, the figure is 95.

## Sensitivity note

The bound is highly sensitive to the base rate, which is itself estimated from roughly 21 runs and so
carries wide uncertainty. A reviewer may reasonably object that a point estimate of 4.8 percent from
about 21 observations does not pin `N` precisely. The honest statement is that 62 clean runs rejects the
no-effect null at the 5 percent level GIVEN a 4.8 percent base rate, and that the base rate itself is
an estimate.

COMPLETION OF THE SENSITIVITY NOTE (added 2026-09-13, maintainer ratification condition 3). The paragraph
above is necessary but not sufficient, because it describes the uncertainty qualitatively without stating
its provenance or its magnitude. Both are stated here.

**Provenance.** The 4.8 percent figure is a point estimate derived from a SINGLE observed failing run —
one failure in approximately 21 runs. It is not an average over repeated failures. A rate estimated from
one event is the weakest form the estimate can take, and the wide interval below is the direct arithmetic
consequence of that single event rather than an incidental caveat.

**Magnitude.** The exact (Clopper-Pearson) 95 percent confidence interval for 1 of 21 is approximately
**[0.0012, 0.2382]**, that is 0.12 percent to 23.82 percent. Both endpoints were re-derived here rather
than carried over:

- Lower endpoint, solving `1 - (1 - p)^21 = 0.025`: `(1 - p) = 0.975^(1/21) = exp(-0.0253178 / 21) =
  exp(-0.00120561) = 0.9987951`, so `p = 0.0012049`.
- Upper endpoint, solving `P(X <= 1 | n = 21, p) = 0.025`, that is `(1-p)^21 + 21p(1-p)^20 = 0.025`. At
  `p = 0.2382`: `(0.7618)^20 = 0.004330`, `(0.7618)^21 = 0.0032993`, `21 x 0.2382 x 0.004330 = 0.021660`,
  sum `= 0.024959`, which meets the 0.025 target. So `p = 0.2382`.

**Consequence, stated plainly.** The interval spans a factor of roughly 200. If the true per-run failure
rate sits near the LOW end of that interval — 0.12 percent rather than 4.8 percent — then the expected
number of failures in 62 runs is about 0.07, so observing zero failures in 62 runs is the overwhelmingly
likely outcome even if the fix did nothing at all, and the streak establishes little. The `N` values
derived in the tables above (62 at alpha 0.05, 95 at alpha 0.01) are conditional on the point estimate
and are not confidence-adjusted. Against the low endpoint the run count required to reach the same
alpha 0.05 would be `ln(0.05) / ln(1 - 0.0012049) = 2.99573 / 0.00120563`, approximately **2,500** runs,
which is not achievable in any form within this item. The record must not imply more confidence than the measurement supports: the statistical
component of AC3 is SUPPORTING evidence conditional on an uncertain base rate, and the blocking weight
of AC3 rests on component (a), the deterministic single-run assertion, which has no base rate at all.

This completion is recorded in both places the figures are used: here, and in the acceptance-governing
artifact `evidence/regression-testing/ac3b-consecutive-runs.2026-09-12T19-00.md`, which already carried
the provenance sentence and the interval as spec AC3 requires.

## Feasibility consequence for the plan, which is the reason this matters

Sixty-two consecutive full-suite runs is very likely infeasible within this item. The plan must not
adopt it uncritically. The acceptance criteria split cleanly into two claims requiring different
evidence, and conflating them is the failure mode to avoid:

1. **Mechanism identification and deterministic reproduction** (criteria 1 and 2). A deterministic
   regression test needs exactly ONE run to demonstrate it, because a deterministic test has no base
   rate. Criterion 2 explicitly forbids a sleep, a retry, and a timing tolerance, which is precisely the
   demand that the reproduction be deterministic rather than statistical. This is the cheap and rigorous
   path and it should carry most of the evidential weight.
2. **Efficacy against the historical flake rate** (criterion 3). This is the expensive statistical claim
   and it is the one the 62-run figure governs.

If criterion 2 is satisfied by a genuinely deterministic reproduction, criterion 3 can be discharged
against the targeted reproduction rather than against the full nine-assembly suite, which lowers the
per-run cost by orders of magnitude and makes a run count in the sixties achievable. The plan must state
explicitly which scope each run count applies to, because "62 runs" against the full instrumented suite
and "62 runs" against one test class are different commitments by a wide margin.

## Evidence-convention interaction

The maintainer decision on issue #671 dated 2026-09-11 requires that PROJECTIONS ONLY be committed: no
new `.trx` and no new `.cobertura.xml` may be written into the repository. With a run count in the
sixties this is not a minor point. Sixty-two raw result files must not be committed. The plan must record
aggregate numeric outcomes inside Markdown evidence artifacts and discard the raw tool output.

## Output Summary

The recorded `0.952^30 = 0.23` figure is confirmed by independent calculation. The smallest sufficient
run count is 62 consecutive clean runs at `alpha = 0.05`, or 95 at `alpha = 0.01`, using the conservative
unrounded 1-in-21 base rate. The plan must scope that count to a targeted deterministic reproduction
rather than the full suite, and must commit aggregate figures in Markdown rather than raw result files.
