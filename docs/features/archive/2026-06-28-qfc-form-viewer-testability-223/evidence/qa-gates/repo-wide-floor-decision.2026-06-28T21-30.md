# P2-T3 — Repo-Wide Floor Decision (Remediation Cycle 1, Issue #223)

Timestamp: 2026-06-28T21-50

FLOOR_DECISION: FLOOR-BELOW

- Measured repo-wide first-party testable-denominator figure: 73.35% (authoritative #197 per-`<line>` method, 39585/53969); 74.11% by Cobertura root aggregate (71654/96685).
- Threshold: `>= 80%`.
- Gap to floor: approximately 6.65 percentage points (authoritative method) / 5.89 pp (root aggregate).
- Outcome: BELOW the `>= 80%` floor under every measurement convention (73.35% / 74.11% / 76.08% vendored-excluded).

Output Summary:
FLOOR-BELOW. The repo-wide first-party testable-denominator coverage (73.35%) is below the `>= 80%` policy floor. Per the plan, the floor is NOT weakened and the cycle does not silently pass: an escalation finding is recorded in P2-T5 and AC5 remains unchecked (Phase 3 FLOOR-BELOW branch). The shortfall is pre-existing first-party debt, not introduced by this refactor (see P2-T5).
