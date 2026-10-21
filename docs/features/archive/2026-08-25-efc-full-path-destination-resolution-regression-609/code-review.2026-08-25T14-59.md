# Code Review: Efc full-path destination resolution regression (#609)

**Review Date:** 2026-08-25
**Reviewer:** feature-reviewer-c3
**Feature Folder:** `docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609`
**Feature Folder Selection Rule:** Refreshed canonical PR context enumerates this active #609 folder and its `issue.md`/`spec.md`; no competing active folder is in scope.
**Base Branch:** `origin/main` at `507a40a549d573b221da0fb59c3e18af5ce8d473`
**Head Branch:** `bug/efc-full-path-destination-resolution-regression-609` at `67db82a928e6b0c023ed16bf42ca48e526f07a0e`
**Review Type:** Final post-remediation re-review

## Executive Summary

The review covers the feature branch relative to merge base `b5c751519c6cf0eaeb2326d9e80b2439aeee7265`. The production delta is limited to `FolderPredictor`: suggestion strings and corresponding row scores now share a private projection that removes only an archive root followed by one separator. The branch also adds direct regression coverage for the display and score representations, including the exact case-variant path that previously exposed a gap.

The review found no actionable implementation defects. The original finding was a case-sensitive root comparison. It is resolved with `StringComparison.OrdinalIgnoreCase` in `ProjectSuggestionPath`, a direct test that failed before the change and passes after it, and final QA evidence recorded for the reviewed head.

**What changed:**

- `FolderPredictor.cs:807,839,845` applies one projection to both string and `FolderRow` suggestion output and creates an aligned `FolderScore` for the displayed value.
- `FolderPredictorTests.cs:192,226` asserts exact-case and case-variant in-root projection, unchanged relative/out-of-root values, separator retention, and score alignment.
- Existing Issue609 router and `EmailFilerConfig` tests protect the full-lookup/archive-relative filing boundary.

**Top 3 risks:**

1. CI status is unavailable from this environment because GitHub CLI is not installed; local canonical evidence is available and was inspected.
2. The full coverage suite was verified from head evidence rather than rerun during this review; the reviewer independently reran the targeted regressions and formatting check.
3. The method intentionally handles only the display projection boundary; future persistence-shape changes require new regression coverage.

**PR readiness recommendation:** **Go** — the prior case-variant defect is directly remediated and all required local evidence is PASS.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 845-858 | Root comparison uses `OrdinalIgnoreCase` against `ArchiveRootPath + "\\"`. | No change required. | The case-insensitive Outlook path contract is restored while a separator blocks a prefix-only match. | Exact diff; direct case-variant regression; `issue-609-case-variant-fail-before.2026-08-25T14-29.md`. |
| Info | `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` | 226-249 | Case-variant in-root projection and score-key alignment are covered. | No change required. | The test targets the previous failure mode at the relevant producer. | Reviewer VSTest: 2/2 passed. |

No Blocker, Major, Minor, or Nit findings.

## Implementation Audit

### C# implementation audit

#### What changed well

- The fix is localized to the archive-root-aware startup projection and is reused by both display representations.
- `archivePrefix` includes one separator, so a path merely beginning with the same root text is not trimmed.
- The row-model score is copied with the projected path, preserving the display/key invariant required by row activation.

#### Type safety and API notes

- The helper remains private and returns a non-null `string`.
- The existing `_globals` guard retains the original input unchanged when globals are unavailable.
- Analyzer and nullable evidence reports zero diagnostics; no public C# API changes were introduced.

#### Error handling and logging

- No new exception path or logging concern is introduced. The change is a pure presentation projection over existing suggestions.

## Test Quality Audit

The focused reviewer command ran `Issue609_FolderPredictor` tests from the built `UtilitiesCS.Test` assembly: 2 passed, 0 failed. The canonical feature evidence records the direct fail-before assertion for the case-variant input, a focused post-fix pass, a full coverage-enabled suite pass of 6,479/6,479, and zero-regression coverage comparison.

### Reviewed test and QA artifacts

- `evidence/regression-testing/issue-609-case-variant-fail-before.2026-08-25T14-29.md` — expected failing test exited 1 because the old result retained the full case-variant path.
- `evidence/regression-testing/issue-609-case-variant-post-fix.2026-08-25T14-29.md` — focused Issue609 FolderPredictor tests passed after the comparison correction.
- `evidence/qa-gates/csharp-tests-coverage-final.2026-08-25T14-29.md` — coverage-enabled suite completed 6,479/6,479.
- `evidence/qa-gates/issue-609-case-variant-coverage-comparison.2026-08-25T14-29.md` — changed method lines and branches are 100% covered.

- **Determinism:** fixed strings and mocked Outlook/global seams.
- **Isolation:** each direct regression constructs a local predictor and scorer.
- **Speed:** reviewer-focused test run completed in 2.4792 seconds.
- **Diagnostics:** fail-before evidence identifies the unprojected case-variant value and suggestion index.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Exact changed C# diff contains only path literals and test setup. |
| No unsafe subprocess or command construction | PASS | No production subprocess changes. |
| Input validation at boundaries | PASS | Existing nullable/global guard is preserved; path projection is bounded by root-plus-separator. |
| Error handling remains explicit | PASS | No new catch, swallow, or error translation behavior. |
| Configuration / path handling is safe | PASS | `OrdinalIgnoreCase` is culture-independent; relative and out-of-root values remain unchanged. |

## Research Log

No external research was required. The review used repository source, refreshed PR context, feature evidence, and check-only local commands.

## Verdict

The feature is ready for normal PR flow. The previously identified defect is directly covered by a failing pre-fix regression and passing post-fix regression, the root-plus-separator invariant is retained, and no prohibited boundary changed. GitHub-hosted CI state remains unavailable locally, but the branch contains current local QA evidence and all reviewer-run checks passed.
