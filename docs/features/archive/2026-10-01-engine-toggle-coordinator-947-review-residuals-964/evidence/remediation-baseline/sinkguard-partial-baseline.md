# SinkGuard partial baseline (P0-T3)

Timestamp: 2026-10-03T09-23 (host clock read at correction; the label first written was composed, not read)
Command: Grep tool counts over TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs for the TOKENS-R2, TOKENS-R1 and TOKENS-FORBIDDEN patterns and `^`; Grep -o `public async Task \w+`; Grep counts over TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests*.cs, EngineToggleStateCoordinatorTests.cs and TaskMaster.Test/TaskMaster.Test.csproj.
EXIT_CODE: 0
Output Summary: every count equals its false-before value; SinkGuard 169 lines; four existing test names; harness members present once each; primary fixture 481 lines; csproj registration 1.

SINKGUARD-LINES-BEFORE: 169

TOKENS-R2 (before):
- `harness\.Engines\.VerifyNoOtherCalls\(\);` = 1
- `harness\.Invalidations\.Should\(\)\.BeEmpty\("a refused click changes no state to display"\);` = 1
- `Engines\.VerifyNoOtherCalls` = 1

TOKENS-R1 (before):
- `\[DataTestMethod\]` = 0
- `\[DataRow\(null\)\]` = 0
- `\[DataRow\(""\)\]` = 0
- R1-NAME = 0
- `\[TestMethod\]` = 4
- `#region ` = 2
- `#endregion ` = 2

TOKENS-FORBIDDEN (combined alternation, one pattern): `Thread\.Sleep`, `Task\.Delay`, `DoNotParallelize`, `GetTempPath`, `File\.`, `DateTime\.Now`, `DateTime\.UtcNow` = 0 matching lines (each 0).
Positive control: `TaskCompletionSource` = 2.

SINKGUARD-NAMES-BEFORE:
- HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow (line 31)
- HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing (line 52)
- HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow (line 88)
- GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain (line 126)

R1-NAME count over the seven partials (EngineToggleStateCoordinatorTests*.cs): 0 matching files.

Harness (primary fixture): `internal Mock<IAppItemEngines> Engines` 1 (line 427), `internal List<string> Notifications` 1 (460), `internal List<LoggedError> Errors` 1 (462), `internal List<string> Invalidations` 1 (458), `internal Action<string> OnNotify` 1 (456), `\[DataRow\(null\)\]` 1 (line 102). Primary fixture `^` count: 481.

TEST-CSPROJ-REGISTRATION: 1
