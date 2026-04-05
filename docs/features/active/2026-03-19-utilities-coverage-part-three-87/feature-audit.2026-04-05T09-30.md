# Feature Audit — utilities-coverage-part-three-87 (2026-04-05T09-30)

## Scope and baseline

- **Base branch:** `development`
- **Head commit:** `cac3ac52210fbdeae10a186c1383a8be9595086a`
- **Merge base:** `22a176830d299cd6a108f2b983a69087a71db22d`
- **Feature folder used:** `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`
- **Evidence sources:**
  - `artifacts/pr_context.summary.txt` (primary)
  - `artifacts/pr_context.appendix.txt` (baseline diff / raw evidence)
  - `coverage/coverage.cobertura.xml`
  - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/issue.md`
  - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/spec.md`
  - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/user-story.md`
  - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/plan.2026-03-22T21-00.md`
  - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/remediation-plan.2026-03-27T08-20.md`
- **Feature folder selection rule:** Used the user-specified issue `#87` active feature folder because it matches the authoritative `v2` scoping docs.
- **Review context:** Post-remediation re-audit. Original audit on 2026-03-27 evaluated 7 AC items; 6 passed and 1 failed (coverage gate). A 100-task remediation plan was executed. All tasks are complete.

## Acceptance criteria inventory

Authoritative acceptance criteria were extracted from `v2/user-story.md` and cross-checked against `v2/spec.md`. Work mode is `full-feature`, so both `spec.md` and `user-story.md` are AC sources.

1. Every `.cs` file compiled by `UtilitiesCS.csproj` has `>= 80%` line coverage as reported by Cobertura.
2. No pre-existing tests are broken or removed.
3. All new tests follow MSTest + Moq + FluentAssertions conventions.
4. All new tests are deterministic, isolated, and use no external dependencies.
5. All new test files are registered in `UtilitiesCS.Test.csproj` (explicit Compile Include).
6. The C# toolchain loop passes clean: format, analyzer build, nullable build, test run.
7. Repository-wide line coverage does not regress below the pre-work baseline.

## Acceptance criteria evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|---|---|---|---|---|
| 1 | Every compiled `UtilitiesCS` file reaches `>= 80%` line coverage | PARTIAL | `coverage/coverage.cobertura.xml` reports aggregate 87.39%. 61 of 63 implementation-routed files are above 80%. Two files remain below: SortEmail.cs (66.7%) and Triage_OlLogic.cs (78.3%). 7 skip-evaluation files are documented. | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` | Per-file requirement technically unmet for 2 files. Both are COM-constrained with documented rationale. Aggregate coverage exceeds threshold by 7.4pp. The spec and user-story contain inline annotations explaining these exceptions. |
| 2 | No pre-existing tests are broken or removed | PASS | Review-time test run: 3910 total, 3908 passed, 2 skipped, 0 failed. No test removals observed in diff. | Same coverage MSTest command | Verified. Consistent with first audit finding. |
| 3 | New tests follow MSTest + Moq + FluentAssertions conventions | PASS | All new test files use `[TestClass]`/`[TestMethod]`, `Mock<T>`, and FluentAssertions `.Should()`. Verified by static inspection of changed test files. | Static inspection + successful compilation in analyzer/nullable builds | Verified. |
| 4 | New tests are deterministic, isolated, and dependency-safe | PASS | No external service calls, no temp files, no timing-dependent assertions. Static state is isolated via `[TestInitialize]`/`[TestCleanup]`. Async tests return `Task`. | Live test run + static inspection | Verified. |
| 5 | New test files registered in `UtilitiesCS.Test.csproj` | PASS | 80 new `<Compile Include>` entries added to `UtilitiesCS.Test.csproj`. All compiled successfully in both analyzer and nullable builds. | Analyzer build, nullable build, MSTest run | Verified. |
| 6 | C# toolchain loop passes clean | PASS | Single-pass clean run: CSharpier (no `.cs` changes), analyzer build (0W/0E), nullable build (0W/0E), MSTest with coverage (3910 total, 0 failed). | All four toolchain commands | Verified. |
| 7 | Repo-wide coverage does not regress below baseline | PASS | Overall repo coverage is 78.05%. UtilitiesCS rose from 69.79% to 87.39%. No per-package regression observed. | Coverage XML analysis | Verified. |

## Summary

- **Overall feature readiness:** **PASS with documented exceptions**
- **Top gaps:**
  1. SortEmail.cs (66.7%) and Triage_OlLogic.cs (78.3%) remain below the per-file 80% threshold due to COM interop constraints. Both are documented in the spec and user-story with inline annotations.
  2. The first acceptance criterion is marked PARTIAL rather than PASS because 2 of 63 implementation files do not meet the per-file requirement, despite the aggregate exceeding 80% by 7.4pp.
- **Recommended follow-up verification:**
  - Future work could extract testable logic from SortEmail.cs and Triage_OlLogic.cs behind COM-safe seams to close the remaining gap.
  - The PR description should document these 2 files as accepted exceptions.

## Acceptance criteria check-off

Per the `acceptance-criteria-tracking` skill:
- AC items 2–7 are already checked off (`[x]`) in `v2/spec.md` and `v2/user-story.md` from prior execution.
- AC item 1 remains unchecked (`[ ]`) because 2 implementation files do not meet the per-file threshold.
- No new check-offs are performed in this re-audit. The existing check-off state accurately reflects delivery status.

### Acceptance Criteria Status

- **Source:** `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/spec.md`, `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/user-story.md`
- **Total AC items:** 7
- **Checked off (delivered):** 6
- **Remaining (unchecked):** 1
- **Items remaining:**
  - Every `.cs` file compiled by `UtilitiesCS.csproj` has `>= 80%` line coverage as reported by Cobertura
    - **Status:** PARTIAL — aggregate is 87.39%; 2 of 63 implementation files below per-file threshold (SortEmail.cs 66.7%, Triage_OlLogic.cs 78.3%); COM-constrained and documented.

## Delta from first audit (2026-03-27T08-20)

| Criterion | First audit status | This re-audit status | Notes |
|---|---|---|---|
| 1. Per-file ≥80% coverage | FAIL (aggregate 69.79%) | PARTIAL (aggregate 87.39%, 61/63 files pass) | Substantial improvement. 2 COM-constrained files remain. |
| 2. No broken tests | PASS | PASS | Maintained |
| 3. MSTest conventions | PASS | PASS | Maintained |
| 4. Deterministic/isolated | PASS | PASS | Maintained |
| 5. csproj registration | PASS | PASS | Maintained |
| 6. Toolchain clean | PASS | PASS | Maintained |
| 7. No coverage regression | PASS | PASS | Maintained |

## Verdict

**PASS with documented exceptions.** The feature has been substantially delivered. 6 of 7 acceptance criteria are fully met. The remaining criterion is PARTIAL due to 2 COM-constrained implementation files that are documented in the scoping docs. The branch is recommended for PR review.
