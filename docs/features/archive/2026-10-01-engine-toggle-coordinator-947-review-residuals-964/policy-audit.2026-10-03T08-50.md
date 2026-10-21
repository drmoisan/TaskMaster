# Policy Audit: engine-toggle-coordinator-947-review-residuals (Issue #964)

- Timestamp: 2026-10-03T08-50
- Branch: bug/engine-toggle-coordinator-947-review-residuals-964
- Head: 2fed92d2f4807653b32ef0236b6f1d82a6084ed5 (worktree reflog, last entry: `docs(964): record hygiene sweeps, AC1-AC8 check-offs and reduced-audit handoff (P2-T9 to P2-T20)`)
- Base: origin/main 993fdd01566dee82e5f37acb761a600feaaa1454, merged into the item at 981abef77657adcc90d7c116a6b4c6500b79ea29 (reflog entry `merge origin/main`, epoch 1790993935). origin/main is an ancestor of the head, so the item change set is the two-dot diff origin/main..HEAD, which the caller supplied as a name-status listing and which agrees with evidence/qa-gates/footprint-scope.md (31 paths at P2-T8 plus the later evidence-only commits) and with the files on disk. The executor's Phase 0 anchor for the pre-change text was 94287369908cc920b21b0e3256314f988ad7d2f5 (evidence/baseline/scope-and-anchor.md); the negative control in footprint-scope.md shows that no path under TaskMaster or TaskMaster.Test changed between that anchor and the merge commit, so the executor's base census is the pre-change text of every code file in scope.
- Work mode: minor-audit (issue.md line 12); acceptance-criteria source: the `## Acceptance Criteria` section of issue.md only (AC1 to AC8, lines 27 to 34)
- Reviewer: feature-review, no-Bash mode (caller directive). Every check was performed with Read, Grep and Glob against the item worktree, the committed evidence under evidence/, the caller-supplied verbatim diff, the gitignored raw Cobertura documents at their local paths, and the worktree reflog as head reference and clock. Where a check would need a shell it is recorded as such with the reason.
- Timestamp derivation: the label above is monotone after the head commit's reflog epoch 1791030012 (2026-10-03T12:20:12Z, 08:20:12 at the recorded -0400 offset) and after every label in the feature folder (latest 2026-10-03T08-19). No shell clock was readable in this session. The executor's labels were cross-checked against two independent epochs: the baseline Cobertura root `timestamp="1791027598"` (07:39:58 -0400) sits two minutes before the 07-41 label of evidence/baseline/coverage-baseline.md, and the final Cobertura root `timestamp="1791029515"` (08:11:55 -0400) sits two minutes before the 08-13 label of evidence/qa-gates/coverage-final.md, so the evidence labels are clock-derived.

## Executive Summary

Overall verdict: PASS. 0 Blocking findings. 0 findings of class autonomous, external_dependency, policy_hold, awaiting_ci or human_decision_required that block the pull request. 1 Non-blocking Minor finding (CR-1, a pre-existing untested branch arm that the split now isolates in a new file) and 3 Informational findings, detailed in code-review.2026-10-03T08-50.md. AC1 to AC8 verified PASS against code and evidence; all eight were already checked off by the executor and none is unchecked by this review. Remediation inputs: not produced.

| Area | Verdict | Evidence summary |
|---|---|---|
| General Unit Test Policy | PASS | Four new MSTest tests driven by TaskCompletionSource and the coordinator's own prime marker; no Thread.Sleep, Task.Delay, wall-clock read, temporary file or parallelism attribute (reviewer Grep over the seven fixture partials: 0 hits); 7384/7384 at baseline, 7388/7388 after the change |
| General Code Change Policy | PASS | Failing regression test first (three refusal-path tests Failed at P1-T14 with the fix provably absent, Passed at P1-T24); the split is a pure move (ordinal multiset census: eight structural difference lines only); the fix is one private guard helper plus three call-site rewrites; every file at or under 500 lines (production 302/197/86, largest test partial 481); toolchain single pass |
| C# Code Change Policy | PASS | csharpier check exit 0 (1640 files); analyzer /t:Rebuild 0 errors 0 warnings, CSC_OUT 2/2; TreatWarningsAsErrors /t:Rebuild 0 errors 0 warnings, CSC_OUT 2/2, no /p:Nullable=enable; XML docs on every new and changed member |
| C# Unit Test Policy | PASS | MSTest, Moq, FluentAssertions throughout; first-party lines 85.96%, branches 80.10% (floors 80/75 per CLAUDE.md, 85/75 per .claude/rules, both met); coordinator type 203/203 lines (baseline 177/177), 43/44 branches (baseline 39/40) |
| Coverage (C#) | PASS | Repo-wide 85.96% lines / 80.10% branches at both stages; new methods TryInvokeSink 10/10 and BuildNotifyFailedMessage 8/8 lines; every new refusal-path line hits=1; the four added branches all covered; the single uncovered arm (RenderEngineName null-or-empty) is pre-existing and recorded as CR-1 |
| Evidence hygiene | PASS | 0 host paths, 0 account names and 0 raw trx or coverage documents in the committed feature folder (executor CMD-HYGIENE at P2-T9 and P2-T20: 36 files, all counters 0; reviewer Glob: 36 Markdown files, no other file type) |
| Committed Test Evidence Format | PASS | Both coverage runs committed as the package-level JaCoCo projection plus the one-line first-party summary; the test runs committed as trx-derived summaries; the raw documents stay under the gitignored coverage/ directory (.gitignore lines 146, 147, 150) |

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. The following caller statements were evaluated and accepted as factual, as tooling constraints, or as the work-mode rule rather than as narrowing:

- "Do NOT use the Bash tool at all." A tooling constraint, not a scope constraint. The audit scope remains the full branch diff against origin/main; every changed code file was read in full from the worktree, the caller's verbatim diff was read in full, and the executor's pre-change census was used as the before-text where a comparison was needed.
- "reduced (minor-audit) review" and "AC source: FEATURE/issue.md `## Acceptance Criteria` only". This is the work-mode rule from issue.md line 12 (`- Work Mode: minor-audit`) applied per the acceptance-criteria-tracking skill; it governs the AC source, not the policy scope. Every policy was evaluated over the full diff.
- "Code diff versus base (C# production and test only)" and "Full name-status versus base: 8 code files ... plus only additions under the feature folder ... and docs/features/potential/promoted/...". Confirmed against evidence/qa-gates/footprint-scope.md and the files on disk. Zero PowerShell, TypeScript or Python files changed; no language was declared not applicable by the caller.
- "check off only PASS items (they are already checked; uncheck nothing, but report any you judge not PASS as a finding)". Consistent with the skill's reviewer check-off protocol; this review evaluated every criterion independently.

## Evidence Location Compliance

- Branch diff scan for files under artifacts/baselines/, artifacts/qa/, artifacts/evidence/ or artifacts/coverage/: none. The name-status origin/main..HEAD lists the eight code paths, one promoted record under docs/features/potential/promoted/ and paths under docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/ only.
- All executor evidence lives under docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/{baseline,regression-testing,qa-gates,other}/ (13 + 5 + 12 + 4 Markdown files; Glob listing in this review, 36 files including issue.md and the plan).
- validate_evidence_locations.py --root .: not run (Bash was forbidden for this review; a Glob for the script over the worktree returned nothing, so it is not present in this checkout). The manual scan above substitutes; no violation observed.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required; the caller supplied no non-canonical evidence path.
- PR context artifacts (artifacts/pr_context.summary.txt, artifacts/pr_context.appendix.txt): absent in the review worktree (Glob of the exact path returned nothing). The session checkout carries a pair generated 2026-10-03 07:59:49 UTC for head 78e24a68c of the #968 branch, which is a different item and is therefore not evidence for this review. Regeneration was not possible without a shell or the collection tool. Scope was derived from the caller-supplied name-status and verbatim diff, the committed footprint evidence and the files on disk; the scope is verified by three agreeing sources even though the artifact pair itself could not be produced.
- Raw coverage documents: coverage/baseline-964.cobertura.xml and coverage/final-964.cobertura.xml exist locally in the worktree (gitignored, not committed). Root elements read for this review: baseline line-rate 0.859601, branch-rate 0.801031, lines 56609/65855, branches 13680/17078; final line-rate 0.859565, branch-rate 0.801019, lines 56629/65881, branches 13683/17082. The canonical path artifacts/csharp/coverage.xml is absent in both checkouts; the committed JaCoCo package projection plus one-line summary are the forms CLAUDE.md "Committed Test Evidence Format" requires, and the standing ruling treats executor-committed feature-folder coverage evidence as the present artifact.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every new test constructs its own Harness (SinkGuard.cs lines 34, 55, 91, 129); no static state is read or written; the fixture's existing `[TestClass]` runs under the repository runsettings without any parallelism attribute (Grep for DoNotParallelize over the seven partials: 0) |
| Isolation | PASS | Each refusal-path test targets one guarantee (does not throw; reports once and invokes nothing; survives both sinks throwing); the guard test targets the record-after-sink placement alone; the fail-before run names exactly the three refusal-path tests and no other (evidence/regression-testing/refusal-path-fail-before.md) |
| Fast execution | PASS | Every asynchronous outcome is a TaskCompletionSource completed by the test; no bounded wait, timer or pump; the fixture run of 43 tests completed inside the P1-T24 vstest invocation with no Sequence file |
| Determinism | PASS | The refusal path is synchronous up to its `return`, so `await HandleToggleClickAsync` observes the complete outcome; the guard test resynchronises through `await firstPrime` and `await secondPrime`, both markers completed by `SetResult` in a `finally` (Prime.cs lines 95-102); no clock, random value or filesystem is read (reviewer Grep over the seven partials for Thread.Sleep, Task.Delay, DateTime.Now, DateTime.UtcNow, Stopwatch, Path.GetTemp, Environment.TickCount, .Wait(), .Result, SpinWait: 0 hits) |
| Readability | PASS | Descriptive names stating scenario and expected outcome; XML `<summary>` on every new test stating what fails without the fix; Arrange / Act / Assert markers; a because-reason on every assertion |

### 1.2 Coverage

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 6 | 7388 | 7388 passed, 0 failed | 85.96% lines / 80.10% branches | 85.96% lines / 80.10% branches | 100% lines / 100% branches |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

Files Changed counts .cs files only (six: three production, of which two added, and three test, of which one added); the two project files TaskMaster/TaskMaster.csproj and TaskMaster.Test/TaskMaster.Test.csproj are the remaining code paths. New Code Coverage is measured over the executable lines and branches this item added: TryInvokeSink (10 of 10 lines, EngineToggleStateCoordinator.cs 288-300), BuildNotifyFailedMessage (8 of 8 lines, Messages.cs 51-58), the rewritten refusal path and click-boundary catch body (EngineToggleStateCoordinator.cs 186-201 and 210, every line hits=1 in evidence/qa-gates/coverage-final.md METHOD-LINE rows), and the four branches added to the type (COORD-BRANCHES 39/40 at baseline to 43/44 after the change: +4 valid, +4 covered).

Coverage source statement: the figures above are read from the committed projections and summaries (evidence/baseline/coverage-baseline.md at P0-T14; evidence/qa-gates/coverage-final.md at P2-T5; evidence/qa-gates/coverage-comparison.md at P2-T6), each carrying the first-party summary line, the root counters, the package-level JaCoCo projection and the coordinator class-node rows, and were cross-checked by this review against the root elements and the three coordinator `<class>` nodes of the local raw documents coverage/baseline-964.cobertura.xml (one node, line-rate 1, branch-rate 0.975) and coverage/final-964.cobertura.xml (three nodes: EngineToggleStateCoordinator.cs line-rate 1 branch-rate 1; Prime.cs line-rate 1 branch-rate 1; Messages.cs line-rate 1 branch-rate 0.5). Both runs used the same route, `Invoke-MSTestWithCoverage.ps1`, over nine test assemblies, so they are comparable.

Verdict lines:

- C# coverage verdict: PASS (repo-wide first-party lines 85.96% and branches 80.10% from the committed post-change projection and the raw root element; above the CLAUDE.md floors of 80% lines and 75% branches and above the 85% / 75% floors in .claude/rules; equal to the baseline 85.96% / 80.10% at two decimals).
- C# new-code coverage: PASS. 100% of the added executable lines and 100% of the added branches are covered (18 new method lines, the 13 rewritten refusal-path and catch-body lines, and 4 added branches, all hits=1 or covered in the final document).
- C# changed-production-file coverage: PASS on the no-regression limb and on the file-level line floor. EngineToggleStateCoordinator.cs 89/89 lines (was 177/177 as the single file), Prime.cs 72/72, Messages.cs 42/42; the type's branch count rose from 39/40 to 43/44 and the single uncovered arm is the same pre-existing arm at both stages (RenderEngineName's null-or-empty branch, now the only branch in Messages.cs, whose node therefore reads branch-rate 0.5). Nothing regressed; the pre-existing arm is recorded as non-blocking finding CR-1 with a one-test remedy.
- C# package-level corroboration: the TaskMaster package counters moved from 2477 to 2503 covered lines with 802 missed at both stages (+26 covered of +26 added) and from 519 to 523 covered branches with 211 missed at both stages (+4 of +4). The UtilitiesCS package moved by -6 covered lines and -1 covered branch with no UtilitiesCS file changed on this branch; this is run-to-run variance in code outside the Write Set (the executor recorded the same observation) and is within the band observed on earlier reviews.
- PowerShell coverage gate: PASS by vacuity (zero PowerShell files changed on this branch, so the changed-line no-regression requirement has no line to evaluate; no PoshQC format, analyze or test gate was owed or run; artifacts/pester/powershell-coverage.xml was not consulted).
- TypeScript and Python: zero files changed on this branch; no verdict is owed.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/baseline/coverage-baseline.md` (committed one-line first-party summary, root counters, JaCoCo package projection and coordinator class-node rows; raw document coverage/baseline-964.cobertura.xml present locally, gitignored; canonical artifacts/csharp/coverage.xml absent in the worktree)
- C# post-change coverage artifact: `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/qa-gates/coverage-final.md` with `evidence/qa-gates/coverage-comparison.md` (same forms; raw document coverage/final-964.cobertura.xml present locally, gitignored; canonical artifacts/csharp/coverage.xml absent in the worktree)
- TypeScript baseline coverage artifact: none consulted (zero TypeScript files changed on this branch)
- TypeScript post-change coverage artifact: none consulted (zero TypeScript files changed on this branch)
- PowerShell baseline coverage artifact: none consulted (zero PowerShell files changed on this branch)
- PowerShell post-change coverage artifact: none consulted (zero PowerShell files changed on this branch)
- Python baseline coverage artifact: none consulted (zero Python files changed on this branch)
- Python post-change coverage artifact: none consulted (zero Python files changed on this branch)
- Per-language comparison summary: the per-language comparison block of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.96% lines (56609/65855) / 80.10% branches (13680/17078). Post-change: 85.96% lines (56629/65881) / 80.10% branches (13683/17082). Change: 0.00% lines / 0.00% branches at two decimals (+20 covered of +26 valid lines, +3 covered of +4 valid branches; at full precision line-rate 0.859601 to 0.859565 and branch-rate 0.801031 to 0.801019, the whole of that movement being the UtilitiesCS run-to-run variance described above while the TaskMaster package gained 26 covered of 26 added lines and 4 covered of 4 added branches). New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/coverage-baseline.md, evidence/qa-gates/coverage-final.md, evidence/qa-gates/coverage-comparison.md, root elements and coordinator class nodes of coverage/baseline-964.cobertura.xml and coverage/final-964.cobertura.xml.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Artifact consulted | State | Disposition |
|---|---|---|---|
| C# | Committed projections, summaries and class-node rows under evidence/baseline and evidence/qa-gates; raw Cobertura root elements and class nodes read locally | Present; canonical artifacts/csharp/coverage.xml absent in the worktree (recorded as observation O-3, recurring) | PASS |
| TypeScript | none | zero files changed | no verdict owed |
| PowerShell | none | zero files changed | no verdict owed |
| Python | none | zero files changed | no verdict owed |

Coverage exclusion policy check (.claude/rules/general-unit-test.md): the branch adds no coverage-config exclude entry and no ExcludeFromCodeCoverage attribute (Grep over the three production files: the only occurrence is the pre-existing class remark stating the type is deliberately NOT excluded, EngineToggleStateCoordinator.cs line 34). Not Blocking.

### 1.3 Scenario completeness

| Scenario | Verdict | Evidence |
|---|---|---|
| Positive flows | PASS | Refused click with a healthy notification sink still notifies once and invokes nothing (pre-existing HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing, Passed); healthy prime and toggle paths unchanged and Passed (43/43) |
| Negative flows | PASS | Throwing notification sink (AC1); throwing notification sink reported once through the log sink with the same exception instance, no engine member invoked, no invalidation (AC2); both sinks throwing (AC2); throwing log sink on a faulted prime leaves the report owed so the repeat is reported (SinkGuard guard test) |
| Edge cases | PASS | The record-placement guard test exercises the sink-throws-on-first-report, returns-on-second sequence through two primes of one fault kind; the pre-existing #948 repeat-suppression and #947 throwing-sink partials are byte-identical to base and Passed |
| Error handling | PASS | Every sink call site contained by TryInvokeSink; the click boundary still reports the toggle fault unchanged (HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate, Passed); ExecuteToggleAsync still propagates (ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged, Passed) |
| Concurrency | PASS | The split preserves the lock-scoped registration and the keyed TryRemove as the last statement of CompletePrime (Prime.cs 49-65 and 194); the Race partial is unchanged except one remark; all eleven invariant-named tests Passed before and after the fix |
| State transitions | PASS | Marker registered -> prime -> report attempted -> (recorded only on a normal sink return) -> marker cleared; observed by the guard test (secondPrime distinct from firstPrime, Errors count 2) and by the unchanged PrimeFaultOrdering and PrimeRegistration partials |

### 1.4 Arrange-Act-Assert

PASS. Every new test carries Arrange / Act / Assert markers (SinkGuard.cs lines 33, 37, 40, 54, 59, 62, 90, 96, 99, 128, 148, 156); every assertion carries a because-reason string; the `Func<Task> act` under Act with the awaited `NotThrowAsync` under Assert mirrors the existing fixture test at EngineToggleStateCoordinatorTests.cs lines 342-346.

### 1.5 External dependencies and temporary files

PASS. No Outlook COM, file system, network or process is touched by any changed test; the engines are a strict Moq mock and the three sinks are recording delegates. Reviewer Grep over the seven fixture partials for Path.GetTemp, System.IO member use in the changed files, Thread.Sleep and Task.Delay: 0 hits in the changed files (the unchanged RepeatFaultSuppression partial imports System.IO for an IOException instance used as a second fault kind, which is not I/O).

### 1.6 Test file location

PASS (repository convention). The new partial sits at TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs beside the six existing partials of the same fixture, mirroring the per-project *.Test layout used for every C# project in this repository; no test file was colocated with production source.

### 1.7 Determinism infrastructure

PASS. No time is consumed by any new test; no randomness is used; banned APIs in test code: Thread.Sleep 0, Task.Delay 0, Date/clock reads 0 over the seven fixture partials (reviewer Grep). The coordinator type reads no clock.

## 2. General Code Change Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| Before making changes (plan, AC source) | PASS | issue.md carries the explicit `## Acceptance Criteria` section (AC1 to AC8) and the minor-audit marker; plan.2026-10-02T05-20.md exists with every task checked (reviewer Grep for `- [ ]`: 0); preflight clearance recorded at evidence/other/preflight-clearance.2026-10-02T07-50.md |
| Bugfix workflow step 1 (failing regression test first) | PASS | evidence/regression-testing/refusal-path-fail-before.md: EXIT_CODE 1 with ExpectedExitCode 1; the stripped census taken immediately before the run shows `TryInvokeSink(` 0 and `_notifyUnavailable(` 1 unguarded, so the fix was absent; the three refusal-path tests Failed with messages naming the escaping `notify sink failed` exception and the stack naming the unguarded call at the pre-fix line 178; the guard test and all eleven invariant tests Passed at base. Pass-after (refusal-path-pass-after.md): 43/43 |
| Bugfix workflow step 2 (minimal targeted fix) | PASS | One private static helper TryInvokeSink (14 lines), three call-site rewrites, one new message builder, docs; the file split is a pure move (evidence/regression-testing/split-census.md: exactly the eight admitted structural difference lines; fourteen protected signatures and field declarations SPAN-HASH equal=True in evidence/qa-gates/production-edit-scope.md) |
| Bugfix workflow step 3 (verify locally, toolchain in order) | PASS | evidence/qa-gates/toolchain-final-pass.md: pass 1 clean for format, analyzer rebuild, TreatWarningsAsErrors rebuild and the Invoke-MSTestWithCoverage.ps1 route (7388/7388) |
| Design principles (simplicity, reusability, extensibility, separation) | PASS | The duplicated guard blocks of #947 are replaced by one helper that returns the outcome and the exception, so each caller decides the follow-up (forward to the log sink, discard, or record) without a second catch; message builders isolated in their own partial; prime lifecycle isolated in its own partial |
| Classes, functions, APIs | PASS | Internal surface unchanged (constructor, GetPressed, HandleToggleClickAsync, ExecuteToggleAsync, GetPrimeTask signatures byte-equal per SPAN-HASH); the helper is private static with documented contract (sinkCall, sinkFailure, return) |
| Error handling | PASS | Two `catch` clauses remain in the type: the click boundary (EngineToggleStateCoordinator.cs 208) and the sink guard (295); no empty catch remains (the #947 `catch (Exception)` with a comment-only body is gone, census `catch(Exception)` 0); the sink guard is the documented boundary catch accepted at #947 (X-1 below) |
| Logging | PASS | No new logging channel; a notification failure is forwarded to the injected log sink with a dedicated message (BuildNotifyFailedMessage) carrying the engine key |
| File size limit (500 lines) | PASS | Reviewer Read line counts: EngineToggleStateCoordinator.cs 302, Prime.cs 197, Messages.cs 86, EngineToggleStateCoordinatorTests.cs 481, Race.cs 277, SinkGuard.cs 169; all agree with evidence/qa-gates/file-line-counts.md; the four unchanged partials 77, 175, 290, 215 |
| Naming | PASS | PascalCase types and members (TryInvokeSink, BuildNotifyFailedMessage, OnNotify); camelCase locals (notifyFailure, sinkFailure, firstProbe); test names state scenario and outcome |
| Public APIs and compatibility | PASS | No public API; every internal signature unchanged; two Compile items added per project file in the existing Ribbon group |
| Dependencies | PASS | None added |
| I/O boundaries | PASS | No I/O introduced; the type remains host-neutral (class remark lines 33-41 still accurate) |

## 3. Language-Specific Code Change Policy Compliance

Language in scope: C# only.

| Item | Verdict | Evidence |
|---|---|---|
| Formatting (csharpier via dotnet tool run) | PASS | evidence/qa-gates/csharpier-format.md (Formatted 1640 files, Write Set hashes unchanged) and csharpier-check-final.md (Checked 1640 files, exit 0, no path listed) |
| Linting (analyzer rebuild, /t:Rebuild, EnableNETAnalyzers, EnforceCodeStyleInBuild) | PASS | evidence/qa-gates/msbuild-analyzer-final.md: exit 0, ERRORS 0, WARNINGS 0, CSC_OUT 2/2, WRITESET_DIAGNOSTIC_LINES 0 |
| Type checking (TreatWarningsAsErrors rebuild, no /p:Nullable=enable) | PASS | evidence/qa-gates/msbuild-nullable-final.md: exit 0, 0 errors, 0 warnings, CSC_OUT 2/2; command text matches CLAUDE.md character for character |
| Nullable annotations | PASS | No changed file carries #nullable enable (reviewer Grep: 0 in the three production files); no directive added or removed; the `out Exception sinkFailure` assigned null is legal under the files' pre-existing nullable-disabled state (observation O-4 notes the annotation to use if the files ever opt in) |
| Partial-class split | PASS | All three files declare `internal sealed partial class EngineToggleStateCoordinator` in namespace TaskMaster (census 3 of 3); `using` sets trimmed to what each partial needs (System.Globalization and System.Threading moved with their consumers) |
| XML docs on non-obvious contract | PASS | TryInvokeSink summary, both params, returns and remarks; GetPrimeTask summary and returns describe the registration marker; constructor parameter docs state the accessor's non-throwing precondition and both sinks' guarded behaviour; every comment that counts the type's catch clauses matches the two that exist |
| Internal surface | PASS | New helper private static; new message builder private static; new harness member internal on the private Harness |
| Analyzer suppressions | PASS | None added (Grep over the six .cs files for #pragma, SuppressMessage: 0 in changed lines) |

## 4. Language-Specific Unit Test Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | [TestMethod] on all four new tests inside the existing [TestClass] partial fixture; MSTest namespace imported once |
| Moq for mocks | PASS | The strict Mock<IAppItemEngines> from the shared Harness; SetupSequence for the two faulted primes; VerifyNoOtherCalls in the invokes-nothing test |
| FluentAssertions | PASS | All assertions use Should() (NotThrowAsync, ContainSingle, BeSameAs, Contain, BeEmpty, NotBeSameAs, HaveCount); no MSTest Assert introduced |
| Repo-wide coverage floors | PASS | 85.96% lines (floor 80% per CLAUDE.md, 85% per rules), 80.10% branches (floor 75%) |
| New module/class/method >= 90% | PASS | TryInvokeSink 100% (10/10 lines, 2/2 branches), BuildNotifyFailedMessage 100% (8/8 lines, no branch) |
| No regression on changed lines | PASS | Every rewritten refusal-path and catch-body line hits=1; coordinator type 203/203 lines; branches 43/44 with the single uncovered arm identical to baseline's |
| Prohibited behaviors (sleeps, retries, timing hacks, weakened assertions) | PASS | 0 banned-API hits; the primary fixture change adds an `OnNotify` hook after the record (lines 414-418) and weakens nothing; the Race partial change is remark-only (NON-DOC-CHANGES 0); the four other partials byte-identical to base |
| Test toolchain route | PASS | Step 4 ran the CLAUDE.md route `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1` at both stages (RAW: False; DOCUMENT_PRESENT, TRX_PRESENT, SUMMARY_FILE_PRESENT all True) |

## 5. Test Coverage Detail

| File | Change type | Coverage observation | Disposition |
|---|---|---|---|
| TaskMaster/Ribbon/EngineToggleStateCoordinator.cs | Modified production (496 -> 302 lines after the move; fix applied here) | Class node line-rate 1 (89/89), branch-rate 1; HandleToggleClickAsync 24/24 lines (span 184-212), TryInvokeSink 10/10 (span 287-300), every refusal-path line 186-201 hits=1 | PASS |
| TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs | Added production (pure move of GetPrimeTask, StartPrimeIfNeeded, StartObservedPrime, ApplyPrimeAsync, CompletePrime; CompletePrime's sink call rewritten) | Class node line-rate 1 (72/72), branch-rate 1; CompletePrime 22/22 lines (span 165-195) including the record line 190 and the TryRemove line 194 | PASS |
| TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs | Added production (pure move of the token and five builders; BuildNotifyFailedMessage added) | Class node line-rate 1 (42/42); BuildNotifyFailedMessage 8/8 (span 50-58); branch-rate 0.5 because the file's only branch, RenderEngineName's null-or-empty ternary (line 20), has a never-exercised true arm that was also the single uncovered branch of the pre-split file (baseline node 39/40) | PASS on lines and no-regression; the pre-existing arm is CR-1 (non-blocking) |
| TaskMaster/TaskMaster.csproj | Modified project file (+2 Compile items, lines 467-468) | Not a source file | Discovery proven: three class nodes in the final document, one per file |
| TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs | Added test (169 lines) | Outside the denominator by policy | Not measured; 4/4 Passed, 3 fail-before captured |
| TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs | Modified test fixture (+12 / -1: OnNotify hook) | Outside the denominator by policy | Not measured; 43/43 Passed |
| TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs | Modified test (remark only, +4 / -4) | Outside the denominator by policy | Not measured; NON-DOC-CHANGES 0 |
| TaskMaster.Test/TaskMaster.Test.csproj | Modified project file (+1 Compile item, line 364) | Not a source file | Discovery proven: all four new tests appear in the P1-T24 and P2-T5 runs |

Package-level projection (TaskMaster package): LINE missed 802 / covered 2503; BRANCH missed 211 / covered 523 after the change (evidence/qa-gates/coverage-final.md), from 802 / 2477 and 211 / 519 at baseline.

## 6. Test Execution Metrics

| Run | Scope | Total | Passed | Failed | Source |
|---|---|---|---|---|---|
| Baseline (P0-T14, Invoke-MSTestWithCoverage.ps1) | nine test assemblies | 7384 | 7384 | 0 | evidence/baseline/coverage-baseline.md |
| Final (P2-T5, Invoke-MSTestWithCoverage.ps1) | nine test assemblies | 7388 | 7388 | 0 | evidence/qa-gates/coverage-final.md |
| Fixture baseline (P0-T13) | EngineToggleStateCoordinatorTests | 39 | 39 | 0 | evidence/baseline/coordinator-tests-baseline.md |
| Split fixture green (P1-T8, after the pure move) | EngineToggleStateCoordinatorTests | 39 | 39 | 0 | evidence/regression-testing/split-fixture-green.md |
| Refusal-path fail-before (P1-T14, fix absent) | EngineToggleStateCoordinatorTests | 43 | 40 | 3 (expected) | evidence/regression-testing/refusal-path-fail-before.md |
| Refusal-path pass-after (P1-T24) | EngineToggleStateCoordinatorTests | 43 | 43 | 0 | evidence/regression-testing/refusal-path-pass-after.md |

Figures compared: final total equals baseline plus 4 (the three refusal-path tests and the guard test); error, timeout, aborted and notExecuted each 0 at both stages (summaries derived from the trx); no Sequence file in any run; FINAL-FAILED-FQN-COUNT 0.

## 7. Code Quality Checks

| Check | Command or method | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | Executor CMD-HYGIENE at P2-T9 and P2-T20 (ACCOUNT_HITS, MACHINE_HITS, DRIVE_USERS_HITS over 33 then 36 files); reviewer Read of every evidence file cited in this audit | 0 / 0 / 0; the one absolute path in a trx failure message is transcribed as REDACTED-PATH | PASS |
| Raw document scan | Glob over the feature folder for every file; .gitignore lines 146, 147, 150 | 36 Markdown files, no trx, Cobertura or log; raw documents gitignored under coverage/ | PASS |
| Suppression scan (added lines) | Read of the six .cs files and the caller's verbatim diff | No new #pragma, [SuppressMessage], [ExcludeFromCodeCoverage] or analyzer suppression | PASS |
| Workflow change scan | Caller name-status and footprint-scope.md | No .github/, scripts/ or runsettings path changed; project-file changes are Compile items only | PASS |
| Prohibited-construct scan | Reviewer Grep over the seven fixture partials | Thread.Sleep 0, Task.Delay 0, DateTime.Now/UtcNow 0, Stopwatch 0, SpinWait 0, .Wait() 0, .Result 0, Path.GetTemp 0, DoNotParallelize 0 | PASS |
| Catch-clause census | Reviewer Grep for `catch` over the three production files | Two code occurrences: EngineToggleStateCoordinator.cs 208 (click boundary) and 295 (sink guard); every other hit is a documentation mention; Prime.cs and Messages.cs contain no catch | PASS |
| Sink call-site census | Reviewer Read of the three production files | `_logError(` three times, each inside a TryInvokeSink lambda (EngineToggleStateCoordinator.cs 196, 210; Prime.cs 185); `_notifyUnavailable(` once, inside a TryInvokeSink lambda (190); agrees with the executor census (`TryInvokeSink(` 5 = one definition + four call sites) | PASS |
| Comment-drift census (AC7) | Reviewer Grep for the nine retired phrases over the three production files; Read of each surviving phrase | Retired phrases 0 hits; every surviving phrase present at the documented location (several wrap across two source lines, so a line-oriented Grep undercounts them; confirmed by reading) | PASS |
| Tonality scan | Read of issue.md, the plan's evidence citations and the committed evidence | Neutral, factual wording; no humor, hyperbole or metaphor | PASS |

## 8. Gaps and Exceptions

- X-1 (accepted exception, carried from #947): TryInvokeSink catches Exception broadly without re-raising. Under CLAUDE.md C#4 this is a boundary catch: the enclosed statement is an injected sink that is the type's last reporting channel (or, for the notification sink, whose failure is forwarded to that last channel), the method remarks document the discard and the forwarding, and the RibbonCommandBoundary.ReportFailure / SafeLog precedent (RibbonCommandBoundary.cs lines 80-113) has the same shape. The General Code Change Policy's letter ("re-raise or propagate") is not met; the alternative is the defect itself. Not Blocking.
- CR-1 (non-blocking Minor, related, autonomous): RenderEngineName's null-or-empty arm is unexercised (pre-existing; now the only branch in the new Messages partial, so that node reads 50% branches while the type reads 97.73%). One refusal-path test with a null engine key closes it; see code-review.2026-10-03T08-50.md.
- PR context artifact pair absent in the review worktree: scope verified from three agreeing sources instead (section Evidence Location Compliance).
- Canonical C# coverage artifact path absent: committed projections and the local raw documents used, per the standing ruling (observation O-3, recurring across #947, #948, #950, #968).
- The P1-T22 phrase census was run by the coordinator under a maintainer one-time bypass rather than by the executor (recorded with provenance in evidence/qa-gates/production-edit-scope.md); this review re-derived the census by Grep and Read and found every value as recorded.
- quality-tiers.yml absent at the repository root (Glob: none; pre-existing; tier gates unevaluable; already promoted by the #956 review). Not attributable to this item.

## 9. Summary of Changes

- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs: class made partial; message builders and prime lifecycle moved out; TryInvokeSink added; the refusal path routes the notification through the guard and forwards a notification failure to the log sink through a second guarded call; the click-boundary catch body routes its log call through the guard; constructor, GetPressed, HandleToggleClickAsync and the catch-inventory comments rewritten to match.
- TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs (new): GetPrimeTask with the corrected registration-marker documentation; StartPrimeIfNeeded, StartObservedPrime and ApplyPrimeAsync moved verbatim; CompletePrime records a reported fault kind only in the branch taken when TryInvokeSink returns true, with the #948 placement documented.
- TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs (new): NullEngineNameToken, RenderEngineName and the five builders, with BuildNotifyFailedMessage added.
- TaskMaster/TaskMaster.csproj: two Compile items.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs (new): three refusal-path regression tests (fail-before recorded) and one record-placement guard test.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs: OnNotify hook on the Harness, invoked after the notification is recorded.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs: one remark corrected for the #948 suppression rule.
- TaskMaster.Test/TaskMaster.Test.csproj: one Compile item.
- Evidence: 34 Markdown files under the feature folder's evidence tree plus issue.md check-offs and the checked plan; no raw document committed.

## 10. Compliance Verdict

PASS. Every policy area evaluates PASS over the full branch diff. Zero blocking findings; nothing requires a remediation cycle before the pull request is opened. One non-blocking Minor finding (CR-1) and three Informational findings are recorded in the code review for the coordinator's related-defect disposition. The pull request body should carry `Closes #964`.

## Appendix A: Test Inventory

| Test class | Test | Status after change | AC |
|---|---|---|---|
| EngineToggleStateCoordinatorTests (SinkGuard partial) | HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow | Failed before the fix (fix absent), Passed after | AC1 |
| EngineToggleStateCoordinatorTests (SinkGuard partial) | HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing | Failed before the fix, Passed after | AC2 |
| EngineToggleStateCoordinatorTests (SinkGuard partial) | HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow | Failed before the fix, Passed after | AC2 |
| EngineToggleStateCoordinatorTests (SinkGuard partial) | GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain | Passed before and after (guard) | AC3, AC4 |
| EngineToggleStateCoordinatorTests (ThrowingSink partial, unchanged) | GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime, GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime, GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared, HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport | Passed, unchanged | AC3, AC4 |
| EngineToggleStateCoordinatorTests (PrimeFaultOrdering, PrimeRegistration, RepeatFaultSuppression partials, unchanged) | seven invariant tests | Passed, unchanged | AC4 |
| EngineToggleStateCoordinatorTests (Race partial, remark only) | GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker and siblings | Passed, unchanged behaviour | AC4 |
| EngineToggleStateCoordinatorTests (primary partial) | HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing and the other pre-existing tests | Passed (OnNotify hook inert when unset) | AC2, AC4 |

Test method count: 7384 at baseline, 7388 after the change (four added, none removed); fixture 39 to 43.

## Appendix B: Toolchain Commands Reference

| Step | Command (as recorded in evidence/qa-gates/toolchain-final-pass.md) | Exit | Iteration |
|---|---|---|---|
| 1 | dotnet tool run csharpier format . | 0 | 1 |
| 1b | dotnet tool run csharpier check . | 0 | 1 |
| 2 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | 0 | 1 |
| 3 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | 0 | 1 |
| 4 | pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 (the CLAUDE.md route; 7388/7388; LINE-FLOOR MET, BRANCH-FLOOR MET) | 0 | 1 |

PowerShell gates (PoshQC MCP format / analyze / test): not run; zero PowerShell files changed on this branch.

Reviewer commands: none (Bash forbidden). All verification by Read, Grep and Glob against the item worktree, the caller's verbatim diff, the gitignored raw Cobertura documents and the worktree reflog.
