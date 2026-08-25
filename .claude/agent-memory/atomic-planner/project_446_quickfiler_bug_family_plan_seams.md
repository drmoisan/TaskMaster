---
name: project-446-quickfiler-bug-family-plan-seams
description: "#446 family plan (quickfiler-bug-family-446) R1+R2 preflight seams: folder slug vs descriptive slug, seam-before-COM-test ordering, AC28/AC18 conflict via unchecked box + REMEDIATION-REQUIRED, reconciliation branches propagated to all downstream consumers"
metadata:
  type: project
---

Preflight R1 seams for `docs/features/active/quickfiler-bug-family-446/plan.2026-08-24T09-37.md` (issues #426/#427-A/#446/#448).

- The active feature folder is `quickfiler-bug-family-446`; the plan's descriptive slug is `quickfiler-queue-datamodel-defects` (H1 title, commit scopes). Never use the descriptive slug in a path.
- Ordering: the `ScoringServiceFactory` seam task must precede any test exercising `DequeueNextItemGroupAsync` with high-confidence mode on, or the test path reaches live Outlook COM via `new FolderScoringService()` (UT4 violation). Now `[P1-T5]` (seam) before `[P1-T6]` (test).
- AC28 names three TYPES but sibling-owned partial files make a literal whole-type >=90% gate conflict with AC18. Resolution: blocking gate on the three changed FILES (`[P5-T7]`), whole-type held to no-regression; `[P5-T17]` leaves the AC28 box unchecked and records `REMEDIATION-REQUIRED: AC28 whole-type reading conflicts with AC18` when any whole-type figure is below 90.00. Terminal gate accepts checked-with-evidence OR unchecked-with-recorded-gap. Do not edit spec.md.
- Known load-flaky tests exist in the coverage-enabled/Workers-0 configuration (UtilitiesCS.Test timing tests, QuickFiler WinFormsPumpHost tests), so `[P5-T5]` and `[P5-T2]` carry baseline-identical reconciliation branches keyed to `[P0-T9]`/`[P0-T12]`; `[P0-T12]` must record failed-test NAMES for that comparison to be satisfiable.
- `Invoke-MSTestWithCoverage.ps1` prints only `Discovered N test assemblies.` (a count, not a list); tasks needing the list must reproduce it via the discovery prelude.
- `QfcItemController.FolderHandlingTests.cs` is 498 lines (compliant), not a 500-cap violator.
- R2 preflight lesson: adding a reconciliation branch to a gate task ([P5-T2]/[P5-T5]) is incomplete until EVERY downstream consumer is branched too — the coverage re-run ([P5-T6]), the clean-pass aggregator ([P5-T9]), the AC check-off ([P5-T16], unchecked box + `REMEDIATION-REQUIRED: AC27 ...` line), AND the phase preamble's restart rule (a step completing on its reconciliation branch is not a failure, else the loop never terminates). Same shape as [[thread-granted-discharges-through-consumers]].
- R2 also caught a residual unscoped `git status --porcelain` in the terminal commit task despite the plan's own scoped-gates convention paragraph — sweep TERMINAL commit tasks specifically when asserting "no unscoped git gate" ([[agent-memory-is-tracked-scope-git-gates]], [[terminal-phase-planner-traps]]).
- spec.md:966-967 stale pointers (old folder slug) were corrected via an extension to [P0-T2] rather than a new task, to avoid renumbering; document-pointer fixes to spec.md are permitted, AC text is not.

**Why:** these were the B/H findings of the atomic-executor preflight rounds 1 and 2 on 2026-08-24; a later remediation cycle on this feature will re-hit the same seams.
**How to apply:** when revising or extending this feature's plan, preserve the seam-before-test ordering, the two-scope coverage split, and the reconciliation branches; see also [[enumerate-condition-outcomes-before-case-list]] and [[coverage-gate-clr-invoked-private-members]].
