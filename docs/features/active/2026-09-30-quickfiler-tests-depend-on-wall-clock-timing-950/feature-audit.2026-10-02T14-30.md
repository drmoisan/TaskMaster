# Feature Audit: quickfiler-tests-depend-on-wall-clock-timing (Issue #950)

- Timestamp: 2026-10-02T14-30
- Branch: bug/quickfiler-tests-depend-on-wall-clock-timing-950, head 523c93ee990aefa3e59877373e73fe3c4b1e4ac4
- Base: 34c2ed88cbb009f2f231453db87bc64d45a9bd51
- Work mode: full-bug (issue.md line 12). Acceptance-criteria source: spec.md only (AC1 to AC17, spec.md lines 266-283).
- Companion artifacts: policy-audit.2026-10-02T14-30.md, code-review.2026-10-02T14-30.md

## Scope and Baseline

- Changed code files (name-status BASE..HEAD, three agreeing sources: caller prompt, evidence/qa-gates/footprint-scope.md, evidence/qa-gates/final-commit.md):
  - M QuickFiler/Controllers/QfcDatamodel.cs (+14 / -2)
  - M QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
  - M QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs
  - M QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs
  - M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
  - A docs/features/potential/promoted/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing.md (inherited promotion record)
  - A docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/** (issue, spec, plan, research, 57 evidence files)
- Baseline state at the anchor: code tree unchanged relative to BASE (scoped --exit-code diff exit 0); baseline full-suite run 7361/7361 passed, first-party lines 85.35%, branches 79.74% (evidence/baseline/coverage-baseline.md).
- Post-change state: 7361/7361 passed, lines 85.36%, branches 79.75%; no new failure; no test method added or removed (evidence/qa-gates/coverage-post-change.md, coverage-comparison.md).
- Review method: no-Bash (caller directive); all five code files read in full from the worktree; evidence files read; Grep censuses re-run with the Grep tool; the worktree reflog used as the clock and head reference.
- Defects addressed: Defect A (datamodel tests blocked on real-time bounded waits because the worker started on an uncontrollable thread-pool thread); Defect B (R4 raced the gate-free `EnsureDispatcher` writer in another class under Workers=0 / ClassLevel).

## Acceptance Criteria Inventory

| AC | Criterion (abbreviated) | Spec state at review start |
|---|---|---|
| AC1 | `WorkerStarter` internal Action over BackgroundWorker; both constructors assign the RunWorkerAsync default; XML doc states purpose and null-on-uninitialized | [x] |
| AC2 | `InitEmailQueue` has no direct RunWorkerAsync; both start sites call `WorkerStarter` | [x] |
| AC3 | QfcDatamodel.cs at or below 500 total lines | [x] |
| AC4 | No SpinWait.SpinUntil, Task.Wait or WaitForState in the three datamodel test files; surviving TimeSpan values are production arguments or fake advances | [x] |
| AC5 | No Thread.Sleep, Task.Delay, [DoNotParallelize], retry, or [Timeout] value change; runsettings unchanged | [x] |
| AC6 | DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle passes via synchronous WorkerStarter | [x] |
| AC7 | RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces passes, flag read synchronously | [x] |
| AC8 | RemainingLoadActive_AfterLoaderCompletes_BecomesFalse passes via release + drain | [x] |
| AC9 | RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally passes via release + drain | [x] |
| AC10 | Worker_DoWork_CapturesRemainingLoadTask passes, signals read synchronously | [x] |
| AC11 | InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker passes, signal read synchronously | [x] |
| AC12 | The other two zero-batch tests assign a synchronous WorkerStarter and pass | [x] |
| AC13 | R4 opens a `using` over EnsureUiThreadDispatcher after transaction A acquires the gate and before the `original` read; keeps both assertions; passes | [x] |
| AC14 | R4 doc comment: no flake-watch instruction; names the gate-free EnsureDispatcher writer; cites #950; states the W2/W5 invariant | [x] |
| AC15 | Negative control recorded for each test in AC6-AC13, failing immediately, Markdown summary under evidence/other | [x] |
| AC16 | Ambient SynchronizationContext.Current recorded once under evidence/other | [x] |
| AC17 | Full C# toolchain single pass: csharpier check, analyzer rebuild, TreatWarningsAsErrors rebuild, Invoke-MSTestWithCoverage with runsettings unchanged | [ ] (dated coordinator note present) |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence (code and artifacts) |
|---|---|---|
| AC1 | PASS | QfcDatamodel.cs line 152 `internal Action<BackgroundWorker> WorkerStarter { get; set; }`; constructors lines 41 and 53 `WorkerStarter = worker => worker.RunWorkerAsync();`; doc comment lines 144-151 names the seam purpose ("Injectable worker-start seam"), the constructor default, the synchronous test use and "stays null on instances built by GetUninitializedObject, so InitEmailQueue fails fast with a NullReferenceException". Census agrees (post-format-census.md: property 1, default 2, doc tokens 1 and 1) |
| AC2 | PASS | Grep over QuickFiler/ for `RunWorkerAsync`: only the two constructor defaults (41, 53), the doc comment (147) and a QueueProcessing doc comment (17); `InitEmailQueue` (lines 271-315) calls `WorkerStarter(worker)` at 285 (zero-batch path) and 312 (positive-batch path) |
| AC3 | PASS | Read of the file: 495 total lines (last line 495 is the closing brace); census LINES 495 |
| AC4 | PASS | Grep over the three datamodel test files for `SpinWait`, `.Wait(`, `WaitForState`: 0 hits. Surviving TimeSpan values: Liveness 139, 141, 156 (`fake.Advance`), Teardown 124 and 155 (`QuiesceLoaderAsync(TimeSpan.FromSeconds(5))`, production argument on a FakeTimeProvider-driven path) and 160 (`fake.Advance`). Census TIMESPAN-UNCLASSIFIED 0; positive control recorded (pre-change census had SpinWait 1/1, .Wait( 2/1/1, WaitForState 5/2) |
| AC5 | PASS | Grep over the four changed test files for `Thread.Sleep`, `Task.Delay`, `DoNotParallelize`, `Retry`: 0. Eight `[Timeout(GateTimeoutMs)]` and `GateTimeoutMs = 60000` unchanged (Read lines 33, 43, 106, 156, 211, 284, 331, 376, 418). Runsettings: not in the name-status; anchored diff exit 0 (prohibited-constructs.md). ADDED-TOKEN counts over 251 added lines all 0 with WorkerStarter 16 as positive control |
| AC6 | PASS | Liveness lines 109-164: `model.WorkerStarter = StartSynchronously` (128), `InitEmailQueue(0, worker)` (132), immediate `loaderEntered.Task.IsCompleted.Should().BeTrue()` (134-136), no wait. Passed in targets-pass-after.md, concurrent-classes-pass-after.md and the final run. Control A1 (no-op starter) failed at the entered assertion in 0.186 s |
| AC7 | PASS | Liveness lines 217-234: `StartHeldOpenLoader` (183-209) asserts entered synchronously; the test reads `ReadLivenessFlag(model)` with no wait (227-231). Passed in all three runs. Control B1 failed at the StartHeldOpenLoader entered assertion in 0.259 s |
| AC8 | PASS | Liveness lines 240-270: pump installed before `StartHeldOpenLoader` (244-246), `release.SetResult(true)` then `pump.Drain()` (256-257), `BeFalse` (260-264), context restored in `finally` (266-269). Passed in all three runs. Controls A2 (release withheld) and C1 (Drain removed) both failed at the `BeFalse` in under 0.17 s |
| AC9 | PASS | Liveness lines 276-310: throwing loader body (286-290), release then Drain (296-297), `BeFalse` (300-304), `finally` restore (306-309). Passed in all three runs. Controls A3 and C2 failed at the `BeFalse` in under 0.003 s |
| AC10 | PASS | Teardown lines 207-242: synchronous starter (222), `InitEmailQueue(0, worker)` (227), `loaderEntered.Task.IsCompleted.Should().BeTrue()` (230-232) and `GetPrivateField(model, "_remainingLoadTask").Should().NotBeNull()` (233-238) read synchronously; `using` over the worker. Passed in all three runs. Control A4 failed in 0.186 s |
| AC11 | PASS | Zero-batch lines 165-183: starter assigned (172), `InitEmailQueue(0, worker)` (176), `loaderInvokedTcs.Task.IsCompleted.Should().BeTrue()` (180-182) with no wait; the inert loader completes its TCS synchronously (105-111). Passed in all three runs. Control A5 failed in 0.008 s |
| AC12 | PASS | Zero-batch lines 143 and 202 assign `StartSynchronously`; both tests pass `new SynchronousBackgroundWorker()` to `InitEmailQueue` (148, 221). Passed in all three runs. Controls A6 and A7 (assignment removed) failed with NullReferenceException at the start site |
| AC13 | PASS | R4 lines 218-223: `BeginTransactionAsync` returns `transactionA`, then `using (IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher())`, then `Dispatcher original = UiThreadDispatcherFixture.Current` (225) and `transactionA.Install(liveA)` (226). Assertions `BeSameAs(original)` (255-261) and `NotBeSameAs(liveA)` (262-268) both present. Passed alone (0.073 s), concurrently with QfcItemController_FocusAndThemeTests (0.0036 s) and in the final run. Fail-before: R-INJECT on the pre-fix shape reproduced the CI message ("Expected observedByB to refer to <null> ... ParkedDispatcher"); the pinned shape with the same injected writer passed (A8). `EnsureDispatcher` takes only FieldLock (fixture lines 128-135), so the pin inside the gate cannot deadlock |
| AC14 | PASS | R4 doc comment lines 192-209: "Issue #950" (197), "gate-free fixture method EnsureDispatcher seeds the parked dispatcher whenever the field is null" (198-199), the two race windows (199-201), the pin rationale (201-204), and "Invariant for future editors: no other class may dispose an ensure scope holding the parked dispatcher (W2), and UiThread.Initialize (W5) must not latch during this test" (205-207). No flake-watch or "Append an observation" text remains (census 0 and 0) |
| AC15 | PASS | evidence/other/negative-controls-summary.md: nine AC15 rows, one per test named in AC6-AC13, each using the mechanism the spec Test Strategy table assigns (no-op starter; release withheld then Drain; assignment removed; R4 injected writer on pre-fix shape then on pinned shape), each failing with an assertion or exception in under 0.3 s, none hanging; two supplementary Drain-dependency rows. Source batches under evidence/regression-testing/ |
| AC16 | PASS | evidence/other/ambient-synchronization-context.md: AMBIENT-SYNCHRONIZATION-CONTEXT: null, THREAD-POOL: True, APARTMENT: MTA, observed once by a temporary always-failing probe under the repository runsettings and reverted (never committed). The design does not depend on the value because the two context-observing tests install their own |
| AC17 | PENDING (not checked; evaluated under the coordinator ruling) | Steps 1-3 passed in a single iteration with exit 0 (csharpier-check-final.md, msbuild-analyzer-final.md, msbuild-nullable-final.md; toolchain-final-pass.md SINGLE-PASS: YES). Step 4 could not run Invoke-MSTestWithCoverage.ps1 verbatim: the stall probe (evidence/baseline/stall-probe.md) shows ShellUtilitiesStatic_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension failing deterministically on this workstation ("Win32 handle that was passed to Icon is not valid or is the wrong type"), a known environmental failure that reproduces on main. The DIRECT fallback (collector over vstest with the four shell-icon classes excluded, repository runsettings unchanged) passed 7361/7361 with coverage not below baseline (lines 85.35 to 85.36, branches 79.74 to 79.75). The pull request does not exist yet, so no CI run on the final head exists; AC17 stays unchecked per the ruling below |

Verification notes on AC6-AC13 beyond the recorded runs: the reviewer traced each synchronous mechanism in source. `SynchronousBackgroundWorker.RaiseDoWork()` calls the protected `OnDoWork(new DoWorkEventArgs(null))`, which raises `DoWork` on the calling thread, so the privately subscribed `Worker_DoWork` runs to its first incomplete await (`await loaderTask`, QfcDatamodel.cs line 219) before `InitEmailQueue` returns. With the pump installed, that await captures the pump as its SynchronizationContext; `release` is created with `RunContinuationsAsynchronously`, so `SetResult` posts the continuation rather than running it inline, and `Drain()` runs it on the test thread, clearing the flag in the `finally` (line 227). The negative controls C1/C2 (Drain removed) confirm empirically that the continuation is observed only through `Drain()`.

## Coordinator Ruling (AC17), recorded verbatim

"COORDINATOR RULING, AC17 OPTION (a) APPROVED. The only local blocker is ShellUtilitiesStatic_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension, which fails deterministically on this workstation (Win32 handle not valid); a known environmental failure that reproduces on main; CI runs the shell-icon classes. The local fallback run passed 7361/7361 with coverage not below baseline. AC17 is checked off ONLY from this pull request's own CI run on the FINAL head."

Reviewer disposition: the ruling is applied. AC17 is left unchecked and evaluated as pending CI evidence. The dated note under AC17 in spec.md (2026-10-02, option (a) approved) is present and consistent with the ruling text. Closure requires recording the CI run ID, the head SHA, the mstest-coverage job pass and fail counts and coverage figures, and the local fallback result, then changing `- [ ] AC17` to `- [x] AC17`. If CI fails any test, AC17 is not met.

## Acceptance Criteria Check-off

- AC1 to AC16 were already checked `[x]` by the executor (evidence/other/ac-status-summary.md, spec check-off diff: sixteen `- [ ] AC` lines changed to `- [x] AC`, criterion text identical). Each was independently evaluated PASS above against the code and the evidence; no discrepancy found.
- Newly checked off by this review: none (no PASS item was unchecked).
- Left unchecked: AC17 (PENDING under the coordinator ruling; do not check off before this pull request's own CI run on the final head).
- spec.md was not modified by this review.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md
- Total AC items: 17
- Checked off (delivered): 16
- Remaining (unchecked): 1
- Items remaining:
  - AC17: The full C# toolchain passes in a single pass in order: csharpier check, the analyzers rebuild, the TreatWarningsAsErrors rebuild, and Invoke-MSTestWithCoverage with the repository runsettings unchanged. (Pending this pull request's own CI run on the final head, per the coordinator ruling, option (a).)

## Findings Summary

- Blocking: 0.
- Non-blocking: 6 (CR-1 helper duplication across three test files, spec-sanctioned; CR-2 stale `RunWorkerAsync` wording in the QueueProcessing doc comment; CR-3 QfcDatamodel.cs at 495/500 lines with dead legacy members; CR-4 undisposed `SynchronousBackgroundWorker` instances in two files; CR-5 R4 `transactionA` without `try`/`finally`, pre-existing shape; CR-6 preflight-clearance-r2 label leads its commit clock by about 7.4 h). Details in code-review.2026-10-02T14-30.md.
- THEME TEST NULL-DISPATCHER EXPOSURE: not observed; structural count of R4 null writes unchanged by this branch; residual hazard belongs to the theme tests' discarded-scope pattern (spec out of scope). See code-review Observation O-1.

## Follow-ups (non-blocking residuals)

- F-1: consolidate `SynchronousBackgroundWorker`, `StartSynchronously` and `DrainableSynchronizationContext` into QuickFiler.Test/TestSupport/ with a `<Compile Include>` when the project file is next edited (CR-1).
- F-2: reword the `_remainingLoadActive` doc comment in QfcDatamodel.QueueProcessing.cs line 17 to name `WorkerStarter` (CR-2).
- F-3: remove or relocate the dead legacy loader variants and `Worker_RunWorkerCompleted` in QfcDatamodel.cs before the next change to that file; it has five lines of headroom (CR-3).
- F-4: add `try`/`finally` around `transactionA` in R4 to match R1/R5/R6 (CR-5).
- F-5: hold the ensure scope (or take a transaction) in the two FocusAndThemeTests theme tests so a null-restoring writer in another class cannot expose them; the spec places this out of scope for #950 (O-1).
- F-6: canonical C# coverage artifact path artifacts/csharp/coverage.xml absent in the worktree; committed projections used as the source (recurring).
- F-7: derive evidence labels from the clock for orchestrator-authored artifacts (CR-6).
- F-8: quality-tiers.yml absent at the repository root (pre-existing; already promoted by the #956 review).
- F-9: close AC17 from this pull request's CI run on the final head as the ruling prescribes.

## Summary

Verdict: PASS. Both defects are resolved by the smallest change the spec allows: a documented, constructor-defaulted worker-start seam in `QfcDatamodel` and a baseline pin taken inside the gate in R4. Sixteen of seventeen acceptance criteria are verified PASS against the source and the committed evidence; AC17 is pending this pull request's own CI run under the approved coordinator ruling and remains unchecked. No Blocking finding. Six Non-blocking findings and nine follow-ups are recorded for the orchestrator; none requires a remediation cycle before the pull request is opened.
