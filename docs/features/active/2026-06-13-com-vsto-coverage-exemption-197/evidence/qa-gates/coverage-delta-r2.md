# Coverage Delta R2 (P10-T8) — Class-Level TaskVisualization vs Assembly-Exclude Variant

Timestamp: 2026-06-13T13-46

Method: production-only first-party deduped, with vendored SVGControl/Swordfish.NET.General held
constant per memo §2.6 (identical to the original coverage-delta.md summing method; verified by
reproducing the prior 51,594 / 37,010 / 71.73% figure exactly from
artifacts/csharp/coverage-firstparty.postexemption.cobertura.xml).

## Figures

| Metric | Baseline (P0-T6/P0-T7) | Prior assembly-exclude (P7-T8) | New class-level (P10-T4/P10-T6) | Delta (class-level vs assembly-exclude) |
|---|---|---|---|---|
| lines-covered | 38,820 (~38,767 documented) | 37,010 | 37,019 | +9 |
| lines-valid | 65,768 | 51,594 | 51,665 | +71 |
| line rate | 59.03% (~58.95% documented) | 71.73% | 71.65% | -0.08 pp |

(Baseline figures restated from coverage-delta.md / the documented roadmap §0.2 value of 58.95%.)

## Interpretation

- The class-level treatment returns the preserved TaskVisualization testable seams to the
  denominator: exactly +71 lines-valid and +13 covered for the TaskVisualization package
  (FlagChangeItem 3, FlagChangeGroup measured remainder 19, FlagChangeTrainingQueue 49). Net of
  benign run-to-run variation in the UtilitiesCS reference package (-4 covered), the aggregate
  change is +71 lines-valid / +9 covered.
- Because the returned TaskVisualization seams are only ~18% covered (13/71), adding them lowers
  the overall rate slightly: 71.73% -> 71.65% (-0.08 pp). This matches the directive's expectation
  that the class-level treatment "is expected to lower the measured rate slightly relative to the
  assembly-exclude variant."
- Versus the 59.03% pre-exemption baseline, the class-level post-exemption rate (71.65%) is
  +12.62 pp.

## Comparison against design memo §3 estimate (AC4 — remains a separate open maintainer item)
- Memo §3 point estimate: ~75.2% (range 73.2%-77.6%).
- Class-level actual: 71.65% on 51,665 lines-valid; 37,019 covered.
- The measured rate (71.65%) is 1.55 pp BELOW the memo §3 lower bound (73.2%), slightly further
  below than the assembly-exclude variant (71.73%) because the class-level treatment correctly
  re-includes lightly-covered TaskVisualization seams that the §3 estimate's assembly-level
  removal had excluded entirely.

## AC4 status
AC4 (measured rate vs the §3 estimate range) REMAINS a separate open maintainer-acknowledgement
item. The class-level change does not by itself resolve AC4; the deviation cause is the same
measurement refinement documented in the original coverage-delta.md (more covered lines left the
denominator than the §3 midpoint assumed, and the §3 figures are explicitly labeled estimates).
The exempt/non-exempt boundary is correct per the directive and design memo §2 (verified in
exemption-boundary-verification-r2.md / P10-T7); no scope change is recommended. Reaching the 80%
floor still requires the roadmap increment tests (spec Non-Goals), which are out of scope here.

## REMEDIATION FLAG
Recorded as a DEVIATION below the §3 estimate range, per the P10-T8 acceptance instruction and
mirroring the original P7-T8 treatment. This is a measurement refinement, not a scope or policy
error; behavior parity holds (P10-T5 PASS).
