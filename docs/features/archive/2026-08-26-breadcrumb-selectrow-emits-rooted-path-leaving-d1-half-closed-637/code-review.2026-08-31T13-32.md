# Code Review: Issue #637 rooted breadcrumb selection normalization

## Executive Summary

The implementation correctly confines normalization to full Outlook paths with a bound archive root, preserves archive-root-exact and no-bound-root behavior, and routes filing normalization through a total pure helper. Current-head formatter, analyzer, nullable, and full MSTest coverage checks passed. One blocking maintainability finding remains: a modified test fixture is 694 lines, exceeding the repository policy limit.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | `:1-694` | The feature modifies a 694-line test file, exceeding the 500-line limit for modified test code. | Split the Issue #439 fixture into cohesive test classes/files below 500 lines; preserve test names/semantics and add required non-SDK project compile includes. Rerun the full C# toolchain. | The repository policy applies the limit to modified test code and provides no legacy-file exception. | Line count is 694; `git diff main...HEAD` shows the file is modified; `AGENTS.md` policy. |

## Reviewed Implementation

- `BreadcrumbBridgeRouter.Selection.SelectRow` rejects out-of-root and root-exact full paths without calling `CommitSelection`, and commits the relative stem for eligible rooted targets.
- `EfcDataModel.ToFilingStemOrVerbatim` returns a non-empty archive-relative stem only for a rooted candidate under its supplied ancestor; all other values are preserved verbatim.
- `MoveToFolderAsync(string, ...)` delegates only its `DestinationOlStem` value to the helper; unrelated overloads and folder-opening paths are not changed.
- The corrected Issue #439 assertion retains the rooted provider lookup while asserting the new relative selected value. AC21 records this as a deliberate specification correction, not a weakened test.

## Verification Evidence

- `git diff --check main...HEAD` passed.
- CSharpier format and check passed at `a314228b9c3d9a4944a9e88e1a4eb4bd9c4b0f7b`.
- Analyzer and nullable rebuilds passed with 0 errors; each retained five existing `System.Reactive` packages.config warnings.
- Current-head coverage passed 6,894 tests with 0 failures across 9 assemblies and reported 85.3545% line coverage.

## Conclusion

The behavioral implementation has no additional code-level blocker. PR readiness is blocked by the test-file-size policy finding until it is remediated.
