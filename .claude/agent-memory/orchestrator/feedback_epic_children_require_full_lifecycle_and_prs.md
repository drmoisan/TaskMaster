---
name: feedback-epic-children-require-full-lifecycle-and-prs
description: Maintainer rejected epic fallbacks — every epic child needs the FULL per-child orchestrator lifecycle AND a real child GitHub PR into the integration branch; no direct --no-ff merges, no executor-driver shortcuts
metadata:
  type: feedback
---

During epic #295 execution (2026-07-10) the epic-orchestrator adopted two
fallbacks and the maintainer rejected BOTH as unacceptable:

1. **Execution-model fallback rejected.** Because `Agent(orchestrator)` was not
   registered in its session, epic-orchestrator acted as the per-feature
   orchestrator itself in an abbreviated form (atomic-executor -> feature-review
   -> direct merge). Not acceptable. When the orchestrator agent type is
   unavailable, the FULL orchestrate lifecycle must still run per child —
   per-child checkpoint, feature-review, remediation loop, pr-author skill flow
   (body + SHA-256 receipt), S9 CI-gate recording, merge-on-green — whoever the
   runner is. The abbreviation, not the runner substitution, is the violation.

2. **Direct integration merges rejected.** Children #293/#296 were fanned into
   the integration branch via `git merge --no-ff` with `pr_number: null`,
   justified by "child->integration CI is vacuous". Not acceptable. Every child
   must fan in via a real GitHub PR to the integration branch
   (`gh pr create --base <integration-branch>` -> `gh pr merge --merge`), even
   when its CI is vacuous — the PR itself is the audit/review record. Precedent:
   epic #260 (store-lockup-resilience) had child PRs #274-#280.

**Why:** The maintainer requires the audited, receipt-bearing lifecycle for every
child; convenience shortcuts that skip PRs or lifecycle stages destroy the audit
trail even when the code work itself was reviewed.

**How to apply:** When kicking off epic execution, state both requirements
explicitly in the epic-orchestrator prompt (no lifecycle abbreviation on agent
unavailability; child PRs mandatory). If an epic checkpoint shows
`merge_method: git merge --no-ff` or `pr_number: null` on a merged child, treat
it as a defect requiring unwind and re-fan-in via PR. See
[[project-epic-295-winforms-testability]].
