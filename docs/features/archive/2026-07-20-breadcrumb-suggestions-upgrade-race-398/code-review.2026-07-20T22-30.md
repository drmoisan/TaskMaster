# Code Review — Issue #398 (breadcrumb-suggestions-upgrade-race)

- Timestamp: 2026-07-20T22-30
- Base: main @ cd6362f0 | Head: bug/breadcrumb-suggestions-upgrade-race-398 @ 1cb031f6
- Files reviewed: 2 modified C# production, 3 modified C# test

## Executive Summary

The remediation is minimal and targeted, consistent with the repo Bugfix Workflow. The root cause — a
transient empty/partial window in `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` caused by a
synchronous `_model.Clear()` before the first `await` — is addressed correctly by building the upgraded
rows into a local `List<BreadcrumbStateRow>` and swapping them into the model in a single call via a new
`BreadcrumbStateModel.ReplaceRows` method. `ReplaceRows` reconciles the selected index against the new
row count before publishing the replacement list, so no reader can observe a replacement list paired
with a stale out-of-range index. The change preserves the existing readback contract and the
scored/unresolvable/non-scored row-construction semantics.

Design quality is good: the seam is small, cohesive, and well-commented with the reason (the "why") for
the atomic swap. The two blocking-for-merge items are structural/procedural rather than logic defects:
two test files exceed the 500-line limit, and the canonical HEAD coverage artifact is absent (detailed
in the policy audit). No logic defect was found in the production change.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs | whole file | File is 536 lines, exceeding the 500-line limit (baseline 474). | Split into scenario-grouped test files, each < 500 lines. | General Code Change Policy §4 / general-code-change File Size Limit; test code is not exempt. | `awk END{NR}` head=536, `git show cd6362f0:` baseline=474 |
| Major | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs | whole file | File is 545 lines, exceeding the 500-line limit (baseline 426). | Split into scenario-grouped test files, each < 500 lines. | General Code Change Policy §4 / general-code-change File Size Limit; test code is not exempt. | `awk END{NR}` head=545, `git show cd6362f0:` baseline=426 |
| Minor | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | field `_rows` (line ~186) and `ReplaceRows` (lines ~222-238) | `_rows` changed from `readonly` to a mutable field published by reference swap; the class is documented as being read/mutated across the UI thread and thread-pool continuations, but the swap is a plain field assignment with no memory barrier. | Optionally document the memory-model assumption (reference assignment is atomic; a lagging reader observes the pre-swap list, which still satisfies the count invariant), or make the field access explicit about single-writer intent. | Reference publication without a barrier is functionally correct here because the selection is reconciled before publish and readers never see a torn or empty list; noting the assumption aids future maintainers. | Diff of BreadcrumbStateModel.cs; issue.md "Secondary concern" note |
| Info | artifacts/csharp/coverage.xml | canonical path | Canonical HEAD C# coverage artifact absent (stale leftover removed); new/changed-code coverage of 100% is documented only in narrative evidence. | Regenerate the canonical artifact at HEAD scoped to first-party instrumented packages, or cite the PR CI coverage run. | Coverage verification is mandatory for changed languages; see policy audit Section 5. | policy-audit.2026-07-20T22-30.md §5.1 |

## Positive Observations

- `SetSuggestionsAsync` no longer mutates shared model state while awaiting the provider; the atomic
  swap removes the race window described in issue #398.
- `ReplaceRows` reconciles selection before publishing the new list — the ordering is correct and the
  intent is documented in an XML doc comment and inline comments explaining the "why".
- Scored/unresolvable-fallback/non-scored row construction is preserved verbatim; the fallback path now
  builds a plain row carrying the score's folder path, retaining the selection contract.
- Tests use a `TaskCompletionSource`-gated fake `IFolderHierarchyProvider` with no timing sleeps,
  satisfying the determinism requirements.

## Toolchain Note

CSharpier, .NET analyzer build, nullable build, and the full MSTest suite (5061/5061) are recorded as
EXIT_CODE 0 in the executor qa-gate evidence. These were not re-executed by the reviewer.
