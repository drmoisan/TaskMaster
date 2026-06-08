# Code Review — quickfiler-high-confidence-prefilter (Issue #171)

- Date: 2026-06-02T10-36
- Reviewer: feature-reviewer agent
- Base: `development` @ `5e944344041b10becb98c56d358176fc9e7b8ee9`
- Head: `ae7eb670ee7738640cab2b41bc7226255224f7ca`
- Scope: full branch diff vs base

## Executive Summary

The implementation is well-structured and matches the design described in `spec.md`. The new pre-filter logic is isolated in a dedicated file, reuses the existing scoring path through a narrow seam, and is covered by focused, deterministic tests. The high-confidence branch in `RunAsync` correctly runs scoring off the UI thread and routes survivors through a carrier-list load path that does not invoke the post-UI removal pass. Folder pre-selection is implemented by extracting a pure `PopulateAndSelectFolder` helper, which is a clean, testable refactor of the prior inline index-1 selection.

No blocking code-quality defects were found in the feature logic itself. The most significant code-review item is an unrelated `TaskMaster.csproj` reformat that conflicts with the repo's C# policy guidance (do not use formatters that rewrite `.csproj`). The substantive blocking issue for delivery is the absent canonical C# coverage artifact, tracked in the policy audit and feature audit rather than as a code defect.

One correctness nuance to verify: the carrier-list `LoadItemsAsync` path calls `await _groups.LoadSecondaryAsync()`, which iterates item groups and calls `AssignFolderComboBox()` per group. This is correct and does not perform below-threshold removal (no `RemoveBelowThresholdAsync`/`ApplyHighConfidenceFilterAsync` call on that path), so AC6 holds. Recorded as informational so a future reader does not mistake `LoadSecondaryAsync` for a removal pass.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking (process) | (build evidence) | `artifacts/csharp/coverage.xml` | Canonical C# coverage artifact absent; coverage verifiable only as feature-folder text. | Generate and persist `artifacts/csharp/coverage.xml` (e.g., convert the `/EnableCodeCoverage` output to Cobertura) so coverage gates are machine-verifiable. | Workflow mandates coverage verification from the canonical artifact for every language with changed files; absence is a defined FAIL. | `policy-audit.2026-06-02T10-36.md` §5; `find artifacts/csharp` returns no dir. |
| Low | `TaskMaster/TaskMaster.csproj` | whole file | Project file reformatted (multi-line attributes collapsed; trailing newline removed). Unrelated to feature behavior; conflicts with C# policy "do not use `dotnet format` / formatters that rewrite `.csproj`". | Revert the `.csproj` reformat or justify it explicitly; restore the trailing newline. | Repo C# policy warns formatter rewrites of `.csproj` can mis-handle legacy VSTO projects; the churn is out of scope for #171. | diff range; qa-final notes a pre-existing CSharpier error in this file. |
| Low | `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | line 11, `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` | Assembly-level attribute placed in a feature source file rather than `AssemblyInfo.cs`; enables Moq to proxy the internal seam. | Acceptable, but prefer centralizing `InternalsVisibleTo` in `AssemblyInfo.cs` or a single `Properties` location for discoverability. | Scattered assembly attributes are harder to find and audit; functional but a maintainability nit. | source line 11. |
| Low | `QuickFiler/Controllers/QfcCollectionController.cs` | carrier `LoadControlsAndHandlers_01Async` | The new carrier overload duplicates the bulk of the existing `IList<MailItem>` overload body (helpers, layout suspend/resume, group encapsulation, helper drain loop) with only the predetermined-folder threading differing. | Consider extracting the shared body into a private helper parameterized by a folder selector to reduce ~85 lines of near-duplication in an already-oversized file. | DRY; reduces future drift between the two overloads and limits growth of an oversized file. | diff lines ~411-505. |
| Info | `QuickFiler/Controllers/QfcFormController.cs` | carrier `LoadItemsAsync(IList<QfcPreScoredItem>, ProgressTracker)` | Path calls `await _groups.LoadSecondaryAsync()` after construction; `LoadSecondaryAsync` runs folder/conversation loads and `AssignFolderComboBox()` but performs no below-threshold removal. | None required; optionally add a one-line comment that `LoadSecondaryAsync` here is load-only, not a removal pass. | Prevents a future reader from assuming the secondary pass re-introduces post-UI filtering; AC6 is satisfied. | `QfcCollectionController.LoadSecondaryAsync` lines 519-573. |
| Info | `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | `FolderScoringService` `[ExcludeFromCodeCoverage]` | The COM-bound adapter is excluded from coverage and not unit-tested. | Acceptable per policy (live Outlook COM prohibited in unit tests); ensure it remains thin and is exercised by the existing item-controller integration path. | Legitimate I/O boundary exclusion; keeps the measured surface focused on testable filter logic. | source lines 141-181, documented in the class remarks. |
| Info | `QuickFiler/Controllers/QfcItemController.cs` | `PopulateAndSelectFolder` | Pure WinForms-only selection helper extracted from `AssignFolderComboBox`; preselects the predetermined folder when present, else index 1. | None. | Good separation of pure UI-selection logic from marshaling, enabling the two `AssignFolderComboBox_*` tests. | diff lines ~905-940. |
| Info | `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | `CarrierLoad_SetsPredeterminedFolderOnItemGroup` | Test verifies the carry contract by replicating the group-level assignment rather than driving the full COM/WinForms overload. | None; documented limitation. The end-to-end overload remains uncovered for the same COM reason as its sibling. | Honest test boundary; the full path is COM-bound and not unit-testable, consistent with policy. | test source lines 290-326. |

## Notes on Test Quality

- New tests are deterministic, isolated, and follow Arrange/Act/Assert with FluentAssertions and Moq (including a `MockBehavior.Strict` scoring mock that also asserts cancellation short-circuits scoring).
- The `[DoNotParallelize]` addition to `OlTableExtensions_Tests` is a correct mitigation of process-wide `Console.Out` state contention and improves determinism.
- Boundary/edge coverage for the filter is thorough (cutoff-inclusive, zero-score exclusion, null/empty, all-below, order preservation, cancellation).

## Overall Recommendation

No-go for merge until the blocking coverage-artifact gap is resolved (persist `artifacts/csharp/coverage.xml` and confirm gates). The feature logic and tests are otherwise of good quality; the `.csproj` reformat should be reverted or justified. See `remediation-inputs.2026-06-02T10-36.md`.
