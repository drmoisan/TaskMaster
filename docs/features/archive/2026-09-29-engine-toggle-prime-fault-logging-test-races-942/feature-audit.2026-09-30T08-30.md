# Feature Audit — engine-toggle-prime-fault-logging-test-races (Issue #942)

- Review date label: 2026-09-30T08-30 (assigned without a clock; later than every executor evidence label)
- Work mode: `full-bug` (`issue.md` line 12) — acceptance-criteria source is `spec.md` only. `issue.md` carries no acceptance-criteria section and `user-story.md` does not exist; neither is consulted as an AC source.
- Companion artifacts: `policy-audit.2026-09-30T08-30.md`, `code-review.2026-09-30T08-30.md`.

## Scope and Baseline

- Base: `231e1c0b55105aeb626bf5a6e8d0266a567cacad` (origin/main as merged into the branch at `fadcb6417`; plan correction C1 re-anchored every gate from the preparation anchor `ddbab26a` to this commit). The executor's P0-T4 verified the anchor is an ancestor of HEAD and equals `git merge-base origin/main HEAD` (`evidence/baseline/scope-and-anchor.md`).
- Branch footprint against the base (executor P3-T14 `git diff --name-status`, corroborated by the caller's `git diff --stat` and by this review's Glob of the feature folder): `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (M), `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` (M), `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` (A), `TaskMaster.Test/TaskMaster.Test.csproj` (M), the feature folder (issue, spec, research, plan, 37 evidence projections), and the inherited promotion record `docs/features/potential/promoted/2026-09-29-engine-toggle-prime-fault-logging-test-races.md` (A).
- Baseline state of the production file: 415 lines, SHA-256 `F2A961DD50F2E4678B5CF8B7FAA3F0316AA22D2FB8FE904AE5D08057F26ACEF0`; `CompletePrime` at lines 341–355 with `TryRemove` (348) before `_logError` (354). Baseline fixture: 24 tests, all passing. Baseline suite with coverage: 7323/7323; first-party 85.32% lines / 79.73% branches; coordinator file 143/143 lines, 37/38 branches.
- Post-change state (direct read): 420 lines; `CompletePrime` at 344–360 with `_logError` (358) before `TryRemove` (359) and the why-comment at 355–357. Fixture: 25 tests. Suite: 7324/7324; first-party 85.31% / 79.72%; coordinator file unchanged at 143/143 and 37/38, line 359 hits=1 (read directly from `coverage/final-942.cobertura.xml`).
- Review tooling: Read, Grep and Glob only; no git or build tool was available. Where a criterion asserts a property of the diff or of a run, the evidence projection is cited and, where possible, cross-checked against the current file content or the raw Cobertura document.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/spec.md`, section `## Acceptance Criteria`, lines 228–241. Fourteen checkbox items, all `- [x]` at review start (executor check-offs P3-T15 to P3-T28).

| ID | Criterion (abbreviated) | State at review start |
|---|---|---|
| AC1 | `CompletePrime` reports through the delegate before removing the key; early return, unwrap and synthesized `TaskCanceledException` unchanged; no `catch`, `try` or lock added | [x] |
| AC2 | Summary describes report-then-clear; adjacent comment states why; `GetPrimeTask` returns states the guarantee in one sentence | [x] |
| AC3 | `Harness.OnLogError` internal settable `Action<string, Exception>`, documented, invoked null-conditionally right after `Errors.Add`; no existing test method modified (hunks only inside `Harness`) | [x] |
| AC4 | New partial with the named test capturing the handle before the trigger, sink-side `GetPrimeTask`, the six listed assertions | [x] |
| AC5 | MSTest, strict Moq, FluentAssertions with reasons, AAA, XML summary naming the issue and invariant, loss-of-isolation comment | [x] |
| AC6 | csproj `Compile Include` for the partial; pass-after lists the test as executed and passed | [x] |
| AC7 | Fail-before projection with Timestamp, Command, non-zero EXIT_CODE = ExpectedExitCode, merge-base commit, new test failing on the same-instance assertion | [x] |
| AC8 | Pass-after projection, EXIT_CODE 0, both tests passed, only production difference is the reorder | [x] |
| AC9 | Every fixture test passes in the pass-after run; original test byte-for-byte unchanged | [x] |
| AC10 | No sleep/delay/retry/wall-clock/timeout/`[DoNotParallelize]`/blocking wait/temp file/scheduler seam; run-settings unmodified | [x] |
| AC11 | Final toolchain pass: csharpier check clean, both rebuilds exit 0 with no skipped CoreCompile, MSTest-with-coverage exit 0, one uninterrupted pass | [x] |
| AC12 | Coverage baseline and post-change projections with the coordinator comparison; changed lines not decreased, method fully covered; no raw trx/xml/coverage file added | [x] |
| AC13 | Diff touches nothing outside the four code files, the feature folder and the promotion record; Race partial, wiring, run-settings, `StartPrimeIfNeeded`, prime gate unchanged | [x] |
| AC14 | Each of the three source files at or below 500 lines | [x] |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence and verification performed |
|---|---|---|
| AC1 | PASS | Direct read of lines 344–360: `if (completed.Status == TaskStatus.RanToCompletion) return;` (346–349), `var failure = (Exception)completed.Exception?.GetBaseException() ?? new TaskCanceledException(completed);` (351–353), `_logError(...)` at 358, `_primeTasks.TryRemove(engineName, out _);` at 359. Span tokens `SPAN_TRY=0`, `SPAN_CATCH=0`, `SPAN_LOCK=0`; file-level `catch (` = 1 (line 181) and `lock (` = 1 (line 271) unchanged from the anchor (`evidence/qa-gates/production-reorder-scope.md`, POST-FORMAT; both counts confirmed by Grep). |
| AC2 | PASS | Summary lines 330–333 contain "and only then is the in-flight marker cleared"; comment lines 355–357 begin "Report-then-clear is load-bearing" and state the caller guarantee; `GetPrimeTask` returns lines 245–247 carry the one-sentence guarantee "the marker is cleared only after that report has returned, so a caller that receives Task.CompletedTask can rely on the fault having been reported". Direct read. |
| AC3 | PASS | Lines 440–445: documented `internal Action<string, Exception> OnLogError { get; set; }`; lambda lines 415–419: `Errors.Add(new LoggedError(message, exception));` then `OnLogError?.Invoke(message, exception);`. Hunks `@@ -415 +415,5 @@` and `@@ -435,0 +440,7 @@`, both inside `Harness` (403–452); `[TestMethod]` count 15 unchanged (`evidence/qa-gates/harness-hook-edit-scope.md`; count confirmed by direct read). File is 470 = 459 + 12 - 1 lines, consistent with the recorded numstat. |
| AC4 | PASS | Direct read of the 77-line partial: handle captured at 35 before `probe.SetException(failure)` at 41; `harness.OnLogError = (_, _) => handleSeenBySink = harness.Coordinator.GetPrimeTask(SpamEngine);` at 37–38; `await prime;` at 42; assertions `BeSameAs(prime)` (47–54), `ContainSingle` (55), `Message.Contain(SpamEngine)` (56–59), `Exception.BeSameAs(failure)` (60–63), `Invalidations.BeEmpty` (64), `GetPrimeTask(...).BeSameAs(Task.CompletedTask)` (65–72). |
| AC5 | PASS | `[TestMethod]` line 26; `Harness.Engines` is `new Mock<IAppItemEngines>(MockBehavior.Strict)` (primary partial line 424); six FluentAssertions calls each with a reason; `// Arrange` / `// Act` / `// Assert` at 29 / 40 / 44; XML summary (18–25) names issue #942 and states the invariant; comment at 45–46 states that a pass without the reorder means the negative control has lost isolation. |
| AC6 | PASS | `TaskMaster.Test.csproj` line 360: `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs" />` immediately after the Race entry at 359 (Grep). Pass-after RESULT line `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed` at P2-T4 and again at P3-T7; total 25 = baseline 24 + 1. |
| AC7 | PASS | `evidence/regression-testing/prime-fault-ordering-fail-before.md`: `Timestamp: 2026-09-30T07-37`, full vstest `Command:`, `EXIT_CODE: 1`, `ExpectedExitCode: 1`, merge base `231e1c0b…` cited with `PROD-HASH-AT-CONTROL` equal to `BASE-HASH-PROD`, hook/partial/csproj present, run settings diff-clean; `MESSAGE` line is the `BeSameAs` failure ("Expected handleSeenBySink to refer to … ContinuationTaskFromTask … but found System.Threading.Tasks.Task") with the sink-handle reason fragment. 24 passed / 1 failed, the only failure being the new test. |
| AC8 | PASS | `evidence/regression-testing/prime-fault-ordering-pass-after.md`: identical command apart from the results segments, `EXIT_CODE: 0`, 25/25, RESULT lines for both named tests `= Passed`, explicit statement that the only production difference is the reorder and documentation in `CompletePrime`/`GetPrimeTask` with `PROD-HASH-AFTER` differing from the base hash. |
| AC9 | PASS | Pass-after and final fixture runs: 25 executed, 25 passed, no FAILED line; population = baseline 24 + 1 (independently recomputed: 15 + 3 data rows + 6 Race + 1 new = 25). Original test: `BASE_METHOD_SHA` = `NOW_METHOD_SHA`, `METHOD_UNCHANGED=True`, re-verified post-format (`evidence/qa-gates/original-test-unchanged.md`); the method text read at lines 212–243 still fetches the handle after the trigger (223–224), as the spec requires it to. |
| AC10 | PASS | `evidence/qa-gates/determinism-tokens.md`: 20 tokens at 0 over the 89 added test lines; `RUNSETTINGS_DIFF_EXIT=0`. Independent direct read of both test files finds no `Thread.Sleep`, `Task.Delay`, retry loop, `DateTime`, `[Timeout]`, `[DoNotParallelize]`, `.Wait(`/`.Result`, temp-file API or `TaskScheduler`. |
| AC11 | PASS (disclosed substitution) | `evidence/qa-gates/toolchain-final-pass.md`: pass 1, in CLAUDE.md order, no intervening rewrite; check "no differences" (1626 files); analyzer and nullable rebuilds exit 0 with `SKIP_CORECOMPILE_LINES: 0` and `CSC_OUT_*` ≥ 1; coverage-enabled run exit 0, 7324/7324. The MSTest-with-coverage step ran the DIRECT route (the runner's own inner collector invocation with four shell-icon classes excluded, plan D-6, after the stall probe recorded a workstation-local failure) rather than `Invoke-MSTestWithCoverage.ps1` verbatim; the substitution is recorded in the plan, the stall-probe artifact and the toolchain artifact, and the criterion's wording ("the MSTest-with-coverage run exiting zero") is met. Recorded as policy-audit PA-2, non-blocking. |
| AC12 | PASS | `evidence/baseline/coverage-baseline.md` and `evidence/qa-gates/coverage-post-change.md` (COMPARISON) exist with the coordinator comparison: lines 143/143 → 143/143, branches 37/38 → 37/38, `COMPLETEPRIME-UNCOVERED-FINAL: 0`, changed line 359 hits=1. Independently verified by reading the class element of `coverage/final-942.cobertura.xml` (document line 230308 onward): every `CompletePrime` line element hits=1, branch conditions 2/2 and 4/4. No trx/xml/coverage file added: `RAW-DOCS-COMMITTED: 0`, `RAW-DOCS-UNTRACKED-IN-FEATURE: 0` (with `--ignored`), the 29-path added list contains only `.cs` and `.md`; Glob of the feature folder lists Markdown only. |
| AC13 | PASS | `evidence/qa-gates/footprint-scope.md` (P3-T14): `THIS-ITEM-FOOTPRINT` is the four code paths plus feature-folder paths; `INHERITED-AND-EXCLUDED` is exactly the promotion record; `NONGOAL_FILES_DIFF_EXIT=0` for the Race partial and the ribbon wiring, `RUNSETTINGS_DIFF_EXIT=0`. The caller's independent `git diff --stat` reports the same footprint. `StartPrimeIfNeeded` (263–280) and `_primeGate` read unchanged from the plan's fact 1 description; the production numstat `10 5` is confined to the two documented hunk windows. This review could not run git; the criterion is credited on two independent git-based enumerations plus direct reads. |
| AC14 | PASS | Direct read: 420, 470, 77 lines (matches `evidence/qa-gates/file-line-counts.md`). |

Summary: 14 PASS, 0 PARTIAL, 0 FAIL, 0 UNVERIFIED.

## Acceptance Criteria Check-off

All fourteen items were already `- [x]` in `spec.md` at review start and every one evaluates PASS above, so no check-off edit and no uncheck was made. No item was unchecked silently; no phantom criterion was added.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/spec.md
- Total AC items: 14
- Checked off (delivered): 14
- Remaining (unchecked): 0
- Items remaining: none

## Summary

- Verdict: **PASS** — 14/14 acceptance criteria verified against evidence; 0 blocking findings in this artifact.
- The defect's mechanism (statement order inside `CompletePrime`, not a missing await) is fixed by the two-statement reorder; the regression test is deterministic by construction and was observed failing against the base production file before the fix.
- Non-blocking follow-ups (owed outside this branch): hazard B (registration racing removal on a synchronous non-success prime; separately promoted), optional `try`/`finally` hardening should a throwing sink ever be injected, and the two other continuation-discarding production sites named in the spec. Policy-audit observations PA-1 to PA-5 are recorded there.
- Merge readiness: the local coverage run excluded four shell-icon test classes that CI executes; the PR's CI `mstest-coverage` check remains the repo-wide gate for those classes.
