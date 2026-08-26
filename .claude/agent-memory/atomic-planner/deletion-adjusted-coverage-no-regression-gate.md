---
name: deletion-adjusted-coverage-no-regression-gate
description: "Reverts/removals that delete fully-covered lines make a raw-rate `post >= baseline` coverage gate arithmetically unsatisfiable; gate on covered/valid counters instead"
metadata:
  type: project
---

When a plan cycle deletes fully-covered production lines (a revert, dead-code removal, or helper deletion), a raw-rate no-regression gate `line-rate_post >= line-rate_base` is unsatisfiable by arithmetic: removing 100%-covered lines from a pool below 100% lowers the aggregate rate even when every surviving line keeps its coverage. A one-re-run allowance does not help — the miss is deterministic, not measurement noise. This is the mirror image of a gate that cannot fail: both fail to measure what they claim (#614 cycle-2 B-2; miss was -0.0020 pp).

Correct gate form, per counter (lines and branches separately), read from the Cobertura roots before and after:

- `valid_post <= valid_base` (the change only removes from the denominator), AND
- `covered_post >= covered_base - (valid_base - valid_post)` (every removed line/branch was covered; no retained one regressed).

Report the raw rates informationally with the arithmetic tying any decrease to the deleted covered lines; a raw rise also satisfies the gate. Keep the single re-run allowance only for a counter-gate miss (denominator nondeterminism).

**Why:** #614 remediation cycle 2 preflight verified the projected raw miss independently and returned REVISIONS REQUIRED; the branch-rate comparison was within the observed ±0.002 pp run-to-run spread, i.e. a coin flip.
**How to apply:** whenever a plan's change set deletes covered production lines, never write `rate_post >= rate_base` as a gate. Pre-compute the projected counters in the plan so the executor can check satisfiability. Related: [[project-deadcode-removal-vs-coverage-exclusion]], [[project-614-store-root-leak-plan-seams]].
