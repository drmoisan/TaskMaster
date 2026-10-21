# Acceptance-Criteria Status Summary (issue 947)

Timestamp: 2026-10-01T18-13

AC1: MET (P1-T12 and FINAL-FIXTURE-RUN: both LaterReadStartsNewPrime variants Passed; POST-FORMAT CompletePrime span: _primeTasks.TryRemove 415 > Intentionally discarded 412; ISSUE-SHAPE after flip AC_OPEN=6 AC_DONE=1)
AC2: MET (PrimeFaultOrdering diff exit 0, hash = BASE-HASH-PFO, POSITIVE-CONTROL-DIFF-EXIT: 1; POST-FORMAT CompletePrime _logError 408 < TryRemove 415; PrimeHandleStaysRegisteredUntilFaultIsLogged Passed in both pass-after runs; ISSUE-SHAPE AC_DONE=2)
AC3: MET (FirstPrimeCompletesAndMarkerIsCleared Passed in both pass-after runs; partial POST-FORMAT `.Be(TaskStatus.RanToCompletion,` = 1; production POST-FORMAT CompletePrime SPAN-CATCH = 1; ISSUE-SHAPE AC_DONE=3)
AC4: MET (fail-before EXIT_CODE 1 with ExpectedExitCode 1, SEQUENCE_FILES 0, PROD-HASH-AT-CONTROL = BASE-HASH-PROD, FAILED and MESSAGE lines of the three prime-site tests carry the required reasons; the same three Passed in both pass-after runs; ISSUE-SHAPE AC_DONE=4)
AC5: MET (partial POST-FORMAT `[TestMethod]` 4, `[TestClass]` 0, `new Mock<` 0, `Thread.Sleep` 0, `Task.Delay` 0; determinism-tokens.md: three using rows 1, every prohibited token 0; toolchain-final-pass.md: pass 1 clean, all loop steps exit 0; COMPARISON: CHANGED-LINES-UNCOVERED: 0 and both CATCH-ARM-UNCOVERED: 0; ISSUE-SHAPE AC_DONE=5)
AC6: MET (WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport and WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate Passed in both pass-after runs; POST-FORMAT HandleToggleClickAsync SPAN-TRY 2, SPAN-CATCH 2, order try 181 < catch (Exception ex) 185 < _logError 189 < catch (Exception) 191 < discard 193; COMPARISON CATCH-ARM 1 owner=HandleToggleClickAsync CATCH-ARM-UNCOVERED: 0; ISSUE-SHAPE AC_DONE=6)
AC7: MET (fail-before FAILED HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport with MESSAGE carrying `the click boundary contains a failure of the sink itself` and `sink failed`, PROD-HASH-AT-CONTROL = BASE-HASH-PROD, EXIT_CODE 1 with ExpectedExitCode 1; Passed in both pass-after runs; ISSUE-SHAPE AC_DONE=7)

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

M and R were read from a fresh ISSUE-SHAPE run at 2026-10-01T18-15 (AC_OPEN=0 AC_DONE=7); `git diff --numstat` reports 7 insertions and 7 deletions in issue.md, one per checkbox.
