# Remediation Inputs: QuickFiler Navigation-Key Collision Fix (Issue #232)

**Generated:** 2026-07-03T16-51
**Base branch:** `main` (merge-base `00507b595297c3e6970634a1855f1144c987dbdf`)
**Head commit:** `90e75ec19e0d0bb88e6d05168354cac4a66a6a2a`
**Source audits:**
- `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/policy-audit.2026-07-03T16-51.md`
- `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/code-review.2026-07-03T16-51.md`
- `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/feature-audit.2026-07-03T16-51.md`

**Total blocking findings: 1**

---

## Blocking Finding 1 — C# coverage artifact absent (machine-readable Cobertura XML)

- **Severity:** Blocker (fail-closed coverage-verification condition).
- **Language:** C# (the only language with changed source files on this branch).
- **Policy basis:** Feature-review Coverage Verification model — coverage must be verified by inspecting a pre-existing machine-readable coverage artifact for every language with changed files. "If no coverage artifact is found for a language that has changed files, flag as FAIL ... coverage verification is mandatory for all languages with changed files."
- **Observed state:**
  - `artifacts/csharp/coverage.xml` (canonical C# coverage artifact path) does not exist.
  - No Cobertura `coverage.xml` is persisted anywhere under `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/**`. (`find . -name coverage.xml` returns only unrelated feature #177 files.)
  - Coverage evidence exists only as transcribed prose in `evidence/baseline/vstest-baseline.md`, `evidence/qa-gates/vstest-final.md`, and `evidence/qa-gates/coverage-delta.md`. The underlying Cobertura runs were written to an ephemeral scratchpad results directory and not retained.
- **Impact:** The coverage claims for this change cannot be independently verified from an artifact:
  - Non-exempt `QfcHighConfidencePreFilter.cs` changed-line coverage claimed 100% (>= 90% target).
  - Repo-wide claimed 76.5758% → 76.5712% (no regression).
- **Affected acceptance criterion:** AC10 (evaluated PARTIAL in the feature audit).
- **Affected artifact paths for remediation output:**
  - Persist coverage XML at `artifacts/csharp/coverage.xml`, or under the canonical evidence path `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/coverage/<yyyy-MM-ddTHH-mm>/coverage.xml` (evidence-location invariant: use the feature `evidence/` tree, not `artifacts/coverage/`).
- **Required remediation actions:**
  1. Re-run the C# coverage collection over the touched test assemblies using the Cobertura runsettings already referenced in the evidence (`UtilitiesCS.Test`, `QuickFiler.Test`, `TaskMaster.Test` with `/InIsolation` and the first-party + Swordfish module set / `[ExcludeFromCodeCoverage]` excludes).
  2. Persist the resulting Cobertura `coverage.xml` to a durable path (canonical `artifacts/csharp/coverage.xml` and/or the feature `evidence/coverage/` tree).
  3. Re-verify from that XML: (a) `QfcHighConfidencePreFilter.cs` mapped classes all line-rate 1 (changed-line coverage >= 90%); (b) repo-wide line-rate shows no regression vs the baseline figure.
  4. Update `evidence/qa-gates/coverage-delta.md` to cite the persisted XML path, and re-run this feature review to confirm the coverage verdict flips to PASS.
- **Non-blocking substantiation:** The transcribed numbers are internally consistent (baseline `<>c__DisplayClass0_0`/`d__0` → post-change `<>c__DisplayClass1_0`/`d__1` renumbering explained by the new `logger` field; repo-wide delta attributed to a flaky-test pass/fail flip). If the artifact is regenerated, the coverage claims are expected to hold; the finding is an evidence-verifiability gap, not evidence of unmet coverage.

---

## Non-blocking items (recorded for awareness; not remediation triggers)

- `QfcDatamodel.cs` log caller-context string names `LoadRemainingEmailsToQueueAsync` while physically located in `ScoreRemainingQueueMailItemAsync` (Minor; AC4 still satisfied).
- `QfcCollectionControllerTests.cs` is at exactly 500 lines (at the cap; split future additions).
- `QfcCollectionController.cs` is 2308 lines (pre-existing > 500-line overage; not introduced by #232).
- PR context summary overview under-reports C# scope (`Core logic changes: 0 files`); audit used `git diff` as authoritative.
- Repo-wide raw coverage 76.57% is below the 80% generic floor but is covered by the ratified COM/VSTO/WinForms testable-denominator exemption (CLAUDE.md; Issue #227) and is not worsened by this change.

---

## Handoff

Route Blocking Finding 1 to remediation (atomic planner / executor) per `remediation-handoff-atomic-planner`. The remediation is coverage-artifact regeneration and persistence plus re-verification; no source-code change to the Part A fix or Part B logging is required by this finding.
