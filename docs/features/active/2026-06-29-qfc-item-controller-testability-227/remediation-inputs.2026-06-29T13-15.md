# Remediation Inputs: QfcItemController / IItemViewer Testability Refactor (Issue #227)

**Generated:** 2026-06-29T13-15
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (`4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head:** `TaskMaster-wt-2026-06-29-09-38` (`bcc7d7e32a12693b732d5c5e133a681890bec412`)
**Source audits:**
- `policy-audit.2026-06-29T13-15.md`
- `code-review.2026-06-29T13-15.md`
- `feature-audit.2026-06-29T13-15.md`

## Disposition Overview

The implementation is behavior-preserving and high quality; no code-defect blocker was found. Two
gating items prevent an unconditional PASS, plus one deferred residual. The two gating items are of
different kinds: one is an artifact-generation task routable to a planner/executor; the other is a
maintainer governance action and is NOT routable to an implementation planner.

Overall recommendation: **Conditional Go** — resolve R1 and R2, then the change is mergeable with
R3 deferred to #197.

---

## R1 — Canonical C# coverage artifact absent

- **Severity: Blocking**
- **Type:** Process / evidence-artifact (workflow-mandated)
- **Finding:** `artifacts/csharp/coverage.xml` (the canonical C# coverage artifact required by the
  feature-review workflow for every changed language) is not present in the tree. Coverage was
  recorded only in feature-folder evidence files
  (`evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`,
  `evidence/qa-gates/p8-coverage-gap.2026-06-29T12-40.md`).
- **Impact:** Fails the coverage-artifact-presence gate; the workflow treats an absent coverage
  artifact for a changed language as FAIL. Substantive coverage was nonetheless independently
  verifiable from the evidence files (484/585 = 82.74% affected testable non-exempt; no
  changed-line regression).
- **Remediation:** Generate the canonical Cobertura `artifacts/csharp/coverage.xml` using the
  documented #223 remediation cycle-1 procedure (merge the `.coverage` to Cobertura at the
  canonical path), then re-run the policy-audit coverage-presence check.
- **Route:** atomic_planner / atomic_executor (artifact generation).
- **Artifact paths:** `artifacts/csharp/coverage.xml` (target);
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/policy-audit.2026-06-29T13-15.md` (finding).

## R2 — Exemption boundary awaits maintainer ratification

- **Severity: Blocking**
- **Type:** Governance / maintainer decision (NOT an implementation task)
- **Finding:** 103 method-level `[ExcludeFromCodeCoverage]` applications define the testable
  denominator for AC5. The boundary was independently verified as honest and not over-broad: the
  source count is exactly 103 (matching `exemption-boundary.2026-06-29T12-40.md`), and every named
  testable seam (`PopulateAndSelectFolder`, `AssignFolderComboBox`, `PackageItems`,
  `GetItemSummary`, `TopFolderScore`, `NotifyPropertyChanged`, `KbdExecuteAsync`,
  `RegisterFocusAsyncActions`, `RegisterExpandedAsyncActions`, `LoadConversationResolverAsync`,
  `MarkItemForDeletion`) is NOT exempted. Spec AC5 conditions the criterion on maintainer
  ratification of this boundary, which has not occurred.
- **Impact:** AC5 cannot be checked off in `spec.md` until ratified.
- **Remediation:** Maintainer reviews the exemption boundary and the repo-wide-floor disposition,
  and records a decision (produce `maintainer-decision.<date>.md` analogous to
  `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md`).
  On ratification, re-check AC5 in `spec.md`.
- **Route:** Project maintainer (Dan Moisan). Do NOT route to an implementation planner.
- **Artifact paths:**
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-06-29T12-40.md`;
  target `docs/features/active/2026-06-29-qfc-item-controller-testability-227/maintainer-decision.<date>.md`.

## R3 — AC5 ≥90% new/extracted sub-target unmet (deferred)

- **Severity: Non-blocking (deferred to #197)**
- **Type:** Coverage uplift (tracked follow-up)
- **Finding:** Aggregate extracted non-exempt coverage is 82.74% (< 90%). Adjudication: the
  executor's denominator (the full 585-line extracted non-exempt aggregate) over-applies the
  "new code ≥90%" rule to verbatim-relocated pre-existing methods. The genuinely-new narrowing
  logic is ≥90% per the evidence. The aggregate is held below 90% by structurally un-coverable
  code: EventWiring inline async-registration lambda bodies (~56 lines, un-exemptable inline
  closures executable only on a live key-press); the `PopulateConversationAsync` non-null render
  path via `UiThread.Dispatcher` (injectable-Dispatcher seam deferred to #197; best-case ~86.8%
  even with the seam); and `ViewerSetup.GetItemSummary` (2 COM lines).
- **Impact:** None this cycle; the ≥80% testable floor is met and there is no changed-line
  regression. Consistent with the spec Non-Goal on introducing the injectable `Dispatcher`.
- **Remediation:** Fold the residual non-exempt uplift into the #197 follow-up (injectable
  `Dispatcher` seam plus EventWiring lambda extraction, if pursued).
- **Route:** Tracked under issue #197; no action required for this cycle's merge.
- **Artifact paths:**
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`.

---

## Items explicitly NOT flagged (verified clean)

- **Behavior preservation:** Verbatim partial split; forwarding members round-trip the underlying
  Designer controls; event handlers are thin delegators; analyzer + nullable/TWAE builds pass. No
  behavior-change risk identified (assessed by inspection + green toolchain; not live-Outlook run).
- **Over-broad exemption:** None. No testable seam was exempted to inflate the percentage.
- **500-line cap (AC6):** All 22 changed production/test files < 500 lines (verified via `awk`).
- **Toolchain (AC7):** Four-step C# toolchain EXIT_CODE 0 in order at the final gate; 233/233 tests pass.
- **Evidence location compliance:** All evidence under canonical `<FEATURE>/evidence/<kind>/`; no
  non-canonical `artifacts/{baselines,qa,evidence,coverage}/` paths in the diff.
- **Scope narrowing:** None supplied; full feature-vs-base audit performed.
