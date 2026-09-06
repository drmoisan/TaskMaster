---
name: expectedexitcode-declared-from-baseline-not-observed-run
description: A carried-blocker ExpectedExitCode rule keyed off the recorded Phase 0 baseline (rather than off whether the blocker recurred in this run) is vacuous toward new regressions and unsatisfiable when the blocker does not recur
metadata:
  type: project
---

When a plan authorizes a non-zero exit for a carried baseline blocker, the rule that selects
`ExpectedExitCode:` must key off **whether the blocker recurred in this run**, not off the
recorded Phase 0 baseline value. Keying off the baseline breaks in both directions at once:

- **Vacuous direction.** "When `BASELINE_FAILURE_SET:` is a name list, declare `ExpectedExitCode: 1`"
  admits *any* non-zero exit, including one caused by a brand-new failure this change introduced.
  The gate cannot distinguish a carried blocker from a regression.
- **Unsatisfiable direction.** If the baseline failures were flaky and do not recur, the run exits 0
  while the artifact declares 1. An acceptance clause reading "the observed `EXIT_CODE:` equals that
  declared expectation" then fails deterministically, and a Phase-6-style restart rule turns that
  into an unbounded loop with no defined escape (each iteration is a full format + two `/t:Rebuild`
  + two full-suite runs).

**Why:** the `ExpectedExitCode:` schema field normalizes observed==expected to `pass`, so the
declaration is only truthful if it describes this run's cause. See
[[project_exact_count_gate_vs_remediation_loop]] for the same loop shape from a pinned count.

**How to apply:** rewrite rule one as "when this run reports at least one Failed test and every
Failed test name it reports appears on `BASELINE_FAILURE_SET:`", and rule two as "when this run
reports no Failed test AND this run's own coverage figure is below the floor". Then add the Failed
name list to the artifact's recorded fields, because the rule now reads it. Do **not** "fix" it by
declaring the expectation to be whatever was observed — that makes the equality clause vacuous.

**Sibling tell.** The defect is visible without running anything: every other carried-blocker gate
in the same plan says a non-zero exit "is authorized for that reason only" (permissive, accepts 0),
and only the broken one states an equality. An asymmetry between one gate and its siblings is the
cheapest signal that the strict one was written against the schema rather than against the run.

**Companion arity defect in the same family.** A producing task that enumerates "every line whose
hit count is 0" (an upper bound, 0..N) paired with a consuming task asserting "all three of which
match the three lines the producer enumerated" strands the consumer whenever fewer than N come out
zero-hit. Whether a null-coalesced default lambda reports a zero-hit line at all depends on how the
formatter wraps it, so the count is not knowable at planning time. State the consumer as "the
zero-hit set this artifact enumerates is identical to the set the producer enumerated" plus a
separate named list of permitted lines.
