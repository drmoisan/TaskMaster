# Feature Audit: engine-toggle-permanent-config-fault-logs-every-poll (Issue #948)

- **Audit date:** 2026-10-02T04-30
- **Work mode:** `full-bug` (persisted marker in `issue.md`); acceptance-criteria source: `spec.md` v1.3 `## Acceptance Criteria` only. No `user-story.md` exists, by design.
- **Branch:** `bug/engine-toggle-permanent-config-fault-logs-every-poll-948`; head `601f06174`; merge base `59cbab04f1c854baa2a03b6cbf755c1df4f961b4` (origin/main)
- **Reviewer:** feature-review agent (read-only; Read, Grep, Glob; no shell)

## Scope and Baseline

- **Baseline:** origin/main at `59cbab04f` after the orchestrator's reconciliation merge (`f96aab71e`), so the merge base equals origin/main and the branch contains the sibling #947 sink guard (shape S). At the merge base the coordinator was 476 lines, the fixture had 32 tests (all passing), and repository first-party coverage was 85.35% lines / 79.75% branches (`evidence/baseline/`).
- **Branch diff against the baseline:** `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (M), `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` (A), `TaskMaster.Test/TaskMaster.Test.csproj` (M, one insertion), Markdown under the feature folder, the inherited promotion record `docs/features/potential/promoted/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll.md` (A) and three inherited agent-memory paths (`evidence/qa-gates/footprint-scope.md`; `evidence/baseline/anchor-merge-base.md`).
- **Defect:** against a permanently faulted configuration `AsyncLazy`, every cache-miss `getPressed` poll re-primed and logged the same error without bound once #944 made re-priming reliable.
- **Fix:** report each (engine key, base-exception type) prime failure once per coordinator lifetime, keep re-priming, state the suppression in the first message.
- **Evidence consulted:** every artifact under `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/` and `evidence/other/` (40 files), the production and test sources, the git-ignored Cobertura document `artifacts/csharp/coverage.xml` in the worktree, spec.md, plan v0.7, issue.md.
- **Assumptions recorded:** head and merge-base identifiers are taken from the caller and the committed anchor evidence (no git access in this review); the four existing partials' byte-identity to the merge base is taken from `PROTECTED_FILES_DIFF_EXIT=0` in `evidence/qa-gates/protected-regions-unchanged.md` and from their absence in the name-status diff.

## Acceptance Criteria Inventory

Source: `spec.md` lines 242-257. Sixteen checkbox items, all `- [x]` at review time (executor check-offs recorded per criterion in `evidence/other/ac-status-summary.md`).

| ID | Criterion (abridged) | State in source |
|---|---|---|
| AC-A | Repeated faulted polls report once while every poll re-primes (`GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly`) | [x] |
| AC-B | Repeated canceled primes report once (`GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly`) | [x] |
| AC-C | A later successful prime recovers (`GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce`) | [x] |
| AC-D | A different failure kind for the same key is reported once more (`GetPressed_WhenFailureKindChanges_LogsNewKindOnce`) | [x] |
| AC-E | Suppression is per key (`GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged`) | [x] |
| AC-F | Toggle-path faults still reported on every click (`HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault` plus the existing toggle-fault test unchanged) | [x] |
| AC-G | First message states repeats are not logged again (`GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain`; `BuildPrimeFailedMessage` ends with the sentence) | [x] |
| AC-H | Fail-before demonstrated and recorded | [x] |
| AC-I | Pass-after demonstrated and recorded | [x] |
| AC-J | Existing coordinator tests pass unchanged; four partials byte-identical | [x] |
| AC-K | Invariants preserved (four named pins pass; `TryRemove` last with the guarded block before it) | [x] |
| AC-L | Throwing-sink behaviour unchanged (no new try/catch/finally; catch count equal; record immediately after the sink call inside the guard) | [x] |
| AC-M | Scope held (three code paths plus feature-folder Markdown; inherited paths excluded; protected methods unchanged; no package manifest change; no xml/trx/coverage file added) | [x] |
| AC-N | Full C# toolchain passes in one final pass in order; DIRECT coverage route recorded with the probe result | [x] |
| AC-O | Coverage: changed lines covered on both guard branches; coordinator file >= 90% lines; repository figures recorded and not lowered | [x] |
| AC-P | File-size ceiling: production and partial under 500; primary fixture unchanged in length | [x] |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence verified in this review |
|---|---|---|
| AC-A | PASS | Test at partial lines 38-66: `Errors.Count.Should().Be(1)`, `BeSameAs(failure)`, `Message.Contain(SpamEngine)`, `Verify(EngineActiveAsync(SpamEngine), Times.Exactly(5))`, `pressed.Should().BeFalse()`, `Invalidations.Should().BeEmpty()`. Passed in `repeat-fault-suppression-pass-after.md` (P2-T5 and P3-T7). |
| AC-B | PASS | Lines 73-98: `ContainSingle`, `BeAssignableTo<OperationCanceledException>`, `Times.Exactly(5)`. Passed in both pass-after runs. |
| AC-C | PASS | Lines 105-140: `SetupSequence` faulted, faulted, `Task.FromResult(true)`; `ContainSingle`, `GetPressed` true, `Invalidations.Equal([SpamToggleControlId])`, `Times.Exactly(3)`. Passed. |
| AC-D | PASS | Lines 147-175: `InvalidOperationException` twice then `IOException` twice; `HaveCount(2)`, `Errors[0]`/`Errors[1]` `BeSameAs` the two instances in order, `Times.Exactly(4)`. Passed. |
| AC-E | PASS | Lines 181-207: two Spam polls, one Triage poll; `HaveCount(2)`; messages contain `SpamEngine` and `TriageEngine` respectively, exceptions pinned by identity. `"Triage"` is a mapped key (`EngineToggleCatalog.cs` line 52). Passed. |
| AC-F | PASS | Lines 213-240: two Spam polls then `HandleToggleClickAsync` with `ToggleEngineAsync` throwing; `HaveCount(2)`, second exception `BeSameAs(toggleFailure)`, no invalidation, no notification. Passed; `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate` Passed in baseline, fail-before and pass-after. `HandleToggleClickAsync` span hash equals the merge base. |
| AC-G | PASS | Lines 246-269: `ContainSingle`, `Contain("not logged again")`, `EndWith("Further failures of this kind for this engine are not logged again.")`. Production lines 477-479 end with that sentence. Passed. |
| AC-H | PASS | `evidence/regression-testing/repeat-fault-suppression-fail-before.md`: command, `EXIT_CODE: 1` with `ExpectedExitCode: 1`, `COUNTERS total=39 ... failed=7`, `RESULT GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly = Failed`, message `Expected harness.Errors.Count to be 1 ... but found 5`, `FAIL-BEFORE-ERROR-COUNT: 5`, production hash equal to the merge-base anchor hash. |
| AC-I | PASS | `evidence/regression-testing/repeat-fault-suppression-pass-after.md`: `EXIT_CODE: 0`, `total=39 executed=39 passed=39 failed=0`, all fourteen named tests Passed, repeated in the FINAL-FIXTURE-RUN section on the P3-T6 rebuild. |
| AC-J | PASS | `PROTECTED_FILES_DIFF_EXIT=0` over the five fixture partials at the merge base (`protected-regions-unchanged.md`, re-run post-format); none of them appears in the name-status diff; 39/39 passed. The line counts of all four existing partials equal their anchors (`file-line-counts.md`). |
| AC-K | PASS | The four named pins Passed in both pass-after runs. Read of `CompletePrime`: guard 421 < sink 425 < record 426 < `TryRemove` 434, which is the last statement (`REMOVE-IS-LAST: True`). `StartPrimeIfNeeded` and `StartObservedPrime` span hashes equal the merge base. |
| AC-L | PASS | Read of lines 405-435: one `try` (423) and one `catch` (428) inside the span, both pre-existing (#947); no `finally`; the record at 426 is the statement immediately after the sink call at 425, inside the try and inside the `if` guard, not in a catch or finally. Catch keywords on code lines: 3 at both the merge base and head (`FILE-CATCH-CODE-LINES 3`, baseline 3); net added try/catch/finally lines 0 (the diff shows the sibling's try/catch once dropped and once re-added at the new indentation). |
| AC-M | PASS | Name-status diff (`footprint-scope.md`): THIS-ITEM-FOOTPRINT is the three code paths plus Markdown under the feature folder; INHERITED-AND-EXCLUDED is the promotion record and exactly the three agent-memory paths recorded at P0-T4. Twelve protected spans hash-equal; `StartPrimeIfNeeded`, `StartObservedPrime`, `ApplyPrimeAsync`, `GetPressed`, the toggle path and the constructor among them. Both `packages.config` files and `TaskMaster.csproj` unchanged. `RAW-DOCS-COMMITTED: 0`; csproj numstat 1/0 (`csproj-registration.md`; Read confirms the single new entry at line 363). |
| AC-N | PASS | `toolchain-final-pass.md`: PASS-NUMBER 1 in CLAUDE.md order; format rewrite count 0; check `Checked 1637 files`, no differences; analyzer and nullable Rebuilds exit 0 with `SKIP_CORECOMPILE_LINES: 0` and CSC output counts >= 1 for both TaskMaster projects; fixture 39/39; coverage run 7361/7361 by `COVERAGE-ROUTE: DIRECT` selected by `STALL-PROBE: REPRODUCES` (`stall-probe.md`: one shell-icon test failed with a Win32 icon-handle `ArgumentException`), with the four classes excluded and the `/Blame` hang timeout appended, exactly as the amended AC-N admits. |
| AC-O | PASS | `coverage-projection.md` COMPARISON and Clause results (plan version 0.7): `CHANGED-LINES-UNCOVERED: 0` of 15 with elements; `GUARD-BRANCH-ROW: ON GUARD LINE` covered 2 of 2; `COORD-FILE-LINE-RATE-FINAL: 100`; first-party baseline 85.35% / 79.75% and final 85.35% / 79.74% recorded; `NOT-LOWERED-STATEMENT: HOLDS: CHANGED PACKAGE NOT LOWER` with the UtilitiesCS variance named. Independently confirmed in this review from the Cobertura class node: `line-rate="1" branch-rate="0.975"`, lines 420-435 and 474-482 all `hits="1"`, line 421 `condition-coverage="100% (2/2)"`; root `lines-covered="56204" lines-valid="65855" branches-covered="13618" branches-valid="17078"`. The 0.01-point repository branch delta is in an untouched package; the changed file's covered lines and branches rose (167 to 177; 37 to 39) with uncovered counts unchanged. |
| AC-P | PASS | `file-line-counts.md` after the repository-wide format: production 496, partial 290, primary fixture 470 (anchor 470). Read of both files confirms the production file ends at line 496 and the partial at line 290. |

Verdict distribution: PASS 16, PARTIAL 0, FAIL 0, UNVERIFIED 0.

## Acceptance Criteria Check-off

- All sixteen criteria were already checked off by the executor in `spec.md` (fail-closed, one task per criterion, P3-T15 to P3-T30), each with a `MET` line in `evidence/other/ac-status-summary.md`.
- Newly checked off by this review: none (nothing remained unchecked).
- Left unchecked by this review: none. No criterion was evaluated PARTIAL, FAIL or UNVERIFIED.
- No criterion text was altered and no criterion was added.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/spec.md` (`## Acceptance Criteria`)
- Total AC items: 16
- Checked off (delivered): 16
- Remaining (unchecked): 0
- Items remaining: none

## Follow-ups (recorded, not filed)

- Correct the stale remark in `EngineToggleStateCoordinatorTests.Race.cs` lines 196-201 (spec Rollout already records it; code review CR-1).
- Split `EngineToggleStateCoordinator.cs` (496/500) and the primary fixture (470/500) before the next change to either (CR-2).
- Add the no-lock serialisation "why" to the `CompletePrime` comment (CR-3).
- Derive evidence `Timestamp:` labels from the clock (CR-5).
- Guard the `_notifyUnavailable` sink (pre-existing, #947 F-1; CR-6).
- Canonical C# coverage artifact path convention (recurring).
- `quality-tiers.yml` absent at the repository root (pre-existing; promoted by the #956 review).

## Summary

**Verdict: PASS.** 16 of 16 acceptance criteria verified against committed evidence and against the sources and the Cobertura document as read in this review; 0 blocking findings. The regression partial fails before and passes after the fix for the asserted reasons, the four existing partials are untouched and green, the toolchain passed in one ordered pass, and coverage on the changed file is 100% lines with both outcomes of the new guard executed. Repository-wide first-party coverage is unchanged on lines (85.35%) and moved 0.01 point on branches in an unrelated package. The feature is ready for the PR gate; no remediation inputs are produced.
