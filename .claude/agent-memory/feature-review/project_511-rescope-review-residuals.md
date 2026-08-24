---
name: 511-rescope-review-residuals
description: 'winformspumphost-511 re-audit (2026-08-24): PASS/0 blocking under re-scoped claims; residuals CR-1 stale spec RCA narrative, CR-2 AC wording vs maintainer-deleted raw TRX; PR must not close #511/#571'
metadata:
  type: project
---

Re-audit of `winformspumphost-suite-determinism-511` after remediation cycle 1 closed with 14/14
AC PASS and 0 blocking findings (artifacts stamped `2026-08-24T00-01`). The feature was re-scoped
by maintainer decision (`decision-record.2026-08-23T20-40.md`): the fixture-hardening remedy was a
measured no-op, so the branch keeps the hardening + two regression tests pinning the measured
WebView2 handle-inheritance state, and claims no repair. #511/#571 are CLOSED NOT_PLANNED,
superseded by #592 (real cause: load-induced 60 s PumpTimeoutMs cascade); #594 = UtilitiesCS.Test
flakes; #597 = analyzer skew.

**Why:** future reviews of related branches (PR for this branch, #592/#594/#597 work) need the
accepted-residual list so they are not re-raised as new findings, and must verify no closing
keyword for #511/#571 appears in the PR body.

**How to apply:**
- Accepted non-blocking residuals: CR-1 — spec `## Root Cause Analysis` still asserts the
  falsified pre-measurement claims ("IsHandleCreated is false for the whole test"; "children never
  obtain a handle") with no revision marker; annotation owed in a follow-up. CR-2 — AC 1/AC 3
  still say "ten TRX results stored under evidence/regression-testing/" though the raw TRX were
  deleted at maintainer instruction after fidelity verification
  (`evidence/other/raw-vstest-artifact-disposition.2026-08-23T21-40.md`). CR-5 — AC 10 line ref
  `:139` drifted to `:148`.
- Maintainer-ratified pattern confirmed again: committed disposition record supersedes AC literal
  wording (raw-artifact deletion), and an absolute-zero suite gate narrows to owned classes with
  the residual promoted (#594) — same precedent as [[441-review-residuals-and-494-handoff]].
- The remediation-inputs carve-out is binding: `remediation-inputs.2026-08-23T20-57.md` contains
  three closing-keyword regex matches inside negations and is exempt by design; also
  `plan.2026-08-21T18-10.md:26` (pre-existing file content). Do not re-raise either.
- pr_context summary again misclassified the 3 changed `.cs` files as docs-only (see
  [[pr-context-summary-misclassifies-cs]]); corrected in place with `- path (+N/-N)` bullets so
  the hook enumerates CSharp.
