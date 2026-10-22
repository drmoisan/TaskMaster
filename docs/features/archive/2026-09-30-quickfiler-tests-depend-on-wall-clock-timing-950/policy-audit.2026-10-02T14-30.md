# Policy Audit: quickfiler-tests-depend-on-wall-clock-timing (Issue #950)

- Timestamp: 2026-10-02T14-30
- Branch: bug/quickfiler-tests-depend-on-wall-clock-timing-950
- Head: 523c93ee990aefa3e59877373e73fe3c4b1e4ac4 (worktree reflog, last entry; commit "docs(950): record the coordinator ruling on the AC17 evidence source")
- Base: 34c2ed88cbb009f2f231453db87bc64d45a9bd51 (post-merge origin/main anchor; verified as merge-base in evidence/baseline/scope-and-anchor.md)
- Work mode: full-bug (issue.md line 12); acceptance-criteria source: spec.md only
- Reviewer: feature-review, no-Bash mode (caller directive). Every check was performed with Read, Grep and Glob against the item worktree plus the committed evidence under evidence/. Where a check needs a shell it is recorded as such with the reason.
- Timestamp derivation: the label above was chosen to be monotone after every label in the feature folder (latest 2026-10-02T12-10) and after the orchestrator checkpoint's latest recorded time (2026-10-02T14:05Z). No shell clock was available. The worktree reflog places the head commit at epoch 1790919527 (2026-10-02T05:38:47Z, 01:38 at the recorded -0400 offset).

## Executive Summary

Overall verdict: PASS. 0 Blocking findings, 6 Non-blocking findings (CR-1 to CR-6, detailed in code-review.2026-10-02T14-30.md). AC1 to AC16 verified PASS against code and evidence; AC17 remains unchecked under the coordinator ruling (option (a): checked off only from this pull request's own CI run on the final head).

| Area | Verdict | Evidence summary |
|---|---|---|
| General Unit Test Policy | PASS | Nine rewritten or pinned tests are deterministic (synchronous worker start, drained test-owned SynchronizationContext, FakeTimeProvider); no Thread.Sleep, Task.Delay, retry, [DoNotParallelize] or temporary file; 7361/7361 pass at baseline and after the change |
| General Code Change Policy | PASS | Minimal fix: one delegate seam in one production file (14 added, 2 deleted lines); every file at or under 500 lines; toolchain evidence single pass |
| C# Code Change Policy | PASS | csharpier check exit 0 (1637 files); analyzer rebuild 0 errors 0 warnings; TreatWarningsAsErrors rebuild 0 errors 0 warnings; no /p:Nullable=enable; /t:Rebuild used |
| C# Unit Test Policy | PASS | MSTest, Moq, FluentAssertions throughout; first-party lines 85.36%, branches 79.75% (floors 80/75 per CLAUDE.md, 85/75 per rules, both met) |
| Coverage (C#) | PASS | Repo-wide not lower than baseline (85.35 to 85.36 lines, 79.74 to 79.75 branches); changed production file attribute-excluded (pre-existing), execution proven by tests and negative controls |
| Evidence hygiene | PASS | 0 host paths, 0 raw trx or coverage documents in the committed feature folder (reviewer sweep and evidence/qa-gates/evidence-hygiene.md agree) |
| Coordinator prohibitions | PASS | No retries, no [DoNotParallelize], no Workers=1, no lengthened timeout, no wall-clock wait, no temporary file (evidence/qa-gates/prohibited-constructs.md plus direct Grep) |

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. Two caller statements were evaluated and accepted as factual rather than narrowing:

- "This item touches no PowerShell file." Confirmed by the caller-supplied name-status, evidence/qa-gates/footprint-scope.md and evidence/qa-gates/final-commit.md: the branch-vs-base diff outside the feature folder is exactly five .cs files plus the promoted record. Zero PowerShell, TypeScript or Python files changed.
- "No artifacts/csharp/coverage.xml exists in the worktree; use the committed projections and summaries as the coverage source and say so." This is an evidence-source instruction consistent with CLAUDE.md "Committed Test Evidence Format" and with the prior ruling that committed feature-folder coverage projections count as the coverage artifact. It does not narrow the audit scope, which remains the full branch diff against the base.

## Evidence Location Compliance

- Branch diff scan for files under artifacts/baselines/, artifacts/qa/, artifacts/evidence/ or artifacts/coverage/: none. The name-status BASE..HEAD recorded in evidence/qa-gates/footprint-scope.md and evidence/qa-gates/final-commit.md lists the five code files, the promoted record and paths under docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/ only.
- All executor evidence lives under docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/evidence/{baseline,qa-gates,regression-testing,other}/ (57 Markdown files; Glob listing in this review).
- validate_evidence_locations.py --root .: not run (Bash was forbidden for this review). The manual scan above substitutes; no violation observed.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required; the caller supplied no non-canonical evidence path.
- PR context artifacts (artifacts/pr_context.summary.txt, artifacts/pr_context.appendix.txt): absent in the review worktree (Grep over artifacts/ found only orchestration/orchestrator-state.json). Regeneration was not possible without a shell or the collection tool. Scope was derived from the caller-supplied name-status, the committed footprint-scope.md (git diff --name-status BASE..HEAD) and the worktree reflog head. Recorded as UNVERIFIED for the artifact pair only; the scope itself is verified by three agreeing sources.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Each test builds its own uninitialized QfcDatamodel and its own worker; the two context-installing tests restore SynchronizationContext.Current in a finally (QfcDatamodelLivenessTests.cs lines 244-269 and 280-309). R4 pins and later releases its own baseline scope (lines 221-270). Concurrent run with QfcItemController_FocusAndThemeTests: 37/37 passed (evidence/regression-testing/concurrent-classes-pass-after.md) |
| Isolation | PASS | Each test targets one behavior of InitEmailQueue / Worker_DoWork or one fixture property; negative controls fail at exactly one named assertion each (evidence/other/negative-controls-summary.md) |
| Fast execution | PASS | Nine target durations between 0.0014 s and 0.265 s (evidence/regression-testing/targets-pass-after.md); the former five-second bounds are gone |
| Determinism | PASS | Synchronous worker start through WorkerStarter; posted continuations observed only through DrainableSynchronizationContext.Drain(); FakeTimeProvider for all time; R4 baseline pinned inside the gate. Negative controls fail immediately when the signal is withheld (ten rows, all under 0.3 s) |
| Readability | PASS | Descriptive names, XML doc comments on every helper, Arrange/Act/Assert markers, failure reasons on every assertion |

### 1.2 Coverage

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 5 | 7361 | 7361 passed, 0 failed | 85.35% lines / 79.74% branches | 85.36% lines / 79.75% branches | N/A (attribute-excluded production file; see the per-language comparison block) |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

Coverage source statement: no artifacts/csharp/coverage.xml exists in the review worktree. The figures above are read from the committed projections and summaries, which are the forms CLAUDE.md "Committed Test Evidence Format" requires: evidence/baseline/coverage-baseline.md (P0-T16) and evidence/qa-gates/coverage-post-change.md (P6-T5), each carrying the first-party summary line, the root counters (lines-covered/lines-valid, branches-covered/branches-valid) and the package-level JaCoCo projection, plus evidence/qa-gates/coverage-comparison.md (P6-T6). Both runs used the DIRECT route with the identical four-class shell-icon exclusion, so they are comparable.

Verdict lines:

- C# coverage verdict: PASS (repo-wide first-party lines 85.36% and branches 79.75% from the committed post-change projection; above the CLAUDE.md floors of 80% lines and 75% branches, which the caller designates as governing, and also above the 85% / 75% floors in .claude/rules; no regression against the baseline 85.35% / 79.74%).
- C# changed-production-file coverage: PASS on the no-regression limb. The only changed production file, QuickFiler/Controllers/QfcDatamodel.cs, carries a pre-existing type-level ExcludeFromCodeCoverage attribute (line 25, introduced by the #197 exemption commit), so neither Cobertura document has a class node for it (QFCDATAMODEL-CLASS-NODES 0 at both stages) and no measured line of it can regress. Execution of the 14 added lines is proven independently: all nine target tests reach WorkerStarter(worker) through InitEmailQueue, and negative controls A6 and A7 (WorkerStarter assignment removed) fail with NullReferenceException at the start site, which shows the call is executed. The four changed test files are outside the coverage denominator by policy.
- C# package-level corroboration: the QuickFiler package counters are identical at both stages (LINE missed 2293 covered 10461; BRANCH missed 699 covered 2518), consistent with a change whose only production lines are in an attribute-excluded type. The +8 lines / +2 branches delta sits entirely in the UtilitiesCS package, which this branch does not touch; the comparison artifact records it as run-to-run variance.
- PowerShell coverage gate: PASS by vacuity (zero PowerShell files changed on this branch, so the changed-line no-regression requirement has no line to evaluate; no PoshQC format, analyze or test gate was owed or run; artifacts/pester/powershell-coverage.xml was not consulted).
- TypeScript and Python: zero files changed on this branch; no verdict is owed.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/evidence/baseline/coverage-baseline.md` (committed JaCoCo package projection + first-party summary + root counters; canonical artifacts/csharp/coverage.xml absent in the worktree)
- C# post-change coverage artifact: `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/evidence/qa-gates/coverage-post-change.md` (same three forms; canonical artifacts/csharp/coverage.xml absent in the worktree)
- TypeScript baseline coverage artifact: none consulted (zero TypeScript files changed on this branch)
- TypeScript post-change coverage artifact: none consulted (zero TypeScript files changed on this branch)
- PowerShell baseline coverage artifact: none consulted (zero PowerShell files changed on this branch)
- PowerShell post-change coverage artifact: none consulted (zero PowerShell files changed on this branch)
- Python baseline coverage artifact: none consulted (zero Python files changed on this branch)
- Python post-change coverage artifact: none consulted (zero Python files changed on this branch)
- Per-language comparison summary: the per-language comparison block of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.35% lines (56204/65855) / 79.74% branches (13618/17078). Post-change: 85.36% lines (56212/65855) / 79.75% branches (13620/17078). Change: +0.01% lines (+8 covered) / +0.01% branches (+2 covered), confined to the UtilitiesCS package which this branch does not change. Disposition: PASS. Evidence: evidence/baseline/coverage-baseline.md, evidence/qa-gates/coverage-post-change.md, evidence/qa-gates/coverage-comparison.md.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Artifact consulted | State | Disposition |
|---|---|---|---|
| C# | Committed projections and summaries under evidence/baseline and evidence/qa-gates | Present; canonical artifacts/csharp/coverage.xml absent in the worktree (recorded as follow-up F-6, recurring) | PASS |
| TypeScript | none | zero files changed | no verdict owed |
| PowerShell | none | zero files changed | no verdict owed |
| Python | none | zero files changed | no verdict owed |

Coverage exclusion policy check (.claude/rules/general-unit-test.md): the branch adds no coverage-config `exclude` entry and no new ExcludeFromCodeCoverage attribute (the QfcDatamodel.cs attribute at line 25 predates the base). Under the standing ruling (CLAUDE.md UT2 COM/VSTO exemption is the more specific, higher-authority clause; the rules file's Blocking clause enumerates config `exclude` globs), the pre-existing attribute is Not Blocking for this change. Its material consequence (no coverage observation for the 14 changed lines) is recorded above rather than waived.

### 1.3 Scenario completeness

| Scenario | Verdict | Evidence |
|---|---|---|
| Positive flows | PASS | Zero-batch start (AC11), positive-batch projection (AC12), liveness flag true across the async-void boundary (AC7), flag cleared on completion (AC8), capture of _remainingLoadTask (AC10) |
| Negative flows | PASS | Unassigned WorkerStarter fails fast with NullReferenceException (controls A6, A7); loader never released leaves the flag true (A2, A3); no-op starter never reaches the loader (A1, A4, A5, B1) |
| Edge cases | PASS | Drain removed with loader released shows the continuation is observed only through Drain (C1, C2) |
| Error handling | PASS | Throwing loader still clears the flag through finally then catch (AC9, test 4) |
| Concurrency | PASS | R4 two-transaction ordering kept; baseline pin closes both race windows; concurrent run with the gate-free writer class passed 37/37 |
| State transitions | PASS | _remainingLoadActive true -> false observed synchronously after Drain |

### 1.4 Arrange-Act-Assert

PASS. Every rewritten test carries Arrange / Act / Assert comments or an "Arrange / Act" combined marker where the helper performs both (RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces, lines 220-233). Every assertion carries a because-reason string.

### 1.5 External dependencies and temporary files

PASS. Outlook COM is mocked (Mock<Application>, Mock<NameSpace>, Mock<MailItem>); the loader is an injected delegate; no file system, network or process is touched; Grep over the four changed test files for Path.GetTempFileName, Path.GetTempPath, File.Create and StreamWriter: no hit (the test-file Grep for prohibited constructs in section 7 covers the wait tokens).

### 1.6 Test file location

PASS (repository convention). Tests live in QuickFiler.Test/Controllers/, mirroring QuickFiler/Controllers/. The rules file names a tests/ tree, but the per-project *.Test layout is the pre-existing repository convention for every C# project; no test file was created or moved by this branch.

### 1.7 Determinism infrastructure

PASS. FakeTimeProvider drives all time (QfcDatamodelLivenessTests.cs lines 114, 139, 141, 156; QfcDatamodelTeardownTests.cs lines 118, 146, 160). Banned APIs in test code (Thread.Sleep, Task.Delay, wall-clock waits): none in the four changed files (Grep: the only `.Wait(` hit is the pre-existing `secondCallerStarted.Wait()` on a ManualResetEventSlim in the R4 file at line 250, a signal wait without a time bound and not in the three datamodel files AC4 names). No seeded RNG is needed; no randomness is used.

## 2. General Code Change Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| Before making changes (plan, spec) | PASS | spec.md (302 lines) and plan.2026-10-01T07-11.md exist; three preflight rounds plus a confirming R2 preflight recorded under evidence/other/ |
| Bugfix workflow step 1 (failing regression test first) | PASS | Defect B: deterministic fail-before via the R-INJECT edit on the unmodified file, run alone (evidence/regression-testing/r4-fail-before.md, CI failure signature reproduced), then pass-after in two runs. Defect A: a deterministic failing run is impossible without a wall-clock wait or thread-pool starvation; the dossier (evidence/regression-testing/fail-before-exception.2026-10-02T01-00.md) substitutes the pre-change wait census plus ten post-fix negative controls that each fail at once when the signal is withheld. The rewritten tests themselves are the regression tests; no test method was added or removed (7361 before and after) |
| Bugfix workflow step 2 (minimal targeted fix) | PASS | Production diff is 14 added / 2 deleted lines in one file (evidence/qa-gates/post-format-census.md numstat 14 2); the R4 change is a using pin plus a doc comment |
| Bugfix workflow step 3 (verify locally, toolchain in order) | PASS with ruling | Steps 1-3 single pass, exit 0 (evidence/qa-gates/toolchain-final-pass.md SINGLE-PASS: YES). Step 4 ran the DIRECT route (collector over vstest with the four shell-icon classes excluded) because the runner's full suite fails deterministically on this workstation in ShellUtilitiesStatic_Tests (Win32 handle not valid; reproduces on main; evidence/baseline/stall-probe.md). Coordinator ruling option (a) accepted; AC17 is pending this PR's CI run |
| Design principles (simplicity, reusability, extensibility, separation) | PASS | Smallest seam: one Action<BackgroundWorker> delegate with a constructor default, mirroring the existing RemainingEmailLoader convention; production path unchanged (one extra delegate invocation). Reusability: helper duplication across three test files is spec-sanctioned by the documented per-file convention (CR-1, non-blocking) |
| Classes, functions, APIs | PASS | Test helpers are small private nested types; the seam is an internal property with a documented contract |
| Error handling | PASS | No new catch; the unassigned-seam NullReferenceException is the intended fail-fast on test-only uninitialized instances (spec Boundaries), unreachable in production because QfcHomeController only reaches InitEmailQueue on constructed instances |
| Logging | PASS | No new logging; existing log4net path unchanged |
| File size limit (500 lines) | PASS | QfcDatamodel.cs 495; QfcDatamodelLivenessTests.cs 312; QfcDatamodelTeardownTests.cs 244; QfcInitEmailQueueZeroBatchTests.cs 232; QfcItemController.UiThreadDispatcherFixtureTests.cs 470 (reviewer Read line counts agree with evidence/qa-gates/post-format-census.md). QfcDatamodel.cs has five lines of headroom (CR-3) |
| Naming | PASS | PascalCase property WorkerStarter; camelCase locals; descriptive test and helper names |
| Public APIs and compatibility | PASS | IQfcDatamodel unchanged; the new member is internal; InternalsVisibleTo already in use |
| Dependencies | PASS | None added; FakeTimeProvider already referenced |
| I/O boundaries | PASS | No I/O introduced |

## 3. Language-Specific Code Change Policy Compliance

Language in scope: C# only.

| Item | Verdict | Evidence |
|---|---|---|
| Formatting (csharpier via dotnet tool run) | PASS | evidence/qa-gates/csharpier-format.md (REWRITTEN-WRITESET: NONE after the Phase 4 scoped format) and csharpier-check-final.md: Checked 1637 files, CSHARPIER_EXIT_CODE 0 |
| Linting (analyzer rebuild, /t:Rebuild, EnableNETAnalyzers, EnforceCodeStyleInBuild) | PASS | evidence/qa-gates/msbuild-analyzer-final.md: MSBUILD_EXIT_CODE 0, ERRORS 0, WARNINGS 0, SKIP_CORECOMPILE_LINES 0, no Write Set diagnostic |
| Type checking (TreatWarningsAsErrors rebuild, no /p:Nullable=enable) | PASS | evidence/qa-gates/msbuild-nullable-final.md: exit 0, 0 errors, 0 warnings, SKIP_CORECOMPILE_LINES 0; command text matches CLAUDE.md character for character |
| Nullable annotations | PASS | QfcDatamodel.cs carries no #nullable enable directive, so the new property is outside nullable analysis by the per-file opt-in rule; the doc comment states the null-on-uninitialized behavior |
| DI seam preference (interface > delegate > adapter) | PASS | A narrow Action<BackgroundWorker> delegate for a single call path, with a safe deterministic default; an interface for one call would be excessive (.claude/rules/csharp.md DI Seams item 2) |
| XML docs on non-obvious contract | PASS | WorkerStarter doc comment lines 144-151 states purpose, default, null-on-uninitialized behavior and the issue number |
| Internal surface | PASS | internal property; QuickFiler.Test consumes it through the existing InternalsVisibleTo |
| Name resolution | PASS | QfcDatamodel.cs imports both System and Microsoft.Office.Interop.Outlook; the generic Action<BackgroundWorker> is unambiguous because Outlook.Action is non-generic, and both rebuilds compiled with 0 errors |
| Analyzer suppressions added | PASS | None (the pre-existing CS0618 pragma at lines 436/457 is untouched) |

## 4. Language-Specific Unit Test Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | [TestClass] / [TestMethod] in all four files |
| Moq for mocks | PASS | Mock<IApplicationGlobals>, Mock<IAppQuickFilerSettings>, Mock<MailItem>, Mock<NameSpace>, Mock<Application> |
| FluentAssertions | PASS | All assertions use Should(); no MSTest Assert calls introduced |
| Repo-wide coverage floors | PASS | 85.36% lines (floor 80% per CLAUDE.md, 85% per rules), 79.75% branches (floor 75%) |
| New module/class/method >= 90% | PASS on the execution limb | The new property sits in an attribute-excluded type and has no coverage observation; its execution on every test path is proven by the nine tests and controls A6/A7 (see section 1.2) |
| No regression on changed lines | PASS | No measured changed line exists; repo-wide and package-level figures are not lower |
| Prohibited behaviors (sleeps, retries, timing hacks, weakened assertions) | PASS | ADDED-TOKEN Thread.Sleep 0, Task.Delay 0, DoNotParallelize 0, Retry( 0, Timeout( 0 over 251 added lines (evidence/qa-gates/prohibited-constructs.md); R4 keeps both BeSameAs(original) and NotBeSameAs(liveA) (lines 255-268); the #424 claim of liveness test 1 is preserved (dequeue still polls _remainingLoadActive, QfcDatamodel.QueueProcessing.cs lines 305 and 406) |
| Test toolchain route | PASS with ruling | Step 4 used the DIRECT route for the environmental reason recorded in section 2; the inner vstest invocation used the repository runsettings (Workers=0, Scope=ClassLevel) unchanged |

## 5. Test Coverage Detail

| File | Change type | Coverage observation | Disposition |
|---|---|---|---|
| QuickFiler/Controllers/QfcDatamodel.cs | Modified production (+14 / -2) | No class node in either Cobertura document (type-level ExcludeFromCodeCoverage, pre-existing). Execution proven: nine tests reach WorkerStarter(worker); controls A6/A7 throw NullReferenceException at the start site when the seam is unassigned | PASS (no-regression limb; unmeasured by pre-existing exemption) |
| QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs | Modified test | Outside the denominator by policy | Not applicable to the metric; four tests Passed |
| QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs | Modified test | Outside the denominator by policy | Not applicable to the metric; five tests Passed |
| QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs | Modified test | Outside the denominator by policy | Not applicable to the metric; three tests Passed |
| QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | Modified test | Outside the denominator by policy | Not applicable to the metric; eight tests Passed |

Package-level projection (QuickFiler package, identical at both stages): LINE missed 2293 / covered 10461; BRANCH missed 699 / covered 2518.

## 6. Test Execution Metrics

| Run | Scope | Total | Passed | Failed | Source |
|---|---|---|---|---|---|
| Baseline (P0-T16, DIRECT route) | nine test assemblies | 7361 | 7361 | 0 | evidence/baseline/coverage-baseline.md |
| Final (P6-T5, DIRECT route) | nine test assemblies | 7361 | 7361 | 0 | evidence/qa-gates/coverage-post-change.md |
| Nine targets alone (P4-T4) | QuickFiler.Test | 9 | 9 | 0 | evidence/regression-testing/targets-pass-after.md |
| Four target classes with FocusAndThemeTests (P4-T5) | QuickFiler.Test | 37 | 37 | 0 | evidence/regression-testing/concurrent-classes-pass-after.md |
| R4 fail-before (P1-T6, R-INJECT, run alone) | one test | 1 | 0 | 1 (expected) | evidence/regression-testing/r4-fail-before.md |
| Stall probe (P0-T15, shell-icon classes) | 23 tests | 23 | 22 | 1 (environmental, reproduces on main) | evidence/baseline/stall-probe.md |

Figures compared (evidence/qa-gates/coverage-post-change.md FIGURES-COMPARED): total, executed, error, timeout, aborted and notExecuted all equal between baseline and final; no Sequence file in any run; FINAL-FAILED-SET empty.

Negative controls (AC15), one row per test named in AC6 to AC13; mechanism per the spec Test Strategy table:

| Control | Test | Signal withheld | Outcome | Duration |
|---|---|---|---|---|
| A1 | DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle | no-op WorkerStarter | Failed | 0.186 s |
| B1 | RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces | no-op WorkerStarter | Failed | 0.259 s |
| A2 | RemainingLoadActive_AfterLoaderCompletes_BecomesFalse | release never set, then Drain() | Failed | 0.009 s |
| A3 | RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally | release never set, then Drain() | Failed | 0.002 s |
| A4 | Worker_DoWork_CapturesRemainingLoadTask | no-op WorkerStarter | Failed | 0.186 s |
| A5 | InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker | no-op WorkerStarter | Failed | 0.008 s |
| A6 | InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing | WorkerStarter assignment removed | Failed (NullReferenceException) | 0.186 s |
| A7 | InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop | WorkerStarter assignment removed | Failed (NullReferenceException) | 0.286 s |
| R-INJECT / A8 | Transaction_SecondCallerCannotInstallUntilTheFirstRestores | injected gate-free writer on the pre-fix shape, run alone / same writer on the pinned shape | Failed with the CI signature / Passed | 0.193 s / 0.083 s |
| C1, C2 | the two Drain-dependent liveness tests | Drain() removed with loader released | Failed | 0.162 s / 0.002 s |

THEME TEST NULL-DISPATCHER EXPOSURE: not observed. Both FocusAndThemeTests theme tests passed in the concurrent run (P4-T5). Analysis of the structural exposure is in code-review.2026-10-02T14-30.md (Observation O-1): the count of null-restoring writes R4 performs is unchanged by this branch, and the residual hazard belongs to the theme tests' discarded-scope pattern, which the spec places out of scope.

## 7. Code Quality Checks

| Check | Command or method | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | Grep over the feature folder for drive-letter paths, user-profile paths, account names and UNC prefixes | 0 hits (agrees with evidence/qa-gates/evidence-hygiene.md PROFILE_PATH_LINES 0 and FEATURE_ROOT_PROFILE_PATH_LINES 0) | PASS |
| Raw document scan | Glob over the feature folder for .trx, .xml, .coverage, .cobertura, .json | 0 files (RAW_DOCUMENTS 0 in the executor gate) | PASS |
| Suppression scan (added lines) | Read of all five changed files | No new #pragma, [SuppressMessage], or analyzer suppression | PASS |
| Workflow change scan | Name-status BASE..HEAD | No .github/, scripts/, runsettings or .csproj path changed | PASS |
| Prohibited-construct scan | Grep over the four changed test files for Thread.Sleep, Task.Delay, DoNotParallelize, SpinWait, .Wait(, WaitForState, Retry | One hit: pre-existing secondCallerStarted.Wait() (ManualResetEventSlim) in the R4 file; none in the three datamodel files | PASS |
| Call-site census | Grep for .InitEmailQueue( and WorkerStarter across the worktree | Production caller QfcHomeController.cs line 252 (constructed instance, seam assigned by both constructors); every test caller on an uninitialized instance assigns the seam (11 WorkerStarter occurrences across the three datamodel test files); QfcHomeControllerRunAsyncTests mocks IQfcDatamodel | PASS |
| Tonality scan | Read of spec.md, issue.md and the committed evidence | Neutral, factual wording; no humor, hyperbole or metaphor | PASS |

## Appendix A: Test Inventory

| Test class | Test | Status after change | AC |
|---|---|---|---|
| QfcDatamodelLivenessTests | DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle | Passed | AC6 |
| QfcDatamodelLivenessTests | RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces | Passed | AC7 |
| QfcDatamodelLivenessTests | RemainingLoadActive_AfterLoaderCompletes_BecomesFalse | Passed | AC8 |
| QfcDatamodelLivenessTests | RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally | Passed | AC9 |
| QfcDatamodelTeardownTests | Worker_DoWork_CapturesRemainingLoadTask | Passed | AC10 |
| QfcDatamodelTeardownTests | TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing, QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout, QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs, Cleanup_CalledTwice_DoesNotThrow | Passed, unchanged | regression |
| QfcInitEmailQueueZeroBatchTests | InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker | Passed | AC11 |
| QfcInitEmailQueueZeroBatchTests | InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing, InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop | Passed | AC12 |
| QfcItemController_UiThreadDispatcherFixtureTests | Transaction_SecondCallerCannotInstallUntilTheFirstRestores | Passed (alone and concurrent) | AC13, AC14 |
| QfcItemController_UiThreadDispatcherFixtureTests | R1, R2, R3, R5, R6, #743 counters, #882 zero-bound | Passed, unchanged | regression |
| QfcItemController_FocusAndThemeTests | SetThemeDark_FromNormal_SelectsDarkNormalTheme, SetThemeLight_FromNormal_SelectsLightNormalTheme | Passed concurrently with the target classes | exposure check |

Test method count: 7361 at baseline and after the change; no method added or removed.

## Appendix B: Toolchain Commands Reference

| Step | Command (as recorded in evidence/qa-gates/toolchain-final-pass.md) | Exit | Iteration |
|---|---|---|---|
| 1 | dotnet tool run csharpier format . | 0 | 1 |
| 1b | dotnet tool run csharpier check . | 0 | 1 |
| 2 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | 0 | 1 |
| 3 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | 0 | 1 |
| 4 | dotnet-coverage collect --output coverage\final-950.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-950.config -- vstest.console.exe <nine test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\950\final" "/Logger:trx;LogFileName=final-950.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (DIRECT route; the runner script Invoke-MSTestWithCoverage.ps1 was dot-sourced for its post-processing functions but not run verbatim, for the environmental reason in section 2) | 0 | 1 |

PowerShell gates (PoshQC MCP format / analyze / test): not run; zero PowerShell files changed on this branch.

Reviewer commands: none (Bash forbidden). All verification by Read, Grep and Glob against the item worktree and its reflog.
