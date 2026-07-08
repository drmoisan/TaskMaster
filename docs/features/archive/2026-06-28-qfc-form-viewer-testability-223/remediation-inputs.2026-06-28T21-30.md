# Remediation Inputs — qfc-form-viewer-testability (#223)

**Cycle entry timestamp:** 2026-06-28T21-30
**Feature folder:** `docs/features/active/2026-06-28-qfc-form-viewer-testability-223`
**Base branch:** `main` (merge-base `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`)
**Head:** `e91927105abde2ceadd10a7011bc17d714108afd`

## Source audit artifacts

- `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/policy-audit.2026-06-28T21-30.md`
- `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/code-review.2026-06-28T21-30.md`
- `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/feature-audit.2026-06-28T21-30.md`

## Blocking findings (must be remediated)

Blocking count: 2 (1 FAIL + 1 blocking PARTIAL), both rooted in a single missing-coverage-evidence cause.

### Finding 1 — FAIL: canonical C# coverage artifact absent; repo-wide >= 80% floor unverified

- **Where:** `artifacts/csharp/coverage.xml` (absent); repo-wide first-party (testable-denominator) coverage is unmeasured.
- **Observed:** The only repo-wide number on record is the single-assembly process-wide 12.86% (9800/76203), which the executor explicitly disclaims as instrumenting all loaded modules and not the policy gate. No Cobertura artifact exists at the canonical path. Per the feature-review-workflow mandatory-coverage rule, an absent coverage artifact for a language with changed files is a FAIL.
- **Expected behavior:** A canonical Cobertura coverage artifact exists at `artifacts/csharp/coverage.xml`, and a repo-wide first-party testable-denominator coverage figure is recorded that confirms the `>= 80%` floor (applying the documented COM/VSTO/WinForms `[ExcludeFromCodeCoverage]` exemptions to the denominator).
- **Verification commands:**
  - `vstest.console.exe <first-party test assemblies> /EnableCodeCoverage` then `dotnet-coverage merge -f cobertura -o artifacts/csharp/coverage.xml`
  - Parse `artifacts/csharp/coverage.xml` repo-wide `line-rate` and confirm `>= 0.80` against the testable denominator.
  - `ls artifacts/csharp/coverage.xml` returns the file.
- **Evidence reference:** policy-audit Section 1.2 (Repo-wide row, FAIL) and Section 8; coverage-delta `evidence/regression-testing/coverage-delta.2026-06-28T20-52.md`.
- **Known environment constraint:** Local full-assembly C# coverage has previously failed on a Moq binding redirect; if local generation is not feasible, the authoritative measurement is the PR CI coverage run. The remediation must still produce/attach the canonical artifact and a confirmed repo-wide figure (CI-produced is acceptable) before exit.

### Finding 2 — PARTIAL (blocking): AC5 repo-wide coverage sub-claim unverified

- **Where:** `issue.md` AC5; `feature-audit.2026-06-28T21-30.md` row 5.
- **Observed:** AC5's test-presence, new-code (100%), and changed-line no-regression (+12.62pp) sub-claims are satisfied and PASS. The "repo-wide coverage stays >= 80%" sub-claim is unverified for the same reason as Finding 1. AC5 was reverted to unchecked `[ ]` in `issue.md`.
- **Expected behavior:** Once Finding 1 is resolved and the repo-wide first-party floor is confirmed `>= 80%`, AC5 is fully satisfied and may be re-checked.
- **Verification commands:** Same as Finding 1, plus re-run the feature-audit AC5 evaluation.
- **Evidence reference:** feature-audit AC Status Summary (6 PASS / 1 PARTIAL).

## Non-blocking observations (recorded; do not require remediation this cycle)

- `QuickFiler/Controllers/QfcCollectionController.cs` (2296 lines, `[ExcludeFromCodeCoverage]`) and `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (821 lines) remain pre-existing 500-line-cap violations, touched net-negative/net-neutral. Accepted as pre-existing-debt dispositions this cycle. A future split is advisable but is out of scope here.

## Do-not-do list

- Do not split `QfcCollectionController.cs` or `QfcFormControllerTests.cs` in this remediation cycle (out of scope; would be a broad refactor of exempt/legacy code).
- Do not modify policy documents under `.claude/rules/` or `CLAUDE.md`, or weaken any coverage threshold or exemption to make the floor pass.
- Do not alter, weaken, or delete existing tests to change coverage numbers.
- Do not narrow the audit scope or mark C# coverage "informational only."
- Do not write coverage or evidence artifacts to non-canonical paths (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`); per-feature evidence belongs under `<FEATURE>/evidence/<kind>/`. The single exception is the canonical C# coverage artifact path `artifacts/csharp/coverage.xml` mandated by the coverage-verification contract.

## Handoff

Remediation is delegated through `remediation-handoff-atomic-planner`: the orchestrator routes these inputs to `atomic-planner` to author `remediation-plan.2026-06-28T21-30.md` (validated via `validate_orchestration_artifacts` `artifact_type: plan`), `atomic-executor` preflights and executes, and `feature-review` reaudits at the exit timestamp.
