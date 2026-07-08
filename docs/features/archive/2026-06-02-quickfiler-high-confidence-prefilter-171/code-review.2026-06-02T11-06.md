# Code Review — quickfiler-high-confidence-prefilter (Issue #171)

- Date: 2026-06-02T11-06
- Reviewer: feature-reviewer agent
- Review type: RE-AUDIT following remediation (supersedes `code-review.2026-06-02T10-36.md`)
- Base: `development` @ `5e944344041b10becb98c56d358176fc9e7b8ee9`
- Head: `bug/quickfiler-high-confidence-prefilter-171` @ `9ddaa32e750be3ef29c9103cb8b7852b8ea6a9e7`
- Scope: full branch diff vs base

## Executive Summary

The implementation is well-structured and matches the design in `spec.md`. The new pre-filter logic is isolated in a dedicated file, reuses the existing scoring path through a narrow seam, and is covered by focused, deterministic tests. The high-confidence branch in `RunAsync` runs scoring off the UI thread and routes survivors through a carrier-list load path that does not invoke the post-UI removal pass. Folder pre-selection is implemented by extracting a pure `PopulateAndSelectFolder` helper, a clean and testable refactor of the prior inline index-1 selection.

This re-audit confirms the two process-level items from the prior round are resolved:
- The blocking coverage-artifact gap is closed: `artifacts/csharp/coverage.xml` now exists and was independently parsed by the reviewer (new file 100%, no changed-line regression).
- The unrelated `TaskMaster.csproj` reformat is reverted; the file matches base and the trailing newline is restored.

No blocking code-quality defects were found in the feature logic. Remaining items are low-severity maintainability nits and informational notes.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Resolved | `artifacts/csharp/coverage.xml` | build artifact | Prior-round blocking finding: canonical C# coverage artifact was absent. Now present (Cobertura, 29.8 MB) with per-line `hits`; reviewer-verified new file 100%, no changed-line regression. | None; gate satisfied. | Workflow mandates coverage verification from the canonical artifact for every language with changed files. | reviewer parse of `artifacts/csharp/coverage.xml`; `policy-audit.2026-06-02T11-06.md` §5. |
| Resolved | `TaskMaster/TaskMaster.csproj` | whole file | Prior-round low finding: project file reformatted (attributes collapsed; trailing newline removed). Now reverted. | None. | C# policy warns formatter rewrites of `.csproj` can mis-handle legacy VSTO projects. | `git diff development -- TaskMaster/TaskMaster.csproj` = no content diff; `evidence/qa/csproj-diff-after-171.2026-06-02T10-36.txt`. |
| Low | `QuickFiler/Controllers/QfcCollectionController.cs` | carrier `LoadControlsAndHandlers_01Async` | The new carrier overload duplicates the bulk of the existing `IList<MailItem>` overload body (helpers, layout suspend/resume, group encapsulation, helper drain loop), differing only in predetermined-folder threading. | Consider extracting the shared body into a private helper parameterized by a folder selector to reduce ~85 lines of near-duplication in an already-oversized file. Defer to a follow-up; spec forbids refactoring this controller in scope. | DRY; reduces future drift between the two overloads and limits growth of an oversized file. | diff carrier overload region. |
| Low | `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` (feature source file) | Assembly-level attribute placed in a feature source file rather than `AssemblyInfo.cs`; enables Moq to proxy the internal seam. | Acceptable; prefer centralizing `InternalsVisibleTo` in `AssemblyInfo.cs` for discoverability. | Scattered assembly attributes are harder to find and audit; functional maintainability nit. | source file header. |
| Info | `QuickFiler/Controllers/QfcFormController.cs` | carrier `LoadItemsAsync(IList<QfcPreScoredItem>, ProgressTracker)` | Path calls `await _groups.LoadSecondaryAsync()` after construction; `LoadSecondaryAsync` runs folder/conversation loads and `AssignFolderComboBox()` but performs no below-threshold removal. | None required; optionally add a one-line comment noting `LoadSecondaryAsync` here is load-only, not a removal pass. | Prevents a future reader from assuming the secondary pass re-introduces post-UI filtering; AC6 is satisfied. | `QfcCollectionController.LoadSecondaryAsync` has no `RemoveBelowThresholdAsync` call. |
| Info | `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | `FolderScoringService` `[ExcludeFromCodeCoverage]` (line 157) | The COM-bound adapter is excluded from coverage and not unit-tested. Verified intact and not relaxed during remediation. | Acceptable per policy (live Outlook COM prohibited in unit tests); keep it thin. | Legitimate I/O boundary exclusion; keeps the measured surface focused on testable filter logic. | source line 157; confirmed by grep during re-audit. |
| Info | `QuickFiler/Controllers/QfcItemController.cs` | `PopulateAndSelectFolder` | Pure WinForms-only selection helper extracted from `AssignFolderComboBox`; preselects the predetermined folder when present, else index 1. | None. | Good separation of pure UI-selection logic from marshaling; enables the two `AssignFolderComboBox_*` tests. | covered per `evidence/coverage/remediation-changed-line-verification-171.2026-06-02T10-36.md`. |
| Info | `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | `CarrierLoad_SetsPredeterminedFolderOnItemGroup` | Test verifies the carry contract by replicating the group-level assignment rather than driving the full COM/WinForms overload. | None; documented limitation. | Honest test boundary; the full path is COM-bound and not unit-testable, consistent with policy. | test source. |

## Notes on Test Quality

- New tests are deterministic, isolated, and follow Arrange/Act/Assert with FluentAssertions and Moq (including a `MockBehavior.Strict` scoring mock that asserts cancellation short-circuits scoring).
- The `[DoNotParallelize]` addition to `OlTableExtensions_Tests` is a correct mitigation of process-wide `Console.Out` state contention and improves determinism.
- Boundary/edge coverage for the filter is thorough (cutoff-inclusive, zero-score exclusion, null/empty, all-below, order preservation, cancellation).
- Coverage of the new file is now machine-verifiable at 100% from the canonical artifact.

## Overall Recommendation

Go for merge. The prior blocking coverage-artifact gap is resolved and verified, the `.csproj` reformat is reverted, and the feature logic and tests are of good quality. The remaining low/info items are non-blocking maintainability nits that may be addressed in follow-up work; the spec explicitly forbids refactoring the oversized controllers in this scope.
