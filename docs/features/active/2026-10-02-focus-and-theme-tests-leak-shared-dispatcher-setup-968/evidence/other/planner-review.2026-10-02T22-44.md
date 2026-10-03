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

## Round-2 delta application (2026-10-02T23-56 deltas)

- Timestamp: 2026-10-03 (session context date; this planning session has no shell clock, so no minute stamp is composed)
- Plan: plan.2026-10-02T05-42.md (revised in place; version 1.2)
- Scope of this pass: the eight round-2 deltas of `preflight-round2-report.2026-10-02T23-56.md` applied in place (the "Orchestration action" bullet of defect 8 is not a plan edit and was not applied), followed by the adversarial re-derivation of every line the deltas touched and of its sibling occurrences across the plan.
- Tooling in this session: Read, Grep, Glob, Edit, Write. No shell; every file and line citation below was re-read from the item worktree.
- Correction to the report: the defect 5 delta states the post-change `QfcDatamodelTests.cs` `FakeTimeProvider` count as 7 ("five untouched lines plus the same 2"). Re-derivation shows the file carries the token on five lines at baseline (99, 216, 224, 249, 258), not six: line 9 is `using Microsoft.Extensions.Time.Testing;`, which does not contain the substring. One of the five (99) lies inside the replaced sibling test, so four untouched lines plus the two `ArmingFakeTimeProvider` lines give 6. The same directive error made the `QfcDatamodelLivenessTests.cs` baseline 2 in fact 17, P0-T13 and P4-T6; the true value is 1 (line 114). The plan now carries 6, 1 and 5 respectively. Each is a stricter, correct observation; no acceptance criterion is weakened.

SELF-REVIEW: RE-DERIVED THIS PASS

Citations re-derived in this pass (file and line, test or identifier):

1. Plan Delivered Source F-SCOPE — plan lines 227 to 269 inclusive, 43 lines; 342 + 5 (F-FIELDS) + 13 (F-CLASSDOC) + 3 (F-ENSURE-DOC) + 1 (F-ENSURE-BODY) + 11 (F-SCOPE) = 375 (defect 1; F-SCOPE prose, P2-T6, P3-T9).
2. QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs — test 1 declaration 110; `(await pending).Should().BeEmpty();` 163; `/// <summary>Reads the issue #424 producer-liveness flag by reflection.</summary>` 166; `private static QfcDatamodel StartHeldOpenLoader(` 183; test 2 declaration 218. CMD-SPAN-TOKEN-COUNT prints `SPAN: <START line>-<END index>`, and the END index is the END line minus one (R4SPAN `212-284` with END at 285), so the baselines are `110-165` and `183-217`, and `(await pending)` is 1 inside T1-LIVE (defects 2 and 3). `await` inside the span 124, 140, 142, 157, 163 (5); whole-file substring 10 lines (50, 102, 178, 263, 288 added), so fact 17 now says "inside test 1". `[TestMethod]` 109, 217, 240, 276 (4). `FakeTimeProvider` on one line (114); line 9 is `using Microsoft.Extensions.Time.Testing;`.
3. QuickFiler.Test/Controllers/QfcDatamodelTests.cs — sibling declaration 96; `public async Task TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate()` 134, so T-SIB prints `96-133`; `FakeTimeProvider` 99, 216, 224, 249, 258 (5; line 9 is the using directive); `fake.Advance` 118, 127, 241, 280 (4); `[TestMethod]` 9; `Task.Yield` 119 only. Post-change: 258 lies before the M3 edit at 261 and 249 before the WaitForQueue test, so 216, 224, 249, 258 are untouched (4) and M-T adds the doc cref and `new ArmingFakeTimeProvider()` (2): 6.
4. QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs — `var gate = new QfcStreamingDequeueConfidenceGate(` 299; `QfcGateBatch batch = await gate.DequeueAsync(quantity, timeOut, _token);` 311; GATE-LAMBDA prints `299-310`.
5. QuickFiler/Controllers/QfcDatamodel.cs — `ForEachAwaitWithCancellationAsync` 431 (comment) and 440 (call), both inside the deleted block 417 to 465, so the baseline is 2 and the post-change value stays 0 (defect 4); `#region` 7 and `#endregion` 7 (32, 105, 107, 154, 156, 184, 186, 267, 269, 467, 469, 472, 474, 493), confirming the P0-T13 values 7, 7 and the post-change 6, 6.
6. Plan Delivered Source L-T1 and M-T after defect 7 — the doc lines now read `a scheduler yield.` and `a scheduler yield,`; `Task.Yield` substring 0 in each block; `ArmingFakeTimeProvider` lines in L-T1: the doc cref (same line, unchanged by the edit) and `new ArmingFakeTimeProvider()` (2); in M-T: the doc cref on the preceding line and `new ArmingFakeTimeProvider()` (2). `worker,` in the post-change Liveness file: three callers plus the `SynchronousBackgroundWorker worker,` parameter line (4; prose only, not gated).
7. Plan numstat convention — P1-T3 writes the project-file row as `1<TAB>0` with a literal tab (Grep `reads \`1\t0\``), so the P4-T9 row `1<TAB>129` is written with a literal tab; 128 deleted lines plus the replaced line 369 give 129 deletions and 1 addition, and `--numstat` is independent of hunk grouping (defect 6). P6-T2 restates P4-T9 by reference and its holds-after-formatting clause now names numstat.
8. Plan occurrence sweep (Grep, before and after editing) — `374` at 271, 1469, 1494 only (all three replaced; none remain); `forty-two` once (replaced); `110-166`, `183-218`, `96-134`, `299-311` at the span-anchor list and P0-T13 only (replaced; the CITATION line `QfcDatamodel.QueueProcessing.cs | ... 299-311` is a line-range citation and is unchanged); `HUNK_COUNT` at the CMD-HUNKS definition, P3-T9 and P8-T27 (TestSupport 2), P5-T12 and P8-T36 (QueueProcessing 2) and P4-T9 (replaced); `FakeTimeProvider` 0 or 5 at 887, 1014, 1528, 1530 only (all replaced); `Task.Yield` 1 at fact 20 (baseline, unchanged), 1014 and 1528 (replaced); `<c>Task.Yield</c>` at 742 and 960 only (replaced); `PRE-IMPLEMENTATION GATE BLOCKED` at D-10 only; `worker,` 3 at 887 only; `no longer names` at 726 only (tightened).
9. D-10 and the payload-channel convention — re-read; the `PWSH CHANNEL REFUSED` rule is distinct and unchanged; the D-10 sentence now names evidence-file Write and pwsh payload refusals and the `PREIMPLEMENTATION_GATE_BLOCKED` prefix (defect 8).
10. Plan structure — `\r$` 0 lines before and after editing (LF preserved); nine `### Phase N — ` headings (0 to 8); task IDs unchanged and sequential per phase.

Sibling-region re-checks: P4-T8 and P8-T37 keep `ForEachAwaitWithCancellationAsync` 0 after the change (both lines are in the deleted block). P5-T1 keeps T1-LIVE `Task.Yield` 3, `fake.Advance` 3, `for (int i` 1 and T-SIB `await Task.Yield();` 1 (baseline, unchanged). P4-T6's interim LIV `FakeTimeProvider` is the baseline value and is now 1. The L1 sentence "the file no longer names `FakeTimeProvider`" is now stated as the type, because the substring count is 2 after L3. CMD-ADDED-SCAN gates `await Task.Yield();` and is unaffected by the doc-line edits. The L-T1 gate-token line already read `Task.Yield` 0 and is now accurate for the whole block. P6-T2's "token, span, hunk and numstat value holds after formatting" inherits the P4-T9 numstat row; the TestSupport and QueueProcessing hunk gates are unchanged. The AC31 wording ("contain no `Task.Yield`") is now met by a whole-file count of 0 in both files, not only by the span gates. No acceptance criterion is weakened: the fixture total, the four spans, the `(await pending)` and `ForEachAwaitWithCancellationAsync` baselines and the `FakeTimeProvider` values are corrected observations, and the numstat row replaces an unsatisfiable hunk count with a check that fails on any extra added or removed line.

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
CITATION: QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs | lines 1-14, 18-24, 47-61, 62-87, 100-166, 166-172, 174-209, 211-234, 236-270, 272-310
CITATION: QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs | lines 12, 18-27, 59-74, 180, 220-222
CITATION: QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs | lines 12, 23-35, 93-100, 114-128, 136-154, 165-183, 195-230
CITATION: QuickFiler.Test/Controllers/QfcDatamodelTests.cs | lines 1-14, 95-134, 201-211, 216, 224, 241, 249, 253-283
CITATION: QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs | lines 357-367
CITATION: QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs | lines 325, 370-380
CITATION: QuickFiler/Controllers/QfcDatamodel.cs | lines 25-26, 32, 34-54, 77-105, 107-112, 128-152, 154-156, 184-195, 197-241, 242-269, 271-315, 335-376, 377-416, 417-465, 467-474, 476-493
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
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round2-report.2026-10-02T23-56.md | defects 1 to 8
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/plan.2026-10-02T05-42.md | lines 7-9, 18-22, 129, 132, 135, 150, 225-271, 726, 732-817, 887, 956-1014, 1293-1296, 1372, 1425, 1448, 1469, 1494, 1509, 1515, 1528-1530, 1554, 1683-1687, 1737-1738 (pre-edit numbering)
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

## Round-3 delta application (2026-10-03T01-01 deltas)

- Timestamp: 2026-10-03 (session context date; this planning session has no shell clock, so no minute stamp is composed)
- Plan: plan.2026-10-02T05-42.md (revised in place; version 1.3; 1,782 lines after this pass; LF line endings preserved, Grep `\r` count 0)
- Scope of this pass: defects 1 to 4 of `preflight-round3-report.2026-10-03T01-01.md` applied verbatim (old to new), plus the optional delta of advisory A1 (elected by the orchestrator because the revised D-10 rule turns a pwsh refusal into a run stop); followed by the adversarial re-derivation of every line the deltas touched and of its sibling occurrences across the plan.
- Tooling in this session: Read, Grep, Glob, Edit, Write. The Bash tool is disabled in this session, so no `pwsh` token count and no `git hash-object` were run; every count below is a Grep or Read observation against the item worktree, and the new plan blob SHA is left for the orchestrator to compute.
- Sibling decision on P6-T2 line 1554 (pre-edit numbering): the task-description line also reads `every P2-T6, P3-T9, P1-T3, P4-T9, P5-T5 and P5-T12`, but it enumerates the commands to re-run, bounded by its parenthesis, not the values that must hold; re-running every listed command is satisfiable, so the line is unchanged. Only the acceptance line (1555) carried the unsatisfiable requirement.
- Edits applied (pre-edit plan line numbers): 7 to 9 (header: Last Updated, Status, Version 1.3); after 22 (Round 3 revision-record bullet); 131 (fact 15: 25 lines in 5 files, 17 in QfcDatamodel.cs with line numbers); 138 (fact 22: 334 lines; `acquired and released inside a held` 4 at 10, 105, 266, 282); 1050 (payload channel: payloads are never merged into one call); 1395 (P0-T2: fact 22 values 1, 1, 4, 1, 2); 1500 (P4-T1: `PRIMARY_LINES: 25`; `METHOD-GROUP-ONE-ARG-OVERLOAD` for lines 40 and 52); 1555 (P6-T2: values as last recorded per file, numstat `1	129` kept for QfcDatamodel.cs, project-file numstat `3	0`, porcelain span supersedes); 1684 (pointer to this section); 1688 (25, 3 and 2; round-3 enumeration appended); after 1739 (CITATION for the round-3 report).
- Sweep result (every other occurrence of a corrected value): `24 lines` and `PRIMARY_LINES: 24` occurred only at 131, 1500 and 1688; `335` elsewhere (130, 1710) is a QfcDatamodel.cs line number; `acquired and released inside a held` elsewhere (143, 1394) is prose and the P0-T2 token list; `plus 2` elsewhere (563, 639, 1516) is line arithmetic or the P4-T9 interim value, correct at its own task; the P1-T3 numstat `1	0` (1449) is correct at P1-T3. No interim P4-T6, P4-T7 or P4-T9 value is restated as final anywhere else: the AC25, AC30 and AC31 check-offs and P8-T8 carry the P5-T5 values.
- Write Set: unchanged by this pass. The set of files the plan creates or modifies during execution is the same fourteen code paths, the two feature documents and the same evidence files.

SELF-REVIEW: RE-DERIVED THIS PASS

Citations re-derived in this pass (file and line, test or identifier):

1. QuickFiler/Controllers/QfcDatamodel.cs — the primary pattern `Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue\b|LoadRemainingEmailsToQueueAsync|Linked List Locking` matches 17 lines: 40, 52, 130, 194, 209, 210, 246, 335, 363, 369, 378, 404, 410, 418, 462, 469, 472 (Grep with line numbers; the delta's list, confirmed line for line).
2. Repository-wide `*.cs` — the same pattern: 25 lines in 5 files (QfcDatamodel.cs 17, QfcHomeController.cs 4, QfcHomeControllerRunAsyncTests.cs 2, QfcDatamodelLivenessTests.cs 1, QfcInitEmailQueueZeroBatchTests.cs 1), equal to the reviewer's `PRIMARY_LINES: 25` and to the fact 15 outside-file list (defect 1).
3. QuickFiler/Controllers/QfcDatamodel.cs 40 and 52 — both read `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` (the method-group conversion to the one-argument overload, fact 14); neither is a declaration, a cref or a nameof, so the new `METHOD-GROUP-ONE-ARG-OVERLOAD` category is required for `INVOCATIONS: 0` to be reachable (defect 2).
4. Sibling region of defect 3 — QuickFiler/Controllers/QfcDatamodel.cs 96 to 112, 190 to 212 and 358 to 372: after the P1 deletions, 98 `_masterQueue = null;` meets 102 `_worker = null;`; 193 meets 195 `}`; 208 `// Start the time-consuming operation.` meets 211 `try`; 362 `{` meets 364 `return false;`; and the retargeted 369 is the sole argument of `logger.Error(` (368 to 370), already on its own line. No deletion produces a double blank line and the retarget cannot be re-wrapped, so the `1	129` numstat row the rewritten P6-T2 line asserts after formatting is not exposed to a CSharpier rewrite of that file.
5. Plan P4-T9, P5-T5, P4-T6, P4-T7 and P1-T3 (pre-edit 1516, 1533, 1510, 1512, 1449) — the interim values the reviewer listed (LIV `using (var worker = new SynchronousBackgroundWorker())` 3, `Task.Yield` 3, `fake.Advance` 3, `FakeTimeProvider` 1, `new ArmingFakeTimeProvider()` 0; DMT `using (var worker = new BackgroundWorker())` 1; PROJ plus 2 and `TestSupport\ArmingFakeTimeProvider.cs` 0; project-file numstat `1	0`) are each superseded by P5-T3, P5-T4, P5-T5 and the two further project items, so the rewritten P6-T2 acceptance names P5-T5 as the last recording task for LIV, DMT, AFTP and PROJ and restates the project-file row as `3	0` (defect 3).
6. Plan P6-T2 task line (pre-edit 1554) — enumerates commands, bounded by its parenthesis (CMD-LINECOUNT on CS13, every CMD-TOKEN-COUNT list, the nine spans, four CMD-HUNKS, numstat with the porcelain span); CMD-EOL is not among them, so no write-mode command is re-run; left unchanged.
7. docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md — 334 lines (Grep `^` count); `acquired and released inside a held` on lines 10, 105, 266 and 282 (Grep with line numbers), so fact 22 and P0-T2 now read 334 and 4 (defect 4); the P0-T2 gate "at least 1" was already satisfiable and is unchanged in form.
8. Plan payload-channel bullet (pre-edit 1050) — the A1 sentence is inserted after "with the substitutions applied." and before "Payloads use double quotes only"; it contains no placeholder character and no apostrophe (caller instruction 2).
9. Plan header 7 to 9, revision record 22, self-review pointer 1684 and summary 1688, CITATION list 1739 — updated to version 1.3, the Round 3 bullet, this section's heading, "25, 3 and 2", the round-3 enumeration and the round-3 report citation; Grep after the edits finds `1	129` at 22 (the round-2 bullet), 1517, 1556 and 1689 and `3	0` at 23 and 1556, each tab-separated; no `PRIMARY_LINES: 24`, `24 lines in 5` or `335 lines` remains.
10. Plan structure after the edits — nine `### Phase` headings (1390, 1443, 1458, 1477, 1498, 1523, 1550, 1573, 1582); task lines P0-T2 1394, P4-T1 1500, P6-T2 1555; 1,782 lines; zero carriage returns.
11. Sibling check-offs P8-T35 (AC25), P8-T37 (AC27), P8-T39 (AC29), P8-T40 (AC30), P8-T41 (AC31) and P8-T8 — read against the corrected values: each carries the P5-T5 final values or the P4-T1 member-set counts (`Primary Count: 4`, `Cross-check Count: 4`, `INVOCATIONS: 0`, test-file hits `DOC-PROSE` or `OTHER-TYPE-SAME-NAME`), none of which the deltas change.

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
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round2-report.2026-10-02T23-56.md | defects 1 to 8
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round3-report.2026-10-03T01-01.md | defects 1 to 4 and advisory A1
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/plan.2026-10-02T05-42.md | lines 7-9, 22, 131, 138, 1050, 1395, 1500, 1554-1555, 1684, 1688, 1739 (pre-edit numbering)
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
