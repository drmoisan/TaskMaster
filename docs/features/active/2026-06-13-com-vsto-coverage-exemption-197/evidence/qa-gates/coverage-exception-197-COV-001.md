# Coverage Exception 197-COV-001

- Issue: #197
- Date: 2026-06-13
- Granted by: Maintainer (Dan Moisan)
- Scope: Acceptance Criterion AC4 of `spec.md` only.

## Statement

The maintainer accepts that the measured post-exemption production-only coverage rate
(**71.65%**, class-level TaskVisualization variant) is below the design memo §3 estimate range
(73.2%–77.6%, midpoint ~75.2%), and elects not to block Issue #197 on that estimate.

## Basis

- The exemption scope (exempt vs preserved) is verified correct against the design memo §2
  tables by feature-review (P7-T7 and P10-T7; `exemption-boundary-verification-r2.md`). This is
  not an implementation defect.
- AC4 tied success to matching a forward-looking estimate of the resulting rate. The estimate
  was optimistic: more incidentally-covered lines left the denominator than the §3 midpoint
  assumed, and the revision-1.1 class-level treatment deliberately re-includes lightly-covered
  TaskVisualization seams (raising the denominator, lowering the rate by ~0.08 pp vs the
  assembly-exclude variant).
- The remedy that would raise the measured rate into the estimated range — the roadmap
  increment tests (Increments 1–3) — is explicitly out of scope for #197 and is tracked as
  separate follow-up work.

## Effect

- AC4 is checked off as ACCEPTED WITH EXCEPTION 197-COV-001.
- This exception does not lower or waive the repository coverage policy floor; it acknowledges
  a single-feature estimate-vs-actual deviation. The COM/VSTO exemption makes the floor
  meaningful and achievable once the out-of-scope roadmap increments are executed.

## Precedent

Analogous to coverage exception 185-COV-001 (authority-scoped, single-feature).
