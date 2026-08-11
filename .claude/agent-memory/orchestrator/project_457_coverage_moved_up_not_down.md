---
name: project_457_coverage_moved_up_not_down
description: Issue #457's denominator fix RAISED the repo line rate (85.3514% -> 85.5355%), and the epic kickoff's stated 85.0317% baseline did not match measurement — verify the figure before #494 reasons from it
metadata:
  type: project
---

Issue #457 (epic `build-ci-coverage-gate-fidelity`, wave 1, merged as PR #542, merge commit `ee082ba1`) removed
lambdas hoisted out of `[ExcludeFromCodeCoverage]` members from the Cobertura denominator. Two facts that are easy
to get backwards:

**1. The fix moved the repository rate UP, not down.** `lines-valid` 62873 -> 62401 and `lines-covered`
53663 -> 53375, so `line-rate` went **0.853514 -> 0.855355**. The intuition that removing lines from a denominator
must help is not automatic — the removed lambda lines included *covered* ones (`<>c__DisplayClass42_0` contributes
two covered lines from the exempt member `DisposeProductionSurface`), so both numerator and denominator shrank. The
direction was measured, not derived; a rate computed as `covered / (valid - N)` would have been wrong.

**2. The epic kickoff's baseline figure did not match measurement.** The kickoff stated the corrected repo-wide rate
sat at **85.0317%** against the 85% floor, a 0.03-point margin framed as the reason to be careful. The child's own
`[P0-T11]` baseline capture on the same integration tip measured **85.3514%**. The child recorded the divergence as
an observation and did not reconcile it, which was correct for its scope.

**Why:** sibling feature #494 (`coverage-threshold-policy-reconciliation`, wave 2) is blocked on #457 specifically so
it can decide thresholds against a corrected figure. If #494 reasons from the kickoff's 85.0317% it starts from a
number no measurement reproduced.

**How to apply:** when #494 (or any threshold work) runs, re-measure rather than inheriting a quoted figure, and
reconcile the 85.0317% / 85.3514% divergence explicitly. Note also the two live measurement caveats: the PowerShell
branch floor is unmeasurable because Pester 5.6.1 emits no branch counter, and `artifacts/pester/powershell-coverage.xml`
reports zero covered lines repo-wide (open issue #536). See [[csharp-coverage-denominator-two-figures]] and
[[feature-review-coverage-85-floor-trap]].
