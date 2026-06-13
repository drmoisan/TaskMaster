# Remediation Inputs — COM/VSTO/WinForms Coverage Exemption (#197)

**Timestamp:** 2026-06-13T15-45
**Base branch:** `origin/main` (merge-base `1b3f5350`)
**Head:** `refactor/com-vsto-coverage-exemption-197` (`a564add0`)
**Source artifacts:**
- `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/policy-audit.2026-06-13T15-45.md`
- `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/code-review.2026-06-13T15-45.md`
- `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/feature-audit.2026-06-13T15-45.md`

## Blocking findings

**Count: 0.**

No finding blocks PR merge. The change is non-behavioral, the full C# toolchain is green in the final pass, behavior parity is confirmed (identical pre/post failing set), and the exemption boundary is verified exact against the design memo §2 (no testable seam exempted; no enumerated target missed).

## Recorded FAIL finding (non-blocking; no code remediation for #197)

### F1 — AC4 post-exemption rate below the design memo §3 estimate range

- **Finding:** The measured production-only deduped post-exemption coverage is **71.73%** (37,010 / 51,594), which is **1.47 pp below** the design memo §3 lower bound of 73.2% (point estimate ~75.2%, range 73.2%–77.6%).
- **Acceptance criterion:** AC4 (`spec.md`) — "The recorded post-exemption rate is consistent with the design memo §3 estimate ... and the figures are written to the feature evidence folder." The figures-written clause is satisfied; the numeric-consistency clause is not.
- **Artifact:** `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-delta.md`
- **Root cause (per coverage-delta.md):** Fewer lines-valid were removed than the §3 midpoint (14,174 vs 15,326), while more incidentally-covered lines were removed than estimated (1,810 vs ~833). A smaller denominator reduction combined with a larger numerator reduction yields a rate just under the conservative bound. This is a refinement of the §3 estimate against measured data.
- **Disposition:** **Non-blocking estimate deviation; no code remediation required for #197.** The exemption scope and boundary are correct (AC2/AC3 PASS, verified exact vs memo §2). The §3 figures are explicitly labeled estimates. The maintainer-ratified spec §Risks ("Floor still not reached") already states the post-exemption rate is expected below 80% and that the roadmap increment tests close the gap. Those increment tests are explicitly OUT OF SCOPE for #197 (spec §Non-Goals).
- **Required action (maintainer, not a code change to this branch):**
  1. Acknowledge the AC4 estimate deviation (71.73% actual vs ~75.2% estimated). Either accept AC4 as deviation-noted-and-closed, or revise the AC4 numeric range to reflect the measured starting point.
  2. Plan the out-of-scope roadmap increment tests (memo Phases 4–8) as the path to the 80% testable-denominator floor, noting the starting point is 71.73% rather than ~75.2% (a modestly larger covered-line gain is needed).
- **Handoff to atomic_planner:** Not warranted for #197. There is no code defect to remediate on this branch; the only follow-up (increment tests) is a separate, scoped-out feature. A remediation plan for #197 would have no in-scope tasks. If the maintainer elects to revise the AC4 wording, that is a one-line spec edit, not an atomic-plan-scale remediation.

## Non-blocking observations (no action required for #197)

- **O1 — Pre-existing oversize C# files.** Several changed `.cs` files exceed the 500-line policy limit at baseline (`QfcCollectionController.cs` 2299, `EfcItemController.cs` 1168, `EfcFormController.cs` 1014, `RibbonController.cs` 986, `QfcDatamodel.cs` 764, `KeyboardHandler.cs` 605, `ToDoEvents.cs` 594). This change adds only 2 lines per file and does not introduce/worsen the violation. Track separately from #197.
- **O2 — `user-story.md` absent.** Full-feature mode expects `spec.md` + `user-story.md`; only `spec.md` exists. `spec.md` carries a complete maintainer-ratified AC section, so this is a documentation-provenance gap, not an acceptance blocker.
- **O3 — Pre-existing flaky tests.** 2 timing/threading tests fail intermittently (identical pre/post set). Pre-existing UT4 shared-global/timing weaknesses, independent of this feature.

## Overall verdict

**PASS / Go for PR** — 0 blocking findings. AC4 is recorded as a non-blocking estimate deviation requiring maintainer acknowledgement, not code remediation on this branch.
