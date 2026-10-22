# Issue Update Mirror — issue #781

Timestamp: 2026-09-05T17-14

Task: [P2-T12]

PostedAs: comment

Comment URL: https://github.com/drmoisan/TaskMaster/issues/781#issuecomment-5554826348

Issue URL: https://github.com/drmoisan/TaskMaster/issues/781

Command: `gh issue comment 781 --body-file <scratchpad path outside the repository>`

EXIT_CODE: 0

## Why a comment rather than a body update

The task permits `PostedAs: body`, `PostedAs: comment`, or a `POSTING BLOCKED` header. `gh` is
present and authenticated in this session, so `POSTING BLOCKED` would have been a false claim.
A body update was not chosen because the remote issue body contains **no** acceptance-criteria
checkbox lines: a query of the remote body for lines matching `- [ ] AC<n>:` or `- [x] AC<n>:`
returned zero rows. Replacing that body with the local `issue.md` content would have restructured
the issue rather than mirrored a checkbox state into it. A comment records the same state without
rewriting the reporter's original text.

Because this is not `PostedAs: body`, no mirror back into the local `issue.md` is required; the
local file is already the authoritative source and is what the comment reports.

## Exact AC checkbox state of `FEATURE/issue.md` after [P2-T11]

- [x] AC1: `ItemViewer.ThrowIfOffUiBoundary` proves UI ownership by owner-thread identity (the thread that constructed the viewer, for example via the `Dispatcher` captured in the constructor or the constructing thread's managed thread id) and no longer compares `SynchronizationContext.Current` by reference against `UiSyncContext`.
- [x] AC2: A guarded member (`InitializeBreadcrumbPipeline`, both `ConfigureBreadcrumbDropDown` overloads, `EnsureBreadcrumbResourceOwnership`) called on the owning thread succeeds regardless of the ambient `SynchronizationContext` at the call site: null, a different plain `SynchronizationContext` instance, or a `DispatcherSynchronizationContext` installed by a WPF dispatcher operation.
- [x] AC3: A guarded member called from a different thread (for example a `Task.Run` worker) still throws `InvalidOperationException` whose message names the operation, and the exception is not an `ObjectDisposedException`.
- [x] AC4: A regression test reproduces the production shape: the viewer is constructed while one `SynchronizationContext` instance is ambient (the dispatcher-operation shape) and `InitializeBreadcrumbPipeline` is then called on the same thread under a different ambient context, and it succeeds. The test is deterministic, uses no sleeps, timers, or temporary files, and needs no message pump.
- [x] AC5: The tests that encoded the old reference comparison (`InitializeBreadcrumbPipeline_..._AmbientNull...` and `InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic` in `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`) are replaced or corrected so the suite asserts the thread-identity contract; the #488 D3 (second-provider fail-fast) and D5 (disposal) behaviors and their tests remain unchanged and passing.
- [x] AC6: The XML documentation on `ThrowIfOffUiBoundary` describes the thread-identity contract and states why context reference equality was unsuitable (viewers built inside dispatcher operations capture a `DispatcherSynchronizationContext`), and the `EnsureBreadcrumbResourceOwnership` statement-order comment stays accurate.
- [x] AC7: The fix does not change `QfcCollectionController.LoadSecondaryAsync`, `QfcItemController.AssignFolderComboBox`, or `QfcItemController.EnsureBreadcrumbPipeline`; scope is limited to `QuickFiler/Viewers/ItemViewer*.cs` production files and their tests.
- [x] AC8: The full C# toolchain passes in one consecutive pass (`dotnet tool run csharpier check .`, analyzer rebuild, nullable rebuild with warnings as errors, `vstest.console.exe ... /EnableCodeCoverage /InIsolation`), new or changed code reaches at least 90 percent line coverage, and the canonical `artifacts/csharp/coverage.xml` is produced.

All eight criteria are checked, each exactly once, as verified against the file on disk.

## Text posted

The comment carries the eight checked criteria above in abbreviated form, a four-row change
summary naming the four Write Set files and the change made to each, and a verification section
recording: the fail-before result (7 executed, 5 failed, 2 passed), the pass-after result (33 of
33 passed), the four toolchain gate outcomes from one consecutive pass, and the coverage figures
(0.848347 before, 0.848316 after, denominator 64740 both times) together with the
`CHANGED-CODE COVERAGE: NOT MEASURABLE` determination and its `[ExcludeFromCodeCoverage]` cause.

Output Summary: The AC state was mirrored to GitHub as a comment on issue #781, exit code 0, and
the comment URL is recorded above. The artifact carries `PostedAs: comment`, satisfying the
[P2-T12] acceptance condition.
