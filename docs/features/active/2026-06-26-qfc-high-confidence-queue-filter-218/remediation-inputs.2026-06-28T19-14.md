# Remediation Inputs (Expanded Cycle 2): qfc-high-confidence-queue-filter (Issue #218)

Timestamp: 2026-06-28T19-14

## Cycle Provenance

This is remediation cycle 2. Cycle 1 (`remediation-inputs.2026-06-26T20-58.md` /
`remediation-plan.2026-06-26T20-58.md`) was executed through a cleared preflight and
then **blocked at [P1-T4]** because reducing the touched production files below the
500-line limit required extraction of controller responsibilities outside the original
issue #218 queue-admission scope. Evidence:
`docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/line-count-remediation-blocker-218.md`.

The user approved an **expanded remediation scope** authorizing the broader extraction
required to bring the touched files under the repository 500-line limit. Per the
Scope-change Rule, cycle 1 is closed as `failed` and this file opens cycle 2 with the
expanded scope.

## Source Review Artifacts

- Policy audit (cycle entry): `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/policy-audit.2026-06-26T20-58.md`
- Code review (cycle entry): `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/code-review.2026-06-26T20-58.md`
- Feature audit (cycle entry): `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/feature-audit.2026-06-26T20-58.md`
- Cycle 1 inputs: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/remediation-inputs.2026-06-26T20-58.md`
- Cycle 1 plan: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/remediation-plan.2026-06-26T20-58.md`
- Cycle 1 blocker evidence: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/line-count-remediation-blocker-218.md`
- Changed-line coverage evidence (cycle 1): `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/changed-line-coverage-218.md`
- Coverage comparison (cycle 1): `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/coverage-comparison-218.md`
- Baseline Cobertura: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/baseline/coverage-baseline-218.cobertura.xml`
- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Acceptance criteria source: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/issue.md`

## Current Ground-Truth State (verified 2026-06-28T19-14)

- `QuickFiler/Controllers/QfcDatamodel.cs`: 790 lines (over 500-line limit).
- `QuickFiler/Controllers/QfcHomeController.cs`: 739 lines (over 500-line limit).
- `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`: 1370 lines (over 500-line limit).
- `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs`: 58 lines (compliant; created by cycle 1).
- `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`: 148 lines (compliant).
- All 5 issue #218 acceptance criteria in `issue.md` are checked and pass.
- Working tree is clean; cycle 1 work is committed (`5b95d115`, `7905efa1`).

## Remediation Trigger Summary

The cycle-entry policy audit records three blocking findings. Cycle 2 must resolve all
three to a defensible disposition (FAIL findings fixed, or authority-scoped exception
documented for the items the bug remediation cannot in-scope resolve), then produce a
clean reaudit.

## Enumerated Fix List

### Finding 1 — Touched C# files exceed the 500-line limit (FAIL; expanded scope, user-approved)

- **Current evidence:** `QfcDatamodel.cs` 790 lines, `QfcHomeController.cs` 739 lines,
  `QfcHomeControllerTests.cs` 1370 lines.
- **Expected behavior:** Each touched file must be brought to 500 lines or fewer through
  cohesive extraction of related responsibilities into new, single-responsibility
  collaborator files under `QuickFiler/Controllers/` (production) and
  `QuickFiler.Test/Controllers/` (tests). The user has authorized extracting controller
  responsibilities beyond the original queue-admission seam where required to satisfy the
  limit. Extractions must preserve public behavior, the `IQfcDatamodel` and home
  controller public surfaces, and all existing test names and assertions.
- **Constraints:** No behavior change beyond mechanical extraction. Preserve cancellation
  propagation, `ConfigureAwait(false)` usage, and existing logging. Update
  `QuickFiler.Test/QuickFiler.Test.csproj` for any new test files. Keep each new file
  cohesive and under 500 lines.
- **Banned-API sweep (in scope for touched production files):** While extracting from
  `QfcDatamodel.cs` and `QfcHomeController.cs`, remediate any banned API found in the
  moved or surrounding touched code: `DateTime.Now`/`DateTime.UtcNow`, `Random.Shared`,
  `Thread.Sleep`, `Task.Delay`. Replace with the repository's injectable clock/delay
  seam if present; if a banned API is found and cannot be removed in scope, record it as
  a new finding for a follow-up cycle rather than silently leaving it.
- **Verification commands:** Deterministic line-count check over all changed C# files;
  full C# toolchain loop (CSharpier check, analyzer build, nullable build, MSTest
  coverage) after extraction.

### Finding 2 — Changed-production-line coverage must be isolated as a numeric percentage (PARTIAL)

- **Current evidence:** `changed-line-coverage-218.md` exists from cycle 1 but predates
  the cycle-2 extraction. After extraction the changed production line set will move
  across files.
- **Expected behavior:** Regenerate a changed-production-line coverage report mapping the
  merge-base (`main`) production diff to the final Cobertura line hits, and state an
  explicit numeric changed-line coverage percentage with a PASS/FAIL result against the
  90%-for-new/changed-code expectation. Any uncovered issue #218 admission or initial-load
  line must receive the smallest deterministic MSTest coverage that does not require live
  Outlook COM.
- **Verification commands:** Generate changed-line coverage from the final Cobertura
  artifact and the `main` merge-base diff.

### Finding 3 — Repository-wide C# coverage below 80% (FAIL; authority-scoped exception)

- **Current evidence:** Cycle-entry coverage is 62.04458810901509% repo-wide line
  coverage (100578 / 162106), versus an 80% raw threshold. Baseline was
  62.02918410429243%; the change produced a positive delta with no regression.
- **Disposition:** This is a **pre-existing, repository-wide shortfall** that a single
  bug remediation cannot and should not close by 18 percentage points. Per `CLAUDE.md`
  (General Unit Test Policy / COM-VSTO coverage exemption), the 80% floor applies to the
  **testable denominator** after excluding VSTO add-in lifecycle classes,
  WinForms/Designer code, and Outlook Interop event-handler classes; the raw 62% figure
  includes that exempt COM-bound code in the denominator. The authorized path is to
  **document an authority-scoped exception** for the repo-wide raw figure, tracked under
  `feature/csharp-coverage-uplift`, and surface it for maintainer ratification — **not**
  to weaken any policy file and **not** to inflate coverage with out-of-scope tests.
- **Expected behavior:** Write a coverage exception/blocker evidence file that records:
  the exact repo-wide figure and threshold gap; that change-scope gates (no regression,
  positive delta, issue #218 changed-line coverage) pass; the CLAUDE.md testable-denominator
  basis for the exemption; and an explicit statement that repo-wide uplift to the raw 80%
  figure is out of scope for issue #218 and requires maintainer ratification via
  `feature/csharp-coverage-uplift`.
- **Verification commands:** Re-derive the repo-wide figure from the final Cobertura
  artifact; cite the CLAUDE.md exemption clause.

## Do Not Do

- Do not modify repository policy files (`CLAUDE.md`, `.claude/rules/**`, `.editorconfig`,
  `.globalconfig`, coverage config) to make the review pass.
- Do not remove or weaken the issue #218 acceptance criteria.
- Do not remove or rename existing focused issue #218 tests; preserve their assertions.
- Do not change production behavior beyond mechanical, behavior-preserving extraction.
- Do not attempt to raise repository-wide coverage to the raw 80% figure with out-of-scope
  tests; handle that finding as a documented authority-scoped exception.
- Do not mark remediation complete without rerunning CSharpier, analyzer build, nullable
  build, MSTest coverage, and the coverage comparison in order.
- Do not introduce temporary files in tests.

## Required Context Package

Any remediation planner or executor must read:

- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/remediation-inputs.2026-06-28T19-14.md` (this file)
- `CLAUDE.md` (coverage exemption clause and C# toolchain order)
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/policy-audit.2026-06-26T20-58.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/code-review.2026-06-26T20-58.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/feature-audit.2026-06-26T20-58.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/line-count-remediation-blocker-218.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/changed-line-coverage-218.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/plan.2026-06-26T20-28.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/issue.md`
- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
