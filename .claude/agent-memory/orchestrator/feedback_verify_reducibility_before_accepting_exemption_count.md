---
name: verify-reducibility-before-accepting-exemption-count
description: Do not accept a delivered [ExcludeFromCodeCoverage] residual count at face value when the maintainer's stated goal is testability; independently re-verify against proven in-repo techniques before ratifying
metadata:
  type: feedback
---

When a seam-based testability refactor delivers a reduced coverage-exemption boundary, do not
treat the delivered count as final just because a feature-review found `blocking_count == 0`.
Independently commission (or re-run) a reducibility audit that checks EVERY residual against
techniques already proven elsewhere in the SAME delivery, not just against the plan's own claims.

**Why:** On issue #227, cycle-2 delivered a 103->41 exemption reduction with a clean feature-review
(0 blocking). The maintainer compared 41 against the original seam-design research's ~6-8 estimate
and directed a rigorous re-check. An independent re-audit found 17 of the 41 were actually reducible
using patterns the SAME cycle had already built for other members (e.g., a `FolderPredictor` factory
delegate identical in shape to the `EmailFiler`/`FlagTasks` factories already built; a `WpfUiDispatcher`
adapter body left untested despite a proven live-dispatcher test technique already used elsewhere in
the same file set; a `MailItemActionsAdapter` already 100%-covered but still carrying a stale
attribute). The honest floor was 24, not 41 and not 6-8 — the original pre-seam estimate couldn't
foresee seam-infrastructure's own residual shape (adapter bodies, async-void shells), and the
delivered 41 included avoidable inconsistency, not just structural necessity.

Even after that correction, a subsequent feature-review caught a SECOND-order problem: 2 of the 24
"genuinely testable" members were de-exempted with tests that called `Mock<IItemViewer>.Invoke()`
but never executed the passed delegate — so the substantive logic was still unverified despite the
attribute being removed. Removing an exemption is not the same as proving testability; the test must
actually exercise the delegate/callback body, not just verify it was marshaled.

**How to apply:**
- When a cycle reduces an exemption count, ask: "does every de-exempted member's test actually
  execute the method's substantive logic and assert on the outcome, or does it only verify a
  marshaling/dispatch call happened?" Mocks that don't invoke their callback hide this gap.
- Cross-check "out of scope" / "irreducible" labels against every OTHER seam/pattern the same delivery
  already built. If a structurally identical collaborator got a seam, the collaborator without one is
  a strong candidate for the same treatment, not a legitimate exception.
- A revised, defensible target must decompose cleanly into named structural categories (framework
  signature constraints, external-runtime dependencies, retained design invariants) — if you can't name
  the specific reason a residual can't be reduced, don't accept it as irreducible.
- This can cascade into multiple remediation cycles (cycle 2: 103->41; cycle 3: 41->24; cycle 4: fix
  test-honesty gap on 2 of the 24, count unchanged). Each cycle's atomic-planner/atomic-executor may
  self-discover a deeper transitive gap mid-work (e.g., a handle-less test double satisfying one guard
  but tripping a second, unrelated null-field dependency several calls deeper) — that is expected
  rigor, not scope creep, as long as it's reported honestly and resolved minimally.

Related: [[remediation-loop-strict-handoff]], [[feedback_no_coverage_exemption_when_purpose_is_testability]].
