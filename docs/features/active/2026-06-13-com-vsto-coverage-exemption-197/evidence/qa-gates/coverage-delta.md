# Coverage Delta (P7-T8) — Baseline vs Post-Exemption

Timestamp: 2026-06-13T14-45

## Figures (production-only first-party deduped; vendored SVGControl/Swordfish held constant per memo §2.6)

| Metric | Baseline (P0-T6/P0-T7) | Post-change (P7-T4/P7-T6) | Delta |
|---|---|---|---|
| lines-covered | 38,820 | 37,010 | -1,810 |
| lines-valid | 65,768 | 51,594 | -14,174 |
| line rate | 59.03% (~58.95% documented) | 71.73% | +12.7 pp |

(Baseline lines-valid 65,768 matches roadmap §0.2 exactly; baseline covered 38,820 is within rounding of the documented 38,767. Post-change figures are computed with the identical reproducible summing method.)

## Comparison against design memo §3 estimate
- Memo §3 point estimate: ~75.2% (lines-valid ~50,442; lines-covered ~37,934).
- Memo §3 range: 73.2% (conservative, 14,000 removed) to 77.6% (aggressive, 17,000 removed).
- Actual: 71.73% on 51,594 lines-valid (14,174 removed), 37,010 covered (1,810 removed).

## DEVIATION NOTE (rate below memo §3 range)
The post-exemption rate 71.73% is 1.47 pp BELOW the memo §3 lower bound (73.2%).

Cause analysis (non-blocking; scope is correct):
- lines-valid removed (14,174) is at the conservative (low) end of the memo's 14,000-17,000 range. Fewer lines were removed than the §3 midpoint (15,326), so the denominator (51,594) is larger than the §3 midpoint estimate (50,442).
- lines-covered removed (1,810) is materially HIGHER than the §3 midpoint estimate (~833). Several annotated controllers/viewers carried more covered lines (exercised via existing test harness setup) than the memo's per-assembly covered-removal estimates assumed. Removing more covered lines lowers the numerator more than projected.
- Net effect: a smaller denominator reduction combined with a larger numerator reduction yields a rate (71.73%) just under the conservative bound.

This is a measurement refinement, not a scope or policy error. The exempt/non-exempt boundary matches design memo §2 exactly (verified in exemption-boundary-verification.md / P7-T7): no testable seam was exempted, and every enumerated COM/VSTO/WinForms target was exempted. The memo §3 figures are explicitly labeled "estimates" using range midpoints.

## REMEDIATION FLAG
- The result is recorded as a DEVIATION below the §3 estimate range, per the P7-T8 acceptance instruction ("record a deviation note and remediation flag if the rate falls outside that range").
- Recommended follow-up for the maintainer (out of scope for this attribute/config/doc-only feature): the roadmap increment tests (memo Phases 4-8, spec Non-Goals) are still required to reach the 80% floor; the slightly lower starting point (71.73% vs ~75.2%) means a modestly larger covered-line gain is needed. No change to the exemption scope is recommended — the boundary is correct per §2.
- This deviation does NOT change behavior parity (P7-T5 PASS) and does NOT indicate an incorrect exemption. It is a refinement of the §3 estimate against measured data.
