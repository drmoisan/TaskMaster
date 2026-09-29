# Fail-Before Exception Dossier (P1-T1), issue #931

Timestamp: 2026-09-29T09-07

WhyFailingRunImpossible: The distinct-thread defect is decided by whether the runtime executes a thread-pool work item inline on the waiting thread, a branch no committed test can force without mutating process-global thread-pool state. The file-handle defect is decided by whether another process holds TaskMaster.sln with a share mode that excludes readers, which a committed test cannot arrange without starting an external process. Both of those arrangements are prohibited by the unit-test policy (no mutable global state, no external processes, no temporary files).

## Alternative Proof

Static reproduction (spec Repro steps 1 to 3). The line citations below refer to the pre-edit file state that P0-T12 re-recorded: P0-T12 measured QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs at 361 lines, QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs at 490 lines and UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs at 359 lines, with SHA-256 values equal to the P0-T4 pre-edit hashes, and recorded one `Task.Run(` occurrence in each of the two QuickFiler.Test files and five `GetSolutionFile` occurrences plus one `TaskMaster.sln` occurrence in the FileInfoWrapper test file.

1. BreadcrumbPopupBoundaryCoverageTests.cs lines 52 to 62 (`Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction`): `CreateOwnerOnlyDispatcher` (lines 124 to 134) builds a `BreadcrumbUiDispatcher` with a null context and the test thread's managed id as owner (line 133). The test queues `dispatcher.Dispatch(...)` through `Task.Run` at line 58 and blocks on it at line 59. If the work item runs inline on the test thread, `IsCurrentBoundary()` (QuickFiler/Viewers/BreadcrumbUiDispatcher.cs lines 255 to 278; thread-id branch at lines 276 to 277) returns true, the action runs, `executions` is one and no error is reported, so the test fails for a reason unrelated to the code under test.
2. ItemViewerBreadcrumbThreadAffinityTests.cs lines 319 to 346 (`InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow`): the test clears the owning dispatcher through `ClearViewerDispatcher` (lines 357 to 371), then calls `InitializeBreadcrumbPipeline` from a `Task.Run` work item at line 332 with a blocking wait at lines 335 to 336. If the work item runs inline on the owner thread, the call is on-thread, and the pre-#781 context-reference guard that the remark at lines 311 to 317 claims to discriminate against would also have admitted it, so the test passes without discriminating. The null-owner escape under test is QuickFiler/Viewers/ItemViewer.Breadcrumb.cs lines 435 to 438.
3. FileInfoWrapper_Tests.cs lines 55 to 67 (`OpenRead_ShouldReturnReadableStreamForWrappedFile`) and 339 to 357 (`GetSolutionFile`): the helper walks up from `AppDomain.CurrentDomain.BaseDirectory` to the repository's TaskMaster.sln (lines 341, 345 and 346), and the test opens a read handle on it with the default `FileShare.Read` share mode of `FileInfo.OpenRead()`.

Recorded observation of the file-handle defect (fact 17): the #900 run recorded exactly this failure. Its final coverage iteration 1 failed on `FileInfoWrapper_Tests.OpenRead_ShouldReturnReadableStreamForWrappedFile` with the solution file held by a resident MSBuild node-reuse worker left by that plan's own multi-process rebuilds (docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/qa-gates/p5-t5-mstest-coverage.2026-09-17T02-35.md, loop-context section).

Deterministic failing evidence planned for this item: P3-T1, P3-T3, P3-T5 and P3-T7 apply a deliberately broken guard or a deliberately broken sentinel to each rewritten test and are designed to demonstrate that each rewritten test fails against that broken guard and passes after the revert. P3-T5 in particular inserts `action();` so the delegate also runs inline on the caller, which is the scheduling branch this dossier cannot force. Those tasks have not run at the time this dossier is written, so no result of theirs is stated here.

SearchScope: docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/
SearchPatterns: `fail-before-exception.*.md`
SearchResult: docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/fail-before-exception.2026-09-29T09-07.md (this file; the directory held no file before it was written)

## Output Summary

- A committed failing run of either defect is structurally impossible under the unit-test policy; this dossier is the schema-valid substitute for the fail-before requirement.
- Alternative proof: static reproduction of all three defect sites at their recorded pre-edit lines, plus the recorded #906 failure from the #900 run.
- Deterministic observed-failing evidence is assigned to the Phase 3 negative controls (P3-T1, P3-T3, P3-T5, P3-T7).
