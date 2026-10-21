# Feature Audit: engine-toggle-coordinator-947-review-residuals (Issue #964), remediation cycle 1 exit

- Date: 2026-10-03 (review label `2026-10-03T09-50`, caller-supplied; see the policy audit's "Timestamp derivation")
- Work mode: `minor-audit` (`issue.md` line 12, the only `- Work Mode:` line). AC source: the `## Acceptance Criteria` section of `issue.md` only (heading at line 23; eight checkbox lines 27-34; next heading `## Environment` at line 36). `spec.md` and `user-story.md` are intentionally absent (Glob of the feature folder: none; `remediation-baseline/scope-and-anchor.md` REQUIREMENTS-DOCUMENTS: none).
- Branch: `bug/engine-toggle-coordinator-947-review-residuals-964`
- Review kind: re-audit at the exit of remediation cycle 1 over the whole branch; the cycle changed one test file and no acceptance criterion (`issue.md` appears in neither cycle footprint listing).

## Scope and Baseline

| Item | Value | Source |
|---|---|---|
| Base | origin/main `993fdd01566dee82e5f37acb761a600feaaa1454`, merged into the item at `981abef77657adcc90d7c116a6b4c6500b79ea29` (reflog entry `merge origin/main`, epoch 1790993935) | caller prompt; worktree reflog; `evidence/qa-gates/footprint-scope.md` |
| Pre-change text anchor | `94287369908cc920b21b0e3256314f988ad7d2f5` (first-cycle Phase 0 anchor); no TaskMaster or TaskMaster.Test path changed between that anchor and the merge commit | `evidence/baseline/scope-and-anchor.md`; `evidence/qa-gates/footprint-scope.md` negative control 2 |
| Head commit | `01dcbe119b74fecb949452397b18883b9731a242` (`docs(964): record remediation cycle 1 post-commit check-offs P2-T14 to P2-T16`, epoch 1791034633) | worktree reflog, last entry |
| Cycle 1 commits | `96d2975d4` (first-cycle review artifacts), `6b8e935c1` (cycle opened: remediation inputs and plan; cycle base), `b27cf3bd2` (`test(964): cover the null or empty engine key refusal path and symmetric sink-guard assertions`; the one code commit of the cycle), `01dcbe119` (check-offs) | reflog; `evidence/other/cycle1-commit-record.md` (PRE-COMMIT-HEAD 6b8e935c1) |
| Code footprint (origin/main..HEAD) | `M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (496 -> 302 lines), `A TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs` (197), `A TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs` (86), `M TaskMaster/TaskMaster.csproj` (+2), `M TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` (470 -> 481), `A TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` (210; 169 before the cycle), `M TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` (remark only), `M TaskMaster.Test/TaskMaster.Test.csproj` (+1) | caller verbatim diff; `evidence/qa-gates/footprint-scope.md`; `evidence/qa-gates/cycle1-footprint.md`; files re-read; reviewer Grep line counts |
| Cycle 1 code footprint (6b8e935c1..HEAD) | exactly `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs`, +41 / -0 | `evidence/qa-gates/cycle1-footprint.md` listing 3; `evidence/regression-testing/cycle1-sinkguard-diff.md` NUMSTAT |
| Non-code footprint | feature folder only (issue.md check-offs, two plans, remediation inputs, 64 evidence files, three first-cycle review artifacts) and `docs/features/potential/promoted/2026-10-01-engine-toggle-coordinator-947-review-residuals.md` | same |
| Baseline behaviour (defect) | `HandleToggleClickAsync` called `_notifyUnavailable(...)` unguarded on the refusal path (pre-fix line 178); `GetPrimeTask` documentation described "the in-flight ... prime"; the file was 496/500 lines | `evidence/regression-testing/refusal-path-fail-before.md`; `evidence/regression-testing/split-census.md` BASE-LINES 496 |
| Baseline tests | fixture 39/39; suite 7384/7384 | `evidence/baseline/coordinator-tests-baseline.md`; `evidence/baseline/coverage-baseline.md` |
| Baseline coverage | first-party 85.96% lines / 80.10% branches; coordinator 177/177 lines, 39/40 branches (one class node) | `evidence/baseline/coverage-baseline.md` |
| Pre-cycle state (first-cycle final) | fixture 43/43; suite 7388/7388; first-party 85.96% / 80.10%; coordinator 203/203 lines, 43/44 branches; Messages class node branch-rate 0.5 | `evidence/qa-gates/coverage-final.md`; `evidence/remediation-baseline/coverage-baseline.md`; `coverage/final-964.cobertura.xml` line 230924 re-read |
| Post-cycle state | fixture 45/45; suite 7390/7390; first-party 85.97% / 80.13%; coordinator 203/203 lines, 44/44 branches; every coordinator class node line-rate 1 and branch-rate 1 | `evidence/regression-testing/cycle1-fixture-run.md`; `evidence/qa-gates/cycle1-coverage.md`; `evidence/qa-gates/cycle1-coverage-comparison.md`; `coverage/remediation-964.cobertura.xml` lines 230619, 230924, 231046 re-read |
| PR context artifacts | absent from the review worktree; the session checkout's pair belongs to the #968 branch; not regenerable without a shell. Scope derived from the three agreeing sources above. | Glob; Read |

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md`, section `## Acceptance Criteria`. All eight are checkbox items and all eight read `- [x]` at head (first-cycle check-offs at P2-T10 to P2-T17; the cycle edited none: `remediation-baseline/scope-and-anchor.md` ISSUE-GREP-COUNTS checked-AC1-8=8, unchecked-AC=0; `cycle1-footprint.md`: issue.md in neither listing).

| ID | Criterion (verbatim opening) | Checkbox at head |
|---|---|---|
| AC1 | (refusal-path notification guard, regression-first): with the engines accessor returning null and a `notifyUnavailable` sink that throws, `EngineToggleStateCoordinator.HandleToggleClickAsync` completes without throwing. A new MSTest regression test proves this; it is recorded failing against the unmodified coordinator ... and passing after the fix. | `[x]` |
| AC2 | (notification failure is reported, not lost): in the AC1 scenario the notification sink is attempted exactly once, the sink's exception is delivered exactly once to `logError` (the same exception instance), no engine member is invoked, and no control is invalidated. When `logError` also throws in that scenario, `HandleToggleClickAsync` still completes without throwing. Both behaviours are asserted by named MSTest tests. | `[x]` |
| AC3 | (one shared sink guard): all three sink call sites ... route through a single private guard helper; no other `catch` clause that discards a sink exception remains in the coordinator's source files. Verified by reading the split source and by the existing #947 tests in `EngineToggleStateCoordinatorTests.ThrowingSink.cs` passing unchanged. | `[x]` |
| AC4 | (prime-marker ordering invariants preserved across the split): ... the marker is registered before the prime starts; a prime fault is reported before the marker is cleared; a sink that throws leaves the fault kind unrecorded (report still owed); a repeated fault kind for the same engine is reported once. Verified by every existing test in the `EngineToggleStateCoordinatorTests` partials ... passing with no assertion weakened or removed. | `[x]` |
| AC5 | (`GetPrimeTask` documentation accuracy): the `GetPrimeTask` `<summary>` and `<returns>` describe the registration marker (completed only after the prime outcome has been observed and reported), not "the prime task" or "the in-flight prime". | `[x]` |
| AC6 | (file-size split): `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` is split into cohesive partial-class files of the same `internal sealed partial class EngineToggleStateCoordinator`, each file at or below 450 lines, each new file registered as a `Compile` item in `TaskMaster/TaskMaster.csproj`; every touched test file stays at or below 500 lines. | `[x]` |
| AC7 | (comment drift in touched files): every comment that counts or locates the coordinator's `catch` clauses ... and the `HandleToggleClickAsync` "never throws" remark match the post-change code, and the constructor's `notifyUnavailable` and `enginesAccessor` parameter docs state the guarded behaviour and the accessor's non-throwing precondition. | `[x]` |
| AC8 | (toolchain and coverage): the CLAUDE.md C# toolchain passes in one clean pass (csharpier check, analyzer `/t:Rebuild`, `TreatWarningsAsErrors` `/t:Rebuild` without `/p:Nullable=enable`, `Invoke-MSTestWithCoverage.ps1`), with no new failing test relative to baseline and the coordinator's line coverage not lower than its baseline figure. | `[x]` |

## Acceptance Criteria Evaluation

Each criterion was re-evaluated against the head tree and the cycle evidence. Where the criterion concerns production code, the cycle changed none, and this review re-read the cited regions on disk rather than relying on the first-cycle verdict alone.

| ID | Status | Evidence and reasoning |
|---|---|---|
| AC1 | PASS | Code (re-read): `EngineToggleStateCoordinator.cs` 186-202, the notification call is the argument of `TryInvokeSink` (189-193), whose `catch (Exception ex)` (295-299) assigns the exception and returns `false`; nothing on the path rethrows. Test: `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow` (`SinkGuard.cs` 30-43). Fail-before: `evidence/regression-testing/refusal-path-fail-before.md` (first cycle; EXIT_CODE 1 with ExpectedExitCode 1; fix provably absent; the message names `notify sink failed`). Pass-after: `refusal-path-pass-after.md`; cycle-1 runs `coordinator-tests-baseline.md` and `cycle1-fixture-run.md` both `rows=1 passed=1`; cycle final FAILED-FQN-COUNT 0. |
| AC2 | PASS | `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing` (51-79): single notification, single error, `BeSameAs(notifyFailure)`, message contains the key, `Engines.VerifyNoOtherCalls()`, `Invalidations` empty. `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow` (87-114): both hooks throw; `NotThrowAsync`, single notification, single log attempt, same instance, and, since the cycle, `Engines.VerifyNoOtherCalls()` and `Invalidations` empty (112-113). Code: the forwarding call at 195-198 is itself guarded and its result discarded. Both tests `rows=1 passed=1` in both cycle runs. |
| AC3 | PASS | Call sites (reviewer Grep, head tree): `_notifyUnavailable(` once (main 190, inside a `TryInvokeSink` lambda); `_logError(` three times (main 196 and 210, Prime.cs 185), each inside a `TryInvokeSink` lambda; `TryInvokeSink` is `private static` (main 287). Catch clauses in the three source files: main 208 (click boundary) and main 295 (the guard); Prime.cs and Messages.cs contain none (every other hit is a `<c>catch</c>` doc mention). `ThrowingSink.cs` unchanged on the branch since the first-cycle anchor and untouched in the cycle (`cycle1-footprint.md`); its four tests `rows=1 passed=1` in both cycle runs. |
| AC4 | PASS | Code unchanged in the cycle; re-read: registration before start (Prime.cs 63 then 64, inside the `_primeGate` lock); report before clear (183-188 then 194); the record at 190 is the only statement of the `if (TryInvokeSink(...))` branch; `ContainsKey` guard at 181. Tests: all eleven invariant-named tests and the record-placement guard `rows=1 passed=1` in `coordinator-tests-baseline.md` (cycle base) and `cycle1-fixture-run.md` (after); the cycle deleted 0 lines in its one changed file and touched no other partial, so no assertion was weakened or removed; first-cycle `test-partials-unchanged.md` still describes the other partials. |
| AC5 | PASS | Prime.cs 10-26 unchanged in the cycle and re-read: the summary opens "The registration marker for an engine key" and states the marker "is not the prime task itself"; the returns opens "The registered marker, or Task.CompletedTask when no marker is registered". Reviewer Grep for `The prime task, or`, `The in-flight` and `most recently completed` over the three files: 0. |
| AC6 | PASS | Three files, each `internal sealed partial class EngineToggleStateCoordinator` in `namespace TaskMaster`; reviewer Grep line counts 302 / 197 / 86 (each at or below 450); `TaskMaster.csproj` 466-468 registers all three (reviewer Grep); touched test files 481 (primary), 277 (Race), 210 (SinkGuard), each at or below 500 (`cycle1-line-counts.md` agrees); `TaskMaster.Test.csproj` 364 registers the SinkGuard partial. Build in the cycle: analyzer and TreatWarningsAsErrors rebuilds exit 0 with `CSC_OUT_TASKMASTER 2` and `CSC_OUT_TASKMASTER_TEST 2`; the post-cycle coverage document carries one `<class>` node per production file (COORD-CLASS-NODES 3). |
| AC7 | PASS | Each named comment re-read against the code (unchanged in the cycle): `HandleToggleClickAsync` summary (main 160-164) and remarks (170-183); `StartObservedPrime` remarks (Prime.cs 71-82); `CompletePrime` summary (131-137) and remarks (157-162); constructor docs for `enginesAccessor` (87-93) and `notifyUnavailable` (98-103). Two catch clauses exist (main 208, 295), so every count is correct. The retired phrases have 0 hits (reviewer Grep). |
| AC8 | PASS | Re-satisfied inside the cycle: `evidence/qa-gates/cycle1-toolchain-pass.md`, pass 1 clean. Step 1: `csharpier format .` left the SinkGuard hash unchanged, `csharpier check .` exit 0, 1640 files. Step 2: analyzer `/t:Rebuild` with `EnableNETAnalyzers` and `EnforceCodeStyleInBuild`, exit 0, 0 errors, 0 warnings, `CSC_OUT 2/2`. Step 3: `/t:Rebuild` with `TreatWarningsAsErrors=true` and no `/p:Nullable=enable`, exit 0, 0 errors, 0 warnings. Step 4: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1`, runner exit 0, 7390/7390 (item baseline 7384/7384; the six additions are the four first-cycle tests and the two cycle-1 data rows), `FINAL-FAILED-FQN-COUNT 0`, LINE-FLOOR and BRANCH-FLOOR MET (85.97% / 80.13%). Coordinator line coverage 100% at every stage (177/177 at baseline, 203/203 before and after the cycle), re-read at the class nodes of both raw documents; branches 44/44 after the cycle. |

Totals: 8 PASS, 0 PARTIAL, 0 FAIL, 0 UNVERIFIED.

### Remediation findings of cycle 1 (from `remediation-inputs.2026-10-03T08-43.md`)

| ID | Required outcome | Status | Evidence |
|---|---|---|---|
| R-1 (from CR-1) | A deterministic MSTest test in the SinkGuard partial covering null and empty engine names on the refusal path: no throw, one notification containing `(null)`, no error, no engine member, no invalidation; the Messages class node reaches branch-rate 1; no production file changes | CLOSED | `SinkGuard.cs` 118-155 (`[DataTestMethod]`, `[DataRow(null)]`, `[DataRow("")]`, the five assertions); `cycle1-fixture-run.md` rows=2 passed=2; `coverage/remediation-964.cobertura.xml` line 230924 `branch-rate="1"` against `0.5` in `coverage/final-964.cobertura.xml` at the same line; COORD-BRANCHES 44/44; `cycle1-footprint.md` listing 3: test file only |
| R-2 (from CR-4) | Both symmetry assertions added to the both-sinks-throw test; the test continues to pass | CLOSED | `SinkGuard.cs` 112-113; `cycle1-r2-edit.md` R2-PLACEMENT 112 and 113; `cycle1-sinkguard-diff.md` 0 deleted lines; `cycle1-fixture-run.md` rows=1 passed=1 |
| Constraints | Test-only change set; SinkGuard at most 500 lines; MSTest, Moq, FluentAssertions; no temporary files or sleeps; full toolchain in order; evidence under FEATURE/evidence only; no AC edited | MET | `cycle1-footprint.md`; `cycle1-line-counts.md` (210); reviewer Read and Grep of the partial; `cycle1-toolchain-pass.md`; Glob of the feature folder; `scope-and-anchor.md` ISSUE-GREP-COUNTS |

## Acceptance Criteria Check-off

All eight criteria were already checked `[x]` by the first-cycle executor (P2-T10 to P2-T17, one flip per task, criterion text unmodified) and remained checked through the cycle (the cycle edited no line of `issue.md`). This review evaluated every criterion PASS, so no checkbox was changed and none was unchecked. Newly checked off by this review: none.

### Acceptance Criteria Status
- Source: docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

## Summary

**Verdict: PASS.** 8/8 acceptance criteria met on evidence at head `01dcbe119`; R-1 and R-2 closed on code and evidence; 0 Blocking findings across the three review artifacts; non-blocking: CR-2 and CR-3 (Informational, unchanged, no change recommended), CR-5 (Informational, new: the SinkGuard partial's type-level summary omits its new region) and P-1 (Minor, evidence provenance: fourteen cycle artifacts re-stamped from composed labels to one host-clock reading with an in-field note; disclosed, no figure affected); 7 observations (O-1 to O-7). Remediation inputs: not produced.

Independent verification performed by this review (no command executed): the caller's verbatim diff at head read in full; the SinkGuard partial read in full; the coordinator main file re-read at the refusal path, click boundary and guard; the primary fixture re-read at the data-row precedent and the Harness; both raw Cobertura documents read at the root and at every coordinator `<class>` node; the worktree reflog read to confirm head, cycle commits and commit times and to cross-check the executor's labels; the feature folder enumerated for file types; the catch-clause, sink-call-site, banned-API and retired-phrase censuses re-derived by Grep; every cycle evidence file read against the plan's acceptance clauses.

### Related-defect dispositions (for the coordinator)

| ID | Item | Related to this item | Disposition requested |
|---|---|---|---|
| CR-1 / R-1 | `RenderEngineName` null-or-empty arm untested | Yes | Closed in cycle 1; no action |
| CR-4 / R-2 | Both-sinks-throw test lacked two symmetry assertions | Yes | Closed in cycle 1; no action |
| CR-5 | SinkGuard partial's type-level `<summary>` (lines 9-19) does not mention the null-or-empty-key test | Yes (touched file, comment drift) | Non-blocking Informational; one-clause addition at the coordinator's discretion under the related-defect directive; no behaviour or coverage effect either way |
| P-1 | Fourteen cycle artifacts re-stamped from composed `Timestamp:` labels to one host-clock reading with an in-field note | Yes (this item's evidence) | Non-blocking Minor; no task owed; the composing pattern should not recur (executor memory) |
| O-1 | Primary fixture at 481/500 lines | Yes, compliant | No action in this item |
| O-6 | Remediation plan header `Status:` / `Last Updated:` not updated after execution; no cycle-1 preflight record under `evidence/other/` | Yes (feature folder) | Orchestrator may refresh the two header lines at its next phase-boundary commit; informational |

### Unrelated defects

None found. Pre-existing repository-level conditions (canonical C# coverage artifact path absent; `quality-tiers.yml` absent) are already tracked and are not defects of this item.
