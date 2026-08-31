# Code Review: Issue #637 rooted breadcrumb selection normalization

## Executive Summary

The review of `main...952a760fb19ff9c10007fe2ebb42f8cadd49a886` found no blocking code defect. `SelectRow` normalizes only full paths with a bound root and preserves all specified non-selection and pass-through behavior. The `MoveToFolderAsync(string, ...)` assignment uses a total helper that leaves ineligible values verbatim. The post-remediation Issue #439 fixture split preserves the existing test identities and compile items while resolving the prior file-size blocker.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| None | N/A | N/A | No code-level blocking finding identified. | No remediation required. | The reviewed implementation, test split, static checks, and exact-head QA evidence satisfy the feature requirements. | `git diff --check main...HEAD`; live CSharpier check; `p1-t5` and `p2-t1` through `p2-t7` evidence. |

## Reviewed Implementation

- `BreadcrumbBridgeRouter.Selection.SelectRow` keeps the no-bound-root pass-through, rejects out-of-root full paths, treats archive-root-exact as a deterministic non-selection, and commits a non-empty archive-relative stem for eligible full paths.
- `EfcDataModel.ToFilingStemOrVerbatim` is an internal pure helper. It returns a non-empty archive-relative stem only when `TryMakeArchiveRelative` succeeds; relative, sentinel, root-exact, outside-root, null, and invalid-ancestor inputs remain verbatim without throwing.
- `MoveToFolderAsync(string, ...)` applies the helper only to `DestinationOlStem`; the two folder-opening paths retain their existing assignments.
- The Issue #439 selected-path assertion now expects `Clients\\North`, while the provider lookup remains rooted. This preserves AC21's explicit invariant-driven deliberate specification correction rather than weakening coverage.
- The formerly oversized Issue #439 test fixture is split into a 455-line retained partial and a 253-line activation partial. The project lists both compilation items once, and the named test inventory is preserved.

## Verification Evidence

- Fresh PR context resolves `main` at `3be3f237a8551df3f27f83d9d1af2f26074fc93a` and HEAD at `952a760fb19ff9c10007fe2ebb42f8cadd49a886`.
- `git diff --check main...HEAD` passed during this review.
- `dotnet tool run csharpier check .` passed during this review (1,565 files checked).
- Current-head QA artifacts record formatter, analyzer, nullable, and MSTest coverage success in order: 6,894 passed, 0 failed, and 85.3389% repository line coverage.
- The exact scope evidence enumerates only the Issue #637 product paths; the post-remediation commit changes the planned test/project surfaces and review/evidence artifacts.

## Conclusion

No blocking code-review finding remains. The fixture-size remediation is verified without altering product behavior or the AC21 deliberate specification-correction wording.
