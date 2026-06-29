# Remediation Inputs: QuickFiler banned-API time/delay seams (Issue #222)

**Generated:** 2026-06-28T19-57
**Base Branch:** `main` (86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a)
**Head:** `TaskMaster-wt-2026-06-28-18-49` (e48932654a6a9b90e94f23f3a87f6f617727ffcc)
**Source artifacts:**
- `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222/policy-audit.2026-06-28T19-57.md`
- `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222/code-review.2026-06-28T19-57.md`
- `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222/feature-audit.2026-06-28T19-57.md`

This change is otherwise compliant and behavior-preserving. One remediation-required finding blocks an unqualified PASS. It is an evidence/verification gap, not a code defect.

---

## Remediation-Required Findings

### R1 (Major) — Repo-wide C# coverage not verifiable; canonical artifact absent

- **What:** The canonical machine-readable C# coverage artifact `artifacts/csharp/coverage.xml` does not exist, and no cobertura XML was committed to this feature's evidence tree. The committed `evidence/qa-gates/final-tests.md` and `coverage-comparison.md` are single-assembly (`QuickFiler.Test`) runs that the evidence itself labels "NOT MEASURABLE" as a repo-wide denominator. The General Unit Test Policy >= 80% repo-wide floor (AC7) therefore cannot be confirmed.
- **Why it blocks:** Policy fail-closed rule — a missing required coverage artifact must not be marked PASS. Coverage verification is mandatory for every language with changed files; C# has changed files in the branch diff.
- **Scope of impact:** AC7 (feature-audit), Section 1.2 / Section 8 (policy-audit), Findings Table R-Major (code-review).
- **Not in dispute:** New/changed-code coverage is independently evidenced at 100% of testable lines (6/6 changed lines covered, per `final-tests.md` per-line hit counts); no coverage regression on changed lines (Metrics.cs class +14.5 points). Only the repo-wide floor sub-criterion is unverified.
- **Required action (any one of the following resolves R1):**
  1. Generate a canonical repo-wide cobertura/JaCoCo coverage artifact at `artifacts/csharp/coverage.xml` covering the relevant test assemblies, and record the repo-wide line-coverage figure against the 80% floor; or
  2. Confirm repo-wide C# line coverage via the PR CI coverage run and cite the CI result; and
  3. If the repo-wide figure is below 80%, document it explicitly as a pre-existing condition (the change is additive and does not regress coverage), apply the CLAUDE.md testable-denominator / COM-VSTO exemption framework, and reference the `feature/csharp-coverage-uplift` tracking item.
- **Evidence path(s):** `evidence/qa-gates/coverage-comparison.md` (line 31), `evidence/qa-gates/final-tests.md` (line 13); `ls artifacts/csharp/coverage.xml` -> absent.
- **Note (known environment constraint):** Local full-assembly C# coverage runs are reported to fail on a Moq binding-redirect in this repo, so the repo-wide gate is typically enforced at PR CI. The remediation may legitimately be satisfied by the CI coverage result rather than a local run.

---

## Non-Blocking Items (informational; not remediation-required)

- (Minor) New tests use reflection into private members; acceptable given COM-boundedness. Consider a narrow internal test seam in a future pass.
- (Minor) The site-8 test verifies the seam in isolation rather than via `NonBlockingProducer`; the production call site (Metrics.cs L222) remains uncovered (documented unreachable defensive branch).
- (Info) `Microsoft.Bcl.TimeProvider` / `Microsoft.Extensions.TimeProvider.Testing` dependency approval should be recorded by the maintainer per spec.md.
- (Info) TaskMaster.csproj/packages.config consumer-reference scope expansion is mechanically required and documented.

---

## Handoff

Route R1 to the C# atomic planner/executor (or confirm via CI). All three review artifacts share timestamp `2026-06-28T19-57` in the feature folder above.
