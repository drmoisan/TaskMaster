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

**When one deleted line was UNCOVERED the line gate carries that many lines of slack**, so it no longer proves "no retained line lost coverage". Close the slack with a per-`filename` comparison, never with the changed-line listing: a retained line is by definition not a changed line, so a retained-line check performed against a changed-line listing returns clean whichever retained line regressed (#614 cycle-2 R3 F5-2 — the R2 edit that prescribed exactly that was itself unfalsifiable). Correct form: define `D_covered` as the NET sum of (`lines-covered` base minus post) over just the EDITED files, read from the two Cobertura files (never by hand-counting deleted source lines — a revert that replaces a body also ADDS measured lines, so a hand count breaks the identity on correct work), then assert `lines-covered_base - lines-covered_post == D_covered`. Two carve-outs are mandatory or the gate fails on correct work: (1) a strict excess means retained lines GAINED coverage (a new test in the same cycle commonly does this) and must pass; (2) an equal-and-opposite gain can mask a single-line regression, so additionally require the signed per-`filename` `lines-covered` delta for every retained file that moved, and fail on any negative delta.

**Why:** #614 remediation cycle 2 preflight verified the projected raw miss independently and returned REVISIONS REQUIRED; the branch-rate comparison was within the observed ±0.002 pp run-to-run spread, i.e. a coin flip.
**How to apply:** whenever a plan's change set deletes covered production lines, never write `rate_post >= rate_base` as a gate. Pre-compute the projected counters in the plan so the executor can check satisfiability. Related: [[project-deadcode-removal-vs-coverage-exclusion]], [[project-614-store-root-leak-plan-seams]].
