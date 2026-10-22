# Policy Audit: engine-toggle-coordinator-947-review-residuals (Issue #964), remediation cycle 1 exit

- Timestamp: 2026-10-03T09-50 (caller-supplied label, chosen by the coordinator to sort after every artifact already in the folder; it is not a host-clock reading by this reviewer, see "Timestamp derivation" below)
- Branch: bug/engine-toggle-coordinator-947-review-residuals-964
- Head: 01dcbe119b74fecb949452397b18883b9731a242 (worktree reflog, last entry: `docs(964): record remediation cycle 1 post-commit check-offs P2-T14 to P2-T16`, epoch 1791034633)
- Base: origin/main 993fdd01566dee82e5f37acb761a600feaaa1454, merged into the item at 981abef77657adcc90d7c116a6b4c6500b79ea29 (reflog entry `merge origin/main`, epoch 1790993935). origin/main is an ancestor of the head, so the item change set is the two-dot diff origin/main..HEAD, which the caller supplied verbatim (scratchpad file `964-code-r1.diff`, read in full) and which agrees with the files on disk and with evidence/qa-gates/footprint-scope.md plus evidence/qa-gates/cycle1-footprint.md.
- Review kind: re-audit at the exit of remediation cycle 1 over the WHOLE branch diff (the same scope as the review labelled 2026-10-03T08-50). Cycle 1 changed one code file, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs (+41 / -0, commit b27cf3bd24b6cf8c5ce9275488bb143e36a61d33, epoch 1791034362), plus the remediation plan and 30 evidence files; no production file, project file, other test file or acceptance criterion changed in the cycle (cycle1-footprint.md listing 3: exactly one path; negative control non-empty).
- Work mode: minor-audit (issue.md line 12); acceptance-criteria source: the `## Acceptance Criteria` section of issue.md only (AC1 to AC8, lines 27 to 34)
- Reviewer: feature-review, no-Bash mode (caller directive). Every check was performed with Read, Grep and Glob against the item worktree, the committed evidence under evidence/, the caller-supplied verbatim diff, the gitignored raw Cobertura documents at their local paths (coverage/final-964.cobertura.xml as the pre-cycle control, coverage/remediation-964.cobertura.xml as the post-cycle document) and the worktree reflog as head reference and clock.
- Timestamp derivation: the reflog gives the only clock readable in this session. Converting its epochs at the recorded -0400 offset: cycle opened at 6b8e935c1 (epoch 1791031450, 08:44:10); cycle commit b27cf3bd2 (1791034362, 09:32:42); head 01dcbe119 (1791034633, 09:37:13). The remediation Cobertura root reads `timestamp="1791034004"` (09:26:44), three minutes before the 09-30 label of evidence/qa-gates/cycle1-coverage.md, and the commit-record and hygiene labels (09-32) match the cycle commit minute. The caller-supplied label 09-50 is later than every label in the folder and every reflog epoch; whether it is at or before the host clock at the moment of this write could not be checked without a shell and is recorded as such. The review labelled 08-50 was likewise caller-supplied; see finding P-1 for the executor-side label issue.

## Executive Summary

Overall verdict: PASS. 0 Blocking findings. 0 findings of class autonomous, external_dependency, policy_hold, awaiting_ci or human_decision_required that block the pull request. Remediation inputs: not produced. Both cycle-1 findings are closed on code and evidence: R-1 (CR-1, the untested null-or-empty arm of `RenderEngineName`) is closed by the data-driven test at SinkGuard.cs 127-153, with the Messages partial's Cobertura class node now reading `branch-rate="1"` (remediation document line 230924) against `0.5` in the pre-cycle control, and the coordinator type at 44/44 branches; R-2 (CR-4, the two missing symmetry assertions) is closed at SinkGuard.cs 112-113. Nothing regressed: AC1 to AC8 re-evaluated PASS, the toolchain passed in one clean pass inside the cycle, the suite reads 7390/7390 (7388 + the two data rows), the coordinator fixture 45/45, and the four ordering invariants are unchanged because no production line changed. Two new non-blocking findings are recorded: P-1 (Minor, evidence provenance: fourteen cycle artifacts first carried composed `Timestamp:` labels and were re-stamped to a single host-clock reading with an in-field note) and CR-5 (Informational, the SinkGuard partial's type-level summary does not mention its new region). CR-2 and CR-3 remain Informational with a "No change" recommendation.

| Area | Verdict | Evidence summary |
|---|---|---|
| General Unit Test Policy | PASS | One new data-driven MSTest test (two rows) and two appended FluentAssertions statements; no Thread.Sleep, Task.Delay, wall-clock read, temporary file or parallelism attribute (reviewer Grep over the seven fixture partials: 0 hits); fixture 43/43 at the cycle base, 45/45 after; suite 7388 -> 7390, 0 failed |
| General Code Change Policy | PASS | Test-only remediation of two review findings, 41 added and 0 deleted lines in one file; every file at or under 500 lines (production 302/197/86, test partials 481/277/210/290/215/175/77); toolchain single pass inside the cycle; cycle footprint exactly one code path |
| C# Code Change Policy | PASS | csharpier check exit 0 (1640 files); analyzer /t:Rebuild 0 errors 0 warnings, CSC_OUT 2/2; TreatWarningsAsErrors /t:Rebuild 0 errors 0 warnings, CSC_OUT 2/2, no /p:Nullable=enable; no production change in the cycle, so the first-cycle production verdicts stand |
| C# Unit Test Policy | PASS | MSTest, Moq, FluentAssertions throughout; first-party lines 85.97%, branches 80.13% (floors 80/75 per CLAUDE.md, 85/75 per .claude/rules, both met); coordinator type 203/203 lines, 44/44 branches (was 43/44) |
| Coverage (C#) | PASS | Repo-wide 85.97% lines / 80.13% branches after the cycle (85.96% / 80.10% before); every coordinator class node line-rate 1 and branch-rate 1; new production code of the item (TryInvokeSink, BuildNotifyFailedMessage, rewritten refusal path) 100% lines and branches; no production code added in the cycle |
| Evidence hygiene | PASS | Executor CMD-HYGIENE at P2-T13: 70 files, 0 account, 0 machine, 0 drive-path hits, 0 raw documents; reviewer Grep over the feature folder for drive-letter paths and the account name: only URLs (issue URL, XML namespace URIs, SDK download URL) match; Glob: Markdown only |
| Committed Test Evidence Format | PASS | Cycle coverage run committed as the JaCoCo package projection plus the one-line first-party summary; cycle test runs committed as trx-derived summaries; raw documents stay under the gitignored coverage/ directory (.gitignore lines 146, 147, 150) |
| Evidence timestamps | PASS with finding P-1 (non-blocking) | Fourteen cycle artifacts carry `Timestamp: 2026-10-03T09-23 (host clock read at correction; the label first written was composed, not read)`; the remaining cycle labels (09-17, 09-24, 09-25, 09-30, 09-31, 09-32) are monotone, bracketed by the cycle-open and cycle-commit epochs, and the 09-30 coverage label sits three minutes after the Cobertura root epoch |

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. The following caller statements were evaluated and accepted as factual, as tooling constraints, as the work-mode rule or as verification focus rather than as narrowing:

- "Do NOT use the Bash tool at all." A tooling constraint, not a scope constraint. The audit scope remains the full branch diff against origin/main; the caller's verbatim full diff was read, every changed code file was read or re-read on disk, and the cycle's single changed file was read in full.
- "re-audit (remediation cycle 1 exit) of the WHOLE feature branch for issue 964, with the same inputs and scope as the original reduced review (no scope narrowing)." Consistent with the scope invariant; the "reduced" qualifier is the minor-audit work-mode rule from issue.md line 12, which governs the AC source, not the policy scope.
- "Verify: R-1 (CR-1) and R-2 (CR-4) are closed in code and evidence ...; every finding of the prior review is either closed or still correctly non-blocking; nothing regressed (AC1..AC8 ...)". Verification focus added on top of the full audit, not a restriction of it; every policy area below was evaluated over the full diff.
- "The cycle changed only TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs (+41/-0) in code." A factual statement, confirmed against the caller's diff, cycle1-footprint.md and cycle1-sinkguard-diff.md (NUMSTAT 41 0); it was not used to limit which files were audited.
- "Write nothing outside FEATURE (your own agent-memory note excepted)." An output constraint consistent with the Required Outputs.

## Evidence Location Compliance

- Branch diff scan for files under artifacts/baselines/, artifacts/qa/, artifacts/evidence/ or artifacts/coverage/: none. The origin/main..HEAD change set consists of the eight code paths, one promoted record under docs/features/potential/promoted/ and paths under docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/ only (caller diff; footprint-scope.md; cycle1-footprint.md).
- All executor evidence lives under docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/{baseline,regression-testing,qa-gates,other,remediation-baseline}/. Cycle 1 added 9 remediation-baseline, 8 regression-testing (including the fail-before exception dossier), 10 qa-gates and 3 other files; reviewer Glob of the feature folder: 71 Markdown files, no other file type.
- validate_evidence_locations.py --root .: not run (Bash was forbidden for this review; Glob for the script over the worktree returned nothing, so it is not present in this checkout). The manual scan above substitutes; no violation observed.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required; the caller supplied no non-canonical evidence path. The remediation plan records the same (`EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied`).
- PR context artifacts (artifacts/pr_context.summary.txt, artifacts/pr_context.appendix.txt): absent in the review worktree (Glob of the exact path returned nothing). The session checkout carries a pair generated 2026-10-03 07:59:49 UTC for head 78e24a68c of the #968 branch, which is a different item and is not evidence for this review. Regeneration was not possible without a shell or the collection tool. Scope was derived from the caller-supplied verbatim diff, the committed footprint evidence of both cycles and the files on disk, three agreeing sources.
- Raw coverage documents: coverage/final-964.cobertura.xml (pre-cycle control; root line-rate 0.859565, branch-rate 0.801019, lines 56629/65881, branches 13683/17082, epoch 1791029515) and coverage/remediation-964.cobertura.xml (post-cycle; root line-rate 0.859717, branch-rate 0.801253, lines 56639/65881, branches 13687/17082, epoch 1791034004) exist locally in the worktree, gitignored, not committed. The canonical path artifacts/csharp/coverage.xml is absent in both checkouts (observation O-3, recurring); the committed JaCoCo package projection plus one-line summary are the forms CLAUDE.md "Committed Test Evidence Format" requires, and the standing ruling treats executor-committed feature-folder coverage evidence as the present artifact.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The new test constructs its own Harness per row (SinkGuard.cs 135); no static state is read or written; no parallelism attribute anywhere in the seven partials (reviewer Grep for DoNotParallelize: 0); the two appended assertions read only the test's own harness |
| Isolation | PASS | The new test targets one behaviour, the refusal path with an unusable key, and asserts the five facts the finding named (no throw, one notice containing the token, no error, no engine member, no invalidation); the appended assertions pin two facts the sibling test already pinned for the same arrange |
| Fast execution | PASS | The refusal path is synchronous up to its `return`; no bounded wait, timer or pump; the fixture run of 45 tests completed inside the P1-T6 vstest invocation with no Sequence file |
| Determinism | PASS | No clock, random value or filesystem is read; reviewer Grep over the seven partials for Thread.Sleep, Task.Delay, DateTime.Now, DateTime.UtcNow, Stopwatch, SpinWait, .Wait(), .Result, Path.GetTemp, Environment.TickCount, File., Directory.: 0 hits; the data rows null and "" are constants |
| Readability | PASS | Descriptive name stating scenario and outcome (`HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing`); XML `<summary>` on the test stating what it exercises; Arrange / Act / Assert markers (134, 137, 140); a because-reason on every assertion (142, 145, 149, 150, 152) |

### 1.2 Coverage

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 6 | 7390 | 7390 passed, 0 failed | 85.96% lines / 80.10% branches | 85.97% lines / 80.13% branches | 100% lines / 100% branches |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

Files Changed counts .cs files only over the whole branch (six: three production, of which two added, and three test, of which one added); the two project files TaskMaster/TaskMaster.csproj and TaskMaster.Test/TaskMaster.Test.csproj are the remaining code paths. Baseline Coverage is the item's original baseline on the pre-change tree (evidence/baseline/coverage-baseline.md, P0-T14 of the first cycle: 56609/65855 lines, 13680/17078 branches). Post-Change Coverage is the cycle-1 final run (evidence/qa-gates/cycle1-coverage.md, P2-T5: 56639/65881 lines, 13687/17082 branches). New Code Coverage is measured over the executable lines and branches this item added to production: TryInvokeSink (10 of 10 lines, EngineToggleStateCoordinator.cs 288-300), BuildNotifyFailedMessage (8 of 8 lines, Messages.cs 51-58), the rewritten refusal path and click-boundary catch body (186-201 and 210), and the four branches added to the type; the cycle added no production line or branch (cycle1-coverage-comparison.md: `NEW-CODE-COVERAGE: not applicable, no production line or branch was added`), so the item-level figure stands and the one arm the item had left uncovered is now covered too.

Coverage source statement: the figures above are read from the committed projections and summaries (evidence/baseline/coverage-baseline.md; evidence/qa-gates/coverage-final.md as the pre-cycle state; evidence/remediation-baseline/coverage-baseline.md; evidence/qa-gates/cycle1-coverage.md; evidence/qa-gates/cycle1-coverage-comparison.md), each carrying the first-party summary line, the root counters, the package-level JaCoCo projection and the coordinator class-node rows, and were cross-checked by this review against the root elements and the three coordinator `<class>` nodes of the local raw documents coverage/final-964.cobertura.xml (main line-rate 1 branch-rate 1; Prime.cs 1 / 1; Messages.cs 1 / 0.5 at line 230924) and coverage/remediation-964.cobertura.xml (main 1 / 1 at line 230619; Messages.cs 1 / 1 at line 230924; Prime.cs 1 / 1 at line 231046). Every run used the same route, `Invoke-MSTestWithCoverage.ps1`, over nine test assemblies, so they are comparable.

Verdict lines:

- C# coverage verdict: PASS (repo-wide first-party lines 85.97% and branches 80.13% from the committed post-cycle projection and the raw root element; above the CLAUDE.md floors of 80% lines and 75% branches and above the 85% / 75% floors in .claude/rules; up from 85.96% / 80.10% at the item baseline and at the pre-cycle state).
- C# new-code coverage: PASS. 100% of the executable lines and 100% of the branches this item added to production are covered; after the cycle the type reads 203/203 lines and 44/44 branches with every coordinator class node at line-rate 1 and branch-rate 1.
- C# changed-production-file coverage: PASS on both limbs. EngineToggleStateCoordinator.cs 89/89 lines and 22/22 branches; Prime.cs 72/72 lines and 20/20 branches; Messages.cs 42/42 lines and 2/2 branches (was 1/2; cycle1-coverage.md COORD-FILE rows). The pre-existing uncovered arm recorded as CR-1 in the review labelled 08-50 is covered by the new test; nothing regressed (BASELINE-COORD-LINES 203/203 = FINAL-COORD-LINES 203/203; branches 43/44 -> 44/44).
- C# package-level corroboration: the TaskMaster package counters moved from 802 missed / 2503 covered lines and 211 missed / 523 covered branches (pre-cycle) to 802 / 2503 lines and 210 / 524 branches (post-cycle): exactly one branch moved from missed to covered with no line change, which is the arithmetic signature of R-1 alone. The repo-wide line gain of +10 covered lines and the remaining +3 covered branches sit in packages no file of this branch touches (run-to-run variance of the kind recorded on earlier reviews); the floors are the gate and both are met.
- PowerShell coverage gate: PASS by vacuity (zero PowerShell files changed on this branch, so the changed-line no-regression requirement has no line to evaluate; no PoshQC format, analyze or test gate was owed or run; artifacts/pester/powershell-coverage.xml was not consulted).
- TypeScript and Python: zero files changed on this branch; no verdict is owed.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/baseline/coverage-baseline.md` (item baseline; committed one-line first-party summary, root counters, JaCoCo package projection and coordinator class-node rows) with `evidence/remediation-baseline/coverage-baseline.md` (cycle baseline, read from the committed first-cycle final evidence and the local raw document coverage/final-964.cobertura.xml, gitignored)
- C# post-change coverage artifact: `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/qa-gates/cycle1-coverage.md` with `evidence/qa-gates/cycle1-coverage-comparison.md` (same forms; raw document coverage/remediation-964.cobertura.xml present locally, gitignored; canonical artifacts/csharp/coverage.xml absent in the worktree)
- TypeScript baseline coverage artifact: none consulted (zero TypeScript files changed on this branch)
- TypeScript post-change coverage artifact: none consulted (zero TypeScript files changed on this branch)
- PowerShell baseline coverage artifact: none consulted (zero PowerShell files changed on this branch)
- PowerShell post-change coverage artifact: none consulted (zero PowerShell files changed on this branch)
- Python baseline coverage artifact: none consulted (zero Python files changed on this branch)
- Python post-change coverage artifact: none consulted (zero Python files changed on this branch)
- Per-language comparison summary: the per-language comparison block of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.96% lines (56609/65855) / 80.10% branches (13680/17078). Post-change: 85.97% lines (56639/65881) / 80.13% branches (13687/17082). Change: +0.01% lines / +0.03% branches at two decimals (+30 covered of +26 valid lines, +7 covered of +4 valid branches over the whole branch; within the cycle alone, +10 covered lines of 0 added and +4 covered branches of 0 added, of which the TaskMaster package accounts for exactly +1 branch, the R-1 arm). New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/coverage-baseline.md, evidence/qa-gates/coverage-final.md, evidence/remediation-baseline/coverage-baseline.md, evidence/qa-gates/cycle1-coverage.md, evidence/qa-gates/cycle1-coverage-comparison.md, root elements and coordinator class nodes of coverage/final-964.cobertura.xml and coverage/remediation-964.cobertura.xml.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Artifact consulted | State | Disposition |
|---|---|---|---|
| C# | Committed projections, summaries and class-node rows under evidence/baseline, evidence/remediation-baseline and evidence/qa-gates; raw Cobertura root elements and class nodes of the pre-cycle and post-cycle documents read locally | Present; canonical artifacts/csharp/coverage.xml absent in the worktree (observation O-3, recurring) | PASS |
| TypeScript | none | zero files changed | no verdict owed |
| PowerShell | none | zero files changed | no verdict owed |
| Python | none | zero files changed | no verdict owed |

Coverage exclusion policy check (.claude/rules/general-unit-test.md): the branch adds no coverage-config exclude entry and no ExcludeFromCodeCoverage attribute (the cycle touched no production file or configuration; the first-cycle Grep result stands: the only occurrence in the three production files is the pre-existing class remark stating the type is deliberately NOT excluded). Not Blocking.

### 1.3 Scenario completeness

| Scenario | Verdict | Evidence |
|---|---|---|
| Positive flows | PASS | Refused click with a healthy notification sink and a usable key still notifies once and invokes nothing (pre-existing `HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing`, Passed in both cycle runs); healthy prime and toggle paths unchanged and Passed |
| Negative flows | PASS | Throwing notification sink (AC1); reported once through the log sink with the same exception instance, no engine member invoked, no invalidation (AC2); both sinks throwing (AC2, now with the two symmetry assertions, R-2); null and empty engine key on the refusal path rendered as the `(null)` token, nothing logged, nothing invoked, nothing invalidated (R-1, two data rows) |
| Edge cases | PASS | The record-placement guard test exercises the sink-throws-on-first-report, returns-on-second sequence; the empty-string row of the new test is the boundary the token exists for; the pre-existing #948 and #947 partials are byte-identical to base and Passed |
| Error handling | PASS | Every sink call site contained by TryInvokeSink (production unchanged in the cycle); the click boundary still reports the toggle fault unchanged; ExecuteToggleAsync still propagates |
| Concurrency | PASS | No production change in the cycle; the lock-scoped registration and the keyed TryRemove as the last statement of CompletePrime stand (Prime.cs 49-65 and 194); the Race partial is unchanged except the first-cycle remark; all invariant-named tests Passed in both cycle runs |
| State transitions | PASS | Marker registered -> prime -> report attempted -> recorded only on a normal sink return -> marker cleared; observed by the guard test and the unchanged PrimeFaultOrdering and PrimeRegistration partials |

### 1.4 Arrange-Act-Assert

PASS. The new test carries Arrange / Act / Assert markers (SinkGuard.cs 134, 137, 140); every assertion carries a because-reason string; the `Func<Task> act` under Act with the awaited `NotThrowAsync` under Assert mirrors the existing refusal-path tests of the same partial and the fixture test at EngineToggleStateCoordinatorTests.cs 342-346. The two appended assertions (112-113) sit after the last existing assertion of their test and before its closing brace (cycle1-r2-edit.md R2-PLACEMENT, confirmed by Read).

### 1.5 External dependencies and temporary files

PASS. No Outlook COM, file system, network or process is touched by any changed test; the engines are a strict Moq mock and the three sinks are recording delegates. Reviewer Grep over the seven fixture partials for Path.GetTemp, File., Directory., Thread.Sleep and Task.Delay: 0 hits. The unchanged RepeatFaultSuppression partial imports System.IO for an IOException instance used as a second fault kind, which is not I/O.

### 1.6 Test file location

PASS (repository convention). The cycle edited the existing partial TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs beside the six other partials of the same fixture, mirroring the per-project *.Test layout used for every C# project in this repository; no test file was created or colocated with production source; no project-file change was needed (the partial was already registered at TaskMaster.Test.csproj line 364).

### 1.7 Determinism infrastructure

PASS. No time is consumed by any changed test; no randomness is used; banned APIs in test code: Thread.Sleep 0, Task.Delay 0, Date/clock reads 0 over the seven fixture partials (reviewer Grep). The coordinator type reads no clock.

## 2. General Code Change Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| Before making changes (plan, AC source) | PASS | remediation-inputs.2026-10-03T08-43.md names R-1 and R-2 with constraints; remediation-plan.2026-10-03T08-43.md exists with all 33 tasks checked (reviewer Grep for `^- [ ]`: 0); Phase 0 policy reads recorded (remediation-baseline/phase0-instructions-read.md); the cycle base 6b8e935c1 confirmed as merge-base and the code tree unchanged at the start (remediation-baseline/scope-and-anchor.md) |
| Bugfix workflow step 1 (failing regression test first) | PASS | For the item: evidence/regression-testing/refusal-path-fail-before.md (three refusal-path tests Failed with the fix provably absent; first cycle). For the cycle: a failing run is structurally impossible (both changes add coverage or assertions over behaviour already correct), and the fail-before exception dossier evidence/regression-testing/fail-before-exception.2026-10-03T09-23.md carries `WhyFailingRunImpossible:` and the alternative proof (R1-NAME 0 before / 1 after; fixture 43 -> 45; Messages class-node branch-rate 0.5 before / 1 after, with the false-before control read on the pre-cycle document). SearchScope: FEATURE/evidence/regression-testing/; SearchPatterns: fail-before-exception.*.md; SearchResult: the dossier named above |
| Bugfix workflow step 2 (minimal targeted fix) | PASS | Cycle: 41 added, 0 deleted lines in one test file (cycle1-sinkguard-diff.md NUMSTAT; reviewer Read of the file against the caller's diff); no production, csproj or other test file changed (cycle1-footprint.md listing 3). Item: one private static helper, three call-site rewrites, one message builder, docs, pure-move split (first-cycle evidence, unchanged) |
| Bugfix workflow step 3 (verify locally, toolchain in order) | PASS | evidence/qa-gates/cycle1-toolchain-pass.md: pass 1 clean for format (SinkGuard hash identical before and after the repository-wide format), check, analyzer rebuild, TreatWarningsAsErrors rebuild and the Invoke-MSTestWithCoverage.ps1 route (7390/7390) |
| Design principles (simplicity, reusability, extensibility, separation) | PASS | The new test reuses the fixture's Harness and the existing `[DataTestMethod]` / `[DataRow]` precedent (EngineToggleStateCoordinatorTests.cs 101-107) rather than adding a helper; the production design verdicts of the first cycle stand unchanged |
| Classes, functions, APIs | PASS | No production surface changed in the cycle; the item's internal surface is byte-equal to base per the first-cycle SPAN-HASH rows |
| Error handling | PASS | Two `catch` clauses remain in the type (EngineToggleStateCoordinator.cs 208 and 295; reviewer Grep: every other hit is a `<c>catch</c>` documentation mention); no empty catch; the sink guard is the documented boundary catch accepted as X-1 |
| Logging | PASS | No new logging channel; the new test asserts `Errors` empty on the refusal path with healthy sinks |
| File size limit (500 lines) | PASS | Reviewer Grep line counts: EngineToggleStateCoordinator.cs 302, Prime.cs 197, Messages.cs 86; test partials 481 (primary), 77, 175, 277, 290, 210 (SinkGuard, from 169), 215; all agree with evidence/qa-gates/cycle1-line-counts.md |
| Naming | PASS | PascalCase members; camelCase locals (`harness`, `act`, `engineName`); the test name states scenario and outcome |
| Public APIs and compatibility | PASS | No public API; no project-file change in the cycle |
| Dependencies | PASS | None added |
| I/O boundaries | PASS | No I/O introduced; the type remains host-neutral |

## 3. Language-Specific Code Change Policy Compliance

Language in scope: C# only.

| Item | Verdict | Evidence |
|---|---|---|
| Formatting (csharpier via dotnet tool run) | PASS | evidence/regression-testing/cycle1-format.md (file-scoped format then check, `Checked 1 files`, exit 0); evidence/qa-gates/cycle1-csharpier-format.md (repository-wide, `Formatted 1640 files`, SinkGuard SHA-256 identical before and after, porcelain identical); evidence/qa-gates/cycle1-csharpier-check.md (`Checked 1640 files in 7110ms.`, exit 0, no path listed) |
| Linting (analyzer rebuild, /t:Rebuild, EnableNETAnalyzers, EnforceCodeStyleInBuild) | PASS | evidence/qa-gates/cycle1-msbuild-analyzer.md: exit 0, ERRORS 0, WARNINGS 0 (baseline 0), CSC_OUT 2/2, WRITESET_DIAGNOSTIC_LINES 0, COORDINATOR_DIAGNOSTIC_LINES 0 |
| Type checking (TreatWarningsAsErrors rebuild, no /p:Nullable=enable) | PASS | evidence/qa-gates/cycle1-msbuild-nullable.md: exit 0, 0 errors, 0 warnings, CSC_OUT 2/2; command text matches CLAUDE.md character for character |
| Nullable annotations | PASS | No file in the cycle carries or changes a `#nullable` directive; the three production files remain nullable-disabled (first-cycle observation O-2 stands) |
| Partial-class split | PASS | Unchanged in the cycle: three files declaring `internal sealed partial class EngineToggleStateCoordinator`, registered at TaskMaster.csproj 466-468 (reviewer Grep) |
| XML docs on non-obvious contract | PASS | The new test carries a `<summary>`; production documentation unchanged and accurate (AC5, AC7 re-read). The partial's type-level summary omits its new region: CR-5, Informational |
| Internal surface | PASS | No new member outside the test partial |
| Analyzer suppressions | PASS | None added (reviewer Read of the 41 added lines: no #pragma, SuppressMessage or ExcludeFromCodeCoverage) |

## 4. Language-Specific Unit Test Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[DataTestMethod]` with two `[DataRow]` attributes (SinkGuard.cs 127-129) inside the existing `[TestClass]` partial fixture; the attribute spelling matches the primary fixture's precedent at lines 101-104; MSTest namespace imported once |
| Moq for mocks | PASS | The strict Mock<IAppItemEngines> from the shared Harness; `Engines.VerifyNoOtherCalls()` in the new test (151) and appended to the both-sinks test (112) |
| FluentAssertions | PASS | All assertions use Should() (NotThrowAsync, ContainSingle, Contain, BeEmpty); no MSTest Assert introduced |
| Repo-wide coverage floors | PASS | 85.97% lines (floor 80% per CLAUDE.md, 85% per rules), 80.13% branches (floor 75%) |
| New module/class/method >= 90% | PASS | Item-level: TryInvokeSink 100% (10/10 lines, 2/2 branches), BuildNotifyFailedMessage 100% (8/8 lines); no new production member in the cycle |
| No regression on changed lines | PASS | No production line changed in the cycle; coordinator 203/203 lines at both cycle stages; branches 43/44 -> 44/44 |
| Prohibited behaviors (sleeps, retries, timing hacks, weakened assertions) | PASS | 0 banned-API hits; 0 deleted lines in the cycle, so no assertion was weakened or removed; the four other partials and the primary fixture are unchanged since the first cycle (cycle1-footprint.md) |
| Test toolchain route | PASS | Step 4 ran the CLAUDE.md route `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1` (RUNNER_EXIT_CODE 0; DOCUMENT_PRESENT, TRX_PRESENT, SUMMARY_FILE_PRESENT all True; THRESHOLD_MESSAGE and COLLECT_FAILURE_MESSAGE empty) |

## 5. Test Coverage Detail

| File | Change type | Coverage observation | Disposition |
|---|---|---|---|
| TaskMaster/Ribbon/EngineToggleStateCoordinator.cs | Modified production in the first cycle (496 -> 302 lines); unchanged in cycle 1 | Class node line-rate 1 (89/89), branch-rate 1 (22/22) in the post-cycle document (line 230619) | PASS |
| TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs | Added production in the first cycle; unchanged in cycle 1 | Class node line-rate 1 (72/72), branch-rate 1 (20/20) (line 231046) | PASS |
| TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs | Added production in the first cycle; unchanged in cycle 1 | Class node line-rate 1 (42/42), branch-rate 1 (2/2) (line 230924); was branch-rate 0.5 (1/2) in the pre-cycle document at the same line; the arm covered is `RenderEngineName`'s null-or-empty branch (line 19), reached through `BuildUnavailableMessage` from the new test's two rows | PASS (CR-1 closed) |
| TaskMaster/TaskMaster.csproj | Modified project file in the first cycle (+2 Compile items, 467-468); unchanged in cycle 1 | Not a source file | Discovery proven: three class nodes in both documents |
| TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs | Added test in the first cycle (169); modified in cycle 1 (+41 / -0, now 210) | Outside the denominator by policy | Not measured; 6 results (5 methods, one with two rows) Passed; fail-before dossier for the cycle |
| TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs | Modified test fixture in the first cycle (+12 / -1); unchanged in cycle 1 (481 lines) | Outside the denominator by policy | Not measured; Passed |
| TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs | Modified test in the first cycle (remark only); unchanged in cycle 1 | Outside the denominator by policy | Not measured; Passed |
| TaskMaster.Test/TaskMaster.Test.csproj | Modified project file in the first cycle (+1 Compile item, 364); unchanged in cycle 1 | Not a source file | Discovery proven: the new rows appear in the P1-T6 and P2-T5 runs (fixture 45, suite 7390) |

Package-level projection (TaskMaster package): LINE missed 802 / covered 2503; BRANCH missed 210 / covered 524 after the cycle (cycle1-coverage.md), from 802 / 2503 and 211 / 523 before it (coverage-final.md), and from 802 / 2477 and 211 / 519 at the item baseline.

## 6. Test Execution Metrics

| Run | Scope | Total | Passed | Failed | Source |
|---|---|---|---|---|---|
| Item baseline (first cycle P0-T14, Invoke-MSTestWithCoverage.ps1) | nine test assemblies | 7384 | 7384 | 0 | evidence/baseline/coverage-baseline.md |
| First-cycle final (P2-T5, Invoke-MSTestWithCoverage.ps1) | nine test assemblies | 7388 | 7388 | 0 | evidence/qa-gates/coverage-final.md |
| Cycle-1 fixture baseline (P0-T9, vstest) | EngineToggleStateCoordinatorTests | 43 | 43 | 0 | evidence/remediation-baseline/coordinator-tests-baseline.md (R1-NAME rows=0, false-before) |
| Cycle-1 fixture after the edits (P1-T6, vstest) | EngineToggleStateCoordinatorTests | 45 | 45 | 0 | evidence/regression-testing/cycle1-fixture-run.md (R1-NAME rows=2 passed=2; R2-NAME rows=1 passed=1) |
| Cycle-1 final (P2-T5, Invoke-MSTestWithCoverage.ps1) | nine test assemblies | 7390 | 7390 | 0 | evidence/qa-gates/cycle1-coverage.md |

Figures compared: cycle final total equals the first-cycle final plus 2 (the two data rows of the one new method); error, timeout, aborted and notExecuted each 0 at every stage (summaries derived from the trx); no Sequence file in any run; FINAL-FAILED-FQN-COUNT 0; TEST-DEFINITIONS 7380 (a data-driven method is one definition with two results).

## 7. Code Quality Checks

| Check | Command or method | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | Executor CMD-HYGIENE at P2-T13 (ACCOUNT_HITS, MACHINE_HITS, DRIVE_USERS_HITS over 70 files); reviewer Grep over the feature folder for `[A-Za-z]:[\\/]`, the account name and `Users[\\/]` | 0 / 0 / 0; reviewer hits are the issue URL, two XML namespace URIs in the plans and the SDK download URL, none a host path | PASS |
| Raw document scan | Glob over the feature folder for every file; .gitignore lines 146, 147, 150 | 71 Markdown files, no trx, Cobertura or log; raw documents gitignored under coverage/ (RAW_DOCUMENTS=0) | PASS |
| Suppression scan (added lines) | Read of the 41 added lines | No #pragma, [SuppressMessage], [ExcludeFromCodeCoverage] or analyzer suppression | PASS |
| Workflow change scan | Caller diff; cycle1-footprint.md | No .github/, scripts/ or runsettings path changed on the branch | PASS |
| Prohibited-construct scan | Reviewer Grep over the seven fixture partials | Thread.Sleep 0, Task.Delay 0, DateTime.Now/UtcNow 0, Stopwatch 0, SpinWait 0, .Wait() 0, .Result 0, Path.GetTemp 0, File. 0, Directory. 0, DoNotParallelize 0 | PASS |
| Catch-clause census | Reviewer Grep for `catch` over the three production files | Two code occurrences: EngineToggleStateCoordinator.cs 208 (click boundary) and 295 (sink guard); every other hit is a documentation mention; Prime.cs and Messages.cs contain no catch | PASS |
| Sink call-site census | Reviewer Grep over the three production files | `_logError(` three times, each inside a TryInvokeSink lambda (main 196, 210; Prime.cs 185); `_notifyUnavailable(` once, inside a TryInvokeSink lambda (190); `TryInvokeSink(` 5 = one definition (287) + four call sites | PASS |
| Comment-drift census (AC7) | Reviewer Grep for the retired phrases over the three production files; Read of each surviving comment | Retired phrases 0 hits; the HandleToggleClickAsync summary and remarks, the StartObservedPrime remarks, the CompletePrime summary and remarks and the constructor parameter docs match the two catch clauses that exist | PASS |
| Cycle footprint | cycle1-footprint.md (three listings and a negative control); cycle1-sinkguard-diff.md | Exactly one changed code path; 41 added, 0 deleted lines; issue.md in neither listing; negative control non-empty | PASS |
| Tonality scan | Read of the remediation inputs, plan, closure, handoff and every cycle evidence file cited | Neutral, factual wording; no humor, hyperbole or metaphor | PASS |

## 8. Gaps and Exceptions

- X-1 (accepted exception, carried from #947 and the review labelled 08-50): TryInvokeSink catches Exception broadly without re-raising. Under CLAUDE.md C#4 this is a boundary catch with documented discard and forwarding, following the RibbonCommandBoundary.ReportFailure / SafeLog precedent. Unchanged in the cycle. Not Blocking.
- P-1 (non-blocking Minor, evidence provenance; no code change): fourteen cycle-1 artifacts (remediation-baseline: scope-and-anchor, sinkguard-partial-baseline, coverage-baseline, bootstrap-probe, csharpier-check-baseline, msbuild-analyzer-baseline, msbuild-nullable-baseline, coordinator-tests-baseline; regression-testing: cycle1-r2-edit, cycle1-r1-edit, cycle1-format, cycle1-token-gates, cycle1-build, cycle1-fixture-run) carry `Timestamp: 2026-10-03T09-23 (host clock read at correction; the label first written was composed, not read)` (reviewer Grep for `composed`: 14 files). The caller reported fifteen; the fail-before dossier (file name and field 09-23) and cycle1-sinkguard-diff.md (field 09-23) carry the same label without the note, and which of them was the fifteenth could not be determined from the tree. Rule engaged: the remediation plan's own evidence rule ("the write time is the `Timestamp:` field") and the evidence-first wording rule of .claude/rules/tonality.md; the skill evidence-and-timestamp-conventions fixes the format, which every label satisfies. Classification: a disclosed deviation, not a falsification. The fourteen labels record the correction time, not each artifact's write time, so the per-artifact write times between the 09-17 policy-read label and the 09-24 format label are not recoverable; no figure in any artifact depends on its label; the labels are monotone with the task order and are bracketed by the cycle-open commit (08:44:10) and the cycle commit (09:32:42); the host-clock labels that follow (09-24, 09-25, 09-30, 09-31, 09-32) agree with the remediation Cobertura root epoch (09:26:44) and the commit epoch to the minute. Severity Minor because the executor disclosed the correction in the field itself and no evidence value is affected. Disposition: no remediation task owed; the pattern (composing a label instead of reading the clock) should not recur, and the orchestrator may wish to carry it to the executor's memory. Note also that the review labels 08-50 and 09-50 were caller-supplied rather than clock-read, which both review artifacts state in their headers; they are later than every evidence label and commit epoch, which is the property the label ordering depends on.
- CR-5 (non-blocking Informational, related, touched file): the type-level `<summary>` of EngineToggleStateCoordinatorTests.SinkGuard.cs (lines 9-19) enumerates the throwing-sink regression tests and the #948 record-placement guard but not the null-or-empty-key refusal-path test the cycle added; see code-review.2026-10-03T09-50.md.
- PR context artifact pair absent in the review worktree: scope verified from three agreeing sources instead (section Evidence Location Compliance).
- Canonical C# coverage artifact path absent: committed projections and the local raw documents used, per the standing ruling (observation O-3, recurring across #947, #948, #950, #968 and the review labelled 08-50).
- Preflight record for the remediation cycle: the remediation plan header still reads `Status: Authored, awaiting preflight` and `Last Updated: 2026-10-03T08-43` although all 33 tasks are checked, and no `preflight-clearance.*.md` for the cycle exists under evidence/other/ (SearchScope: FEATURE/evidence/other/ and the FEATURE root; SearchPatterns: preflight-clearance.*.md; SearchResult: evidence/other/preflight-clearance.2026-10-02T07-50.md, the first-cycle record only). Whether the cycle's preflight was recorded in gitignored orchestrator state is not observable from the tree. Observation O-6; not attributable to the executor and not blocking.
- quality-tiers.yml absent at the repository root (Glob: none; pre-existing; tier gates unevaluable; already promoted by the #956 review). Not attributable to this item.

## 9. Summary of Changes

Whole branch (unchanged from the review labelled 08-50 except where noted):

- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs: class made partial; message builders and prime lifecycle moved out; TryInvokeSink added; the refusal path routes the notification through the guard and forwards a notification failure to the log sink through a second guarded call; the click-boundary catch body routes its log call through the guard; documentation rewritten to match.
- TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs (new): GetPrimeTask with the corrected registration-marker documentation; StartPrimeIfNeeded, StartObservedPrime and ApplyPrimeAsync moved verbatim; CompletePrime records a reported fault kind only in the branch taken when TryInvokeSink returns true.
- TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs (new): NullEngineNameToken, RenderEngineName and the five builders, with BuildNotifyFailedMessage added.
- TaskMaster/TaskMaster.csproj: two Compile items.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs (new; extended in cycle 1): three refusal-path regression tests (fail-before recorded), one record-placement guard test, and, from cycle 1, one data-driven refusal-path test for a null or empty engine key (two rows) plus two symmetry assertions appended to the both-sinks-throw test.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs: OnNotify hook on the Harness.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs: one remark corrected for the #948 suppression rule.
- TaskMaster.Test/TaskMaster.Test.csproj: one Compile item.
- Evidence: 34 first-cycle Markdown files plus 30 cycle-1 files under the feature folder's evidence tree, the remediation inputs and plan, issue.md check-offs and the checked plans; no raw document committed.

## 10. Compliance Verdict

PASS. Every policy area evaluates PASS over the full branch diff at head 01dcbe119. Zero blocking findings; nothing requires a further remediation cycle before the pull request is opened. R-1 (CR-1) and R-2 (CR-4) are closed on code and evidence. One non-blocking Minor finding (P-1, evidence provenance, disclosed by the executor) and one non-blocking Informational finding (CR-5) are new; CR-2 and CR-3 stand as Informational with no change recommended. The pull request body should carry `Closes #964`.

## Appendix A: Test Inventory

| Test class | Test | Status after the cycle | AC or finding |
|---|---|---|---|
| EngineToggleStateCoordinatorTests (SinkGuard partial) | HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow | Failed before the fix (first cycle), Passed after; Passed in both cycle-1 runs | AC1 |
| EngineToggleStateCoordinatorTests (SinkGuard partial) | HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing | Failed before the fix, Passed after; Passed in both cycle-1 runs | AC2 |
| EngineToggleStateCoordinatorTests (SinkGuard partial) | HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow (two assertions appended in cycle 1) | Failed before the fix, Passed after; Passed in both cycle-1 runs | AC2, R-2 |
| EngineToggleStateCoordinatorTests (SinkGuard partial) | HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing (null row, "" row; added in cycle 1) | rows=0 at the cycle base (false-before), rows=2 passed=2 after | R-1 |
| EngineToggleStateCoordinatorTests (SinkGuard partial) | GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain | Passed before and after (guard) | AC3, AC4 |
| EngineToggleStateCoordinatorTests (ThrowingSink partial, unchanged) | GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime, GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime, GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared, HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport | Passed, unchanged | AC3, AC4 |
| EngineToggleStateCoordinatorTests (PrimeFaultOrdering, PrimeRegistration, RepeatFaultSuppression partials, unchanged) | seven invariant tests | Passed, unchanged | AC4 |
| EngineToggleStateCoordinatorTests (Race partial, remark only) | GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker and siblings | Passed, unchanged behaviour | AC4 |
| EngineToggleStateCoordinatorTests (primary partial) | HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing and the other pre-existing tests | Passed | AC2, AC4 |

Test result count: 7384 at the item baseline, 7388 after the first cycle, 7390 after cycle 1 (one method with two rows added, none removed); fixture 39 -> 43 -> 45.

## Appendix B: Toolchain Commands Reference

| Step | Command (as recorded in evidence/qa-gates/cycle1-toolchain-pass.md) | Exit | Iteration |
|---|---|---|---|
| 1 | dotnet tool run csharpier format . | 0 | 1 |
| 1b | dotnet tool run csharpier check . | 0 | 1 |
| 2 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | 0 | 1 |
| 3 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | 0 | 1 |
| 4 | pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 (the CLAUDE.md route; 7390/7390; LINE-FLOOR MET, BRANCH-FLOOR MET) | 0 | 1 |

PowerShell gates (PoshQC MCP format / analyze / test): not run; zero PowerShell files changed on this branch.

Reviewer commands: none (Bash forbidden). All verification by Read, Grep and Glob against the item worktree, the caller's verbatim diff, the gitignored raw Cobertura documents and the worktree reflog.
