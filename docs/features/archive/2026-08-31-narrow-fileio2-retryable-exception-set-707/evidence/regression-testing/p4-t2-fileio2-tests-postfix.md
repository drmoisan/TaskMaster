Timestamp: 2026-09-03T13-25
Command: & $vstest "UtilitiesCS.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~FileIO2_Tests" "/Logger:trx;LogFileName=p4-t2.trx" "/ResultsDirectory:coverage\testresults\p4-t2"
(where $vstest resolved via vswhere to "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe")
EXIT_CODE: 0

TOTAL: 12
PASSED: 12
FAILED: 0
SKIPPED: 0
Failed test names: none

All 12 tests (the 11 pre-existing tests plus the new regression test) passed:
1. DeleteTextFile_WhenTargetIsMissing_ShouldNotThrow — Passed
2. WriteTextFile_WhenDevicePathIsUsed_ShouldThrowNotSupportedException — Passed
3. WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying — Passed
4. WriteTextFileAsync_WhenDirectoryDoesNotExist_ShouldReturnFalseWithoutRetrying — Passed (post-fix regression evidence)
5. WriteTextFileAsync_WhenEveryOpenAttemptFails_ShouldReturnFalseAfterBudget — Passed
6. WriteTextFileAsync_WhenTransientOpenFailureThenSucceeds_ShouldReturnTrueAndWriteAllLines — Passed
7. WriteTextFileAsync_WhenTokenAlreadyCancelled_ShouldThrowBeforeOpening — Passed
8. WriteTextFileAsync_WhenCancelledDuringRetryWindow_ShouldThrowPromptly — Passed
9. WriteTextFileAsync_WhenRetrying_ShouldPassCallerTokenToDelay — Passed
10. CsvReaders_WithFixtureAndMissingFiles_ShouldRespectHeaderOptions — Passed
11. SplitArrayTo2D_ShouldSupportZeroAndOneBasedLayouts — Passed
12. CsvReadTo2D_AndCsvReadToJagged_ShouldProjectFixtureRows — Passed

Output Summary: Test Run Successful. Total 12, Passed 12, Failed 0, Skipped 0. WriteTextFileAsync_WhenDirectoryDoesNotExist_ShouldReturnFalseWithoutRetrying passed post-fix; all 11 pre-existing tests named in evidence/baseline/p1-t3-pre-change-test-baseline.md also passed (no regression).
