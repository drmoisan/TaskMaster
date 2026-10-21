# Feature Audit: focus-and-theme-tests-leak-shared-dispatcher-setup (Issue #968, folding Issue #972)

- Timestamp: 2026-10-03T04-00
- Branch: bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968, head 5570b337cfd11e57579228b36bc919f9673b6195
- Base: origin/main 993fdd01566dee82e5f37acb761a600feaaa1454
- Work mode: full-bug (issue.md line 12). Acceptance-criteria source: spec.md only (AC1 to AC32, spec.md lines 275-307; amendment 1.2 added AC25 to AC32 and amended AC20).
- Companion artifacts: policy-audit.2026-10-03T04-00.md, code-review.2026-10-03T04-00.md, remediation-inputs.2026-10-03T04-00.md

## Scope and Baseline

- Changed code files (name-status origin/main..HEAD, three agreeing sources: caller prompt, evidence/qa-gates/footprint-scope.md, evidence/qa-gates/final-commit.md):
  - M QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
  - M QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs
  - M QuickFiler.Test/Controllers/QfcDatamodelTests.cs
  - M QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs
  - M QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs
  - M QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
  - M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
  - M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
  - A QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs
  - M QuickFiler.Test/QuickFiler.Test.csproj
  - A QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs
  - A QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs
  - M QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs
  - M QuickFiler/Controllers/QfcDatamodel.cs
  - A docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md and A docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md (inherited promotion records, committed before the plan's first task)
  - A docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/** (issue, spec, plan, two research records, 74 evidence files)
- Baseline state at the executor's anchor (94287369908cc920b21b0e3256314f988ad7d2f5): code tree unchanged relative to the anchor (scoped --exit-code diff exit 0, evidence/baseline/scope-and-anchor.md); baseline full-suite run 7361/7361 passed, first-party lines 85.35%, branches 79.73% (evidence/baseline/coverage-summary.md).
- Post-change state: 7365/7365 passed (four pin-count tests added, none removed), lines 85.36%, branches 79.75%; no new failure (evidence/qa-gates/coverage-summary.md, coverage-comparison.md).
- Review method: no-Bash (caller directive); all fourteen code paths read in full from the worktree; the ten pre-existing modified files compared against the session checkout's unchanged copies; evidence files read; Grep censuses re-run; the worktree reflog used as the clock and head reference.
- Defects addressed: the fixture's ensure pin was not reference counted (the installing pin's release nulled the shared static while other pins were live); the two theme tests wrote the shared static without reading it; R4 held a pin that outlived its gate hold; the five #972 residuals of the #950 review; the two dequeue-liveness tests depended on thread-pool scheduling.

## Acceptance Criteria Inventory

| AC | Criterion (abbreviated) | Spec state at review start |
|---|---|---|
| AC1 | Counted pin, non-last release keeps the parked dispatcher; proved by the fail-before regression test passing after the fix | [x] |
| AC2 | Last release reverts only the fixture's own seeding; final assertion of the regression test and the release-order specification test | [x] |
| AC3 | A foreign transaction value is never nulled by pin release; specification test with ShutdownDispatcher in a finally, plus R1 | [x] |
| AC4 | Ownership flag cleared on the last release; fresh single pin installs and restores | [x] |
| AC5 | Fail-before and pass-after evidence isolates the fixture change; message names the first-release assertion and found <null>; summaries only | [x] |
| AC6 | Pin-count class labels: regression test "fails before", three specification tests "pass before and after", class doc states why the regression lives at the fixture level | [x] |
| AC7 | Dead theme-test calls removed: grep for EnsureUiThreadDispatcher in FocusAndThemeTests returns zero; both theme tests pass | [x] |
| AC8 | Gated-caller census: two strategies agree; every invocation other than the forwarder acquired and released inside a held transaction with no Install between | [x] |
| AC9 | Counter and flag are private statics; every access inside lock (FieldLock); last-release null write inline, not through CompareExchange | [x] |
| AC10 | All existing fixture tests pass; the file diff touches only R4's doc, baseline-pin removal and disposal structure | [x] |
| AC11 | Fixture docs describe counting, ownership, discard consequence and residual; leaks exactly and installed nothing carries return zero | [x] |
| AC12 | Wrapper doc describes the counted pin and names the fixture tests as remaining callers; three stale phrases return zero | [x] |
| AC13 | R4 doc states the counting guarantee, keeps the UiThread.Initialize residual; no other class may dispose returns zero | [x] |
| AC14 | R4 disposes transactionA in a finally in addition to the explicit dispose | [x] |
| AC15 | Private BuildExecutingViewer removed; callers use the shared helper; shared-helper doc corrected; every FocusAndThemeTests test passes | [x] |
| AC16 | Theme-test arrange comments name the injected IUiDispatcher mock | [x] |
| AC17 | EnsureSynchronizationContext unchanged | [x] |
| AC18 | Every touched or added C# file at or under 500 physical lines; counts recorded | [x] |
| AC19 | No prohibited constructs added; runsettings unchanged; recorded grep | [x] |
| AC20 | Diff lists only QuickFiler.Test/, the feature folder and exactly the two production paths (after excluding the inherited committed set) | [x] |
| AC21 | Compile item for the pin-count file; all four tests appear as passed in the coverage route's summary | [x] |
| AC22 | Full toolchain pass in order, single pass, no skipped compile target, recorded | [ ] (dated orchestrator ruling note present) |
| AC23 | First-party line and branch coverage not lower than baseline; summaries and projections recorded | [x] |
| AC24 | Three #968 classes together under the CLI runsettings all pass | [x] |
| AC25 | Exactly one class SynchronousBackgroundWorker, internal sealed, with StartSynchronously; no nested copies; doc text corrected; Compile item; three classes pass | [x] |
| AC26 | _remainingLoadActive doc names WorkerStarter, states why IsBusy cannot serve and why volatile; two stale phrases return zero; TryUnhookOrReplace citation carries no line range | [x] |
| AC27 | Zero-caller proof recorded before removal; four members, commented references and empty region gone; nameof retargeted; IQfcDatamodel still implemented; both rebuilds pass | [x] |
| AC28 | QfcDatamodel.cs at or under 400 lines with the before figure recorded | [x] |
| AC29 | ExcludeFromCodeCoverage unchanged; no removed member referenced by a test; AC23 satisfied | [x] |
| AC30 | Every test-created worker in the three named files constructed in a using header owned by the test method; StartHeldOpenLoader receives its worker; tests pass | [x] |
| AC31 | Both dequeue-liveness tests: no Task.Yield, no loop around an advance, WhenAny over ArmingFakeTimeProvider.Armed and the pending dequeue, await the dequeue task; exception dossier; labelled sensitivity check; both pass | [x] |
| AC32 | Four datamodel classes together under the CLI runsettings all pass | [x] |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence (code and artifacts) |
|---|---|---|
| AC1 | PASS | QfcItemController.UiThreadDispatcherPinCountTests.cs lines 36-75: transaction, Install(null), pinA and pinB, afterBothPins read, pinA disposed, afterFirstRelease asserted BeSameAs(afterBothPins) with the spec's because text. Fail-before on the fixture at base content (hash equal to BASE-HASH) failed exactly there with "but found <null>" (evidence/regression-testing/fail-before-pin-count.md); pass-after Passed (pass-after-pin-count.md); Passed again in the concurrent set and the final run |
| AC2 | PASS | Final assertion afterLastRelease BeNull (lines 67-69) passed; EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome (lines 84-123) disposes pinB first and asserts identically; Passed before the fix (specification-tests-before-fix.md) and after. Fixture lines 296-304: null written only when count is zero, flag set and the field still holds the parked instance |
| AC3 | PASS | EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher (lines 134-172): StartRunningDispatcher, Install(live), two pins released, afterAllReleased BeSameAs(live); ShutdownDispatcher(live) in the outer finally (line 170). R1 unchanged and Passed (lines 44-98 identical to the pre-change copy) |
| AC4 | PASS | EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores (lines 184-246): full cycle, fresh pin NotBeNull then BeNull after release; second transaction installs the captured parked instance, one pin released, BeSameAs(parked). Passed before and after. Fixture line 303 clears the flag on the last release |
| AC5 | PASS | fail-before-pin-count.md: fixture hash equals the P0-T12 BASE-HASH, MESSAGE contains "to refer to", "ParkedDispatcher", "a holder that did not take the last pin must not lose the dispatcher" and "but found <null>"; pass-after-pin-count.md: Passed, with the porcelain differing from the fail-before state by exactly the fixture file. Both are trx-derived summaries; Glob of the feature folder finds no raw document |
| AC6 | PASS | Class doc lines 9-23 (theme path dispatches through the injected IUiDispatcher mock and never reads the static; defect observable on one thread); test 1 doc "Regression test: fails before the fix" (line 30); tests 2, 3, 4 docs "Specification test: passes before and after the fix" (lines 78, 126, 175) |
| AC7 | PASS | Reviewer Grep over QuickFiler.Test/Controllers for EnsureUiThreadDispatcher: no hit in QfcItemController.FocusAndThemeTests.cs (the pre-change copy had calls at lines 452 and 468); SetThemeDark_FromNormal_SelectsDarkNormalTheme and SetThemeLight_FromNormal_SelectsLightNormalTheme Passed in the concurrent set and the final run |
| AC8 | PASS | evidence/qa-gates/call-site-census.md: PRIMARY 16 lines, CROSS 32 lines, MEMBER-SET-COMPARISON AGREE, no CROSS-only invocation; per-method nesting readings for R1, R2, R3, T1 to T4 all NESTED: YES with INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE; R4 carries no pin; 13 of 13 invocations nested. Reviewer Grep reproduces the sixteen primary lines (TestSupport 240-241, fixture 143, fixture tests 60, 119, 166, pin-count 45, 46, 93, 94, 146, 147, 194, 195, 201, 230) and the reviewer confirmed each invocation's acquisition and disposal lines lie between its transaction's BeginTransactionAsync and first Dispose, with the only Install calls preceding the acquisitions |
| AC9 | PASS | Fixture lines 55-56 private static int _pinCount; private static bool _fixtureInstalledParked; accesses at lines 151, 155 (inside lock (FieldLock) 149-157) and 295-303 (inside lock (FieldLock) 293-305); the null write is DispatcherField.SetValue(null, null) at line 302 in the same block as the decrement; CompareExchange absent from EnsureScope (SCOPE census 0) |
| AC10 | PASS | Executor hunk census (evidence/qa-gates/test-edit-census.md): three hunks at old ranges 194-210, 218-226, 268-273, all inside R4; reviewer comparison against the pre-change copy: R1 to R3, R5, R6, #743 and #882 bodies identical; R4 assertions (lines 253-266) and because texts identical to the pre-change lines 255-268. Eight fixture tests Passed in the concurrent set and the final run |
| AC11 | PASS | Fixture class doc lines 31-43 (counting under FieldLock, ownership, "A discarded scope therefore pins for the process lifetime", residual); EnsureDispatcher doc lines 134-142; EnsureScope doc lines 265-272. Reviewer Grep for leaks exactly and installed nothing carries over the Controllers folder: no hit |
| AC12 | PASS | TestSupport.cs lines 216-239: counted pin described, "The remaining legitimate callers are the fixture tests QfcItemController_UiThreadDispatcherFixtureTests and QfcItemController_UiThreadDispatcherPinCountTests", dispose-inside-the-transaction rule. Reviewer Grep for Becomes moot, leaks exactly, still delegate to a callee: no hit |
| AC13 | PASS | Fixture tests lines 196-208: "Issue #968 removed that pin: the fixture now counts pins, so only the last release can revert the fixture's own seeding ... a pin must stay nested inside its caller's transaction, and UiThread.Initialize (W5) must not latch during this test". Reviewer Grep for no other class may dispose: no hit |
| AC14 | PASS | Lines 218-272: transactionA acquired, try at 221, explicit transactionA.Dispose() at 249 (the act), finally at 269-272 re-disposing; R5 (lines 287-326) proves the second Dispose is inert. R4 Passed alone-class, concurrent and final. CLOSES-972-ITEM-5: YES recorded in evidence/other/ac-status-summary.md |
| AC15 | PASS | Reviewer Grep for private static Mock<IItemViewer> BuildExecutingViewer: no hit; seven call sites use QfcItemControllerTestSupport.BuildExecutingViewer (lines 176, 196, 218, 237, 297, 314, 350) and the header comment at 162-169 names the switch; TestSupport.cs lines 284-290 now read "Since issue #968 this is the single implementation"; Grep for not reachable from another test file: no hit; 17/17 FocusAndThemeTests Passed in the concurrent set |
| AC16 | PASS | Lines 433-437: "queues the theme application through the theme's injected IUiDispatcher mock (see BuildColorTheme), which absorbs the delegate without running it, so ... the shared UiThread static is irrelevant to this path (issue #968 deleted the former ensure call)"; lines 452-453 reference it for SetThemeLight |
| AC17 | PASS | TestSupport.cs lines 85-96 identical to the pre-change copy; executor hunk census: both hunks start at old line 214 or later |
| AC18 | PASS | Reviewer Read line counts (last line numbers): 375, 482, 442, 472, 248, 352, 229, 226, 394, 367, 413, 27, 49; all equal evidence/qa-gates/file-line-counts.md and all at or under 500 |
| AC19 | PASS | evidence/qa-gates/prohibited-constructs-grep.md over 696 added lines: Thread.Sleep 0, Task.Delay 0, DoNotParallelize 0, Retry( 0, Path.GetTempFileName 0, Path.GetTempPath 0, Workers 0, Timeout( 4 all [Timeout(GateTimeoutMs)] with GateTimeoutMs = 60000 equal to the sibling; runsettings diff --exit-code 0 and porcelain empty; reviewer reads of the thirteen test files agree |
| AC20 | PASS | footprint-scope.md and final-commit.md: name-status paths outside the feature folder are the fourteen Write Set code paths plus the two inherited promoted records; the only paths under QuickFiler/ are QfcDatamodel.cs and QfcDatamodel.QueueProcessing.cs; the caller's independently verified name-status lists the same sixteen paths |
| AC21 | PASS | QuickFiler.Test.csproj line 204 Compile Include="Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs" (adjacent to the fixture-test item at 203); evidence/qa-gates/coverage-summary.md RESULT lines list all four pin-count tests as Passed in the final coverage run |
| AC22 | PENDING CI (not checked; evaluated under the orchestrator ruling) | Steps 1 to 4 of the local toolchain passed in one iteration with exit 0 (toolchain-final.md SINGLE-PASS: YES; csharpier check 1640 files; both rebuilds ERRORS 0 WARNINGS 0 SKIP_CORECOMPILE_LINES 0). The coverage step could not run Invoke-MSTestWithCoverage.ps1 verbatim: the stall probe (evidence/baseline/stall-probe.md) shows ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension failing deterministically on this workstation ("Win32 handle that was passed to Icon is not valid or is the wrong type"), a known environmental failure that reproduces on main. The DIRECT route (collector over vstest with the four shell-icon classes excluded, repository runsettings unchanged) passed 7365/7365 with coverage not below baseline. The pull request does not exist yet, so no CI run on the final head exists; AC22 stays unchecked per the ruling below. Blocking finding B-1, class awaiting_ci |
| AC23 | PASS | evidence/qa-gates/coverage-comparison.md: lines 85.35% -> 85.36% (+0.01), branches 79.73% -> 79.75% (+0.02), lines-valid and branches-valid equal; one-line summaries and JaCoCo package projections present in both evidence/baseline and evidence/qa-gates; the local raw document's root element agrees with the post-change figures |
| AC24 | PASS | evidence/regression-testing/concurrent-set-test-summary.md: one vstest invocation under scripts\vscode\TaskMaster.cli.runsettings over the three classes, 29/29 Passed, CONCURRENT-NOT-PASSED: NONE |
| AC25 | PASS | Reviewer Grep for class SynchronousBackgroundWorker over *.cs: exactly QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs line 15, internal sealed, deriving from BackgroundWorker, with internal static void StartSynchronously(BackgroundWorker worker) at line 24; Liveness, Teardown and ZeroBatch declare no nested worker and no private StartSynchronously (reads); "Duplicated per file" absent (fold-edit census 0); csproj line 230; 21/21 datamodel-set tests Passed |
| AC26 | PASS | QueueProcessing.cs lines 15-23: names WorkerStarter, states "BackgroundWorker.IsBusy already reads idle at that handler's first incomplete await" and "Volatile: the writers and the readers share no other fence"; reviewer Grep for RunWorkerAsync over QuickFiler/Controllers: hits only in QfcDatamodel.cs (constructors and the WorkerStarter doc), none in QueueProcessing.cs; written on the worker thread and read: no hit; line 285 TryUnhookOrReplace citation carries no line range. The line 52 comment on _remainingLoadTask was checked and left (D-20; production-accurate) |
| AC27 | PASS | evidence/qa-gates/qfc-datamodel-legacy-callers.md: run before the removal, PRIMARY 25 / LOG 3 / CROSS 2 lines each classified, INVOCATIONS 0, member sets identical (4 and 4); post-change reviewer Grep over QuickFiler/ for Worker_RunWorkerCompleted, LoadRemainingEmailsToQueue word boundary, Linked List Locking and ILog log: no hit in QfcDatamodel.cs (the only Worker_RunWorkerCompleted hits are QfcHomeController's own member); line 335 nameof(LoadRemainingEmailsToQueueAsync); constructors still bind the one-argument loader (lines 40, 52); Cleanup, InitEmailQueue, InitEmailQueueAsync, DequeueNextItemGroup, UndoMove, QuiesceLoaderAsync, Complete, MovedItems present; both rebuilds exit 0 with SKIP_CORECOMPILE_LINES 0 |
| AC28 | PASS | QfcDatamodel.cs last line 367 (reviewer Read); file-line-counts.md records 367 beside QFCDATAMODEL-LINES-BEFORE 495 |
| AC29 | PASS | [ExcludeFromCodeCoverage] at QfcDatamodel.cs line 25 unchanged (count 1 in both copies); every test-file hit of a removed member name is DOC-PROSE or OTHER-TYPE-SAME-NAME (legacy-callers table); QFCDATAMODEL_CLASS_ENTRIES 0 at both stages; AC23 met |
| AC30 | PASS | Liveness: test 1 using at line 119; tests 2 to 4 using at lines 248, 281, 321 passing worker into StartHeldOpenLoader(worker, ...) (signature lines 211-215). ZeroBatch: using at lines 132, 161, 211; no inline construction inside an InitEmailQueue argument. DatamodelTests: using at lines 116 and 282. 21/21 Passed |
| AC31 | PASS | Liveness lines 102-172: ArmingFakeTimeProvider (106), Armed asserted complete then ReArm (136-141), Advance then Task first = await Task.WhenAny(clock.Armed, pending) (144-145), BeSameAs(clock.Armed) and pending.IsCompleted false, flag read false after release, final Advance and (await pending) empty; no Task.Yield, no for loop. DatamodelTests lines 103-152: same shape with the flag written by reflection. ArmingFakeTimeProvider.cs lines 24-48 with csproj line 231. Dossier fail-before-exception.2026-10-03T03-09.md (exactly one such file) records why the old shape cannot be forced to fail; liveness-sensitivity-check.md records both tests Failed on their re-arm assertion with the lambda forced false and the production file proven byte-identical to base afterwards; liveness-pass-after.md 2/2 Passed |
| AC32 | PASS | evidence/regression-testing/datamodel-set-test-summary.md: one vstest invocation under the CLI runsettings over the four classes, 21/21 Passed, DATAMODEL-NOT-PASSED: NONE |

Verification notes beyond the recorded runs: the reviewer traced the fixture under the four state families recorded as code-review observation O-1 (fresh field; foreign value; residual parked-with-flag; pins under a foreign value restored to null) and found the counted release correct in each; traced R4 after the pin removal (observation O-2); and traced the two liveness rewrites step by step (observation O-3), confirming that no step depends on thread-pool scheduling because the continuations the test relies on are registered under a null SynchronizationContext and the dequeue task is awaited directly.

## Orchestrator Ruling (AC22), recorded verbatim from spec.md

"Note (2026-10-03, orchestrator ruling under plan D-6 and the AC22-under-DIRECT clause, following the #950 AC17 precedent): the intent of AC22 is unchanged; only the evidence source for the test stage changes. Locally, the first four steps passed in one uninterrupted pass with no skipped compile target, and the coverage stage ran by the DIRECT route because the P0-T16 probe recorded a deterministic shell-icon failure on this workstation (`Win32 handle that was passed to Icon is not valid`), a known environmental failure that reproduces on main; CI runs the shell-icon classes. The local DIRECT run passed 7365 of 7365 with coverage not below baseline (lines 85.35 to 85.36, branches 79.73 to 79.75). AC22 is checked off only from this pull request's own CI run on the final head, recording the run ID, the head SHA, the C# test-and-coverage job result, and the local DIRECT result. If CI fails any test, AC22 is not met."

Reviewer disposition: the ruling is applied. AC22 is left unchecked and evaluated as PENDING CI; it is the single blocking finding of this review (B-1, remediability class awaiting_ci) and is recorded in remediation-inputs.2026-10-03T04-00.md with the closure steps. Closure requires recording the CI run ID, the final head SHA, the C# test-and-coverage job pass and fail counts and coverage figures, and the local DIRECT result, then changing `- [ ] AC22` to `- [x] AC22` in the item's own worktree and re-running the CI green gate against the new head. If CI fails any test, AC22 is not met.

## Acceptance Criteria Check-off

- AC1 to AC21 and AC23 to AC32 were already checked `[x]` by the executor (evidence/other/ac-status-summary.md; spec check-off diff: thirty-one `- [ ] AC` lines changed to `- [x] AC` with criterion text identical). Each was independently evaluated PASS above against the code and the evidence; no discrepancy found.
- Newly checked off by this review: none (no PASS item was unchecked).
- Left unchecked: AC22 (PENDING CI under the orchestrator ruling; do not check off before this pull request's own CI run on the final head).
- spec.md was not modified by this review.

### Acceptance Criteria Status
- Source: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md
- Total AC items: 32
- Checked off (delivered): 31
- Remaining (unchecked): 1
- Items remaining:
  - AC22: Full toolchain pass: csharpier check, the analyzer rebuild, the warnings-as-errors rebuild and the MSTest coverage route complete in that order with every step passing in one uninterrupted pass after the last edit, the two rebuild logs contain no skipped compile target, and the commands with exit codes are recorded in this feature's qa-gates evidence folder. (Pending this pull request's own CI run on the final head, per the orchestrator ruling.)

## Findings Summary

- Blocking: 1 (B-1, AC22 awaiting this pull request's CI run; class awaiting_ci; no local remediation exists).
- Autonomous: 0.
- Non-blocking: 2 (CR-1 pre-existing commented-out statements left in QfcDatamodel.cs by the spec's confined production edit; CR-2 the sibling liveness test's dependence on a pumping ambient context, accepted by the spec). Details in code-review.2026-10-03T04-00.md.
- Observations: 6 (O-1 fixture soundness trace, O-2 R4 determinism, O-3 liveness determinism, O-4 canonical coverage artifact path absent, O-5 the _remainingLoadTask comment left by D-20, O-6 post-dispose continuation in liveness test 2).
- Unrelated defects to file: none; quality-tiers.yml absence is pre-existing and already promoted.

## Orchestrator action items (not findings against the item)

- Open the pull request with a body carrying exactly the two closing lines `Closes #968` and `Closes #972` and no other issue number beside a closing keyword (spec Dependencies and Rollout).
- After the CI green gate on the final head, close AC22 as the ruling prescribes and re-verify the head SHA.
- The three review artifacts and remediation-inputs.2026-10-03T04-00.md are untracked in the item worktree and belong to its feature folder; commit them from the item worktree.

## Summary

Verdict: AWAITING_CI. Thirty-one of thirty-two acceptance criteria are verified PASS against the source and the committed evidence: the fixture pin is reference counted at its single mutation point, the dead theme-test calls are gone, the regression test fails on the base fixture and passes on the fixed one with nothing else changed, every remaining pin nests inside a held transaction, and the folded #972 items and the liveness rewrites are delivered as specified with zero-caller proofs, caller-owned disposal and explicit signals. AC22 is pending this pull request's own CI run under the recorded ruling and remains unchecked; it is the only blocking finding and is not remediable locally. No autonomous finding; no remediation cycle is required before the pull request is opened.
