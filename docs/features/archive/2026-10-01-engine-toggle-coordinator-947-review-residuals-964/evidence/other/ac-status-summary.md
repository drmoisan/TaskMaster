# AC Status Summary (P2-T10 to P2-T17)

Timestamp: 2026-10-03T08-16
Task: P2-T10 to P2-T17
Command: Read of the cited evidence artifacts and source files; check-off edits `- [ ] ACn (` to `- [x] ACn (` in docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md
EXIT_CODE: 0

Output Summary:
- Source: docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md, section `## Acceptance Criteria`.
- Per-criterion results are appended below, one task at a time.

## AC1 (P2-T10)

AC1: MET
- P1-T14 (FEATURE/evidence/regression-testing/refusal-path-fail-before.md): `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow` = Failed; its message carries both fragments `a throwing notification sink must not escape the refusal path` and `notify sink failed`.
- P1-T24 (FEATURE/evidence/regression-testing/refusal-path-pass-after.md): the same test = Passed.
- P2-T5 (FEATURE/evidence/qa-gates/coverage-final.md): FAILED-FQN-COUNT 0, so no FAILED-FQN row equals `TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow`.
- issue.md: `- [ ] AC1 (` changed to `- [x] AC1 (`; no other text changed.

## AC2 (P2-T11)

AC2: MET
- P1-T14: `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing` = Failed for its recorded reason (`threw exception` and `notify sink failed`); `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow` = Failed for its recorded reason (`the refusal path contains a failure of both sinks` and `notify sink failed`).
- P1-T24: both tests = Passed (COUNTERS total=43 passed=43 failed=0).
- issue.md: `- [ ] AC2 (` changed to `- [x] AC2 (`; no other text changed.

## AC3 (P2-T12)

AC3: MET
- P1-T22 (FEATURE/evidence/qa-gates/production-edit-scope.md): `catch(` = 2, `catch(Exception)` = 0, `TryInvokeSink(` = 5; the four call-site tokens (`TryInvokeSink(()=>_notifyUnavailable(BuildUnavailableMessage(engineName)),outvarnotifyFailure)`, `TryInvokeSink(()=>_logError(BuildNotifyFailedMessage(engineName),notifyFailure),out_)`, `TryInvokeSink(()=>_logError(BuildToggleFailedMessage(engineName),ex),out_)`, `failure),out_)){_reportedPrimeFaults[reportKey]=0;}`) = 1 each.
- `CATCH-SITES:` names `HandleToggleClickAsync` (lines 208 to 211) and `TryInvokeSink` (lines 295 to 299) only.
- P1-T25 (FEATURE/evidence/qa-gates/test-partials-unchanged.md): `UNCHANGED TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs exit=0`.
- P1-T24: the four ThrowingSink tests (`GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime`, `GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime`, `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared`, `HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport`) = Passed.
- issue.md: `- [ ] AC3 (` changed to `- [x] AC3 (`; no other text changed.

## AC4 (P2-T13)

AC4: MET
- P1-T8 (FEATURE/evidence/regression-testing/split-fixture-green.md): COUNTERS total=39 passed=39 failed=0; all 11 INVARIANT-NAMES = Passed.
- P1-T24: COUNTERS total=43 passed=43 failed=0; all 11 INVARIANT-NAMES = Passed; `GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain` = Passed.
- P1-T25: every clause met (four partials `exit=0`, Race `NON-DOC-CHANGES` 0, primary fixture single MINUS `message => Notifications.Add(message),`).
- P1-T22: all 14 SPAN-HASH lines `equal=True`; `failure),out_)){_reportedPrimeFaults[reportKey]=0;}` = 1.
- issue.md: `- [ ] AC4 (` changed to `- [x] AC4 (`; no other text changed.

## AC5 (P2-T14)

AC5: MET
- P1-T22: `The in-flight` = 0, `most recently completed` = 0, `The prime task, or` = 0; `The registration marker for an engine key` = 1, `The marker is not the prime task itself` = 1, `The registered marker, or` = 1 (coordinator-run phrase census, recorded in production-edit-scope.md).
- Read of TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs lines 10 to 26: the `GetPrimeTask` `<summary>` and `<returns>` match delivered source F5 verbatim and describe the registration marker, completed only after the prime outcome has been observed and reported.
- issue.md: `- [ ] AC5 (` changed to `- [x] AC5 (`; no other text changed.

## AC6 (P2-T15)

AC6: MET
- P1-T26 (FEATURE/evidence/qa-gates/file-line-counts.md): every clause met: PRODUCTION-FILES 3 (302, 197, 86 lines, each at most 450); TEST-PARTIALS 7 (largest 481, each at most 500); 3 production and 7 test compile entries, each LINES file registered exactly once.
- P1-T7 (FEATURE/evidence/regression-testing/split-census.md): census verdict PASS (split is a pure move).
- P2-T3: CSC_OUT_TASKMASTER 2, ERRORS 0. P2-T4: CSC_OUT_TASKMASTER 2, ERRORS 0.
- issue.md: `- [ ] AC6 (` changed to `- [x] AC6 (`; no other text changed.

## AC7 (P2-T16)

AC7: MET
- P1-T22: every remaining PHRASES-DOC entry holds its final value (all 24 rows MATCH; coordinator-run phrase census recorded in production-edit-scope.md).
- Read of TaskMaster/Ribbon/EngineToggleStateCoordinator.cs: constructor parameter docs `enginesAccessor` (87 to 93), `notifyUnavailable` (98 to 103) and `logError` (104 to 108) match F1; the `HandleToggleClickAsync` summary (160 to 164) and remarks (170 to 183) match F3. The code they describe holds: the refusal path routes the notification and the follow-up log call through `TryInvokeSink` (188 to 199), and the click boundary `catch` (208 to 211) routes its log call through `TryInvokeSink`; the only other `catch` is in `TryInvokeSink` (295 to 299).
- Read of TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs: the `StartObservedPrime` remarks (71 to 82) match F6 and name the two `catch` clauses that exist; the `CompletePrime` summary (131 to 137) and remarks (138 to 164) match F7a and F7b, and the code holds: the record `_reportedPrimeFaults[reportKey] = 0;` (190) is the only statement of the branch taken when `TryInvokeSink` returns `true` (183 to 191).
- issue.md: `- [ ] AC7 (` changed to `- [x] AC7 (`; no other text changed.

## AC8 (P2-T17)

AC8: MET
- P2-T7 (FEATURE/evidence/qa-gates/toolchain-final-pass.md): one clean pass (pass 1) of all four steps.
- P2-T5: runner exit code 0 (no failing test; no pre-existing FAILED-FQN values), LINE-FLOOR MET (85.96%), BRANCH-FLOOR MET (80.10%).
- P2-T6: FINAL-COORD-LINE-RATE 100 at least BASELINE-COORD-LINE-RATE 100.
- issue.md: `- [ ] AC8 (` changed to `- [x] AC8 (`; no other text changed.

### Acceptance Criteria Status
- Source: docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none
