# Remediation Inputs — outlook-store-exclusion (Issue #328)

- Timestamp: 2026-07-15T21-22
- Author: feature-review
- Base branch: `main` @ `26905c4b737b7fb20cf4e05b92d44fdefb18894e`
- Head: `c0414696f91f03b1ca8e5b33f6920473c9178da8`
- Triggering audit artifacts:
  - `docs/features/active/2026-07-15-outlook-store-exclusion-328/policy-audit.2026-07-15T21-22.md`
  - `docs/features/active/2026-07-15-outlook-store-exclusion-328/feature-audit.2026-07-15T21-22.md`
  - `docs/features/active/2026-07-15-outlook-store-exclusion-328/code-review.2026-07-15T21-22.md`

## Why remediation was triggered

- Coverage FAIL: the canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent on the
  review host (mandatory FAIL for a language with changed files).
- Coverage FAIL (branch floor): `StoreWrapper` branch coverage is 64.81%, below the 75% branch floor
  (pre-existing; baseline 65.38%).
- Acceptance criteria AC12 and US-AC4 are graded PARTIAL on the same basis.

Both are procedural / pre-existing coverage items. The feature's code correctness, toolchain, and
new/changed-line coverage are verified as passing; no code-correctness defect requires a fix.

## Enumerated remediation items

### R1 — Emit C# coverage at the canonical artifact path (procedural)

- Files / paths: produce `artifacts/csharp/coverage.xml` (JaCoCo or the format the coverage hook
  parses), OR confirm the PR CI coverage run as the authoritative repo-wide C# coverage gate.
- Expected behavior: the review coverage procedure and `validate-feature-review-coverage.ps1` can read
  a repo-wide C# coverage number directly from the canonical path rather than relying on the
  feature-evidence Cobertura the reviewer parsed by hand.
- Verification: `Test-Path artifacts/csharp/coverage.xml` is true and the file parses to a repo-wide
  first-party C# line coverage >= 85%.
- Note: coverage was in fact produced at
  `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml`
  and independently verified (touched non-exempt classes >= 95% line). This item is about artifact
  placement/format, not missing test coverage.

### R2 — Disposition `StoreWrapper` branch coverage below the 75% floor

- File: `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` (branch coverage 64.81%).
- Expected behavior: either add tests that exercise the uncovered branches to reach >= 75% branch
  coverage, or record a ratified disposition that the sub-floor branch coverage is pre-existing and
  out of this feature's incremental scope (the new `StoreId` capture branches are already covered on
  both arms; the deficit is in pre-existing branches).
- Verification: re-parse the Cobertura `StoreWrapper` `branch-rate` and confirm >= 0.75, or attach the
  maintainer-ratified exemption note.

### R3 — Reconcile AC6 dead-method deletion with the documented non-goal (documentation)

- Files: `docs/features/active/2026-07-15-outlook-store-exclusion-328/spec.md` (§2.2),
  `user-story.md` (Non-Goals), and AC6 wording.
- Expected behavior: the two dead `ToDoEvents` methods (`GetListOfToDoItemsInView`,
  `GetToDoItemsInView`) were deleted, whereas the spec/user-story said they would be threaded and
  deletion would be a separate issue. Update the scoping-doc wording (or open the separate issue
  retroactively) so the documented intent matches the delivered behavior.
- Verification: spec/user-story text and AC6 no longer contradict the deletion.

## Do-not-do list

- Do not weaken any coverage threshold or add a production-source `exclude` entry to satisfy the floor.
- Do not refactor the accepted filter-predicate duplication (`ShouldIncludeStore` / `Decide` /
  `StoreIsIncluded`) under this remediation; it is spec-accepted out of scope.
- Do not re-run or alter the passing toolchain stages except as needed to regenerate coverage.
- Do not narrow scope; the feature-vs-base audit stands.
- Do not add temporary files or non-deterministic APIs to any new tests.

## Pointer to audit artifacts

See the three audit artifacts listed above for full evidence and per-criterion verdicts.
