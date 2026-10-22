# Policy Audit: focus-and-theme-tests-leak-shared-dispatcher-setup (Issue #968, folding Issue #972)

- Timestamp: 2026-10-03T04-00
- Branch: bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968
- Head: 5570b337cfd11e57579228b36bc919f9673b6195 (worktree reflog, last entry: merge of origin/main 993fdd01566dee82e5f37acb761a600feaaa1454 into the item)
- Base: origin/main 993fdd01566dee82e5f37acb761a600feaaa1454. The executor's Phase 0 anchor was 94287369908cc920b21b0e3256314f988ad7d2f5 (merge-base at that time, evidence/baseline/scope-and-anchor.md); origin/main advanced to 993fdd015 before the final head and was merged into the item, so origin/main is now an ancestor of the head and the item change set is the two-dot diff origin/main..HEAD, which the caller verified by name-status and which agrees with evidence/qa-gates/footprint-scope.md and evidence/qa-gates/final-commit.md.
- Work mode: full-bug (issue.md line 12); acceptance-criteria source: spec.md only (AC1 to AC32, spec.md lines 275-307)
- Reviewer: feature-review, no-Bash mode (caller directive). Every check was performed with Read, Grep and Glob against the item worktree, the committed evidence under evidence/, the session checkout's unchanged copies of the modified files as the pre-change text, and the worktree reflog as the head reference and clock. Where a check needs a shell it is recorded as such with the reason.
- Timestamp derivation: the label above is monotone after the head commit's reflog epoch 1791013176 (2026-10-03T07:39:36Z, 03:39:36 at the recorded -0400 offset) and after every label in the feature folder (latest 2026-10-03T03-37). No shell clock was readable in this session. The executor's labels were cross-checked against the reflog: the implementation commit label 03-23 matches epoch 1791012226 (03:23:46 -0400) and the final-commit label 03-37 matches epoch 1791013062 (03:37:42 -0400), so the evidence labels are clock-derived.

## Executive Summary

Overall verdict: AWAITING_CI. 1 Blocking finding (B-1: AC22, the full-toolchain criterion, is checked off only from this pull request's own CI run on the final head under the orchestrator ruling; remediability class awaiting_ci; nothing is remediable locally). 0 autonomous findings. 0 findings of class external_dependency, policy_hold or human_decision_required. Non-blocking findings and observations are detailed in code-review.2026-10-03T04-00.md. AC1 to AC21 and AC23 to AC32 verified PASS against code and evidence; AC22 is PENDING CI and stays unchecked.

| Area | Verdict | Evidence summary |
|---|---|---|
| General Unit Test Policy | PASS | Four new fixture-level tests and two rewritten liveness tests are deterministic (single-threaded pin cycle under a held transaction; explicit armed-timer and completion signals; no clock-advance-then-yield step, no retry loop); no Thread.Sleep, Task.Delay, [DoNotParallelize] or temporary file added; 7361/7361 at baseline, 7365/7365 after the change |
| General Code Change Policy | PASS | Failing regression test first for the fixture defect (fail-before captured with the fixture at base content, pass-after with the fixture change as the only difference); production edits limited to dead-code removal, one nameof retarget and comment rewrites in an attribute-excluded type; every touched file at or under 500 lines; toolchain evidence single pass |
| C# Code Change Policy | PASS | csharpier check exit 0 (1640 files); analyzer /t:Rebuild 0 errors 0 warnings, SKIP_CORECOMPILE_LINES 0; TreatWarningsAsErrors /t:Rebuild 0 errors 0 warnings, SKIP_CORECOMPILE_LINES 0; no /p:Nullable=enable |
| C# Unit Test Policy | PASS | MSTest, Moq, FluentAssertions throughout; first-party lines 85.36%, branches 79.75% (floors 80/75 per CLAUDE.md, 85/75 per .claude/rules, both met) |
| Coverage (C#) | PASS | Repo-wide not lower than baseline (85.35 to 85.36 lines, 79.73 to 79.75 branches); the two changed production files belong to the attribute-excluded type QfcDatamodel (pre-existing attribute, unchanged) and the production edit removes unreachable members and rewrites comments only |
| Evidence hygiene | PASS | 0 host paths and 0 raw trx or coverage documents in the committed feature folder (reviewer Grep and Glob sweep; evidence/qa-gates/evidence-hygiene.md PROFILE_PATH_LINES 0, RAW_DOCUMENTS 0) |
| Coordinator prohibitions | PASS | No retries, no [DoNotParallelize], no Workers=1, no lengthened timeout, no wall-clock wait, no temporary file, runsettings unchanged (evidence/qa-gates/prohibited-constructs-grep.md over 696 added lines plus direct reads) |
| Toolchain step 4 route | PENDING CI (B-1) | Step 4 ran by the DIRECT route (collector over vstest with the four shell-icon classes excluded) because the stall probe reproduced a deterministic environmental failure that also reproduces on main; AC22 closes from the pull request's CI run per the ruling recorded under AC22 in spec.md |

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. The following caller statements were evaluated and accepted as factual or as tooling constraints rather than as narrowing:

- "DO NOT use the Bash tool at all." A tooling constraint, not a scope constraint. The audit scope remains the full branch diff against origin/main; every changed file was read in full from the worktree and compared against the session checkout's unchanged copy where the file pre-existed.
- "The item's change set versus origin/main is exactly (name-status ...)" with sixteen paths plus the feature folder. Confirmed against evidence/qa-gates/footprint-scope.md and evidence/qa-gates/final-commit.md (both record git diff --name-status BASE HEAD) and against the files on disk. Zero PowerShell, TypeScript or Python files changed; no language was declared not applicable by the caller.
- "Evaluate AC22 as PENDING CI (remediability class awaiting_ci), not FAIL." A ruling on one criterion's evidence source, recorded verbatim under AC22 in spec.md and following the #950 AC17 precedent. It does not narrow the audit scope; this review evaluates every criterion and every policy over the full diff.

## Evidence Location Compliance

- Branch diff scan for files under artifacts/baselines/, artifacts/qa/, artifacts/evidence/ or artifacts/coverage/: none. The name-status origin/main..HEAD lists the fourteen code paths, the two promoted records under docs/features/potential/promoted/ and paths under docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/ only.
- All executor evidence lives under docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/{baseline,qa-gates,regression-testing,other}/ (74 Markdown files; Glob listing in this review).
- validate_evidence_locations.py --root .: not run (Bash was forbidden for this review; the script is not present in this checkout). The manual scan above substitutes; no violation observed.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required; the caller supplied no non-canonical evidence path.
- PR context artifacts (artifacts/pr_context.summary.txt, artifacts/pr_context.appendix.txt): absent in both the review worktree and the session checkout (Read of each exact path failed). Regeneration was not possible without a shell or the collection tool. Scope was derived from the caller-supplied name-status, the committed footprint evidence and the files on disk; the scope is verified by three agreeing sources even though the artifact pair itself could not be produced.
- Raw coverage document: coverage/final-968.cobertura.xml exists locally in the worktree (gitignored, not committed; root element read for this review: line-rate 0.853557, branch-rate 0.797517, lines-covered 56211, lines-valid 65855, branches-covered 13620, branches-valid 17078, timestamp 1791012617 = 03:30:17 -0400, consistent with the 03-31 label of evidence/qa-gates/coverage-summary.md). The canonical path artifacts/csharp/coverage.xml is absent in both checkouts; the committed JaCoCo package projection plus one-line summary are the forms CLAUDE.md "Committed Test Evidence Format" requires, and the standing ruling treats executor-committed feature-folder coverage evidence as the present artifact.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every pin-count test acquires its own UiThreadDispatcherFixture transaction first (QfcItemController.UiThreadDispatcherPinCountTests.cs lines 39-41, 87-89, 140-142, 188-190, 224-226), so the pin count is zero and the baseline known for the whole test; every test disposes its transaction in a finally. Every datamodel test builds its own uninitialized QfcDatamodel and owns its worker in a using block. The liveness test restores SynchronizationContext.Current through the SynchronizationContextScope disposer (QfcDatamodelLivenessTests.cs lines 189-199). Concurrent run of the three #968 classes 29/29 and of the four datamodel classes 21/21 |
| Isolation | PASS | Each pin-count test targets one property of the counted pin (non-last release, release order, foreign-value protection, flag reset); each rewritten liveness test targets the gate's re-arm decision; the fail-before run names exactly one failing assertion (afterFirstRelease, found null) |
| Fast execution | PASS | Pin-count tests 0.001 s to 0.055 s (evidence/regression-testing/pass-after-pin-count.md); rewritten liveness tests 0.160 s and 0.162 s alone (liveness-pass-after.md); no bounded wait remains |
| Determinism | PASS | The regression is observed on one thread with no concurrency; the census proves every pin is acquired and released inside a held transaction with no Install between (13 of 13 nested, evidence/qa-gates/call-site-census.md), so the final "last release nulls" assertion cannot be disturbed by another class; the liveness tests use ArmingFakeTimeProvider.Armed, Task.WhenAny and the dequeue task itself as signals, with the production awaits registered under a null SynchronizationContext so the loader's completion clears the flag inline; the sensitivity check shows both rewritten tests fail crisply (not hang) when the liveness lambda is forced false |
| Readability | PASS | Descriptive names; XML doc on every test, helper and fixture member; Arrange / Act / Assert markers; a because-reason on every assertion |

### 1.2 Coverage

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 13 | 7365 | 7365 passed, 0 failed | 85.35% lines / 79.73% branches | 85.36% lines / 79.75% branches | N/A (no instrumented production line added; see the per-language comparison block) |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

Files Changed counts .cs files only (thirteen: ten modified, three added); the project file QuickFiler.Test/QuickFiler.Test.csproj and the two promoted Markdown records are the remaining three changed paths.

Coverage source statement: the figures above are read from the committed projections and summaries (evidence/baseline/coverage-summary.md and coverage-jacoco-projection.md at P0-T17; evidence/qa-gates/coverage-summary.md and coverage-jacoco-projection.md at P8-T5; evidence/qa-gates/coverage-comparison.md at P8-T6), each carrying the first-party summary line, the root counters and the package-level JaCoCo projection, and were cross-checked against the root element of the local raw document coverage/final-968.cobertura.xml. Both runs used the DIRECT route with the identical four-class shell-icon exclusion, so they are comparable.

Verdict lines:

- C# coverage verdict: PASS (repo-wide first-party lines 85.36% and branches 79.75% from the committed post-change projection and the raw root element; above the CLAUDE.md floors of 80% lines and 75% branches and above the 85% / 75% floors in .claude/rules; not lower than the baseline 85.35% / 79.73%).
- C# changed-production-file coverage: PASS on the no-regression limb. Both changed production files, QuickFiler/Controllers/QfcDatamodel.cs and QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs, are partials of the type QfcDatamodel, which carries a pre-existing type-level ExcludeFromCodeCoverage attribute (QfcDatamodel.cs line 25, unchanged by this branch), so neither Cobertura document has a class node for the type (QFCDATAMODEL_CLASS_ENTRIES 0 at both stages) and no measured line can regress. The production edit adds no executable line: it removes four caller-free private members, their commented-out references and an empty region, retargets one nameof, and rewrites two doc comments. The thirteen changed or added test files are outside the denominator by policy.
- C# package-level corroboration: the QuickFiler package counters moved by one covered line (LINE missed 2294 / covered 10460 after; the +5 covered lines and +3 covered branches of the root delta are run-to-run variance across packages this branch does not instrument differently), consistent with a change whose only production lines are in an attribute-excluded type.
- PowerShell coverage gate: PASS by vacuity (zero PowerShell files changed on this branch, so the changed-line no-regression requirement has no line to evaluate; no PoshQC format, analyze or test gate was owed or run; artifacts/pester/powershell-coverage.xml was not consulted).
- TypeScript and Python: zero files changed on this branch; no verdict is owed.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/baseline/coverage-summary.md` with `evidence/baseline/coverage-jacoco-projection.md` (committed one-line first-party summary, root counters and JaCoCo package projection; canonical artifacts/csharp/coverage.xml absent in the worktree)
- C# post-change coverage artifact: `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/qa-gates/coverage-summary.md` with `evidence/qa-gates/coverage-jacoco-projection.md` (same three forms; raw document coverage/final-968.cobertura.xml present locally, gitignored; canonical artifacts/csharp/coverage.xml absent in the worktree)
- TypeScript baseline coverage artifact: none consulted (zero TypeScript files changed on this branch)
- TypeScript post-change coverage artifact: none consulted (zero TypeScript files changed on this branch)
- PowerShell baseline coverage artifact: none consulted (zero PowerShell files changed on this branch)
- PowerShell post-change coverage artifact: none consulted (zero PowerShell files changed on this branch)
- Python baseline coverage artifact: none consulted (zero Python files changed on this branch)
- Python post-change coverage artifact: none consulted (zero Python files changed on this branch)
- Per-language comparison summary: the per-language comparison block of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.35% lines (56206/65855) / 79.73% branches (13617/17078). Post-change: 85.36% lines (56211/65855) / 79.75% branches (13620/17078). Change: +0.01% lines (+5 covered) / +0.02% branches (+3 covered), with lines-valid and branches-valid identical at both stages. Disposition: PASS. Evidence: evidence/baseline/coverage-summary.md, evidence/qa-gates/coverage-summary.md, evidence/qa-gates/coverage-comparison.md, root element of coverage/final-968.cobertura.xml.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Artifact consulted | State | Disposition |
|---|---|---|---|
| C# | Committed projections and summaries under evidence/baseline and evidence/qa-gates; raw Cobertura root element read locally | Present; canonical artifacts/csharp/coverage.xml absent in the worktree (recorded as observation O-4, recurring) | PASS |
| TypeScript | none | zero files changed | no verdict owed |
| PowerShell | none | zero files changed | no verdict owed |
| Python | none | zero files changed | no verdict owed |

Coverage exclusion policy check (.claude/rules/general-unit-test.md): the branch adds no coverage-config exclude entry and no new ExcludeFromCodeCoverage attribute (Grep over the fourteen code paths: the only occurrence is the pre-existing QfcDatamodel.cs line 25; the three new files carry none). Under the standing ruling (the CLAUDE.md UT2 COM/VSTO exemption is the more specific clause; the rules file's Blocking clause enumerates config exclude globs), the pre-existing attribute is Not Blocking for this change. Its consequence (no coverage observation for the removed lines) is immaterial here because the removed members were unreachable (zero callers, evidence/qa-gates/qfc-datamodel-legacy-callers.md) and both rebuilds compiled every remaining reference.

### 1.3 Scenario completeness

| Scenario | Verdict | Evidence |
|---|---|---|
| Positive flows | PASS | Two pins, first released, dispatcher kept (AC1); last release reverts (AC2); fresh pin after a full cycle installs and restores (AC4); liveness gate re-arms while the loader produces (AC31) |
| Negative flows | PASS | Pins taken under a transaction's live dispatcher install nothing and null nothing at count zero (AC3, R1); a pin under a transaction that installed the parked instance as its own value leaves it in place (AC4 second block); liveness sensitivity: with the liveness lambda forced false both rewritten tests fail on the re-arm assertion |
| Edge cases | PASS | Release order independence (AC2 specification test); idempotent scope dispose (R3, unchanged); double transaction dispose (R5, unchanged); flag-true-but-field-changed branch documented as a residual and shown unreached by the census (no Install between any pin's acquisition and release) |
| Error handling | PASS | R4 now releases its gate through finally on any throw (AC14); the throwing-loader liveness test still clears the flag through finally (unchanged, passes) |
| Concurrency | PASS | Three-class concurrent run 29/29 and four-class datamodel run 21/21 under Workers=0 / ClassLevel; the R4 two-transaction ordering kept with assertions unchanged; the pin lifetime nests inside the gate hold at every call site |
| State transitions | PASS | Count 0 -> 1 -> 2 -> 1 -> 0 with the field observed at each step (tests 1 and 2); flag set on first seeding and cleared on last release (test 4); _remainingLoadActive true -> false observed through ReadLivenessFlag before the final advance |

### 1.4 Arrange-Act-Assert

PASS. Every new and rewritten test carries Arrange / Act / Assert markers (pin-count tests lines 38, 49, 55, 86, 97, 103, 136, 149, 154, 186, 200, 206, 223, 234; liveness test lines 104, 143, 147, 157; sibling test lines 105, 133, 137); every assertion carries a because-reason string.

### 1.5 External dependencies and temporary files

PASS. No Outlook COM, file system, network or process is touched by any changed test; the parked and running dispatchers are in-process WPF dispatchers on background threads created by the existing fixture helpers; Grep over the thirteen changed test files for Path.GetTempFileName and Path.GetTempPath: 0 hits (evidence/qa-gates/prohibited-constructs-grep.md ADDED-TOKEN 0 and 0).

### 1.6 Test file location

PASS (repository convention). Tests live in QuickFiler.Test/Controllers/ and QuickFiler.Test/TestSupport/, mirroring the per-project *.Test layout used for every C# project in this repository; the two new TestSupport files sit beside the existing WinFormsPumpHost.cs and DedicatedWorkerThread.cs helpers. The rules file names a tests/ tree; the per-project layout is the pre-existing convention and no test file was colocated with production source.

### 1.7 Determinism infrastructure

PASS. Time is driven by FakeTimeProvider through the ArmingFakeTimeProvider subclass in both rewritten tests (QfcDatamodelLivenessTests.cs line 106, QfcDatamodelTests.cs line 107); the pin-count tests use no time at all. Banned APIs in test code: Thread.Sleep 0, Task.Delay 0, await Task.Yield() 0 over the 696 added lines; the only timed construct is the sibling file's [Timeout(GateTimeoutMs)] attribute with the same 60000 ms constant (four occurrences in the new class, a failure bound, not a wait). No randomness is used.

## 2. General Code Change Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| Before making changes (plan, spec) | PASS | spec.md (336 lines, amendments 1.1 and 1.2) and plan.2026-10-02T05-42.md (123 of 123 tasks checked) exist; six preflight rounds and a clearance recorded under evidence/other/ |
| Bugfix workflow step 1 (failing regression test first) | PASS | Fixture defect: EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease failed on the fixture at base content (hash equal to the P0-T12 BASE-HASH) with the predicted message "... a holder that did not take the last pin must not lose the dispatcher, but found <null>" (evidence/regression-testing/fail-before-pin-count.md), then passed with the fixture change as the only working-copy difference (pass-after-pin-count.md, porcelain comparison). Liveness rewrites: a deterministic failing run of the old shape is structurally impossible (its failure depends on thread-pool scheduling); the dossier fail-before-exception.2026-10-03T03-09.md records the reason, and the labelled sensitivity check (liveness-sensitivity-check.md) shows the new shape fails crisply when the liveness signal is dishonest, with the temporary production edit reverted and proven byte-identical to base. Dead-code removal: unreachable code with no behaviour to regress; the zero-caller proof (qfc-datamodel-legacy-callers.md, two strategies, member sets identical, INVOCATIONS 0) plus two rebuilds are the proof |
| Bugfix workflow step 2 (minimal targeted fix) | PASS | Fixture diff 48 added / 15 deleted lines, all in EnsureDispatcher, EnsureScope and docs; the production diff removes 129 lines and adds 1 in QfcDatamodel.cs and changes 8 comment lines in QueueProcessing.cs; no reachable production behaviour changes |
| Bugfix workflow step 3 (verify locally, toolchain in order) | PENDING CI (B-1) | Steps 1-3 single pass, exit 0, SKIP_CORECOMPILE_LINES 0 on both rebuilds (evidence/qa-gates/toolchain-final.md SINGLE-PASS: YES). Step 4 ran the DIRECT route because the stall probe recorded ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension failing deterministically on this workstation (Win32 handle not valid; reproduces on main; evidence/baseline/stall-probe.md). The orchestrator ruling under AC22 in spec.md defers the check-off to this pull request's CI run on the final head |
| Design principles (simplicity, reusability, extensibility, separation) | PASS | One counter and one flag at the single mutation point of the shared static; the dead theme-test calls deleted instead of pinning state the tests never read; one shared SynchronousBackgroundWorker replaces three byte-identical copies; ArmingFakeTimeProvider is a 49-line subclass modelled on the existing UtilitiesCS.Test ArmingBarrierTimeProvider |
| Classes, functions, APIs | PASS | Fixture public shape unchanged (internal static IDisposable EnsureDispatcher()); new helpers internal sealed with documented contracts; StartHeldOpenLoader takes the caller-owned worker |
| Error handling | PASS | No new catch; R4 gains try/finally; the hard cast in StartSynchronously fails fast on misuse |
| Logging | PASS | No new logging; the retargeted nameof corrects the method named in an existing log line |
| File size limit (500 lines) | PASS | Reviewer Read line counts: fixture 375, FocusAndThemeTests 482, TestSupport 442, fixture tests 472, pin-count tests 248, Liveness 352, Teardown 229, ZeroBatch 226, DatamodelTests 394, QfcDatamodel.cs 367 (was 495), QueueProcessing.cs 413, SynchronousBackgroundWorker.cs 27, ArmingFakeTimeProvider.cs 49; all agree with evidence/qa-gates/file-line-counts.md |
| Naming | PASS | PascalCase types and members (ArmingFakeTimeProvider, StartSynchronously, ReArm); camelCase locals (pinA, afterFirstRelease, foreignTransaction); descriptive test names without digits |
| Public APIs and compatibility | PASS | IQfcDatamodel unchanged; every removed member was private with zero callers; the one-argument LoadRemainingEmailsToQueueAsync the constructors bind is kept |
| Dependencies | PASS | None added; Microsoft.Extensions.Time.Testing already referenced by the test project |
| I/O boundaries | PASS | No I/O introduced |

## 3. Language-Specific Code Change Policy Compliance

Language in scope: C# only.

| Item | Verdict | Evidence |
|---|---|---|
| Formatting (csharpier via dotnet tool run) | PASS | evidence/qa-gates/csharpier-format-final.md (REWRITTEN-WRITESET: NONE) and csharpier-check-final.md: Checked 1640 files, CSHARPIER_EXIT_CODE 0 |
| Linting (analyzer rebuild, /t:Rebuild, EnableNETAnalyzers, EnforceCodeStyleInBuild) | PASS | evidence/qa-gates/msbuild-analyzer-final.md: MSBUILD_EXIT_CODE 0, ERRORS 0, WARNINGS 0, SKIP_CORECOMPILE_LINES 0, WRITESET_DIAGNOSTIC_LINES 0 |
| Type checking (TreatWarningsAsErrors rebuild, no /p:Nullable=enable) | PASS | evidence/qa-gates/msbuild-nullable-final.md: exit 0, 0 errors, 0 warnings, SKIP_CORECOMPILE_LINES 0; command text matches CLAUDE.md character for character |
| Nullable annotations | PASS | No changed file carries #nullable enable; no nullable directive was added or removed |
| DI seams | PASS | No new production seam; the liveness rewrite uses the existing TimeProvider property and the existing WorkerStarter and RemainingEmailLoader delegates |
| XML docs on non-obvious contract | PASS | Fixture class doc, EnsureDispatcher doc and EnsureScope doc describe counting, ownership, discard consequence and residual; wrapper doc names the remaining callers and the nesting rule; ArmingFakeTimeProvider remarks state the consecutive-Advance prohibition |
| Internal surface | PASS | Both new helpers internal sealed; the fixture's new fields private static |
| Name resolution | PASS | SynchronousBackgroundWorker.StartSynchronously takes BackgroundWorker in a file that imports no Outlook namespace; the datamodel test files that import Microsoft.Office.Interop.Outlook write System.Action where a bare delegate is needed (QfcDatamodelTeardownTests.cs line 175, QfcInitEmailQueueZeroBatchTests.cs line 135) |
| Analyzer suppressions | PASS | None added; the file's only pragma (CS0618 around the removed two-argument overload) disappears with the dead member |

## 4. Language-Specific Unit Test Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | [TestClass] / [TestMethod] in every changed test file; the new class uses the sibling [Timeout(GateTimeoutMs)] convention |
| Moq for mocks | PASS | Mock<IApplicationGlobals>, Mock<IAppQuickFilerSettings>, Mock<MailItem>, Mock<NameSpace>, Mock<Application>, Mock<IItemViewer> where mocks are needed; the pin-count class states why Moq is not imported |
| FluentAssertions | PASS | All assertions use Should(); no MSTest Assert introduced |
| Repo-wide coverage floors | PASS | 85.36% lines (floor 80% per CLAUDE.md, 85% per rules), 79.75% branches (floor 75%) |
| New module/class/method >= 90% | PASS on the applicable limb | No new production module, class or method exists; the three new files are test files outside the denominator |
| No regression on changed lines | PASS | No measured changed production line exists (attribute-excluded type); repo-wide and package-level figures are not lower |
| Prohibited behaviors (sleeps, retries, timing hacks, weakened assertions) | PASS | ADDED-TOKEN Thread.Sleep 0, Task.Delay 0, DoNotParallelize 0, Retry( 0, await Task.Yield() 0, for (int i 0 over 696 added lines; GateTimeoutMs = 60000 equals the sibling constant; R4 keeps BeSameAs(original) and NotBeSameAs(liveA) with because texts unchanged; R1 to R3 assertions unchanged (reviewer comparison against the session checkout's copy) |
| Test toolchain route | PENDING CI (B-1) | Step 4 used the DIRECT route for the environmental reason recorded in section 2; the inner vstest invocation used the repository runsettings (Workers=0, Scope=ClassLevel) unchanged |

## 5. Test Coverage Detail

| File | Change type | Coverage observation | Disposition |
|---|---|---|---|
| QuickFiler/Controllers/QfcDatamodel.cs | Modified production (+1 / -129) | No class node in either Cobertura document (type-level ExcludeFromCodeCoverage, pre-existing). Removed members had zero callers; the surviving one-argument loader is unchanged except the nameof operand | PASS (no-regression limb; no executable line added) |
| QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | Modified production (comment-only, 8 lines) | Same excluded type; every changed line begins with /// (evidence/qa-gates/queue-processing-comment-census.md transcribed diff, confirmed by reading) | PASS (no executable line changed) |
| QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs | Modified test support (+48 / -15) | Outside the denominator by policy | Not measured; fixture tests 8/8 and pin-count tests 4/4 Passed |
| QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs | Added test (248 lines) | Outside the denominator by policy | Not measured; 4/4 Passed, 1 fail-before captured |
| QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | Modified test (+16 / -14) | Outside the denominator by policy | Not measured; 8/8 Passed |
| QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs | Modified test (+20 / -35) | Outside the denominator by policy | Not measured; 17/17 Passed |
| QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs | Modified test support (+20 / -18, doc only) | Outside the denominator by policy | Not measured |
| QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs | Modified test (+137 / -101) | Outside the denominator by policy | Not measured; 4/4 Passed |
| QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs | Modified test (+2 / -17) | Outside the denominator by policy | Not measured; 5/5 Passed |
| QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs | Modified test (+41 / -49) | Outside the denominator by policy | Not measured; 3/3 Passed |
| QuickFiler.Test/Controllers/QfcDatamodelTests.cs | Modified test (+68 / -47) | Outside the denominator by policy | Not measured; 9/9 Passed |
| QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs | Added test support (27 lines) | Outside the denominator by policy | Not measured; exercised by every datamodel test that starts a worker |
| QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs | Added test support (49 lines) | Outside the denominator by policy | Not measured; exercised by the two rewritten liveness tests |
| QuickFiler.Test/QuickFiler.Test.csproj | Modified project file (+3 Compile items) | Not a source file | Discovery proven: all four pin-count tests appear in the final run |

Package-level projection (QuickFiler package): LINE missed 2294 / covered 10460; BRANCH missed 699 / covered 2518 after the change (evidence/qa-gates/coverage-jacoco-projection.md).

## 6. Test Execution Metrics

| Run | Scope | Total | Passed | Failed | Source |
|---|---|---|---|---|---|
| Baseline (P0-T17, DIRECT route) | nine test assemblies | 7361 | 7361 | 0 | evidence/baseline/coverage-summary.md |
| Final (P8-T5, DIRECT route) | nine test assemblies | 7365 | 7365 | 0 | evidence/qa-gates/coverage-summary.md |
| Pin-count regression fail-before (P1-T5, fixture at base) | one test | 1 | 0 | 1 (expected) | evidence/regression-testing/fail-before-pin-count.md |
| Specification tests before the fix (P1-T6) | three tests | 3 | 3 | 0 | evidence/regression-testing/specification-tests-before-fix.md |
| Pin-count class pass-after (P2-T8) | four tests | 4 | 4 | 0 | evidence/regression-testing/pass-after-pin-count.md |
| Liveness sensitivity check (P5-T8, lambda forced false, reverted) | two tests | 2 | 0 | 2 (expected, labelled) | evidence/regression-testing/liveness-sensitivity-check.md |
| Liveness pass-after (P5-T7) | two tests | 2 | 2 | 0 | evidence/regression-testing/liveness-pass-after.md |
| Concurrent set, three #968 classes (P6-T7) | QuickFiler.Test | 29 | 29 | 0 | evidence/regression-testing/concurrent-set-test-summary.md |
| Datamodel set, four classes (P6-T8) | QuickFiler.Test | 21 | 21 | 0 | evidence/regression-testing/datamodel-set-test-summary.md |
| Stall probe (P0-T16, shell-icon classes) | 23 tests | 23 | 22 | 1 (environmental, reproduces on main) | evidence/baseline/stall-probe.md |

Figures compared (evidence/qa-gates/coverage-summary.md FIGURES-COMPARED): final total equals baseline plus 4 (the four pin-count tests; the fold rewrites two tests and adds none); error, timeout, aborted and notExecuted each 0 at both stages; no Sequence file in any run; FINAL-FAILED-SET empty; MESSAGE lines none (no LEAK-DEPENDENT TEST EXPOSED).

## 7. Code Quality Checks

| Check | Command or method | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | Grep over the feature folder for drive-letter paths, user-profile paths and account names | 0 hits (agrees with evidence/qa-gates/evidence-hygiene.md PROFILE_PATH_LINES 0 after the recorded redaction of inherited preflight reports) | PASS |
| Raw document scan | Glob over the feature folder for non-Markdown files | 0 files (RAW_DOCUMENTS 0 in the executor gate) | PASS |
| Suppression scan (added lines) | Read of all fourteen code paths | No new #pragma, [SuppressMessage], [ExcludeFromCodeCoverage] or analyzer suppression; one pre-existing pragma removed with its dead member | PASS |
| Workflow change scan | Name-status origin/main..HEAD | No .github/, scripts/ or runsettings path changed; the only project-file change is three Compile items (evidence/qa-gates/file-line-counts.md: 190 = 187 + 3) | PASS |
| Prohibited-construct scan | Grep over the thirteen changed test files and the added-lines scan | Thread.Sleep 0, Task.Delay 0, DoNotParallelize 0, Retry( 0, Workers 0, Task.Yield 0 in the two rewritten tests; four [Timeout(GateTimeoutMs)] at the sibling constant | PASS |
| Call-site census | Grep for EnsureUiThreadDispatcher and EnsureDispatcher across the worktree | Sixteen primary lines: two declarations, one forwarder, thirteen invocations (three in R1 to R3, ten in the pin-count class), each nested inside a held transaction with no Install between acquisition and release; zero in FocusAndThemeTests; zero in R4 (reviewer Grep agrees with evidence/qa-gates/call-site-census.md) | PASS |
| Shared-helper census | Grep for class SynchronousBackgroundWorker over *.cs | Exactly one declaration, QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs line 15 | PASS |
| Tonality scan | Read of spec.md, issue.md, both research records and the committed evidence | Neutral, factual wording; no humor, hyperbole or metaphor | PASS |

## 8. Gaps and Exceptions

- B-1 (Blocking, awaiting_ci): AC22 requires the MSTest coverage route (Invoke-MSTestWithCoverage.ps1) to pass locally in the same uninterrupted pass as the three preceding steps. The runner cannot pass on this workstation because one UtilitiesCS.Test shell-icon test fails deterministically for an environmental reason that reproduces on main; the DIRECT route substituted with the same runsettings and the same assembly set minus four shell-icon classes, 7365/7365. The orchestrator ruling under AC22 in spec.md (following the #950 AC17 precedent) closes AC22 only from this pull request's own CI run on the final head. No local action can discharge it. Recorded in remediation-inputs.2026-10-03T04-00.md.
- PR context artifact pair absent: scope verified from three agreeing sources instead (section Evidence Location Compliance).
- Canonical C# coverage artifact path absent: committed projections and the local raw document used, per the standing ruling (observation O-4, recurring across #948, #950, #956).
- Liveness-test fail-before: structurally impossible for the old shape; exception dossier plus labelled sensitivity check recorded, as the spec (decision 7) and the atomic-plan contract permit.
- quality-tiers.yml absent at the repository root (pre-existing; tier gates unevaluable; already promoted by the #956 review). Not attributable to this item.

## 9. Summary of Changes

- Fixture (QfcItemController.UiThreadDispatcherFixture.cs): ensure pins are reference counted under FieldLock with an install-ownership flag; the last release writes null only when the fixture seeded the parked dispatcher and the field still holds it; the decrement and the conditional revert are inline in one critical section; class, method and scope docs rewritten (D1).
- Theme tests (QfcItemController.FocusAndThemeTests.cs): the two dead EnsureUiThreadDispatcher() calls deleted with corrected arrange comments (D6); the private BuildExecutingViewer copy deleted in favour of the shared helper (D5).
- Test support (QfcItemController.TestSupport.cs): wrapper doc describes the counted pin and names the remaining callers (D2); shared-helper doc no longer cites an unreachable private copy (D5). EnsureSynchronizationContext untouched (D8).
- Fixture tests (QfcItemController.UiThreadDispatcherFixtureTests.cs): R4 doc rewritten to the counting guarantee and nesting invariant (D3); R4's baseline pin removed and transactionA wrapped in try/finally (D4, closing #972 item 5); assertions and because texts unchanged.
- New pin-count test class (QfcItemController.UiThreadDispatcherPinCountTests.cs): one fail-before regression test and three labelled specification tests; Compile item added.
- Folded #972 items 1 to 4 and the #968 liveness comment: shared SynchronousBackgroundWorker helper with the StartSynchronously starter; _remainingLoadActive comment rewritten and the stale TryUnhookOrReplace line range dropped; four caller-free QfcDatamodel members, their commented-out references and the empty region removed with the nameof retarget (495 -> 367 lines); every test-created worker owned in a using block; the two dequeue-liveness tests rewritten to explicit signals through the new ArmingFakeTimeProvider; two Compile items added.
- Evidence: 74 Markdown files under the feature folder (baseline, regression-testing, qa-gates, other); no raw document committed.

## 10. Compliance Verdict

AWAITING_CI. Every policy area evaluates PASS over the full branch diff. One blocking finding remains, B-1, of remediability class awaiting_ci: AC22's test-stage evidence comes from this pull request's CI run on the final head under the recorded ruling. Zero autonomous findings; nothing requires a remediation cycle before the pull request is opened. The pull request body must carry the two closing lines Closes #968 and Closes #972 and no other issue number beside a closing keyword (spec Dependencies and Rollout).

## Appendix A: Test Inventory

| Test class | Test | Status after change | AC |
|---|---|---|---|
| QfcItemController_UiThreadDispatcherPinCountTests | EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease | Failed before the fix (fixture at base), Passed after | AC1, AC2, AC5, AC6 |
| QfcItemController_UiThreadDispatcherPinCountTests | EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome | Passed before and after | AC2, AC6 |
| QfcItemController_UiThreadDispatcherPinCountTests | EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher | Passed before and after | AC3, AC6 |
| QfcItemController_UiThreadDispatcherPinCountTests | EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores | Passed before and after | AC4, AC6 |
| QfcItemController_UiThreadDispatcherFixtureTests | Transaction_SecondCallerCannotInstallUntilTheFirstRestores (R4) | Passed, alone and concurrent | AC10, AC13, AC14 |
| QfcItemController_UiThreadDispatcherFixtureTests | R1, R2, R3, R5, R6, #743 counters, #882 zero-bound | Passed, unchanged | AC3, AC10 |
| QfcItemController_FocusAndThemeTests | SetThemeDark_FromNormal_SelectsDarkNormalTheme, SetThemeLight_FromNormal_SelectsLightNormalTheme | Passed (ensure calls removed) | AC7, AC16 |
| QfcItemController_FocusAndThemeTests | fifteen other tests | Passed, unchanged behaviour (shared BuildExecutingViewer) | AC15, AC24 |
| QfcDatamodelLivenessTests | DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle | Passed (rewritten); Failed under the sensitivity check | AC31 |
| QfcDatamodelLivenessTests | RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces, RemainingLoadActive_AfterLoaderCompletes_BecomesFalse, RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally | Passed (caller-owned worker) | AC25, AC30 |
| QfcDatamodelTests | DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive | Passed (rewritten); Failed under the sensitivity check | AC31 |
| QfcDatamodelTests | eight other tests | Passed (two gain using blocks) | AC30, AC32 |
| QfcDatamodelTeardownTests | five tests | Passed (shared helper) | AC25, AC32 |
| QfcInitEmailQueueZeroBatchTests | three tests | Passed (shared helper, using blocks) | AC25, AC30, AC32 |

Test method count: 7361 at baseline, 7365 after the change (four added, none removed).

## Appendix B: Toolchain Commands Reference

| Step | Command (as recorded in evidence/qa-gates/toolchain-final.md) | Exit | Iteration |
|---|---|---|---|
| 1 | dotnet tool run csharpier format . | 0 | 1 |
| 1b | dotnet tool run csharpier check . | 0 | 1 |
| 2 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | 0 | 1 |
| 3 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | 0 | 1 |
| 4 | dotnet-coverage collect --output coverage\final-968.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-968.config -- vstest.console.exe <nine test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\968\final" "/Logger:trx;LogFileName=final-968.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (DIRECT route; the runner script Invoke-MSTestWithCoverage.ps1 was not run verbatim, for the environmental reason in section 2; PASS 7365/7365) | 0 | 1 |

PowerShell gates (PoshQC MCP format / analyze / test): not run; zero PowerShell files changed on this branch.

Reviewer commands: none (Bash forbidden). All verification by Read, Grep and Glob against the item worktree, the session checkout's pre-change copies and the worktree reflog.
