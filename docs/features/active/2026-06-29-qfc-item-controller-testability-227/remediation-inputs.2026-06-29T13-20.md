# Remediation Inputs — Cycle 1 (Issue #227)

**Generated:** 2026-06-29T13-20 (orchestrator, cycle entry)
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (`4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head:** `TaskMaster-wt-2026-06-29-09-38` (`bcc7d7e32a12693b732d5c5e133a681890bec412`)
**Source review (cycle 0):** `remediation-inputs.2026-06-29T13-15.md`, `policy-audit.2026-06-29T13-15.md`, `code-review.2026-06-29T13-15.md`, `feature-audit.2026-06-29T13-15.md`

## Cycle scope

The cycle-0 feature-review returned a Conditional Go with two blocking findings (R1, R2) and one
deferred residual (R3). The two blockers are of different kinds; this cycle routes each to its
correct owner.

### In scope for this implementation cycle (atomic-planner → atomic-executor → feature-review)

**R1 — Generate the canonical C# coverage artifact (Severity: Blocking, implementable).**
- The workflow-mandated canonical Cobertura coverage artifact `artifacts/csharp/coverage.xml` is
  absent. Coverage was recorded only in feature-folder evidence files.
- Remediation: run the C# test+coverage toolchain and produce/merge the canonical Cobertura
  `artifacts/csharp/coverage.xml` at the canonical path, following the #223 cycle-1 procedure
  (merge the `.coverage` output to Cobertura at `artifacts/csharp/coverage.xml`). Do not alter
  production or test code; this is an evidence-artifact generation task. Confirm the recorded
  coverage is consistent with the existing evidence (484/585 = 82.74% affected testable
  non-exempt; 233/233 tests pass).
- Acceptance: `artifacts/csharp/coverage.xml` exists, is valid Cobertura, reflects the
  QuickFiler.Test run, and the policy-audit coverage-presence gate passes on re-audit.

### Routed to the maintainer (NOT to an implementation delegate)

**R2 — Exemption-boundary ratification (Severity: Blocking, governance).**
- 103 method-level `[ExcludeFromCodeCoverage]` applications define the AC5 testable denominator.
  Cycle-0 review independently verified the boundary is honest and not over-broad (source count
  exactly 103; no named testable seam exempted). Spec AC5 conditions the criterion on maintainer
  ratification, which has not occurred.
- This is a project-maintainer decision. It is escalated to the maintainer (Dan Moisan) for
  ratification. On ratification, a `maintainer-decision.<date>.md` is recorded (analogous to the
  #223 `maintainer-decision.2026-06-29.md`) and AC5 is re-checked in `spec.md`.
- Per the remediation-loop protocol, this item is NOT routed to atomic-planner or atomic-executor.
  The exit gate cannot close until it is ratified.

### Deferred (not a blocker this cycle)

**R3 — AC5 ≥90% new/extracted sub-target residual.**
- The genuinely-new narrowing logic meets ≥90%; the aggregate is held below 90% by structurally
  un-coverable verbatim-extracted code (EventWiring async-registration lambdas; Dispatcher-bound
  async render; `GetItemSummary`). The ≥80% testable floor is met with no changed-line regression.
- Deferred to #197 (injectable `Dispatcher` seam + EventWiring lambda extraction, if pursued).
  No action this cycle.

## Exit condition for cycle 1

`blocking_count == 0` across the re-audit, which requires BOTH:
1. R1 resolved (canonical `artifacts/csharp/coverage.xml` present and validated by re-audit), AND
2. R2 ratified by the maintainer (decision recorded; AC5 re-checked).

R3 remains deferred and does not affect the exit gate.
