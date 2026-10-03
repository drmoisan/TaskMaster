# Planner review records, revision round 1 (issue #968, with #972 folded in)

- Timestamp: 2026-10-02T22-44
- Plan: plan.2026-10-02T05-42.md (revised in place; version 1.1)
- Scope of this pass: the ten round-1 deltas (option B of defect 2 superseded as directed), spec amendment 1.2 (AC25 to AC32, amended AC20), and a full re-derivation of every citation the deltas or the fold touch, plus their sibling regions.
- Tooling in this session: Read, Grep, Glob, Edit, Write. No shell (the Bash tool was not available), so git state was taken from the worktree's `.git` metadata and the coordinator's statement; every file and line citation was re-read from the item worktree.

SELF-REVIEW: RE-DERIVED THIS PASS

Citations re-derived in this pass (file and line, test or identifier):

1. QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs — 342 lines (Grep `^`); `lock (FieldLock)` 66, 79, 94, 128 (4); `CompareExchange(` 92, 271, 336 (3); every line CRLF. Sibling check: the F-FIELDS comment now reads `only while FieldLock is held`, so the post-change `lock (FieldLock)` count is 5, not 6 (round-1 defect 3).
2. QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs — 470 lines; `[TestMethod]` 8; the 20-line repository-wide `EnsureUiThreadDispatcher|EnsureDispatcher` census (fixture 5, test support 2, fixture tests 10, focus-and-theme 2, InitializationTests.Part2 1) and the 23-line `BeginTransactionAsync\(` control (6 files) re-counted.
3. QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs — 497 lines, `[TestMethod]` 17. QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs — 440 lines. (The round-1 reviewer verified the remaining line citations of these four files against the same tree, and P0-T3's `CODE-TREE-AT-BASE` gate re-asserts that no code path moved since BASE.)
4. QuickFiler.Test/QuickFiler.Test.csproj — Compile items 155, 157, 161, 183 (datamodel tests), 200, 201, 203, 212, 227 (`TestSupport\WinFormsPumpHost.cs`), 228 (`TestSupport\DedicatedWorkerThread.cs`), 229 (`TestSupport\WinFormsPumpHostTests.cs`); no item for the three new files.
5. QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs — namespace `QuickFiler.Test.TestSupport` (line 4), `internal static class` (line 21); 48 lines; no `#nullable`. Nine files carry `using QuickFiler.Test.TestSupport;` (Grep), for example QfcItemController.SeamFactoryTests.cs line 13 between `using QuickFiler.Interfaces;` and `using TaskVisualization;`.
6. QuickFiler/Controllers/QfcDatamodel.cs — 495 lines; `[ExcludeFromCodeCoverage]` 25; `log` field 109-111; `//worker.RunWorkerCompleted` 194; `//e.Result =` 209-210; `_remainingLoadTask = loaderTask;` 218; `_remainingLoadActive = false;` 227; `Worker_RunWorkerCompleted` 243-265 with blank 242; `InitEmailQueue` 271-315 (`_remainingLoadActive = true;` 284, 311; `WorkerStarter(worker);` 285, 312); one-argument `LoadRemainingEmailsToQueueAsync` 335-376 (commented `nameof` 363, live `nameof(LoadRemainingEmailsToQueue)` 369); synchronous `LoadRemainingEmailsToQueue` 378-416; two-argument `LoadRemainingEmailsToQueueAsync` 418-465 (`#pragma` 436, 457); empty region 469-472; `Application_NewMailEx` 476-491; seven `#region`/`#endregion` pairs; removal total 128 lines, expected 367 after.
7. Zero-caller proof (fact 15): Grep `Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue\b|LoadRemainingEmailsToQueueAsync|Linked List Locking` over `*.cs` = 24 lines in 5 files; `\blog\b` over QfcDatamodel*.cs = 3 lines; string-literal/reflection sweep = 2 lines (QfcHomeControllerRunAsyncTests.cs 376 invoked on `_controller` at 373-380; QfcDatamodel.cs 130 cref); IQfcDatamodel.cs members at 103, 117, 131, 138-148, 164, 166 (none of the four); InternalsVisibleTo grants at QuickFiler/Properties/AssemblyInfo.cs 5, QuickFiler/Controllers/QfcHomeController.cs 15, QuickFiler/Legacy/IAcceleratorCallbacks.cs 5, QuickFiler/Controllers/QfcHighConfidencePreFilter.cs 11; QfcDatamodel.FrameBuilding.cs references none of the four.
8. QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs — 413 lines; `_remainingLoadActive` doc 15-23 (`RunWorkerAsync` 17, `written on the worker thread and read` 22), declaration 24; `_remainingLoadTask` 43; QuiesceLoaderAsync comment `written on the worker thread` 52; `TryUnhookOrReplace` 146, 285 (with `(:31-66)`), 364; gate construction 299-309 with `() => _remainingLoadActive,` 305; `await gate.DequeueAsync` 311; `WaitForQueue` 404-411. Repository-wide `_remainingLoadActive|_remainingLoadTask` = 20 lines in 7 files.
9. QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs — `DequeueAsync` 190-301; `alreadyWaitedForEmptySource` 215, 252; empty-take branch 244-257 with `ConfigureAwait(false)` at 255.
10. QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs — 312 lines, `[TestMethod]` 4; usings 9 and 12-13; nested worker 47-61; DrainableSynchronizationContext 62-87; test 1 100-164 (`new FakeTimeProvider()` 114, worker 127-128, `Task.Yield` 140, 142, 157, `fake.Advance` 139, 141, 156, retry loop 154); ReadLivenessFlag 166-172; StartHeldOpenLoader 174-209 (worker 201-202); callers 221, 249, 285; `FakeTimeProvider` only at 9 and 114; spans T1-LIVE 110-166 and HELD 183-218.
11. QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs — 244 lines, `[TestMethod]` 5; nested worker 59-74; `using` blocks at 180 and 220; starter 222; `Duplicated per file` 63.
12. QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs — 232 lines, `[TestMethod]` 3; remarks 33; doc 97-99; nested worker 114-128; sites 143/148, 172/173/176, 202/221; `Duplicated per file` 117.
13. QuickFiler.Test/Controllers/QfcDatamodelTests.cs — 371 lines, `[TestMethod]` 9; sibling test 95-131 (`new BackgroundWorker()` 108, `Task.Yield` 119, because text 123, `await pending` 128); WaitForQueue test 253-283 (`new BackgroundWorker()` 261); `fake.Advance` 118, 127, 241, 280 (4); `FakeTimeProvider` 9, 99, 216, 224, 249, 258 (6); span T-SIB 96-134.
14. UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs — 55 lines; `Armed` 26, `ReArm` 28, `CreateTimer` 39-49, `NewSignal` 52-53. QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs — `CountingTimeProvider : FakeTimeProvider` 358-367 with the `CreateTimer` override at 362. `ArmingFakeTimeProvider|NoSynchronizationContext` over `*.cs`: 0 hits.
15. Worktree git metadata: `.git/worktrees/agent-a291a7fbabf9d0229/HEAD` reads `ref: refs/heads/bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968`; that ref is `87cca65ede6a900cb5c4e5cce93ff4409ed08730`. Both promoted records exist under docs/features/potential/promoted/ (Glob).
16. docs/features/active/.../spec.md — 335 lines; `- [ ] AC` 32 (lines 275-306), `- [x] AC` 0, `Amendment 1.2` 1, `Amendment 1.1` 1, `acquired and released inside a held` 3, `the removal of its baseline pin` 1, `inherited committed set` 2 (36 distinct matching lines). issue.md line 12 and the Coordinator Scope Amendment at 65-77.
17. Round-1 report and summary (evidence/other/preflight-round1*.2026-10-02T08-40.md) read in full; each of the ten deltas traced to its plan location.

Sibling-region re-checks: the N1 test 4 replacement changes the per-file PC counts (`EnsureUiThreadDispatcher()` 10, `.BeginTransactionAsync()` 5, `foreignTransaction.Install(parked);` 1) and therefore the census (PRIMARY 16, CROSS 32, CONTROL 28, CROSS-only still 16) and the nesting gate (13 of 13); the `transaction.Dispose();` and `transaction.Install(null);` counts are unchanged because the match is case-sensitive. Removing `using Microsoft.Extensions.Time.Testing;` from the Liveness file is deferred to the test-1 rewrite (L1b in P5-T3) because test 1 still constructs a `FakeTimeProvider` after the Phase 4 edits; QfcDatamodelTests keeps that using (two other tests use it). The `written on the worker thread` token stays at 1 after the change because the unchanged QuiesceLoaderAsync comment at line 52 carries it; AC26's token is the longer `written on the worker thread and read`. The `nameof(LoadRemainingEmailsToQueueAsync)` count is 1 before (the commented line 462) and 1 after (the retargeted line 369), so the discriminating tokens are the `} Error.` and `} Task cancelled` suffixed forms. `#region`/`#endregion` substring counts do not overlap (`#endregion` does not contain `#region`). The sensitivity check (P5-T8) runs before the QueueProcessing comment edits (P5-T11) so its revert proof is an anchored `--exit-code` diff against BASE. `FILTER-DATAMODEL`'s trailing dots keep `QfcDatamodelTests.` from matching `QfcDatamodelTeardownTests`, `QfcDatamodelLivenessTests` or `QfcDatamodelRethrowTests` (21 tests: 4, 5, 3, 9). CMD-TOKEN-COUNT tokens never contain a double quote.

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs | lines 12-31, 34-46, 92-104, 116-138, 146, 231, 243-274, 284-341
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | lines 33, 44-98, 107-149, 157-190, 192-276, 285
CITATION: QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs | lines 27, 98-117, 181-186, 193, 213, 235, 254, 314, 331, 367, 447-478
CITATION: QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs | lines 85-96, 161-181, 216-239, 251-280, 282-305
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | lines 17, 35, 155, 157, 161, 183, 196-215, 226-230
CITATION: QuickFiler.Test/SetupAssemblyInitializer.cs | lines 14-25
CITATION: QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs | line 124
CITATION: QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs | line 203
CITATION: QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs | line 13
CITATION: QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs | lines 4, 21
CITATION: QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs | lines 9, 12-13, 18-24, 47-61, 62-87, 100-164, 166-172, 174-209, 211-234, 236-270, 272-310
CITATION: QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs | lines 12, 18-27, 59-74, 180, 220-222
CITATION: QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs | lines 12, 23-35, 93-100, 114-128, 136-154, 165-183, 195-230
CITATION: QuickFiler.Test/Controllers/QfcDatamodelTests.cs | lines 9, 12, 95-131, 201-211, 253-283
CITATION: QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs | lines 357-367
CITATION: QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs | lines 325, 370-380
CITATION: QuickFiler/Controllers/QfcDatamodel.cs | lines 25-26, 34-54, 77-103, 107-112, 128-152, 188-195, 197-241, 242-267, 271-315, 335-376, 377-416, 417-465, 467-474, 476-491
CITATION: QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | lines 15-24, 37-43, 48-66, 146, 280-291, 299-311, 364, 404-411
CITATION: QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs | line 11
CITATION: QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | lines 190-301
CITATION: QuickFiler/Controllers/QfcHomeController.cs | lines 92, 132, 344, 379
CITATION: QuickFiler/Interfaces/IQfcDatamodel.cs | lines 103, 117, 131, 138-148, 164, 166
CITATION: QuickFiler/Properties/AssemblyInfo.cs | line 5
CITATION: QuickFiler/Legacy/IAcceleratorCallbacks.cs | line 5
CITATION: QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | line 11
CITATION: QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs | lines 274-286
CITATION: UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs | lines 427-445
CITATION: UtilitiesCS/Threading/UiThread.cs | lines 266-285
CITATION: UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs | lines 19-54
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | lines 89-93, 97-134, 262, 297-298, 348-355, 399-423, 430-453, 459-461
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 | lines 117-123
CITATION: scripts/vscode/TaskMaster.cli.runsettings | lines 4-7
CITATION: scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 | line 21
CITATION: scripts/vscode/Install-RepoDotNetSdk.ps1 | line 3
CITATION: scripts/vscode/Invoke-Restore.ps1 | lines 1-10
CITATION: .gitignore | lines 26, 140, 141, 146, 150, 151
CITATION: .gitattributes | line 4
CITATION: .csharpierignore | lines 4, 12
CITATION: global.json | lines 2-9
CITATION: dotnet-tools.json | line 6
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md | lines 6-13, 56-79, 94-108, 138-171, 173-186, 237-270, 274-306, 308-318
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/issue.md | lines 12, 65-77
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md | sections 1.1, 2.1, 2.2, 3, 4, 5, 6, 7
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T22-20-qfc-datamodel-972-fold-research.md | sections 1 to 8 and Numeric Derivation Evidence
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round1-report.2026-10-02T08-40.md | defects 1 to 10
CITATION: docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md | exists (Glob)
CITATION: docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md | exists (Glob)
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17, AC18, AC19, AC20, AC21, AC22, AC23, AC24, AC25, AC26, AC27, AC28, AC29, AC30, AC31, AC32
AC-MAPPING: AC1 | IMPLEMENTATION: P1-T1, P2-T1 to P2-T5 | TESTS: P1-T5, P2-T8, P8-T5 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC2 | IMPLEMENTATION: P1-T1, P2-T4, P2-T5 | TESTS: P2-T8, P8-T5 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC3 | IMPLEMENTATION: P1-T1, P2-T5 | TESTS: P1-T6, P2-T8, P6-T5, P6-T2 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC4 | IMPLEMENTATION: P1-T1, P2-T5 | TESTS: P1-T6, P2-T8, P8-T5 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC5 | IMPLEMENTATION: P1-T1, P1-T2, P2-T1 to P2-T5 | TESTS: P1-T5, P2-T8 | EVIDENCE: FEATURE/evidence/regression-testing/fail-before-pin-count.md
AC-MAPPING: AC6 | IMPLEMENTATION: P1-T1 | TESTS: P1-T6, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC7 | IMPLEMENTATION: P3-T3, P3-T4 | TESTS: P6-T6, P6-T2 | EVIDENCE: FEATURE/evidence/regression-testing/focus-and-theme-class-pass-after.md
AC-MAPPING: AC8 | IMPLEMENTATION: P3-T3, P3-T4, P3-T8 | TESTS: P7-T1, P7-T2 | EVIDENCE: FEATURE/evidence/qa-gates/call-site-census.md
AC-MAPPING: AC9 | IMPLEMENTATION: P2-T1, P2-T4, P2-T5 | TESTS: P2-T6, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC10 | IMPLEMENTATION: P3-T7, P3-T8 | TESTS: P6-T5, P6-T2 | EVIDENCE: FEATURE/evidence/regression-testing/fixture-class-pass-after.md
AC-MAPPING: AC11 | IMPLEMENTATION: P2-T2, P2-T3, P2-T5 | TESTS: P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC12 | IMPLEMENTATION: P3-T5 | TESTS: P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC13 | IMPLEMENTATION: P3-T7 | TESTS: P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC14 | IMPLEMENTATION: P3-T8 | TESTS: P6-T5, P6-T2 | EVIDENCE: FEATURE/evidence/regression-testing/fixture-class-pass-after.md
AC-MAPPING: AC15 | IMPLEMENTATION: P3-T1, P3-T2, P3-T6 | TESTS: P6-T6, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC16 | IMPLEMENTATION: P3-T3, P3-T4 | TESTS: P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC17 | IMPLEMENTATION: P3-T5, P3-T6 | TESTS: P3-T9, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC18 | IMPLEMENTATION: P1-T1 to P5-T11, P6-T1 | TESTS: P8-T8 | EVIDENCE: FEATURE/evidence/qa-gates/file-line-counts.md
AC-MAPPING: AC19 | IMPLEMENTATION: P1-T1 to P5-T11 | TESTS: P7-T3 | EVIDENCE: FEATURE/evidence/qa-gates/prohibited-constructs-grep.md
AC-MAPPING: AC20 | IMPLEMENTATION: P6-T9 | TESTS: P8-T9 | EVIDENCE: FEATURE/evidence/qa-gates/footprint-scope.md
AC-MAPPING: AC21 | IMPLEMENTATION: P1-T2 | TESTS: P1-T4, P8-T5 | EVIDENCE: FEATURE/evidence/qa-gates/coverage-summary.md
AC-MAPPING: AC22 | IMPLEMENTATION: P8-T1 to P8-T5 | TESTS: P8-T5 | EVIDENCE: FEATURE/evidence/qa-gates/toolchain-final.md
AC-MAPPING: AC23 | IMPLEMENTATION: P0-T17, P8-T5 | TESTS: P8-T6 | EVIDENCE: FEATURE/evidence/qa-gates/coverage-comparison.md
AC-MAPPING: AC24 | IMPLEMENTATION: P1-T1 to P3-T8 | TESTS: P6-T7 | EVIDENCE: FEATURE/evidence/regression-testing/concurrent-set-test-summary.md
AC-MAPPING: AC25 | IMPLEMENTATION: P4-T2 to P4-T6 | TESTS: P4-T11, P6-T8, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC26 | IMPLEMENTATION: P5-T11 | TESTS: P5-T12, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/queue-processing-comment-census.md
AC-MAPPING: AC27 | IMPLEMENTATION: P4-T1, P4-T8 | TESTS: P4-T10, P8-T3, P8-T4, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/qfc-datamodel-legacy-callers.md
AC-MAPPING: AC28 | IMPLEMENTATION: P4-T8 | TESTS: P8-T8 | EVIDENCE: FEATURE/evidence/qa-gates/file-line-counts.md
AC-MAPPING: AC29 | IMPLEMENTATION: P4-T8 | TESTS: P6-T2, P8-T6 | EVIDENCE: FEATURE/evidence/qa-gates/coverage-comparison.md
AC-MAPPING: AC30 | IMPLEMENTATION: P4-T5, P4-T6, P4-T7, P5-T3, P5-T4 | TESTS: P6-T8, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC31 | IMPLEMENTATION: P5-T2, P5-T3, P5-T4 | TESTS: P5-T1, P5-T7, P5-T8, P5-T10, P6-T2 | EVIDENCE: FEATURE/evidence/regression-testing/liveness-sensitivity-check.md
AC-MAPPING: AC32 | IMPLEMENTATION: P4-T2 to P5-T11 | TESTS: P6-T8 | EVIDENCE: FEATURE/evidence/regression-testing/datamodel-set-test-summary.md
UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
Executor preflight for this revision has not yet run; the signal below is the planner's request line for the confirming round, not a self-approval and not a discovered defect.
PREFLIGHT: REVISIONS REQUIRED
